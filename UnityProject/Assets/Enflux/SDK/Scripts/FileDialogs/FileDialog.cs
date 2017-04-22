// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

namespace Enflux.SDK.FileDialogs
{
    /// <summary>
    /// This is a multiplatform wrapper to access file dialogs for opening and saving files and folders.
    /// </summary>
    /// <remarks>
    /// Implementation is based off of gkngkc's awesome work on UnityStandaloneFileBrowser: https://github.com/gkngkc/UnityStandaloneFileBrowser
    /// </remarks>
    public class FileDialog
    {
        private static IFileDialog _nativeFileDialog;

        private static IFileDialog NativeFileDialog
        {
            get
            {
                if (_nativeFileDialog == null)
                {
#if UNITY_STANDALONE_OSX
                    _nativeFileDialog = new FileDialogMac();
#elif UNITY_STANDALONE_WIN
                    _nativeFileDialog = new FileDialogWindows();
#elif UNITY_EDITOR
                    _nativeFileDialog = new FileDialogUnityEditor();
#else
                    _nativeFileDialog = new FileDialogUnsupportedPlatform();
#endif
                }
                return _nativeFileDialog;
            }
        }

        /// <summary>
        /// Open native file picker dialog.
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public static string[] OpenFilePanel(string title, string directory, string extension, bool multiselect = false)
        {
            var extensions = string.IsNullOrEmpty(extension) ? null : new[] {new ExtensionFilter("", extension)};
            return OpenFilePanel(title, directory, extensions, multiselect);
        }

        /// <summary>
        /// Open native file picker dialog.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="directory">Root directory.</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection.</param>
        /// <returns>Returns array of chosen paths, or a zero length array if cancelled.</returns>
        public static string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect = false)
        {
            return NativeFileDialog.OpenFilePanel(title, directory, extensions, multiselect);
        }

        /// <summary>
        /// Open native folder picker dialog.
        /// NOTE: Multiple folder selection isn't supported on Windows.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="directory">Root directory.</param>
        /// <param name="multiselect"></param>
        /// <returns>Returns array of chosen paths, or a zero length array if cancelled.</returns>
        public static string[] OpenFolderPanel(string title, string directory, bool multiselect = false)
        {
            return NativeFileDialog.OpenFolderPanel(title, directory, multiselect);
        }

        /// <summary>
        /// Open native save dialog.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="directory">Root directory.</param>
        /// <param name="defaultName">Default file name.</param>
        /// <param name="extension">File extension.</param>
        /// <returns>Returns chosen path, or an empty string if cancelled.</returns>
        public static string SaveFilePanel(string title, string directory, string defaultName, string extension)
        {
            var extensions = string.IsNullOrEmpty(extension) ? null : new[] {new ExtensionFilter("", extension)};
            return SaveFilePanel(title, directory, defaultName, extensions);
        }

        /// <summary>
        /// Open native save dialog.
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <returns>Returns chosen path. Empty string when cancelled</returns>
        public static string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            return NativeFileDialog.SaveFilePanel(title, directory, defaultName, extensions);
        }
    }
}