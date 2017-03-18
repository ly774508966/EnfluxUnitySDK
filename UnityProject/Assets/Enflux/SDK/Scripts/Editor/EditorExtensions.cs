// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor
{
    public static class EditorExtensions
    {
        public static bool IsGuiEnabled(this UnityEditor.Editor editor)
        {
            var target = editor.target as Behaviour;
            var isTargetActiveAndEnabled = target == null || target.isActiveAndEnabled;
            return Application.isPlaying && !EditorApplication.isPaused && editor.target != null && isTargetActiveAndEnabled;
        }
    }
}

#endif