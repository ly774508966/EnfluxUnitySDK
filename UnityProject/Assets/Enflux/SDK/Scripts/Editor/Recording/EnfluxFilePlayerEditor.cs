// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System;
using Enflux.SDK.Core;
using Enflux.SDK.Recording;
using Enflux.SDK.Recording.DataTypes;
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor.Recording
{
    [CustomEditor(typeof(EnfluxFilePlayer))]
    [CanEditMultipleObjects]
    public class EnfluxFilePlayerEditor : UnityEditor.Editor
    {
        private EnfluxFilePlayer _filePlayer;
        private TimeSpan _durationTimeSpan;
        private string _durationText = "00:00.000";
        private static string _filenameToLoad;
        private string _errorType;
        private string _errorMessage;
        private const string SettingsPrefix = "Enflux-EnfluxFilePlayerEditor-";


        private void OnEnable()
        {
            _filePlayer = (EnfluxFilePlayer) target;
            LoadSettings();
            _filePlayer.StateChanged += FilePlayerOnStateChanged;
            _filePlayer.PlaybackReceivedError += FilePlayerOnPlaybackReceivedError;
        }

        private void OnDisable()
        {
            _filePlayer.StateChanged -= FilePlayerOnStateChanged;
            SaveSettings();
        }

        private void FilePlayerOnStateChanged(StateChange<PlaybackState> stateChange)
        {
            if (_filePlayer.IsLoaded)
            {
                _durationTimeSpan = TimeSpan.FromMilliseconds(_filePlayer.DurationMs);
                _durationText = string.Format("{0:00}:{1:00}.{2:000}",
                    _durationTimeSpan.Minutes, _durationTimeSpan.Seconds, _durationTimeSpan.Milliseconds);
            }
            else
            {
                _durationTimeSpan = TimeSpan.Zero;
                _durationText = "00:00.000";
            }
            EditorUtility.SetDirty(this);
        }

        private void FilePlayerOnPlaybackReceivedError(Notification<PlaybackResult> errorNotification)
        {
            _errorType = errorNotification.Value.ToString();
            _errorMessage = errorNotification.Message;
            EditorUtility.SetDirty(this);
        }

        public override void OnInspectorGUI()
        {
            // Settings
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                GUI.enabled = false;
                EditorGUILayout.EnumPopup("State", _filePlayer.State);
                GUI.enabled = true;
            }

            _filePlayer.Speed = EditorGUILayout.FloatField("Speed", _filePlayer.Speed);
            _filePlayer.IsLooping = EditorGUILayout.Toggle("Loop", _filePlayer.IsLooping);
            _filePlayer.AutoplayOnLoad = EditorGUILayout.Toggle("Autoplay On Load", _filePlayer.AutoplayOnLoad);
            EditorGUILayout.EndVertical();

            // Load/unload files
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Load New File", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorStyles.textField.wordWrap = true;
            _filenameToLoad = EditorGUILayout.TextArea(_filenameToLoad);
            GUILayout.BeginHorizontal();
            EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
            if (GUILayout.Button("Browse Files"))
            {
                GUI.FocusControl(null);
                var filters = new[] {"Enflux Animation", "enfl", "All Files", "*"};
                var path = EditorUtility.OpenFilePanelWithFilters("Open .enfl File", Application.streamingAssetsPath, filters);
                if (path != null)
                {
                    _filenameToLoad = path;
                }
                EditorUtility.SetDirty(this);
            }
            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_filenameToLoad)))
                {
                    if (GUILayout.Button("Load"))
                    {
                        _filePlayer.Load(_filenameToLoad);
                    }
                }
                if (_filePlayer.IsLoaded)
                {
                    if (GUILayout.Button("X", GUILayout.Width(50f)))
                    {
                        _filePlayer.Unload();
                    }
                }
            }
            EnfluxEditorUtils.SetDefaultTheme();
            GUILayout.EndHorizontal();
            // Error dialog
            if (_filePlayer.IsError)
            {
                EnfluxEditorUtils.SetEnfluxErrorTheme();
                EditorGUILayout.TextArea(string.Format("{0} error: {1}", _errorType, _errorMessage), EditorStyles.wordWrappedMiniLabel);
                EnfluxEditorUtils.SetDefaultTheme();
            }
            EditorGUILayout.EndVertical();

            // Current file metadata
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Current File", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            var filename = _filePlayer.IsLoaded ? _filePlayer.Filename : "No file loaded.";
            EditorGUILayout.SelectableLabel(filename, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled() || _filePlayer.IsError || _filePlayer.IsUnloaded))
            {
                if (_filePlayer.IsLoaded)
                {
                    EditorGUILayout.TextField("Duration", _durationText + " (mm:ss.ms)", EditorStyles.wordWrappedLabel);
                    EditorGUILayout.TextField("Shirt frames", _filePlayer.NumShirtFrames.ToString(), EditorStyles.wordWrappedLabel);
                    EditorGUILayout.TextField("Pants frames", _filePlayer.NumPantsFrames.ToString(), EditorStyles.wordWrappedLabel);
                    EditorGUILayout.Space();
                }

                // Time controls
                var inputCurrentTimeNormalized = GUILayout.HorizontalSlider(_filePlayer.CurrentTimeNormalized, 0f, 1f);
                // If slider moved
                if (!Mathf.Approximately(inputCurrentTimeNormalized, _filePlayer.CurrentTimeNormalized))
                {
                    _filePlayer.CurrentTimeNormalized = inputCurrentTimeNormalized;
                    if (!_filePlayer.IsPaused)
                    {
                        _filePlayer.Pause();
                    }
                    _filePlayer.CurrentTimeNormalized = inputCurrentTimeNormalized;
                }
                string timelineText;
                if (_filePlayer.IsLoaded)
                {
                    var currentTimeSpan = TimeSpan.FromMilliseconds(_filePlayer.CurrentTimeMs);
                    timelineText = string.Format("{0:00}:{1:00}.{2:000} / {3}",
                        currentTimeSpan.Minutes,
                        currentTimeSpan.Seconds,
                        currentTimeSpan.Milliseconds,
                        _durationText);
                }
                else
                {
                    timelineText = "00:00.000 / 00:00.000";
                }
                EditorGUILayout.LabelField(timelineText);

                // Playback controls
                GUILayout.BeginHorizontal();
                EnfluxEditorUtils.SetEnfluxStopButtonTheme();
                if (GUILayout.Button("Stop"))
                {
                    _filePlayer.Stop();
                }
                EnfluxEditorUtils.SetEnfluxPlayButtonTheme();
                if (!_filePlayer.IsPlaying)
                {
                    if (GUILayout.Button("Play"))
                    {
                        _filePlayer.Play();
                    }
                }
                else
                {
                    EnfluxEditorUtils.SetEnfluxPauseButtonTheme();
                    if (GUILayout.Button("Pause"))
                    {
                        _filePlayer.Pause();
                    }
                }
                EnfluxEditorUtils.SetDefaultTheme();
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(SettingsPrefix + "ErrorType", _errorType);
            EditorPrefs.SetString(SettingsPrefix + "ErrorMessage", _errorMessage);
            EditorPrefs.SetString(SettingsPrefix + "FilenameToLoad", _filenameToLoad);
        }

        private void LoadSettings()
        {
            _errorType = EditorPrefs.GetString(SettingsPrefix + "ErrorType");
            _errorMessage = EditorPrefs.GetString(SettingsPrefix + "ErrorMessage");
            _filenameToLoad = EditorPrefs.GetString(SettingsPrefix + "FilenameToLoad");
        }
    }
}
