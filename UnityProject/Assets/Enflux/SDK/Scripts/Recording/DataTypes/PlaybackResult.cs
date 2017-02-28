using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enflux.SDK.Recording
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
