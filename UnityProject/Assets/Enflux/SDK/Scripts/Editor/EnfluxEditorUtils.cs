// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Enflux.SDK.Editor
{
    public static class EnfluxEditorUtils
    {
        private static readonly Color32 EnfluxBlue = new Color32(0x26, 0xAC, 0xE2, 0xFF);
        private static readonly Color32 CharcoalBlack = new Color32(0x3F, 0x3F, 0x3F, 0xFF);

        public static class GuiStyles
        {
            public const string Box = "box";
        }


        public static void SetEnfluxNormalButtonTheme()
        {
            if (EditorGUIUtility.isProSkin)
            {
                GUI.color = Color.white;
                GUI.contentColor = EnfluxBlue;
                GUI.backgroundColor = CharcoalBlack;
            }
            else
            {
                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;
            }
        }

        public static void SetEnfluxPlayButtonTheme()
        {
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.green;
        }

        public static void SetEnfluxPauseButtonTheme()
        {
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.yellow;
        }

        public static void SetEnfluxStopButtonTheme()
        {
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.red;
        }

        public static void SetEnfluxErrorTheme()
        {
            GUI.color = Color.white;
            GUI.contentColor = Color.red;
            GUI.backgroundColor = Color.red;
        }

        public static void SetDefaultTheme()
        {
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif