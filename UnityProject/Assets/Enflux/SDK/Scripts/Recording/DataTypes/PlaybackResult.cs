namespace Enflux.SDK.Recording.DataTypes
{
    public enum PlaybackResult
    {
        Success = 0,
        FileDoesNotExist,
        CannotParseHeader,
        CannotParseFrame,
        UnsupportedVersion
    }
}
