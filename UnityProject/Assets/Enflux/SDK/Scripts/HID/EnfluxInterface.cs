﻿// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;
using Enflux.SDK.Core.DataTypes;

namespace Enflux.SDK.HID
{
    public class EnfluxInterface
    {
        public enum StreamingStatus
        {
            Uninitialized,
            Connected,
            Disconnected
        }


        public StreamingStatus ShirtStatus = StreamingStatus.Uninitialized;
        public StreamingStatus PantsStatus = StreamingStatus.Uninitialized;

        public event Action<InputCommands> ReceivedShirtStatus;
        public event Action<InputCommands> ReceivedPantsStatus;

        public SByteQuaternion[] PantsRotations = new SByteQuaternion[5];
        public SByteQuaternion[] ShirtRotations = new SByteQuaternion[5];


        // Temporary variables, will be removed in future release!
        public static byte[] PantsRpy = new byte[20];
        public static byte[] ShirtRpy = new byte[20];


        public void StartStreaming(EnfluxDevice deviceType)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            EnfluxNativePull.StartStreamingPull(deviceType);
#endif
        }

        public void EndStreaming()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            EnfluxNativePull.EndStreamingThread();
#endif
        }

        public void StartCalibration(EnfluxDevice deviceType)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            EnfluxNativePull.StartCalibrationPull(deviceType);
#endif
        }

        public void PollDevices()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // Check for a new shirt command
            if (HasNewCommand(EnfluxDevice.Shirt))
            {
                var command = PopCommand(EnfluxDevice.Shirt);
                var handler = ReceivedShirtStatus;
                if (handler != null)
                {
                    handler(command);
                }
                if (command == InputCommands.DeviceConnected)
                {
                    ShirtStatus = StreamingStatus.Connected;
                }
                else if (command == InputCommands.DeviceDisconnected)
                {
                    ShirtStatus = StreamingStatus.Disconnected;
                }
            }
            // Check for a new pants command
            if (HasNewCommand(EnfluxDevice.Pants))
            {
                var command = PopCommand(EnfluxDevice.Pants);
                var handler = ReceivedPantsStatus;
                if (handler != null)
                {
                    handler(command);
                }
                if (command == InputCommands.DeviceConnected)
                {
                    PantsStatus = StreamingStatus.Connected;
                }
                else if (command == InputCommands.DeviceDisconnected)
                {
                    PantsStatus = StreamingStatus.Disconnected;
                }
            }

            if (ShirtStatus == StreamingStatus.Connected)
            {
                RPY.LoadRotations(EnfluxDevice.Shirt, ShirtRpy);
            }
            if (PantsStatus == StreamingStatus.Connected)
            {
                RPY.LoadRotations(EnfluxDevice.Pants, PantsRpy);
            }
#endif
        }

        private bool HasNewCommand(EnfluxDevice deviceType)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return Convert.ToBoolean(EnfluxNativePull.HasNewCommand(deviceType));
#else
            return false;
#endif
        }

        private InputCommands PopCommand(EnfluxDevice deviceType)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return (InputCommands) EnfluxNativePull.PopCommand(deviceType);
#else
            return InputCommands.DeviceDisconnected;
#endif
        }
    }
}