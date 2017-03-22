// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using Enflux.SDK.Core;
using UnityEngine;

namespace Enflux.SDK.Animation
{
    /// <summary>
    /// Defines core functionality required to animate a rig from a Humanoid via forward kinematics.
    /// </summary>
    public abstract class RigMapper : MonoBehaviour
    {
        [SerializeField] private Humanoid _humanoid;

        public virtual Transform Core { get; protected set; }
        public virtual Transform LeftUpperArm { get; protected set; }
        public virtual Transform LeftLowerArm { get; protected set; }
        public virtual Transform RightUpperArm { get; protected set; }
        public virtual Transform RightLowerArm { get; protected set; }
        public virtual Transform Waist { get; protected set; }
        public virtual Transform LeftUpperLeg { get; protected set; }
        public virtual Transform LeftLowerLeg { get; protected set; }
        public virtual Transform RightUpperLeg { get; protected set; }
        public virtual Transform RightLowerLeg { get; protected set; }

        public Humanoid Humanoid
        {
            get { return _humanoid; }
            set { _humanoid = value; }
        }


        protected virtual void Reset()
        {
            Humanoid = GetComponentInChildren<Humanoid>(true);
        }

        protected virtual void Awake()
        {
            Humanoid = Humanoid ?? FindObjectOfType<Humanoid>();
            if (Humanoid == null)
            {
                Debug.LogError("Humanoid is not assigned and there is no instance in the scene!");
            }
        }

        protected virtual void LateUpdate()
        {
            ApplyHumanoidToRig();
        }

        public virtual void ApplyHumanoidToRig()
        {
            if (Humanoid == null)
            {
                return;
            }

            // Apply upper body rotations to rig
            if (Core != null)
            {
                Core.rotation = Humanoid.LocalAngles.Chest;
            }
            if (LeftUpperArm != null)
            {
                LeftUpperArm.rotation = Humanoid.LocalAngles.LeftUpperArm;
            }
            if (LeftLowerArm != null)
            {
                LeftLowerArm.rotation = Humanoid.LocalAngles.LeftLowerArm;
            }
            if (RightUpperArm != null)
            {
                RightUpperArm.rotation = Humanoid.LocalAngles.RightUpperArm;
            }
            if (RightLowerArm != null)
            {
                RightLowerArm.rotation = Humanoid.LocalAngles.RightLowerArm;
            }

            // Apply lower body rotations to rig
            if (Waist != null)
            {
                Waist.rotation = Humanoid.LocalAngles.Waist;
            }
            if (LeftUpperLeg != null)
            {
                LeftUpperLeg.rotation = Humanoid.LocalAngles.LeftUpperLeg;
            }
            if (LeftLowerLeg != null)
            {
                LeftLowerLeg.rotation = Humanoid.LocalAngles.LeftLowerLeg;
            }
            if (RightUpperLeg != null)
            {
                RightUpperLeg.rotation = Humanoid.LocalAngles.RightUpperLeg;
            }
            if (RightLowerLeg != null)
            {
                RightLowerLeg.rotation = Humanoid.LocalAngles.RightLowerLeg;
            }
        }
    }
}