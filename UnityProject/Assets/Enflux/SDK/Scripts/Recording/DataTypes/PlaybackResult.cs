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
