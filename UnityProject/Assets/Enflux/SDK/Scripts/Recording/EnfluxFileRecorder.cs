// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.IO;
using Enflux.Attributes;
using Enflux.SDK.Core;
using Enflux.SDK.Extensions;
using UnityEngine;
using System;
using Enflux.SDK.Recording.DataTypes;

namespace Enflux.SDK.Recording
{
    public class EnfluxFileRecorder : MonoBehaviour
    {
        public string Filename = "";
        [SerializeField, Readonly] private bool _isRecording;
        [SerializeField] private EnfluxSuitStream _sourceSuitStream;

        public event Action<RecordingResult> RecordingError;


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
                Debug.LogError(name + ": SourceSuitStream is not assigned and no EnfluxSuitStream instance is in the scene!");
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
                    var error = StartRecording(Filename);
                    // Close any persisting file (can happen in editor)
                    if (error == RecordingResult.FileAlreadyOpen)
                    {
                        EndRecording();
                        error = StartRecording(Filename);
                    }

                    if (error == RecordingResult.Success)
                    {
                        _isRecording = true;
                    }
                    else
                    {
                        Debug.LogError(name + " - Unable to start recording. Error: " + error);
                        IsRecording = false;
                        if (RecordingError != null)
                        {
                            RecordingError(error);
                        }
                        return;
                    }
                }
                else
                {
                    RecordingResult error = EndRecording();
                    if (error != RecordingResult.Success)
                    {
                        Debug.LogError(name + " - Unable to end recording. Error: " + error);
                        if (RecordingError != null)
                        {
                            RecordingError(error);
                        }
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
            return RecordingResult.Success;
#endif
        }

        private RecordingResult EndRecording()
        {
            if (_sourceSuitStream != null)
            {
                RecordingResult shirtError = SetShirtBaseOrientation(_sourceSuitStream.ShirtBaseOrientation);
                RecordingResult pantsError = SetPantsBaseOrientation(_sourceSuitStream.PantsBaseOrientation);
                if(shirtError != RecordingResult.Success)
                {
                    Debug.LogError(name + " - Unable to set shirt base. Error: " + shirtError);
                    if (RecordingError != null)
                    {
                        RecordingError(shirtError);
                    }
                }
                if (pantsError != RecordingResult.Success)
                {
                    Debug.LogError(name + " - Unable to set pants base. Error: " + pantsError);
                    if (RecordingError != null)
                    {
                        RecordingError(pantsError);
                    }
                }
            }
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.EndRecording();
#else
            return RecordingResult.Success;
#endif
        }

        private RecordingResult SetShirtBaseOrientation(Vector3 orientation)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.SetShirtBaseOrientation(orientation.ToEnfluxVector3());
#else
            return RecordingResult.Success;
#endif
        }

        private RecordingResult SetPantsBaseOrientation(Vector3 orientation)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return EnfluxNativeFileRecorder.SetPantsBaseOrientation(orientation.ToEnfluxVector3());
#else
            return RecordingResult.Success;
#endif
        }
    }
}