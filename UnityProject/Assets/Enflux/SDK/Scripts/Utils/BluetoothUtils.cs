// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System;
using System.Diagnostics;
using UnityEngine;

namespace Enflux.SDK.Utils
{
    public static class BluetoothUtils
    {
        public static bool LaunchBluetoothManager()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer &&
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                return false;
            }
            if (Environment.OSVersion.Version.Major != 10)
            {
                return false;
            }

            // Windows 10
            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = "powershell.exe",
                    Arguments = "start ms-settings:bluetooth"
                }
            };

            return process.Start();
        }
    }
}