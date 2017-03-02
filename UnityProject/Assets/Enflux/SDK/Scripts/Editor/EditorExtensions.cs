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