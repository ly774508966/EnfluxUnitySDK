// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.IO;
using Enflux.SDK.Recording;
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor.Recording
{
    [CustomEditor(typeof(EnfluxFileRecorder))]
    [CanEditMultipleObjects]
    public class EnfluxFileRecorderEditor : UnityEditor.Editor
    {
        private EnfluxFileRecorder _fileRecorder;
        private SerializedProperty _sourceSuitStreamProperty;
        private static string _previousDirectory;
        private const string SettingsPrefix = "Enflux-EnfluxFileRecorderEditor-";


        private void OnEnable()
        {
            _fileRecorder = (EnfluxFileRecorder) target;
            _sourceSuitStreamProperty = serializedObject.FindProperty("_sourceSuitStream");
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
                EditorGUILayout.Toggle("Is Recording", _fileRecorder.IsRecording);
            }
            EditorGUILayout.PropertyField(_sourceSuitStreamProperty);

            // Browse for enfl file
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Record File", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorStyles.textField.wordWrap = true;
            _fileRecorder.Filename = EditorGUILayout.TextArea(_fileRecorder.Filename);
            GUILayout.BeginHorizontal();
            EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
            if (GUILayout.Button("Browse Files"))
            {
                GUI.FocusControl(null);
                var filename = EditorUtility.SaveFilePanel("Set .enfl Recording Filename", _previousDirectory, "recording.enfl", "enfl");
                if (!string.IsNullOrEmpty(filename))
                {
                    _previousDirectory = Path.GetDirectoryName(filename);
                    _fileRecorder.Filename = filename;
                }
                EditorUtility.SetDirty(this);
            }

            // Start/stop recording
            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                if (!_fileRecorder.IsRecording)
                {
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_fileRecorder.Filename)))
                    {
                        if (GUILayout.Button("Start Recording"))
                        {
                            _fileRecorder.IsRecording = true;
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Recording"))
                    {
                        _fileRecorder.IsRecording = false;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // Apply changes to any modified serialized properties
            serializedObject.ApplyModifiedProperties();
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