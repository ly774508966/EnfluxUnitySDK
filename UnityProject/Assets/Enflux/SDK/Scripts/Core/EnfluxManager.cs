// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using Enflux.SDK.Core.DataTypes;
using Enflux.SDK.HID;
using UnityEngine;

namespace Enflux.SDK.Core
{
    /// <summary>
    /// Handles the state of individual Enflux clothing and provides methods to connect, disconnect, and calibrate shirt and pants.
    /// </summary>
    public class EnfluxManager : EnfluxSuitStream
    {
        [SerializeField] private bool _connectOnStart;

        private readonly EnfluxInterface _interface = new EnfluxInterface();

        public bool IsShirtActive
        {
            get { return ShirtState != DeviceState.Disconnected; }
        }

        public bool ArePantsActive
        {
            get { return PantsState != DeviceState.Disconnected; }
        }

        public bool IsAnyDeviceActive
        {
            get { return IsShirtActive || ArePantsActive; }
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
            _interface.ReceivedShirtStatus += OnReceivedShirtStatus;
            _interface.ReceivedPantsStatus += OnReceivedPantsStatus;
        }

        private void OnDisable()
        {
            _interface.ReceivedShirtStatus -= OnReceivedShirtStatus;
            _interface.ReceivedPantsStatus -= OnReceivedPantsStatus;
        }

        private void Start()
        {
            if (_connectOnStart)
            {
                Connect(EnfluxDevice.All);
            }
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
            _interface.PollDevices();

            if (IsShirtActive)
            {
                var upperModuleAngles = RPY.ParseDataForOrientationAngles(EnfluxInterface.ShirtRpy);
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
                var lowerModuleAngles = RPY.ParseDataForOrientationAngles(EnfluxInterface.PantsRpy);
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
            _interface.StartStreaming(device);
        }

        public void Disconnect()
        {
            if (!IsShirtActive && !ArePantsActive)
            {
                Debug.LogError("No devices are connected!");
                return;
            }
            Debug.Log("Disconnecting all devices...");
            _interface.EndStreaming();

            ShirtState = DeviceState.Disconnected;
            PantsState = DeviceState.Disconnected;
        }

        private void Shutdown()
        {
            _interface.EndStreaming();

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
            _interface.StartCalibration(device);
        }
    }
}