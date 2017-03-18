// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
namespace Enflux.SDK.Recording.DataTypes
{
    public enum PlaybackResult
    {
        Success = 0,
        UnknownError,
        FileDoesNotExist,
        InvalidHeader,
        InvalidFrame,
        InvalidFormat,
        UnsupportedVersion,
        PermissionError,
        NoFileLoaded
    }
}
