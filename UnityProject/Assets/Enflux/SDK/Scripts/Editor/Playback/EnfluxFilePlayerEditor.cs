// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using Enflux.SDK.Recording;
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor.Playback
{
    [CustomEditor(typeof(EnfluxFilePlayer))]
    [CanEditMultipleObjects]
    public class EnfluxFilePlayerEditor : UnityEditor.Editor
    {
        private EnfluxFilePlayer _filePlayer;
        private static string _previousFilepath;
        private const string SettingsPrefix = "Enflux-EnfluxFilePlayerEditor-";


        private void OnEnable()
        {
            _filePlayer = (EnfluxFilePlayer) target;
            LoadSettings();
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Is Playing", _filePlayer.IsPlaying);
            }

            // Browse for enfl file
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Load New File", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorStyles.textField.wordWrap = true;
            _filePlayer.Filename = EditorGUILayout.TextArea(_filePlayer.Filename);
            GUILayout.BeginHorizontal();
            EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
            if (GUILayout.Button("Browse Files"))
            {
                GUI.FocusControl(null);
                var filters = new[] {"Enflux Animation", "enfl", "All Files", "*"};
                var path = EditorUtility.OpenFilePanelWithFilters("Open .enfl File", _previousFilepath, filters);
                if (path != null)
                {
                    _previousFilepath = path;
                    _filePlayer.Filename = path;
                }
                EditorUtility.SetDirty(this);
            }

            // Start/stop playback
            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                if (!_filePlayer.IsPlaying)
                {
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_filePlayer.Filename)))
                    {
                        if (GUILayout.Button("Play"))
                        {
                            _filePlayer.IsPlaying = true;
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop"))
                    {
                        _filePlayer.IsPlaying = false;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(SettingsPrefix + "PreviousFilepath", _previousFilepath);
        }

        private void LoadSettings()
        {
            _previousFilepath = EditorPrefs.GetString(SettingsPrefix + "PreviousFilepath", Application.streamingAssetsPath);
        }
    }
}