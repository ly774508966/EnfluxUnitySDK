// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using Enflux.SDK.Core;
using Enflux.SDK.Core.DataTypes;
using Enflux.SDK.Utils;
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor
{

    [CustomEditor(typeof(EnfluxManager))]
    [CanEditMultipleObjects]
    public class EnfluxManagerEditor : UnityEditor.Editor
    {
        private EnfluxManager _manager;

        private void OnEnable()
        {
            _manager = (EnfluxManager) target;
        }

        public override void OnInspectorGUI()
        {
            // Connection
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.EnumPopup("Shirt State", _manager.ShirtState);
                    EditorGUILayout.EnumPopup("Pants State", _manager.PantsState);
                }
                // Connect/disconnect shirt or pants
                GUILayout.BeginHorizontal();
                EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
                if (!_manager.IsShirtActive)
                {
                    if (GUILayout.Button("Connect Shirt"))
                    {
                        _manager.Connect(EnfluxDevice.Shirt);
                    }
                }
                if (!_manager.ArePantsActive)
                {
                    if (GUILayout.Button("Connect Pants"))
                    {
                        _manager.Connect(EnfluxDevice.Pants);
                    }
                }
                if (!_manager.IsShirtActive && !_manager.ArePantsActive)
                {
                    if (GUILayout.Button("Connect Shirt and Pants"))
                    {
                        _manager.Connect(EnfluxDevice.All);
                    }
                }
                if (_manager.IsShirtActive && !_manager.ArePantsActive)
                {
                    if (GUILayout.Button("Disconnect Shirt"))
                    {
                        _manager.Disconnect();
                    }
                }
                else if (!_manager.IsShirtActive && _manager.ArePantsActive)
                {
                    if (GUILayout.Button("Disconnect Pants"))
                    {
                        _manager.Disconnect();
                    }
                }
                else if (_manager.IsShirtActive && _manager.ArePantsActive)
                {
                    if (GUILayout.Button("Disconnect Shirt and Pants"))
                    {
                        _manager.Disconnect();
                    }
                }
                EnfluxEditorUtils.SetDefaultTheme();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Calibration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                // Calibrate shirt or pants
                GUILayout.BeginHorizontal();
                EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
                using (new EditorGUI.DisabledScope(_manager.IsShirtActive))
                {
                    if (GUILayout.Button("Calibrate Shirt"))
                    {
                        _manager.Calibrate(EnfluxDevice.Shirt);
                    }
                }
                using (new EditorGUI.DisabledScope(_manager.ArePantsActive))
                {
                    if (GUILayout.Button("Calibrate Pants"))
                    {
                        _manager.Calibrate(EnfluxDevice.Pants);
                    }
                }
                EnfluxEditorUtils.SetDefaultTheme();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Alignment", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
            using (new EditorGUI.DisabledScope(!this.IsGuiEnabled()))
            {
                // Align shirt + pants
                using (new EditorGUI.DisabledScope(!_manager.IsAnyDeviceActive))
                {
                    if (GUILayout.Button("Reset Orientation"))
                    {
                        _manager.ResetFullBodyBaseOrientation();
                    }
                }
            }
            EnfluxEditorUtils.SetDefaultTheme();
            GUILayout.EndVertical();

            // Bluetooth
            EditorGUILayout.BeginVertical(EnfluxEditorUtils.GuiStyles.Box);
            EditorGUILayout.LabelField("Bluetooth", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EnfluxEditorUtils.SetEnfluxNormalButtonTheme();
            if (GUILayout.Button("Open Bluetooth Manager"))
            {
                BluetoothUtils.LaunchBluetoothManager();
            }
            EnfluxEditorUtils.SetDefaultTheme();
            GUILayout.EndVertical();
        }
    }
}