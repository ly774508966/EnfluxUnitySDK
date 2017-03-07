// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using Enflux.Attributes;
using Enflux.SDK.Core.DataTypes;
using Enflux.SDK.HID;
using UnityEngine;

namespace Enflux.SDK.Core
{
    public class EnfluxManager : EnfluxSuitStream
    {
        [SerializeField, Readonly] private DeviceState _shirtState = DeviceState.Disconnected;
        [SerializeField, Readonly] private DeviceState _pantsState = DeviceState.Disconnected;

        private readonly EnfluxPullInterface _pullInterface = new EnfluxPullInterface();

        public bool IsShirtActive
        {
            get { return _shirtState != DeviceState.Disconnected; }
        }

        public bool ArePantsActive
        {
            get { return _pantsState != DeviceState.Disconnected; }
        }

        public bool IsAnyDeviceActive
        {
            get { return IsShirtActive || ArePantsActive; }
        }

        public DeviceState ShirtState
        {
            get { return _shirtState; }
            private set
            {
                if (_shirtState == value)
                {
                    return;
                }
                var previous = _shirtState;
                _shirtState = value;
                ChangeShirtState(new StateChange<DeviceState>(previous, _shirtState));
            }
        }

        public DeviceState PantsState
        {
            get { return _pantsState; }
            private set
            {
                if (_pantsState == value)
                {
                    return;
                }
                var previous = _pantsState;
                _pantsState = value;
                ChangePantsState(new StateChange<DeviceState>(previous, _pantsState));
            }
        }


        public interface IQueuedEvent
        {
            void OnDequeue();
        }

        public class DataEvent : IQueuedEvent
        {
            public SByteQuaternion[] Data;
            public bool IsPants;

            public DataEvent(SByteQuaternion[] data, bool isPants)
            {
                Data = data;
                IsPants = isPants;
            }

            void IQueuedEvent.OnDequeue()
            {
            }
        }

        public class DeviceStatusEvent : IQueuedEvent
        {
            public DeviceStatusEvent(EnfluxDevice device, int status)
            {
            }

            void IQueuedEvent.OnDequeue()
            {
            }
        }

        private void OnEnable()
        {
            _pullInterface.ReceivedShirtStatus += OnReceivedShirtStatus;
            _pullInterface.ReceivedPantsStatus += OnReceivedPantsStatus;
        }

        private void OnDisable()
        {
            _pullInterface.ReceivedShirtStatus -= OnReceivedShirtStatus;
            _pullInterface.ReceivedPantsStatus -= OnReceivedPantsStatus;
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        private void OnReceivedShirtStatus(InputCommands status)
        {
            switch (status)
            {
                case InputCommands.DeviceConnected:
                    ShirtState = DeviceState.Initializing;
                    break;

                case InputCommands.DeviceDisconnected:
                    ShirtState = DeviceState.Disconnected;
                    break;

                case InputCommands.CalibrationStarted:
                    ShirtState = DeviceState.Calibrating;
                    break;

                case InputCommands.CalibrationFinished:
                    Disconnect();
                    break;

                case InputCommands.ResetOrientation:
                    ResetFullBodyBaseOrientation();
                    break;

                case InputCommands.ErrorCalibrationFailed:
                    ShirtState = DeviceState.Connected;
                    RaiseShirtErrorEvent(DeviceError.CalibrationFailed);
                    Debug.LogError("Device 'Shirt' failed to calibrate!");
                    break;

                case InputCommands.ErrorNoCalibration:
                    ShirtState = DeviceState.Connected;
                    RaiseShirtErrorEvent(DeviceError.NoCalibration);
                    Debug.LogError("Device 'Shirt' isn't calibrated. Please calibrate.");
                    break;

                case InputCommands.ErrorNoShirtPants:
                    ShirtState = DeviceState.Connected;
                    RaiseShirtErrorEvent(DeviceError.UnknownDevice);
                    Debug.LogError("Attempted to connect unknown device. Defaulting to 'Shirt'. Please fix this with the firmware update tools.");
                    break;
            }
        }

        private void OnReceivedPantsStatus(InputCommands status)
        {
            switch (status)
            {
                case InputCommands.DeviceConnected:
                    PantsState = DeviceState.Initializing;
                    break;

                case InputCommands.DeviceDisconnected:
                    PantsState = DeviceState.Disconnected;
                    break;

                case InputCommands.CalibrationStarted:
                    PantsState = DeviceState.Calibrating;
                    break;

                case InputCommands.CalibrationFinished:
                    Disconnect();
                    break;

                case InputCommands.ResetOrientation:
                    ResetFullBodyBaseOrientation();
                    break;

                case InputCommands.ErrorCalibrationFailed:
                    PantsState = DeviceState.Connected;
                    RaisePantsErrorEvent(DeviceError.CalibrationFailed);
                    Debug.LogError("Device 'Pants' failed to calibrate!");
                    break;

                case InputCommands.ErrorNoCalibration:
                    PantsState = DeviceState.Connected;
                    RaisePantsErrorEvent(DeviceError.NoCalibration);
                    Debug.LogError("Device 'Pants' isn't calibrated. Please calibrate.");
                    break;

                case InputCommands.ErrorNoShirtPants:
                    PantsState = DeviceState.Connected;
                    RaisePantsErrorEvent(DeviceError.UnknownDevice);
                    Debug.LogError("Attempted to connect unknown device. Defaulting to 'Shirt'. Please fix this with the firmware update tools.");
                    break;
            }
        }

        private void Update()
        {
            _pullInterface.PollDevices();

            if (IsShirtActive)
            {
                var upperModuleAngles = RPY.ParseDataForOrientationAngles(EnfluxPullInterface.ShirtRPY);
                if (ShirtState == DeviceState.Initializing && upperModuleAngles.IsInitialized)
                {
                    ShirtState = DeviceState.Streaming;
                    upperModuleAngles.ApplyUpperAnglesTo(AbsoluteAngles);
                    ResetFullBodyBaseOrientation();
                }
                else if (ShirtState == DeviceState.Streaming)
                {
                    upperModuleAngles.ApplyUpperAnglesTo(AbsoluteAngles);
                }
            }
            if (ArePantsActive)
            {
                var lowerModuleAngles = RPY.ParseDataForOrientationAngles(EnfluxPullInterface.PantsRPY);
                if (PantsState == DeviceState.Initializing && lowerModuleAngles.IsInitialized)
                {
                    PantsState = DeviceState.Streaming;
                    lowerModuleAngles.ApplyLowerAnglesTo(AbsoluteAngles);
                    ResetFullBodyBaseOrientation();
                }
                else if (PantsState == DeviceState.Streaming)
                {
                    lowerModuleAngles.ApplyLowerAnglesTo(AbsoluteAngles);
                }
            }
        }

        public void Connect(EnfluxDevice device)
        {
            if (device == EnfluxDevice.None)
            {
                Debug.LogError("Device is 'None'!");
                return;
            }
            if (IsActive(device))
            {
                Debug.LogError("Device '" + device + "' is already connected!");
                return;
            }
            // If should connect to both but we're already connected to one device, still connect to the other.
            if (device == EnfluxDevice.All && IsShirtActive)
            {
                Debug.LogError("Device 'Shirt' is already connected!");
                device = EnfluxDevice.Pants;
            }
            else if (device == EnfluxDevice.All && ArePantsActive)
            {
                Debug.LogError("Device 'Pants' is already connected!");
                device = EnfluxDevice.Shirt;
            }
            else if (device == EnfluxDevice.Shirt && ArePantsActive)
            {
                Disconnect();
                device = EnfluxDevice.All;
            }
            else if (device == EnfluxDevice.Pants && IsShirtActive)
            {
                Disconnect();
                device = EnfluxDevice.All;
            }

            Debug.Log("Connecting '" + device + "'...");
            _pullInterface.StartStreaming(device);
        }

        public void Disconnect()
        {
            if (!IsShirtActive && !ArePantsActive)
            {
                Debug.LogError("No devices are connected!");
                return;
            }
            Debug.Log("Disconnecting all devices...");
            _pullInterface.EndStreaming();

            ShirtState = DeviceState.Disconnected;
            PantsState = DeviceState.Disconnected;
        }

        private void Shutdown()
        {
            _pullInterface.EndStreaming();

            ShirtState = DeviceState.Disconnected;
            PantsState = DeviceState.Disconnected;
        }

        public bool IsActive(EnfluxDevice device)
        {
            switch (device)
            {
                case EnfluxDevice.Shirt:
                    return IsShirtActive;

                case EnfluxDevice.Pants:
                    return ArePantsActive;

                case EnfluxDevice.All:
                    return IsShirtActive && ArePantsActive;
            }
            return false;
        }

        public void Calibrate(EnfluxDevice device)
        {
            if (device == EnfluxDevice.None)
            {
                Debug.LogError("Device is 'None'!");
                return;
            }
            if (IsActive(device))
            {
                Debug.LogError("Device '" + device + "' must be disconnected to calibrate!");
                return;
            }
            Debug.Log("Calibrating '" + device + "'...");
            _pullInterface.StartCalibration(device);
        }
    }
}