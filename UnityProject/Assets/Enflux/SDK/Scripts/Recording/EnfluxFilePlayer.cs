// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Enflux.SDK.Attributes;
using Enflux.SDK.Core;
using Enflux.SDK.Core.DataTypes;
using Enflux.SDK.Extensions;
using Enflux.SDK.Recording.DataTypes;
using Enflux.SDK.Utils;
using UnityEngine;

namespace Enflux.SDK.Recording
{
    /// <summary>
    /// Plays back .enfl files. Current angles are accessible through <see cref="AbsoluteAngles"/>. During playback, the file data is streamed from disk.
    /// </summary>
    public class EnfluxFilePlayer : EnfluxSuitStream
    {
        [SerializeField, Readonly] private PlaybackState _state = PlaybackState.Loading;
        [SerializeField] private uint _currentTimeMs;
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private bool _isLooping;
        [SerializeField] private bool _autoplayOnLoad = true;
        [SerializeField, Readonly] private string _filename = "";

        private IEnumerator _co_playback;
        private AnimationHeader _header;
        private Stream _dataStream;
        private long _headerEndPosition;
        private long _nextShirtFramePosition;
        private long _nextPantsFramePosition;
        private readonly byte[] _timestampBuffer = new byte[NumBytesTimestamp];
        private readonly byte[] _dataBuffer = new byte[NumBytesFrame];
        private EnfluxSampleVector3 _currentShirtFrame;
        private EnfluxSampleVector3 _nextShirtFrame;
        private EnfluxSampleVector3 _currentPantsFrame;
        private EnfluxSampleVector3 _nextPantsFrame;

        private const int NumBytesTimestamp = 4;
        private const int NumBytesFrame = 20;

        /// <summary>
        /// Invoked when the state of the file player has changed.
        /// </summary>
        public event Action<StateChange<PlaybackState>> StateChanged;

        /// <summary>
        /// Invoked when the file player has received an error.
        /// </summary>
        public event Action<Notification<PlaybackResult>> PlaybackReceivedError;

        /// <summary>
        /// Invoked when the last frame played, regardless of looping.
        /// </summary>
        public event Action LastFramePlayed;

        /// <summary>
        /// Invoked when the playback rate has changed.
        /// </summary>
        public event Action<float> SpeedChanged;

        /// <summary>
        /// Invoked when looping has changed.
        /// </summary>
        public event Action<bool> IsLoopingChanged;

        /// <summary>
        /// Invoked when autoplay has changed.
        /// </summary>
        public event Action<bool> AutoplayOnLoadChanged;

        /// <summary>
        /// The current operation state of the playback.
        /// </summary>
        public PlaybackState State
        {
            get { return _state; }
            private set
            {
                if (_state == value)
                {
                    return;
                }
                var previous = _state;
                _state = value;

                switch (_state)
                {
                    case PlaybackState.Unloaded:
                    case PlaybackState.Paused:
                    case PlaybackState.Stopped:
                    case PlaybackState.Error:
                        if (_state == PlaybackState.Unloaded)
                        {
                            Filename = "";
                        }
                        if (_state == PlaybackState.Stopped)
                        {
                            CurrentTimeMs = 0;
                            SetFramesToTime(CurrentTimeMs);
                        }
                        if (_co_playback != null)
                        {
                            StopCoroutine(_co_playback);
                            _co_playback = null;
                        }
                        break;

                    case PlaybackState.Finished:
                        CurrentTimeMs = DurationMs;
                        _co_playback = null;
                        break;

                    case PlaybackState.Playing:
                        if (previous == PlaybackState.Finished)
                        {
                            CurrentTimeMs = 0;
                        }
                        if (_co_playback == null)
                        {
                            _co_playback = Co_Playback();
                        }
                        StartCoroutine(_co_playback);
                        break;
                }

                var handler = StateChanged;
                if (handler != null)
                {
                    handler.Invoke(new StateChange<PlaybackState>(previous, _state));
                }
            }
        }

        /// <summary>
        /// Current time of the playback in seconds.
        /// </summary>
        public float CurrentTime
        {
            get { return CurrentTimeMs * EnfluxMath.MillisecondsToSeconds; }
            set
            {
                if (!IsLoaded)
                {
                    return;
                }
                CurrentTimeMs = (uint) (value * EnfluxMath.SecondsToMilliseconds);
            }
        }

        /// <summary>
        /// Current time of the playback in milliseconds.
        /// </summary>
        public uint CurrentTimeMs
        {
            get { return _currentTimeMs; }
            set
            {
                if (_currentTimeMs == value || !IsLoaded)
                {
                    return;
                }
                _currentTimeMs = value;
                SetFramesToTime(_currentTimeMs);
            }
        }

        /// <summary>
        /// Current time of the playback in normalized time. 0 is the first frame of playback, 1 is the last frame;
        /// </summary>
        public float CurrentTimeNormalized
        {
            get { return IsLoaded ? CurrentTime / Duration : 0; }
            set
            {
                if (!IsLoaded)
                {
                    return;
                }
                var clampedValue = Mathf.Clamp01(value);
                CurrentTime = clampedValue * Duration;
            }
        }

        /// <summary>
        /// Total duration of the playback in seconds.
        /// </summary>
        public float Duration
        {
            get { return DurationMs * EnfluxMath.MillisecondsToSeconds; }
        }

        /// <summary>
        /// Total duration of the playback in milliseconds.
        /// </summary>
        public uint DurationMs
        {
            get { return IsLoaded ? _header.Duration : 0; }
        }

        /// <summary>
        /// Total number of shirt frames of the playback.
        /// </summary>
        public ulong NumShirtFrames
        {
            get { return IsLoaded ? _header.NumShirtFrames : 0; }
        }

        /// <summary>
        /// Total number of pants frames of the playback.
        /// </summary>
        public ulong NumPantsFrames
        {
            get { return IsLoaded ? _header.NumPantsFrames : 0; }
        }

        /// <summary>
        /// Speed multiplier of the playback. Cannot be negative.
        /// </summary>
        public float Speed
        {
            get { return _speed; }
            set
            {
                if (Mathf.Approximately(_speed, value))
                {
                    return;
                }
                _speed = Mathf.Max(0f, value);
                var handler = SpeedChanged;
                if (handler != null)
                {
                    handler.Invoke(_speed);
                }
            }
        }

        /// <summary>
        /// Should playback loop instead of finishing?
        /// </summary>
        public bool IsLooping
        {
            get { return _isLooping; }
            set
            {
                if (_isLooping == value)
                {
                    return;
                }
                _isLooping = value;
                var handler = IsLoopingChanged;
                if (handler != null)
                {
                    handler.Invoke(_isLooping);
                }
            }
        }

        /// <summary>
        /// Should playback start as soon as a file is loaded?
        /// </summary>
        public bool AutoplayOnLoad
        {
            get { return _autoplayOnLoad; }
            set
            {
                if (_autoplayOnLoad == value)
                {
                    return;
                }
                _autoplayOnLoad = value;
                var handler = AutoplayOnLoadChanged;
                if (handler != null)
                {
                    handler.Invoke(_autoplayOnLoad);
                }
            }
        }

        /// <summary>
        /// The absolute path + name of the currently loaded playback file.
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            private set { _filename = value; }
        }

        /// <summary>
        /// Is there no playback file loaded?
        /// </summary>
        public bool IsUnloaded
        {
            get { return State == PlaybackState.Unloaded; }
        }

        /// <summary>
        /// Is a playback file currently loading?
        /// </summary>
        public bool IsLoading
        {
            get { return State == PlaybackState.Loading; }
        }

        /// <summary>
        /// Is a playback file loaded and ready to play?
        /// </summary>
        public bool IsLoaded
        {
            get { return !IsUnloaded && !IsLoading && !IsError; }
        }

        /// <summary>
        /// Is the playback currently playing?
        /// </summary>
        public bool IsPlaying
        {
            get { return State == PlaybackState.Playing; }
        }

        /// <summary>
        /// Is the playback currently paused?
        /// </summary>
        public bool IsPaused
        {
            get { return State == PlaybackState.Paused; }
        }

        /// <summary>
        /// Is the playback stopped?
        /// </summary>
        public bool IsStopped
        {
            get { return State == PlaybackState.Stopped; }
        }

        /// <summary>
        /// Has the playback successfully finished?
        /// </summary>
        public bool IsFinished
        {
            get { return State == PlaybackState.Finished; }
        }

        /// <summary>
        /// Is the playback in an error state?
        /// </summary>
        public bool IsError
        {
            get { return State == PlaybackState.Error; }
        }


        private void OnApplicationQuit()
        {
            if (IsLoaded)
            {
                Unload();
            }
        }

        private static EnfluxSampleVector3 ToPlaybackFrame(byte[] rawFrame)
        {
            var angles = RPY.ParseDataForOrientationAngles(rawFrame);
            return new EnfluxSampleVector3
            {
                Center = new Vector3(angles.Center.Roll, angles.Center.Pitch, angles.Center.Yaw) * Mathf.Rad2Deg,
                LeftUpper = new Vector3(angles.LeftUpper.Roll, angles.LeftUpper.Pitch, angles.LeftUpper.Yaw) * Mathf.Rad2Deg,
                LeftLower = new Vector3(angles.LeftLower.Roll, angles.LeftLower.Pitch, angles.LeftLower.Yaw) * Mathf.Rad2Deg,
                RightUpper = new Vector3(angles.RightUpper.Roll, angles.RightUpper.Pitch, angles.RightUpper.Yaw) * Mathf.Rad2Deg,
                RightLower = new Vector3(angles.RightLower.Roll, angles.RightLower.Pitch, angles.RightLower.Yaw) * Mathf.Rad2Deg
            };
        }

        private IEnumerator Co_Playback()
        {
            SetFramesToTime(CurrentTimeMs);
            while (CurrentTimeMs <= DurationMs)
            {
                // Seek next shirt frame if necessary
                if (CurrentTimeMs > _nextShirtFrame.TimeMs)
                {
                    _currentShirtFrame = _nextShirtFrame;
                    var seekedShirtFrame = SeekNextFrame(_dataStream, _nextShirtFramePosition, CurrentTimeMs, EnfluxDevice.Shirt, ref _nextShirtFrame, ref _nextShirtFramePosition);
                    if (!seekedShirtFrame)
                    {
                        _nextShirtFrame = _currentShirtFrame;
                    }
                }

                // Seek next pants frame if necessary
                if (CurrentTimeMs > _nextPantsFrame.TimeMs)
                {
                    _currentPantsFrame = _nextPantsFrame;
                    var seekedPantsFrame = SeekNextFrame(_dataStream, _nextPantsFramePosition, CurrentTimeMs, EnfluxDevice.Pants, ref _nextPantsFrame, ref _nextPantsFramePosition);
                    if (!seekedPantsFrame)
                    {
                        _nextPantsFrame = _currentPantsFrame;
                    }
                }

                // Smooth animation by interpolating current frame with next frame
                var shirtT = Mathf.InverseLerp(
                    _currentShirtFrame.TimeMs * EnfluxMath.MillisecondsToSeconds,
                    _nextShirtFrame.TimeMs * EnfluxMath.MillisecondsToSeconds,
                    CurrentTime
                );
                var pantsT = Mathf.InverseLerp(
                    _currentPantsFrame.TimeMs * EnfluxMath.MillisecondsToSeconds,
                    _nextPantsFrame.TimeMs * EnfluxMath.MillisecondsToSeconds,
                    CurrentTime
                );
                var lerpedShirtFrame = EnfluxSampleVector3.Lerp(_currentShirtFrame, _nextShirtFrame, shirtT);
                var lerpedPantsFrame = EnfluxSampleVector3.Lerp(_currentPantsFrame, _nextPantsFrame, pantsT);

                // Apply interpolated frame orientations
                AbsoluteAngles.SetUpperBodyAngles(
                    lerpedShirtFrame.Center,
                    lerpedShirtFrame.LeftUpper,
                    lerpedShirtFrame.LeftLower,
                    lerpedShirtFrame.RightUpper,
                    lerpedShirtFrame.RightLower
                );

                AbsoluteAngles.SetLowerBodyAngles(
                    lerpedPantsFrame.Center,
                    lerpedPantsFrame.LeftUpper,
                    lerpedPantsFrame.LeftLower,
                    lerpedPantsFrame.RightUpper,
                    lerpedPantsFrame.RightLower
                );

                yield return null;
                // Don't assign current time to property to avoid refreshing current frame
                _currentTimeMs += (uint) (Time.deltaTime * EnfluxMath.SecondsToMilliseconds * Speed);
                if (CurrentTimeMs > DurationMs)
                {
                    var handler = LastFramePlayed;
                    if (handler != null)
                    {
                        handler.Invoke();
                    }
                    if (IsLooping)
                    {
                        _currentTimeMs = 0;
                        SetFramesToTime(_currentTimeMs);
                    }
                }
            }
            State = PlaybackState.Finished;
        }

        /// <summary>
        /// Attempts to seek the next frame defined by a starting position, starting time, and device type.
        /// </summary>
        /// <param name="stream">Stream to seek in.</param>
        /// <param name="startPosition">The starting position in <paramref name="stream"/></param>
        /// <param name="startTimeMs">The starting time the next frame must start at.</param>
        /// <param name="deviceType">The device type of the next frame.</param>
        /// <param name="nextFrame">A reference to store the next frame in.</param>
        /// <param name="endPosition">A reference to store the ending seeked position of the stream. If a next frame was found, this position is one byte after this frame.</param>
        /// <returns>bool indicating a matching next frame was found.</returns>
        private bool SeekNextFrame(Stream stream, long startPosition, uint startTimeMs, EnfluxDevice deviceType, ref EnfluxSampleVector3 nextFrame, ref long endPosition)
        {
            if (stream == null)
            {
                return false;
            }
            try
            {
                if (!stream.CanRead)
                {
                    var errorMessage = string.Format("Unable to read '{0}'. Do you have permissions to read the file?", Filename);
                    RaisePlaybackErrorEvent(PlaybackResult.PermissionError, errorMessage);
                }

                stream.Position = startPosition;
                while (stream.Position < stream.Length)
                {
                    var currentFramePosition = stream.Position;
                    // Verify timestamp bytes
                    if (stream.Read(_timestampBuffer, 0, NumBytesTimestamp) != NumBytesTimestamp)
                    {
                        var errorMessage = string.Format("Unable to read timestamp at position {0}. The file may be corrupt!", currentFramePosition);
                        RaisePlaybackErrorEvent(PlaybackResult.InvalidFrame, errorMessage);
                    }
                    // Verify timestamp value
                    var timestamp = BitConverter.ToUInt32(_timestampBuffer, 0);
                    if (timestamp > _header.Duration)
                    {
                        var errorMessage = string.Format("Position {0}: timestamp of {1} is greater than duration of {2}!", currentFramePosition, timestamp, _header.Duration);
                        RaisePlaybackErrorEvent(PlaybackResult.InvalidFrame, errorMessage);
                    }
                    // Verify device data bytes
                    var frameDeviceType = (EnfluxDevice)stream.ReadByte();
                    if (stream.Read(_dataBuffer, 0, NumBytesFrame) != NumBytesFrame)
                    {
                        var errorMessage = string.Format("Unable to read {0} angle data at position {1}. The file may be corrupt!", frameDeviceType, stream.Position);
                        RaisePlaybackErrorEvent(PlaybackResult.InvalidFrame, errorMessage);
                    }

                    // Return frame if it matches device type and at least the start time
                    if (deviceType == frameDeviceType && timestamp >= startTimeMs)
                    {
                        nextFrame = ToPlaybackFrame(_dataBuffer);
                        nextFrame.TimeMs = timestamp;
                        endPosition = stream.Position;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                var errorMessage = string.Format("File error seeking frame at position {0} for device {1} @ start time {2}ms: '{3}'", startPosition, deviceType, startTimeMs, e.Message);
                RaisePlaybackErrorEvent(PlaybackResult.UnknownError, errorMessage);
            }
            return false;
        }

        /// <summary>
        /// Force both shirt and pants orientations to the frames at the specified time.
        /// </summary>
        /// <param name="timeMs">Time in millseconds to set frames too.</param>
        private void SetFramesToTime(uint timeMs)
        {
            SeekNextFrame(_dataStream, _headerEndPosition, timeMs, EnfluxDevice.Shirt, ref _currentShirtFrame, ref _nextShirtFramePosition);
            _nextShirtFrame = _currentShirtFrame;

            AbsoluteAngles.SetUpperBodyAngles(
                _currentShirtFrame.Center,
                _currentShirtFrame.LeftUpper,
                _currentShirtFrame.LeftLower,
                _currentShirtFrame.RightUpper,
                _currentShirtFrame.RightLower
            );

            SeekNextFrame(_dataStream, _headerEndPosition, timeMs, EnfluxDevice.Pants, ref _currentPantsFrame, ref _nextPantsFramePosition);
            _nextPantsFrame = _currentPantsFrame;

            AbsoluteAngles.SetLowerBodyAngles(
                _currentPantsFrame.Center,
                _currentPantsFrame.LeftUpper,
                _currentPantsFrame.LeftLower,
                _currentPantsFrame.RightUpper,
                _currentPantsFrame.RightLower
            );
        }

        private void RaisePlaybackErrorEvent(PlaybackResult errorType, string message)
        {
            if (IsLoaded)
            {
                Unload();
            }
            State = PlaybackState.Error;
            var handler = PlaybackReceivedError;
            if (handler != null)
            {
                handler(new Notification<PlaybackResult>(errorType, message));
            }
            Debug.LogError(string.Format("{0}: {1}", errorType, message));
        }

        /// <summary>
        /// Loads a .enfl file at the specified absolute filepath. If a file is already loaded, then it is unloaded first.
        /// </summary>
        /// <param name="absoluteFilename">Path of the .enfl file to load.</param>
        public void Load(string absoluteFilename)
        {
            if (IsLoaded)
            {
                Unload();
            }

            State = PlaybackState.Loading;
            Filename = absoluteFilename;
            var playbackMessage = PlaybackUtils.IsValidEnflFile(absoluteFilename);
            if (playbackMessage.Value != PlaybackResult.Success)
            {
                RaisePlaybackErrorEvent(playbackMessage.Value, playbackMessage.Message);
                return;
            }

            _dataStream = File.OpenRead(Filename);
            var numBytesAnimationHeader = Marshal.SizeOf(typeof(AnimationHeader));
            var rawHeader = new byte[numBytesAnimationHeader];
            // Read file header
            if (_dataStream.CanRead &&
                _dataStream.Read(rawHeader, 0, numBytesAnimationHeader) == numBytesAnimationHeader)
            {
                _header = AnimationHeader.InitializeFromArray(rawHeader);
            }
            else
            {
                var errorMessage = string.Format("Unable to read the file header for '{0}'", Filename);
                RaisePlaybackErrorEvent(PlaybackResult.InvalidHeader, errorMessage);
                return;
            }

            ShirtBaseOrientation = _header.ShirtBaseOrientation.ToUnityVector3();
            PantsBaseOrientation = _header.PantsBaseOrientation.ToUnityVector3();
            _headerEndPosition = _dataStream.Position;

            State = AutoplayOnLoad ? PlaybackState.Playing : PlaybackState.Stopped;
            CurrentTimeMs = 0;
        }

        /// <summary>
        /// Unloads the currently loaded .enfl file.
        /// </summary>
        public void Unload()
        {
            if (!IsLoaded)
            {
                RaisePlaybackErrorEvent(PlaybackResult.NoFileLoaded, "No file loaded!");
                return;
            }
            if (_dataStream != null)
            {
                _dataStream.Dispose();
                _dataStream = null;
            }
            State = PlaybackState.Unloaded;
        }

        /// <summary>
        /// Plays the currently loaded .enfl file.
        /// </summary>
        public void Play()
        {
            if (!IsLoaded)
            {
                RaisePlaybackErrorEvent(PlaybackResult.NoFileLoaded, "No file loaded!");
                return;
            }
            State = PlaybackState.Playing;
        }

        /// <summary>
        /// Pauses the currently loaded .enfl file.
        /// </summary>
        public void Pause()
        {
            if (!IsLoaded)
            {
                RaisePlaybackErrorEvent(PlaybackResult.NoFileLoaded, "No file loaded!");
                return;
            }
            State = PlaybackState.Paused;
        }

        /// <summary>
        /// Stops the currently loaded .enfl file.
        /// </summary>
        public void Stop()
        {
            if (!IsLoaded)
            {
                RaisePlaybackErrorEvent(PlaybackResult.NoFileLoaded, "No file loaded!");
                return;
            }
            State = PlaybackState.Stopped;
        }
    }
}