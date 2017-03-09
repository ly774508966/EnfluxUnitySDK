// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using Enflux.Attributes;
using Enflux.SDK.Core;
using Enflux.SDK.Extensions;
using UnityEngine;
using System;
using System.IO;
using Enflux.SDK.Recording.DataTypes;

namespace Enflux.SDK.Recording
{
    public class EnfluxFileRecorder : MonoBehaviour
    {
        public string Filename = "";
        [SerializeField, Readonly] private bool _isRecording;
        [SerializeField] private EnfluxSuitStream _sourceSuitStream;

        public event Action<Notification<RecordingResult>> RecordingReceivedError;


        private string DefaultFilename
        {
            get { return Application.streamingAssetsPath + "/PoseRecordings/recording.enfl"; }
        }

        private void Reset()
        {
            Filename = DefaultFilename;
        }

        private void OnEnable()
        {
            _sourceSuitStream = _sourceSuitStream ?? FindObjectOfType<EnfluxManager>();

            if (_sourceSuitStream == null)
            {
                Debug.LogError(name + ", SourceSuitStream is not assigned and no EnfluxSuitStream instance is in the scene!");
            }
        }

        private void Start()
        {
            if (Filename == "")
            {
                Filename = DefaultFilename;
            }
        }

        private void OnApplicationQuit()
        {
            if (_isRecording)
            {
                EndRecording();
            }
        }

#if TEST_RECORD_PLAYBACK
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                IsRecording = !IsRecording;
            }
        }
#endif

        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                if (IsRecording == value)
                {
                    return;
                }
                if (!_isRecording)
                {
                    // Create directory for recording if it doesn't exist
                    try
                    {
                        var directoryPath = Path.GetDirectoryName(Filename);
                        Directory.CreateDirectory(directoryPath ?? "");
                    }
                    catch (Exception e)
                    {
                        IsRecording = false;
                        RaiseRecordingErrorEvent(RecordingResult.FileOpenError, e.GetType() + " - " + e.Message);
                        return;
                    }
                    var result = StartRecording(Filename);
                    // Close any persisting file (can happen in editor)
                    if (result == RecordingResult.FileAlreadyOpen)
                    {
                        EndRecording();
                        result = StartRecording(Filename);
                    }

                    if (result == RecordingResult.Success)
                    {
                        _isRecording = true;
                    }
                    else
                    {
                        var errorMessage = "Unable to start recording.";
                        IsRecording = false;
                        RaiseRecordingErrorEvent(result, errorMessage);
                    }
                }
                else
                {
                    var result = EndRecording();
                    if (result != RecordingResult.Success)
                    {
                        var errorMessage = "Unable to end recording.";
                        RaiseRecordingErrorEvent(result, errorMessage);
                    }
                    _isRecording = value;
                }
            }
        }

        private RecordingResult StartRecording(string filename)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.StartRecording(filename);
#else
            return RecordingResult.PlatformNotSupported;
#endif
        }

        private RecordingResult EndRecording()
        {
            if (_sourceSuitStream != null)
            {
                var shirtError = SetShirtBaseOrientation(_sourceSuitStream.ShirtBaseOrientation);
                var pantsError = SetPantsBaseOrientation(_sourceSuitStream.PantsBaseOrientation);
                if (shirtError != RecordingResult.Success)
                {
                    var errorMessage = "Unable to set shirt base orientation.";
                    RaiseRecordingErrorEvent(shirtError, errorMessage);
                }
                if (pantsError != RecordingResult.Success)
                {
                    var errorMessage = "Unable to set pants base orientation.";
                    RaiseRecordingErrorEvent(pantsError, errorMessage);
                }
            }
            else
            {
                Debug.LogWarning("SourceSuitStream is null! Both shirt and pants base orientation were not set.");
            }
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.EndRecording();
#else
            return RecordingResult.PlatformNotSupported;
#endif
        }

        private RecordingResult SetShirtBaseOrientation(Vector3 orientation)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.SetShirtBaseOrientation(orientation.ToEnfluxVector3());
#else
            return RecordingResult.PlatformNotSupported;
#endif
        }

        private RecordingResult SetPantsBaseOrientation(Vector3 orientation)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.SetPantsBaseOrientation(orientation.ToEnfluxVector3());
#else
            return RecordingResult.PlatformNotSupported;
#endif
        }

        private void RaiseRecordingErrorEvent(RecordingResult result, string errorMessage)
        {
            var handler = RecordingReceivedError;
            if (handler != null)
            {
                handler(new Notification<RecordingResult>(result, errorMessage));
            }
            Debug.LogError(string.Format("{0}: {1}", result, errorMessage));
        }
    }
}