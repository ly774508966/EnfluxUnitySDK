using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enflux.SDK.Recording
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
    }
}
