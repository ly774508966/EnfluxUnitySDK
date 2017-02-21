// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System.Runtime.InteropServices;
using Enflux.SDK.Core.DataTypes;

namespace Enflux.SDK.HID
{
    public static class EnfluxNativePush
    {
        private const string DllName = "EnfluxHID";


        // This callback updates the module rotations.
        public delegate void DataStreamCallback([In] EnfluxDevice device, [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)] [In] ShortQuaternion[] data);

        // This callback updates the device status with input commands.
        public delegate void StatusCallback([In] EnfluxDevice device, InputCommands status);

        // This callback updates the device status with raw data.
        public delegate void RawDataCallback([In] EnfluxDevice device, int sensor, ref RawData data);

        public struct Callbacks
        {
            public StatusCallback StatusReceived;
            public DataStreamCallback DataStreamRecieved;
        };

        public struct RawCallbacks
        {
            public StatusCallback StatusReceived;
            public RawDataCallback RawDataReceived;
        }

        // Sets the connection interval on the module.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetInterval(EnfluxDevice devices, ushort intervalMs);

        // Starts streaming rotation data from the device. The callbacks will be called with updates.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartStreamingThread(EnfluxDevice devices, Callbacks callbacks);

        // Starts streaming raw sensor data from the device. The callbacks will be called with updates.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartRawDataThread(EnfluxDevice devices, RawCallbacks callbacks);

        // Starts calibrating the device. The callbacks will be called with updates.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartCalibrationThread(EnfluxDevice devices, Callbacks callbacks);

        // Stop streaming and receiving commands from a module.
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EndStreamingThread();
    }
}
