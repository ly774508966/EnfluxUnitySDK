// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.IO;
using Enflux.SDK.Recording;
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor.Recording
{
    [CustomEditor(typeof(EnfluxFilePlayer))]
    [CanEditMultipleObjects]
    public class EnfluxFilePlayerEditor : UnityEditor.Editor
    {
        private EnfluxFilePlayer _filePlayer;
        private static string _previousDirectory;
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
            EditorGUILayout.LabelField("Load File", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorStyles.textField.wordWrap = true;
            _filePlayer.Filename = EditorGUILayout.TextArea(_filePlayer.Filename);
            GUILayout.BeginHorizontal();
            EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
            if (GUILayout.Button("Browse Files"))
            {
                GUI.FocusControl(null);
                var filters = new[] {"Enflux Animation", "enfl", "All Files", "*"};
                var filename = EditorUtility.OpenFilePanelWithFilters("Open .enfl File", _previousDirectory, filters);
                if (!string.IsNullOrEmpty(filename))
                {
                    _previousDirectory = Path.GetDirectoryName(filename);
                    _filePlayer.Filename = filename;
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
            EditorPrefs.SetString(SettingsPrefix + "PreviousDirectory", _previousDirectory);
        }

        private void LoadSettings()
        {
            _previousDirectory = EditorPrefs.GetString(SettingsPrefix + "PreviousDirectory", Application.streamingAssetsPath);
        }
    }
}