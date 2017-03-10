// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using Enflux.SDK.Core;
using Enflux.SDK.Utils;
using UnityEngine;

namespace Enflux.SDK.Animation
{
    public class TransformRigMapper : RigMapper
    {
        [SerializeField] private Transform _chest;
        [SerializeField] private Transform _leftUpperArm;
        [SerializeField] private Transform _leftLowerArm;
        [SerializeField] private Transform _rightUpperArm;
        [SerializeField] private Transform _rightLowerArm;
        [SerializeField] private Transform _waist;
        [SerializeField] private Transform _leftUpperLeg;
        [SerializeField] private Transform _leftLowerLeg;
        [SerializeField] private Transform _rightUpperLeg;
        [SerializeField] private Transform _rightLowerLeg;


        public override Transform Core
        {
            get { return _chest; }
            protected set { _chest = value; }
        }

        public override Transform LeftUpperArm
        {
            get { return _leftUpperArm; }
            protected set { _leftUpperArm = value; }
        }

        public override Transform LeftLowerArm
        {
            get { return _leftLowerArm; }
            protected set { _leftLowerArm = value; }
        }

        public override Transform RightUpperArm
        {
            get { return _rightUpperArm; }
            protected set { _rightUpperArm = value; }
        }

        public override Transform RightLowerArm
        {
            get { return _rightLowerArm; }
            protected set { _rightLowerArm = value; }
        }

        public override Transform Waist
        {
            get { return _waist; }
            protected set { _waist = value; }
        }

        public override Transform LeftUpperLeg
        {
            get { return _leftUpperLeg; }
            protected set { _leftUpperLeg = value; }
        }

        public override Transform LeftLowerLeg
        {
            get { return _leftLowerLeg; }
            protected set { _leftLowerLeg = value; }
        }

        public override Transform RightUpperLeg
        {
            get { return _rightUpperLeg; }
            protected set { _rightUpperLeg = value; }
        }

        public override Transform RightLowerLeg
        {
            get { return _rightLowerLeg; }
            protected set { _rightLowerLeg = value; }
        }



        protected override void Reset()
        {
            base.Reset();
            Humanoid = FindObjectOfType<Humanoid>();
            Core = RigMapperUtils.ResolveCore(transform);
            LeftUpperArm = RigMapperUtils.ResolveLeftUpperArm(transform);
            LeftLowerArm = RigMapperUtils.ResolveLeftLowerArm(transform);
            RightUpperArm = RigMapperUtils.ResolveRightUpperArm(transform);
            RightLowerArm = RigMapperUtils.ResolveRightLowerArm(transform);
            Waist = RigMapperUtils.ResolveWaist(transform);
            LeftUpperLeg = RigMapperUtils.ResolveLeftUpperLeg(transform);
            LeftLowerLeg = RigMapperUtils.ResolveLeftLowerLeg(transform);
            RightUpperLeg = RigMapperUtils.ResolveRightUpperLeg(transform);
            RightLowerLeg = RigMapperUtils.ResolveRightLowerLeg(transform);
        }

        public void SetCore(Transform chest)
        {
            Core = chest;
        }

        public void SetLeftUpperArm(Transform leftUpperArm)
        {
            LeftUpperArm = leftUpperArm;
        }

        public void SetLeftLowerArm(Transform leftLowerArm)
        {
            LeftLowerArm = leftLowerArm;
        }

        public void SetRightUpperArm(Transform rightUpperArm)
        {
            RightUpperArm = rightUpperArm;
        }

        public void SetRightLowerArm(Transform rightLowerArm)
        {
            RightLowerArm = rightLowerArm;
        }

        public void SetWaist(Transform waist)
        {
            Waist = waist;
        }

        public void SetLeftUpperLeg(Transform leftUpperLeg)
        {
            LeftUpperLeg = leftUpperLeg;
        }

        public void SetLeftLowerLeg(Transform leftLowerLeg)
        {
            LeftLowerLeg = leftLowerLeg;
        }

        public void SetRightUpperLeg(Transform rightUpperLeg)
        {
            RightUpperLeg = rightUpperLeg;
        }

        public void SetRightLowerLeg(Transform rightLowerLeg)
        {
            RightLowerLeg = rightLowerLeg;
        }
    }
}