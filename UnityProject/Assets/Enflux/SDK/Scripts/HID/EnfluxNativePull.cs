// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System.Runtime.InteropServices;
using Enflux.SDK.Core;
using Enflux.SDK.Core.DataTypes;

namespace Enflux.SDK.HID
{
    public static class EnfluxNativePull
    {
        private const string DllName = "EnfluxHID";

        #region Obsolete
        public static class RPY
        {
            // Temporary method, will be removed in future release!
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int LoadRotations(EnfluxDevice device,
                [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = 20)] byte[] outData);
        }
        #endregion


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartStreamingPull(EnfluxDevice device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartCalibrationPull(EnfluxDevice device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int HasNewCommand(EnfluxDevice device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PopCommand(EnfluxDevice device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadRotations(EnfluxDevice device,
            [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeConst = 5)] SByteQuaternion[]
                outData);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int EndStreamingThread();
    }
}
