// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;
using System.IO;
using System.Runtime.InteropServices;
using Enflux.SDK.Core;
using Enflux.SDK.Core.DataTypes;
using Enflux.SDK.Recording;
using Enflux.SDK.Recording.DataTypes;

namespace Enflux.SDK.Utils
{
    public static class PlaybackUtils
    {
        public static Notification<PlaybackResult> IsValidEnflFile(string filename)
        {
            const int numBytesTimestamp = 4;
            const int numBytesFrame = 20;

            if (!File.Exists(filename))
            {
                var errorMessage = "File path doesn't exist: '" + filename + "'";
                return new Notification<PlaybackResult>(PlaybackResult.FileDoesNotExist, errorMessage);
            }
            using (var fileStream = File.OpenRead(filename))
            {
                var numBytesAnimationHeader = Marshal.SizeOf(typeof(AnimationHeader));
                var rawHeader = new byte[numBytesAnimationHeader];
                var rawTimestamp = new byte[numBytesTimestamp];
                var lastShirtFrame = new byte[numBytesFrame];
                var lastPantsFrame = new byte[numBytesFrame];
                // Read file header
                AnimationHeader header;
                if (fileStream.CanRead &&
                    fileStream.Read(rawHeader, 0, numBytesAnimationHeader) == numBytesAnimationHeader)
                {
                    header = AnimationHeader.InitializeFromArray(rawHeader);
                }
                else
                {
                    var errorMessage = "Unable to read the file header for: '" + filename + "'";
                    return new Notification<PlaybackResult>(PlaybackResult.InvalidHeader, errorMessage);
                }
                if (!IsValidHeader(header))
                {
                    var errorMessage =
                        string.Format(
                            "Incorrect file header version! Have 'HeaderVersion: {0}, FrameVersion: {1}', expected 'HeaderVersion: {2}, FrameVersion: {3}'",
                            header.HeaderVersion,
                            header.FrameVersion,
                            AnimationHeader.SupportedHeaderVersion,
                            AnimationHeader.SupportedFrameVersion
                        );
                    return new Notification<PlaybackResult>(PlaybackResult.UnsupportedVersion, errorMessage);
                }
                ulong numReadShirtFrames = 0;
                ulong numReadPantsFrames = 0;
                var currentFrame = 1;
                if (header.NumShirtFrames != 0 || header.NumPantsFrames != 0)
                {
                    // Validate file contents
                    while (fileStream.Position < fileStream.Length)
                    {
                        if (!fileStream.CanRead)
                        {
                            var errorMessage = string.Format("Unable to to read '{0}'. Is the file closed, or do you have permissions to read it?", filename);
                            return new Notification<PlaybackResult>(PlaybackResult.PermissionError, errorMessage);
                        }
                        // Verify timestamp bytes
                        if (fileStream.Read(rawTimestamp, 0, numBytesTimestamp) != numBytesTimestamp)
                        {
                            var errorMessage = string.Format("Unable to read timestamp at frame {0}. The file may be corrupt!", currentFrame);
                            return new Notification<PlaybackResult>(PlaybackResult.InvalidFrame, errorMessage);
                        }
                        // Verify timestamp value
                        var timestamp = BitConverter.ToUInt32(rawTimestamp, 0);
                        if (timestamp > header.Duration)
                        {
                            var errorMessage = string.Format("Frame {0}: timestamp of {1} is greater than duration of {2}!", currentFrame, timestamp, header.Duration);
                            return new Notification<PlaybackResult>(PlaybackResult.InvalidFrame, errorMessage);
                        }
                        // Verify device data bytes
                        var deviceType = (EnfluxDevice) fileStream.ReadByte();
                        if (deviceType == EnfluxDevice.Shirt)
                        {
                            if (fileStream.Read(lastShirtFrame, 0, numBytesFrame) != numBytesFrame)
                            {
                                var errorMessage = string.Format("Unable to read shirt angle data at frame {0}. The file may be corrupt!", currentFrame);
                                return new Notification<PlaybackResult>(PlaybackResult.InvalidFrame, errorMessage);
                            }
                            ++numReadShirtFrames;
                        }
                        else if (deviceType == EnfluxDevice.Pants)
                        {
                            if (fileStream.Read(lastPantsFrame, 0, numBytesFrame) != numBytesFrame)
                            {
                                var errorMessage = string.Format("Unable to read pants angle data at frame {0}. The file may be corrupt!", currentFrame);
                                return new Notification<PlaybackResult>(PlaybackResult.InvalidFrame, errorMessage);
                            }
                            ++numReadPantsFrames;
                        }
                        ++currentFrame;
                    }
                }
                else if (fileStream.Position < fileStream.Length)
                {
                    var errorMessage = string.Format("File contained no recorded frames, but there were {0} bytes after the header!", fileStream.Length - rawHeader.Length);
                    return new Notification<PlaybackResult>(PlaybackResult.InvalidFormat, errorMessage);
                }
                if (numReadShirtFrames != header.NumShirtFrames)
                {
                    var errorMessage = string.Format("File contained {0} shirt frames, expected {1}", numReadShirtFrames, header.NumShirtFrames);
                    return new Notification<PlaybackResult>(PlaybackResult.InvalidFormat, errorMessage);
                }
                if (numReadPantsFrames != header.NumPantsFrames)
                {
                    var errorMessage = string.Format("File contained {0} pants frames, expected {1}", numReadPantsFrames, header.NumPantsFrames);
                    return new Notification<PlaybackResult>(PlaybackResult.InvalidFormat, errorMessage);
                }
            }
            var successMessage = filename + " is a valid .enfl file.";
            return new Notification<PlaybackResult>(PlaybackResult.Success, successMessage);
        }

        public static bool IsValidHeader(AnimationHeader header)
        {
            if (header.HeaderVersion != AnimationHeader.SupportedHeaderVersion)
            {
                return false;
            }
            if (header.FrameVersion != AnimationHeader.SupportedFrameVersion)
            {
                return false;
            }
            return true;
        }
    }
}