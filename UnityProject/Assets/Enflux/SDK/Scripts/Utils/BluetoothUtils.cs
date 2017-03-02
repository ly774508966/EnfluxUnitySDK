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

            // Untested Windows 8 Command:
            // control / name Microsoft.BluetoothDevices
        }
    }
}