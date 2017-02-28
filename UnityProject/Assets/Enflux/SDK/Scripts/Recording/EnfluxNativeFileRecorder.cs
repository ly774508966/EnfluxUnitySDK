// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;
using System.Runtime.InteropServices;
using Enflux.SDK.Core.DataTypes;

namespace Enflux.SDK.Recording
{
    public static class EnfluxNativeFileRecorder
    {
        private const string DllName = "EnfluxHID";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern RecordingResult StartRecording(string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern RecordingResult EndRecording();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern RecordingResult SetShirtBaseOrientation(ShortVector3 orientation);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern RecordingResult SetPantsBaseOrientation(ShortVector3 orientation);
    }
}