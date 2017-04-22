// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System;

namespace Enflux.SDK.FileDialogs
{
    public class FileDialogUnsupportedPlatform : IFileDialog
    {
        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            throw new PlatformNotSupportedException();
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect)
        {
            throw new PlatformNotSupportedException();
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            throw new PlatformNotSupportedException();
        }
    }
}

