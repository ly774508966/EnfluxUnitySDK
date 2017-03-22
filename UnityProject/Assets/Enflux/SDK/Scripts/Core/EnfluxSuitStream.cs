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

        private Vector3 _shirtBaseOrientation;
        private Vector3 _pantsBaseOrientation;
        private readonly HumanoidAngles<Vector3> _absoluteAngles = new HumanoidAngles<Vector3>();

        public event Action<StateChange<DeviceState>> ShirtStateChanged;
        public event Action<StateChange<DeviceState>> PantsStateChanged;
        public event Action<DeviceNotification> ShirtReceivedNotification;
        public event Action<DeviceNotification> PantsReceivedNotification;
        public event Action<DeviceError> ShirtReceivedError;
        public event Action<DeviceError> PantsReceivedError;


        #region alignmentvars
        private readonly ImuOrientations _imuOrientation = new ImuOrientations();
        private SensorAlignment _sensorAlignment = new SensorAlignment();

        private bool _aligningUpper = false;
        private bool _aligningLower = false;
        private Vector4 upperModuleQuatComponents;
        private Vector4 lowerModuleQuatComponents;
        private Quaternion chestInitialQuat;
        private Quaternion waistInitialQuat;

        private Quaternion chest_imu;
        private Quaternion leftUpperArm_imu;
        private Quaternion leftLowerArm_imu;
        private Quaternion rightUpperArm_imu;
        private Quaternion rightLowerArm_imu;

        private Quaternion waist_imu;
        private Quaternion leftUpperLeg_imu;
        private Quaternion leftLowerLeg_imu;
        private Quaternion rightUpperLeg_imu;
        private Quaternion rightLowerLeg_imu;

        private Quaternion firstUpperModuleQuaternion;
        private bool hasFirstUpperModQuaternion = false;
        private int upperModuleQuatCounter = 0;
        private Quaternion firstLowerModuleQuaternion;
        private bool hasFirstLowerModQuaternion = false;
        private int lowerModuleQuatCounter = 0;

        public Quaternion chestCorrection
        {
            get { return _sensorAlignment.chestCorrection; }
        }
        public Quaternion leftUpperArmCorrection
        {
            get { return _sensorAlignment.leftUpperArmCorrection; }
        }
        public Quaternion leftLowerArmCorrection
        {
            get { return _sensorAlignment.leftLowerArmCorrection; }
        }
        public Quaternion rightUpperArmCorrection
        {
            get { return _sensorAlignment.rightUpperArmCorrection; }
        }
        public Quaternion rightLowerArmCorrection
        {
            get { return _sensorAlignment.rightLowerArmCorrection; }
        }
        public Quaternion waistCorrection
        {
            get { return _sensorAlignment.waistCorrection; }
        }
        public Quaternion leftUpperLegCorrection
        {
            get { return _sensorAlignment.leftUpperLegCorrection; }
        }
        public Quaternion leftLowerLegCorrection
        {
            get { return _sensorAlignment.leftLowerLegCorrection; }
        }
        public Quaternion rightUpperLegCorrection
        {
            get { return _sensorAlignment.rightUpperLegCorrection; }
        }

        public Quaternion rightLowerLegCorrection
        {
            get { return _sensorAlignment.rightLowerLegCorrection; }
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
            if (!_aligningUpper)
            {
                hasFirstUpperModQuaternion = false;
                upperModuleQuatComponents = new Vector4();
                upperModuleQuatCounter = 0;
                chestInitialQuat = new Quaternion();
                firstUpperModuleQuaternion = new Quaternion();
                AbsoluteAngles.UpperBodyAnglesChanged += OnUpperSensorFrame;
            }
            else
            {
                AbsoluteAngles.UpperBodyAnglesChanged -= OnUpperSensorFrame;
                _sensorAlignment.UpperBodyAlignment(
                    chestInitialQuat,
                    chest_imu,
                    leftUpperArm_imu,
                    leftLowerArm_imu,
                    rightUpperArm_imu,
                    rightLowerArm_imu);
            }
            _aligningUpper = !_aligningUpper;
        }

        private void PantsAlignmentBase()
        {
            if (!_aligningLower)
            {
                hasFirstLowerModQuaternion = false;
                lowerModuleQuatComponents = new Vector4();
                lowerModuleQuatCounter = 0;
                waistInitialQuat = new Quaternion();
                firstLowerModuleQuaternion = new Quaternion();
                AbsoluteAngles.LowerBodyAnglesChanged += OnLowerSensorFrame;
            }
            else
            {
                AbsoluteAngles.LowerBodyAnglesChanged -= OnLowerSensorFrame;
                _sensorAlignment.LowerBodyAlignment(
                    waistInitialQuat,
                    waist_imu,
                    leftUpperLeg_imu,
                    leftLowerLeg_imu,
                    rightUpperLeg_imu,
                    rightLowerLeg_imu);
            }
            _aligningLower = !_aligningLower;
        }

        private void OnUpperSensorFrame(HumanoidAngles<Vector3> absoluteAngle)
        {
            chest_imu = _imuOrientation.BaseOrientation(absoluteAngle.Chest);
            leftUpperArm_imu = _imuOrientation.LeftOrientation(absoluteAngle.LeftUpperArm);
            leftLowerArm_imu = _imuOrientation.LeftOrientation(absoluteAngle.LeftLowerArm);
            rightUpperArm_imu = _imuOrientation.RightOrientation(absoluteAngle.RightUpperArm);
            rightLowerArm_imu = _imuOrientation.RightOrientation(absoluteAngle.RightLowerArm);
            if (!hasFirstUpperModQuaternion)
            {
                firstUpperModuleQuaternion = chest_imu;
                hasFirstUpperModQuaternion = !hasFirstUpperModQuaternion;
                upperModuleQuatCounter++;
            }
            else
            {
                chestInitialQuat = Math3d.AverageQuaternion(
                    ref upperModuleQuatComponents,
                    chest_imu,
                    firstUpperModuleQuaternion,
                    upperModuleQuatCounter);

                upperModuleQuatCounter++;
            }
        }

        private void OnLowerSensorFrame(HumanoidAngles<Vector3> absoluteAngle)
        {
            waist_imu = _imuOrientation.BaseOrientation(absoluteAngle.Waist);
            leftUpperLeg_imu = _imuOrientation.LeftOrientation(absoluteAngle.LeftUpperLeg);
            leftLowerLeg_imu = _imuOrientation.LeftOrientation(absoluteAngle.LeftLowerLeg);
            rightUpperLeg_imu = _imuOrientation.RightOrientation(absoluteAngle.RightUpperLeg);
            rightLowerLeg_imu = _imuOrientation.RightOrientation(absoluteAngle.RightLowerLeg);
            if (!hasFirstLowerModQuaternion)
            {
                firstLowerModuleQuaternion = waist_imu;
                hasFirstLowerModQuaternion = !hasFirstLowerModQuaternion;
                lowerModuleQuatCounter++;
            }
            else
            {
                waistInitialQuat = Math3d.AverageQuaternion(
                    ref lowerModuleQuatComponents,
                    waist_imu,
                    firstLowerModuleQuaternion,
                    lowerModuleQuatCounter);
                lowerModuleQuatCounter++;
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