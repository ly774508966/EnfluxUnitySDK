// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;
using Enflux.SDK.Attributes;
using UnityEngine;
using Enflux.SDK.Alignment;

namespace Enflux.SDK.Core
{
    /// <summary>
    /// Defines an Enflux suit (pair of Enflux shirt+pants). 
    /// Provides absolute sensor orientations (relative to real-world directions), the starting absolute orientation of the suit, and outputs events for device state, device notifications, and device errors.
    /// </summary>
    public abstract class EnfluxSuitStream : MonoBehaviour
    {
        [SerializeField, Readonly] private DeviceState _shirtState = DeviceState.Disconnected;
        [SerializeField, Readonly] private DeviceState _pantsState = DeviceState.Disconnected;

        #region Alignment variables

        private readonly ImuOrientations _imuOrientation = new ImuOrientations();
        private readonly SensorAlignment _sensorAlignment = new SensorAlignment();

        private bool _isAligningUpper;
        private bool _isAligningLower;
        private Vector4 _upperModuleQuatComponents;
        private Vector4 _lowerModuleQuatComponents;
        private Quaternion _chestInitialQuat;
        private Quaternion _waistInitialQuat;

        private Quaternion _chestImu;
        private Quaternion _leftUpperArmImu;
        private Quaternion _leftLowerArmImu;
        private Quaternion _rightUpperArmImu;
        private Quaternion _rightLowerArmImu;

        private Quaternion _waistImu;
        private Quaternion _leftUpperLegImu;
        private Quaternion _leftLowerLegImu;
        private Quaternion _rightUpperLegImu;
        private Quaternion _rightLowerLegImu;

        private Quaternion _firstUpperModuleQuat;
        private bool _hasFirstUpperModQuat;
        private int _upperModuleQuatCounter;
        private Quaternion _firstLowerModuleQuat;
        private bool _hasFirstLowerModQuat;
        private int _lowerModuleQuatCounter;

        #endregion

        private Vector3 _shirtBaseOrientation;
        private Vector3 _pantsBaseOrientation;
        private readonly HumanoidAngles<Vector3> _absoluteAngles = new HumanoidAngles<Vector3>();

        public event Action<StateChange<DeviceState>> ShirtStateChanged;
        public event Action<StateChange<DeviceState>> PantsStateChanged;
        public event Action<DeviceNotification> ShirtReceivedNotification;
        public event Action<DeviceNotification> PantsReceivedNotification;
        public event Action<DeviceError> ShirtReceivedError;
        public event Action<DeviceError> PantsReceivedError;


        #region Alignment properties

        public Quaternion ChestCorrection
        {
            get { return _sensorAlignment.ChestCorrection; }
        }

        public Quaternion LeftUpperArmCorrection
        {
            get { return _sensorAlignment.LeftUpperArmCorrection; }
        }

        public Quaternion LeftLowerArmCorrection
        {
            get { return _sensorAlignment.LeftLowerArmCorrection; }
        }

        public Quaternion RightUpperArmCorrection
        {
            get { return _sensorAlignment.RightUpperArmCorrection; }
        }

        public Quaternion RightLowerArmCorrection
        {
            get { return _sensorAlignment.RightLowerArmCorrection; }
        }

        public Quaternion WaistCorrection
        {
            get { return _sensorAlignment.WaistCorrection; }
        }

        public Quaternion LeftUpperLegCorrection
        {
            get { return _sensorAlignment.LeftUpperLegCorrection; }
        }

        public Quaternion LeftLowerLegCorrection
        {
            get { return _sensorAlignment.LeftLowerLegCorrection; }
        }

        public Quaternion RightUpperLegCorrection
        {
            get { return _sensorAlignment.RightUpperLegCorrection; }
        }

        public Quaternion RightLowerLegCorrection
        {
            get { return _sensorAlignment.RightLowerLegCorrection; }
        }

        #endregion

        public DeviceState ShirtState
        {
            get { return _shirtState; }
            protected set
            {
                if (_shirtState == value)
                {
                    return;
                }
                var previous = _shirtState;
                _shirtState = value;
                var handler = ShirtStateChanged;
                if (handler != null)
                {
                    handler.Invoke(new StateChange<DeviceState>(previous, _shirtState));
                }
            }
        }

        public DeviceState PantsState
        {
            get { return _pantsState; }
            protected set
            {
                if (_pantsState == value)
                {
                    return;
                }
                var previous = _pantsState;
                _pantsState = value;
                var handler = PantsStateChanged;
                if (handler != null)
                {
                    handler.Invoke(new StateChange<DeviceState>(previous, _pantsState));
                }
            }
        }

        /// <summary>
        /// The suit's limb angles in absolute real-world coordinates. For example, a rotation's y component of 0 corresponds to a yaw pointing north in real-world coordinates, 180 corresponds to south.
        /// </summary>
        public HumanoidAngles<Vector3> AbsoluteAngles
        {
            get { return _absoluteAngles; }
        }

        /// <summary>
        /// The base real-world orientation of the core module of the shirt.
        /// </summary>
        /// <param name="value">A y component of 0 corresponds to a yaw pointing north in real-world coordinates, 180 corresponds to south.</param>
        public Vector3 ShirtBaseOrientation
        {
            get { return _shirtBaseOrientation; }
            set
            {
                _shirtBaseOrientation = value;
                RaiseShirtNotificationEvent(DeviceNotification.ResetOrientation);
            }
        }

        /// <summary>
        /// The base real-world orientation of the waist module of the pants.
        /// </summary>
        /// <param name="value">A y component of 0 corresponds to a yaw pointing north in real-world coordinates, 180 corresponds to south.</param>
        public Vector3 PantsBaseOrientation
        {
            get { return _pantsBaseOrientation; }
            set
            {
                _pantsBaseOrientation = value;
                RaisePantsNotificationEvent(DeviceNotification.ResetOrientation);
            }
        }

        protected void RaiseShirtNotificationEvent(DeviceNotification shirtNotification)
        {
            var handler = ShirtReceivedNotification;
            if (handler != null)
            {
                handler(shirtNotification);
            }
        }

        protected void RaisePantsNotificationEvent(DeviceNotification pantsNotification)
        {
            var handler = PantsReceivedNotification;
            if (handler != null)
            {
                handler(pantsNotification);
            }
        }

        protected void RaiseShirtErrorEvent(DeviceError shirtError)
        {
            var handler = ShirtReceivedError;
            if (handler != null)
            {
                handler(shirtError);
            }
        }

        protected void RaisePantsErrorEvent(DeviceError pantsError)
        {
            var handler = PantsReceivedError;
            if (handler != null)
            {
                handler(pantsError);
            }
        }

        private void ShirtAlignmentBase()
        {
            if (!_isAligningUpper)
            {
                _hasFirstUpperModQuat = false;
                _upperModuleQuatComponents = new Vector4();
                _upperModuleQuatCounter = 0;
                _chestInitialQuat = new Quaternion();
                _firstUpperModuleQuat = new Quaternion();
                AbsoluteAngles.UpperBodyAnglesChanged += OnUpperSensorFrame;
            }
            else
            {
                AbsoluteAngles.UpperBodyAnglesChanged -= OnUpperSensorFrame;
                _sensorAlignment.UpperBodyAlignment(
                    _chestInitialQuat,
                    _chestImu,
                    _leftUpperArmImu,
                    _leftLowerArmImu,
                    _rightUpperArmImu,
                    _rightLowerArmImu);
            }
            _isAligningUpper = !_isAligningUpper;
        }

        private void PantsAlignmentBase()
        {
            if (!_isAligningLower)
            {
                _hasFirstLowerModQuat = false;
                _lowerModuleQuatComponents = new Vector4();
                _lowerModuleQuatCounter = 0;
                _waistInitialQuat = new Quaternion();
                _firstLowerModuleQuat = new Quaternion();
                AbsoluteAngles.LowerBodyAnglesChanged += OnLowerSensorFrame;
            }
            else
            {
                AbsoluteAngles.LowerBodyAnglesChanged -= OnLowerSensorFrame;
                _sensorAlignment.LowerBodyAlignment(
                    _waistInitialQuat,
                    _waistImu,
                    _leftUpperLegImu,
                    _leftLowerLegImu,
                    _rightUpperLegImu,
                    _rightLowerLegImu);
            }
            _isAligningLower = !_isAligningLower;
        }

        private void OnUpperSensorFrame(HumanoidAngles<Vector3> absoluteAngle)
        {
            _chestImu = _imuOrientation.BaseOrientation(absoluteAngle.Chest);
            _leftUpperArmImu = _imuOrientation.LeftOrientation(absoluteAngle.LeftUpperArm);
            _leftLowerArmImu = _imuOrientation.LeftOrientation(absoluteAngle.LeftLowerArm);
            _rightUpperArmImu = _imuOrientation.RightOrientation(absoluteAngle.RightUpperArm);
            _rightLowerArmImu = _imuOrientation.RightOrientation(absoluteAngle.RightLowerArm);
            if (!_hasFirstUpperModQuat)
            {
                _firstUpperModuleQuat = _chestImu;
                _hasFirstUpperModQuat = !_hasFirstUpperModQuat;
                ++_upperModuleQuatCounter;
            }
            else
            {
                _chestInitialQuat = Math3d.AverageQuaternion(
                    ref _upperModuleQuatComponents,
                    _chestImu,
                    _firstUpperModuleQuat,
                    _upperModuleQuatCounter);

                ++_upperModuleQuatCounter;
            }
        }

        private void OnLowerSensorFrame(HumanoidAngles<Vector3> absoluteAngle)
        {
            _waistImu = _imuOrientation.BaseOrientation(absoluteAngle.Waist);
            _leftUpperLegImu = _imuOrientation.LeftOrientation(absoluteAngle.LeftUpperLeg);
            _leftLowerLegImu = _imuOrientation.LeftOrientation(absoluteAngle.LeftLowerLeg);
            _rightUpperLegImu = _imuOrientation.RightOrientation(absoluteAngle.RightUpperLeg);
            _rightLowerLegImu = _imuOrientation.RightOrientation(absoluteAngle.RightLowerLeg);
            if (!_hasFirstLowerModQuat)
            {
                _firstLowerModuleQuat = _waistImu;
                _hasFirstLowerModQuat = !_hasFirstLowerModQuat;
                ++_lowerModuleQuatCounter;
            }
            else
            {
                _waistInitialQuat = Math3d.AverageQuaternion(
                    ref _lowerModuleQuatComponents,
                    _waistImu,
                    _firstLowerModuleQuat,
                    _lowerModuleQuatCounter);
                ++_lowerModuleQuatCounter;
            }
        }

        public void AlignUpperBodySensors()
        {
            ShirtAlignmentBase();
        }

        public void AlignLowerBodySensors()
        {
            PantsAlignmentBase();
        }

        public void AlignFullBodySensors()
        {
            AlignUpperBodySensors();
            AlignLowerBodySensors();
        }

        /// <summary>
        /// Sets ShirtBaseOrientation to the current orientation of the shirt's chest module.
        /// </summary>
        public void ResetShirtBaseOrientation()
        {
            ShirtBaseOrientation = AbsoluteAngles.Chest;
        }

        /// <summary>
        /// Sets PantsBaseOrientation to the current orientation of the pant's waist module.
        /// </summary>
        public void ResetPantsBaseOrientation()
        {
            PantsBaseOrientation = AbsoluteAngles.Waist;
        }

        /// <summary>
        /// Sets ShirtBaseOrientation to the current orientation of the shirt's chest module and PantsBaseOrientation to the current orientation of the pant's waist module.
        /// </summary>
        public void ResetFullBodyBaseOrientation()
        {
            ResetShirtBaseOrientation();
            ResetPantsBaseOrientation();
        }
    }
}