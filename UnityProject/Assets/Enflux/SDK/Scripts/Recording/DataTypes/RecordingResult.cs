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
