// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
namespace Enflux.SDK.Recording.DataTypes
{
    public enum RecordingResult
    {
        Success = 0,
        FileNotOpen,
        FileOpenError,
        FileWriteError,
        FileCloseError,
        FileDoesNotExist,
        FileLengthError,
        FileNameEmpty,
        FileAlreadyOpen,

        //Keep this at the end to not interfere with native code enum values
        PlatformNotSupported
    }
}
