// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using Enflux.SDK.Core;
using UnityEditor;

namespace Enflux.SDK.Editor.Core
{
    [CustomEditor(typeof(Humanoid))]
    [CanEditMultipleObjects]
    public class HumanoidEditor : UnityEditor.Editor
    {
        private Humanoid _humanoid;
        private SerializedProperty _absoluteAnglesStreamProperty;


        private void OnEnable()
        {
            _humanoid = (Humanoid) target;
            _absoluteAnglesStreamProperty = serializedObject.FindProperty("_absoluteAnglesStream");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var previousAbsoluteAnglesStream = _humanoid.AbsoluteAnglesStream;
            EditorGUILayout.PropertyField(_absoluteAnglesStreamProperty);
      
            serializedObject.ApplyModifiedProperties();
            if (previousAbsoluteAnglesStream != _absoluteAnglesStreamProperty.objectReferenceValue)
            {
                _humanoid.AbsoluteAnglesStream = (EnfluxSuitStream)_absoluteAnglesStreamProperty.objectReferenceValue;
            }
        }
    }
}