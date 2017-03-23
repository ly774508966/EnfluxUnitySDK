// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using UnityEngine;
using Enflux.SDK.Alignment;

namespace Enflux.SDK.Core
{
    /// <summary>
    /// Transforms absolute sensor orientations into local orientations (each sensor orientation relative to its parent limb sensor)
    /// </summary>
    public class Humanoid : MonoBehaviour
    {
        [SerializeField, HideInInspector] private EnfluxSuitStream _absoluteAnglesStream;       
        private bool _isSubscribed;
        private readonly ImuOrientations _imuOrientation = new ImuOrientations();

        // Caching these guys rather than re-allocating every frame
        private Vector3 _yawAdjustedChestAngles;
        private Vector3 _yawAdjustedLeftUpperArmAngles;
        private Vector3 _yawAdjustedLeftLowerArmAngles;
        private Vector3 _yawAdjustedRightUpperArmAngles;
        private Vector3 _yawAdjustedRightLowerArmAngles;

        private Vector3 _yawAdjustedWaistAngles;
        private Vector3 _yawAdjustedLeftUpperLegAngles;
        private Vector3 _yawAdjustedLeftLowerLegAngles;
        private Vector3 _yawAdjustedRightUpperLegAngles;
        private Vector3 _yawAdjustedRightLowerLegAngles;

        /// <summary>
        /// The angles of each limb in the humanoid relative to its parent limb.
        /// </summary>
        public readonly HumanoidAngles<Quaternion> LocalAngles = new HumanoidAngles<Quaternion>();


        /// <summary>
        /// The source of the absolute angles used to calculate local angles for each limb.
        /// </summary>
        public EnfluxSuitStream AbsoluteAnglesStream
        {
            get { return _absoluteAnglesStream; }
            // HumanoidEditor calls this to correctly handle switching event subscription for the inspector
            set
            {
                if (_absoluteAnglesStream == value)
                {
                    return;
                }
                if (_absoluteAnglesStream != null)
                {
                    UnsubscribeFromEvents();
                }
                _absoluteAnglesStream = value;
                if (_absoluteAnglesStream != null)
                {
                    SubscribeToEvents();
                }
            }
        }

        private Vector3 ChestBaseOrientation
        {
            get { return AbsoluteAnglesStream != null ? AbsoluteAnglesStream.ShirtBaseOrientation : Vector3.zero; }
        }

        private Vector3 WaistBaseOrientation
        {
            get { return AbsoluteAnglesStream != null ? AbsoluteAnglesStream.PantsBaseOrientation : Vector3.zero; }
        }


        protected virtual void Reset()
        {
            AbsoluteAnglesStream = AbsoluteAnglesStream ?? FindObjectOfType<EnfluxManager>();
        }

        private void Awake()
        {
            if (AbsoluteAnglesStream != null)
            {
                SubscribeToEvents();
            }
        }

        private void OnDestroy()
        {
            if (_isSubscribed)
            {
                UnsubscribeFromEvents();
            }
        }

        private void OnUpperBodyAnglesChanged(HumanoidAngles<Vector3> absoluteAngles)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            // Pack absolute angles into arrays for calculations
            var baseChestYaw = ChestBaseOrientation.z;

            _yawAdjustedChestAngles = absoluteAngles.Chest;
            _yawAdjustedChestAngles.z = absoluteAngles.Chest.z - baseChestYaw;

            _yawAdjustedLeftUpperArmAngles = absoluteAngles.LeftUpperArm;
            _yawAdjustedLeftUpperArmAngles.z = absoluteAngles.LeftUpperArm.z - baseChestYaw;

            _yawAdjustedLeftLowerArmAngles = absoluteAngles.LeftLowerArm;
            _yawAdjustedLeftLowerArmAngles.z = absoluteAngles.LeftLowerArm.z - baseChestYaw;

            _yawAdjustedRightUpperArmAngles = absoluteAngles.RightUpperArm;
            _yawAdjustedRightUpperArmAngles.z = absoluteAngles.RightUpperArm.z - baseChestYaw;

            _yawAdjustedRightLowerArmAngles = absoluteAngles.RightLowerArm;
            _yawAdjustedRightLowerArmAngles.z = absoluteAngles.RightLowerArm.z - baseChestYaw;

            // Transform absolute upper body angles into local ones
            var localAngleChest = Quaternion.identity *
                _imuOrientation.BaseOrientation(_yawAdjustedChestAngles) *
                AbsoluteAnglesStream.ChestCorrection;

            var localAngleLeftUpperArm = Quaternion.Inverse(localAngleChest) *
                _imuOrientation.LeftOrientation(_yawAdjustedLeftUpperArmAngles) *
                AbsoluteAnglesStream.LeftUpperArmCorrection;

            var localAngleLeftLowerArm = Quaternion.Inverse(localAngleLeftUpperArm) *
                Quaternion.Inverse(localAngleChest) *
                _imuOrientation.LeftOrientation(_yawAdjustedLeftLowerArmAngles) *
                AbsoluteAnglesStream.LeftLowerArmCorrection;

            var localAngleRightUpperArm = Quaternion.Inverse(localAngleChest) *
                _imuOrientation.RightOrientation(_yawAdjustedRightUpperArmAngles) *
                AbsoluteAnglesStream.RightUpperArmCorrection;

            var localAngleRightLowerArm = Quaternion.Inverse(localAngleRightUpperArm) *
                Quaternion.Inverse(localAngleChest) *
                _imuOrientation.RightOrientation(_yawAdjustedRightLowerArmAngles) *
                AbsoluteAnglesStream.RightLowerArmCorrection;

            LocalAngles.SetUpperBodyAngles(
                localAngleChest,
                localAngleLeftUpperArm,
                localAngleLeftLowerArm,
                localAngleRightUpperArm,
                localAngleRightLowerArm);

        }

        private void OnLowerBodyAnglesChanged(HumanoidAngles<Vector3> absoluteAngles)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            var baseWaistYaw = WaistBaseOrientation.z;

            _yawAdjustedWaistAngles = absoluteAngles.Waist;
            _yawAdjustedWaistAngles.z = absoluteAngles.Waist.z - baseWaistYaw;

            _yawAdjustedLeftUpperLegAngles = absoluteAngles.LeftUpperLeg;
            _yawAdjustedLeftUpperLegAngles.z = absoluteAngles.LeftUpperLeg.z - baseWaistYaw;

            _yawAdjustedLeftLowerLegAngles = absoluteAngles.LeftLowerLeg;
            _yawAdjustedLeftLowerLegAngles.z = absoluteAngles.LeftLowerLeg.z - baseWaistYaw;

            _yawAdjustedRightUpperLegAngles = absoluteAngles.RightUpperLeg;
            _yawAdjustedRightUpperLegAngles.z = absoluteAngles.RightUpperLeg.z - baseWaistYaw;

            _yawAdjustedRightLowerLegAngles = absoluteAngles.RightLowerLeg;
            _yawAdjustedRightLowerLegAngles.z = absoluteAngles.RightLowerLeg.z - baseWaistYaw;

            // Transform absolute lower body angles into relative ones
            var localAngleWaist = Quaternion.identity *
                _imuOrientation.BaseOrientation(_yawAdjustedWaistAngles) *
                AbsoluteAnglesStream.WaistCorrection;

            var localAngleLeftUpperLeg = Quaternion.Inverse(localAngleWaist) *
                _imuOrientation.LeftOrientation(_yawAdjustedLeftUpperLegAngles) *
                AbsoluteAnglesStream.LeftUpperLegCorrection;

            var localAngleLeftLowerLeg = Quaternion.Inverse(localAngleLeftUpperLeg) *
                Quaternion.Inverse(localAngleWaist) *
                _imuOrientation.LeftOrientation(_yawAdjustedLeftLowerLegAngles) *
                AbsoluteAnglesStream.LeftLowerLegCorrection;

            var localAngleRightUpperLeg = Quaternion.Inverse(localAngleWaist) *
                _imuOrientation.RightOrientation(_yawAdjustedRightUpperLegAngles) *
                AbsoluteAnglesStream.RightUpperLegCorrection;

            var localAngleRightLowerLeg = Quaternion.Inverse(localAngleRightUpperLeg) *
                Quaternion.Inverse(localAngleWaist) *
                _imuOrientation.RightOrientation(_yawAdjustedRightLowerLegAngles) *
                AbsoluteAnglesStream.RightLowerLegCorrection;

            LocalAngles.SetLowerBodyAngles(
                localAngleWaist,
                localAngleLeftUpperLeg,
                localAngleLeftLowerLeg,
                localAngleRightUpperLeg,
                localAngleRightLowerLeg);
        }

        private void SubscribeToEvents()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            _isSubscribed = true;
            AbsoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged += OnUpperBodyAnglesChanged;
            AbsoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged += OnLowerBodyAnglesChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            _isSubscribed = false;
            AbsoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged -= OnUpperBodyAnglesChanged;
            AbsoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged -= OnLowerBodyAnglesChanged;
        }
    }
}