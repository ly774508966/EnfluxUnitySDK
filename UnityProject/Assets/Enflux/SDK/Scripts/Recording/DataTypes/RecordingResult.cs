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
        FileAlreadyOpen
    }
}
