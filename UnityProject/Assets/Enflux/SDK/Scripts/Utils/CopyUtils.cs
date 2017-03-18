// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.Runtime.InteropServices;

namespace Enflux.SDK.Utils
{
    public static class CopyUtils
    {
        public static byte[] StructToByteArray<T>(T obj) where T : struct
        {
            var len = Marshal.SizeOf(typeof(T));
            var arr = new byte[len];
            var ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static T ByteArrayToStruct<T>(byte[] bytearray) where T : struct 
        {
            var len = Marshal.SizeOf(typeof(T));
            var i = Marshal.AllocHGlobal(len);

            Marshal.Copy(bytearray, 0, i, len);
            var obj = Marshal.PtrToStructure(i, typeof(T));
            Marshal.FreeHGlobal(i);
            return (T) obj;
        }
    }
}