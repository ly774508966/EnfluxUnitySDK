// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;

namespace Enflux.SDK.Core
{
    [Serializable]
    public class HumanoidAngles<T>
    {
        private T _chest;
        private T _leftUpperArm;
        private T _leftLowerArm;
        private T _rightUpperArm;
        private T _rightLowerArm;
        private T _waist;
        private T _leftUpperLeg;
        private T _leftLowerLeg;
        private T _rightUpperLeg;
        private T _rightLowerLeg;


        public event Action<HumanoidAngles<T>> UpperBodyAnglesChanged;
        public event Action<HumanoidAngles<T>> LowerBodyAnglesChanged;


        /// <summary>
        /// Chest orientation in degrees.
        /// </summary>
        public T Chest
        {
            get { return _chest; }
            set
            {
                if (_chest.Equals(value))
                {
                    return;
                }
                _chest = value;
                RaiseUpperBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Left bicep orientation in degrees.
        /// </summary>
        public T LeftUpperArm
        {
            get { return _leftUpperArm; }
            set
            {
                if (_leftUpperArm.Equals(value))
                {
                    return;
                }
                _leftUpperArm = value;
                RaiseUpperBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Left forearm orientation in degrees.
        /// </summary>
        public T LeftLowerArm
        {
            get { return _leftLowerArm; }
            set
            {
                if (_leftLowerArm.Equals(value))
                {
                    return;
                }
                _leftLowerArm = value;
                RaiseUpperBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Right bicep orientation in degrees.
        /// </summary>
        public T RightUpperArm
        {
            get { return _rightUpperArm; }
            set
            {
                if (_rightUpperArm.Equals(value))
                {
                    return;
                }
                _rightUpperArm = value;
                RaiseUpperBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Right forearm orientation in degrees.
        /// </summary>
        public T RightLowerArm
        {
            get { return _rightLowerArm; }
            set
            {
                if (_rightLowerArm.Equals(value))
                {
                    return;
                }
                _rightLowerArm = value;
                RaiseUpperBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Waist orientation in degrees.
        /// </summary>
        public T Waist
        {
            get { return _waist; }
            set
            {
                if (_waist.Equals(value))
                {
                    return;
                }
                _waist = value;
                RaiseLowerBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Left thigh orientation in degrees.
        /// </summary>
        public T LeftUpperLeg
        {
            get { return _leftUpperLeg; }
            set
            {
                if (_leftUpperLeg.Equals(value))
                {
                    return;
                }
                _leftUpperArm = value;
                RaiseLowerBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Left shin orientation in degrees.
        /// </summary>
        public T LeftLowerLeg
        {
            get { return _leftLowerLeg; }
            set
            {
                if (_leftLowerLeg.Equals(value))
                {
                    return;
                }
                _leftLowerLeg = value;
                RaiseLowerBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Right thigh orientation in degrees.
        /// </summary>
        public T RightUpperLeg
        {
            get { return _rightUpperLeg; }
            set
            {
                if (_rightUpperLeg.Equals(value))
                {
                    return;
                }
                _rightUpperLeg = value;
                RaiseLowerBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Right shin orientation in degrees.
        /// </summary>
        public T RightLowerLeg
        {
            get { return _rightLowerLeg; }
            set
            {
                if (_rightLowerLeg.Equals(value))
                {
                    return;
                }
                _rightLowerLeg = value;
                RaiseLowerBodyAnglesChangedEvent();
            }
        }

        /// <summary>
        /// Applies angles to the upper body all at once.
        /// </summary>
        /// <param name="upperBodyAngles"></param>
        public void SetUpperBodyAngles(HumanoidAngles<T> upperBodyAngles)
        {
            if (_waist.Equals(upperBodyAngles.Waist) &&
                _leftUpperArm.Equals(upperBodyAngles.LeftUpperArm) &&
                _leftLowerArm.Equals(upperBodyAngles.LeftLowerArm) &&
                _rightUpperArm.Equals(upperBodyAngles.RightUpperArm) &&
                _rightLowerArm.Equals(upperBodyAngles.RightLowerArm))
            {
                return;
            }
            _chest = upperBodyAngles._chest;
            _leftUpperArm = upperBodyAngles._leftUpperArm;
            _leftLowerArm = upperBodyAngles._leftLowerArm;
            _rightUpperArm = upperBodyAngles._rightUpperArm;
            _rightLowerArm = upperBodyAngles._rightLowerArm;

            RaiseUpperBodyAnglesChangedEvent();
        }

        /// <summary>
        /// Applies angles to the upper body all at once.
        /// </summary>
        /// <param name="chest"></param>
        /// <param name="leftUpperArm"></param>
        /// <param name="leftLowerArm"></param>
        /// <param name="rightUpperArm"></param>
        /// <param name="rightLowerArm"></param>
        public void SetUpperBodyAngles(
            T chest,
            T leftUpperArm,
            T leftLowerArm,
            T rightUpperArm,
            T rightLowerArm)
        {
            if (_chest.Equals(chest) &&
                _leftUpperArm.Equals(leftUpperArm) &&
                _leftLowerArm.Equals(leftLowerArm) &&
                _rightUpperArm.Equals(rightUpperArm) &&
                _rightLowerArm.Equals(rightLowerArm))
            {
                return;
            }
            _chest = chest;
            _leftUpperArm = leftUpperArm;
            _leftLowerArm = leftLowerArm;
            _rightUpperArm = rightUpperArm;
            _rightLowerArm = rightLowerArm;

            RaiseUpperBodyAnglesChangedEvent();
        }

        /// <summary>
        /// Applies angles to the lower body all at once.
        /// </summary>
        /// <param name="lowerBodyAngles"></param>
        public void SetLowerBodyAngles(HumanoidAngles<T> lowerBodyAngles)
        {
            if (_waist.Equals(lowerBodyAngles.Waist) &&
                _leftUpperLeg.Equals(lowerBodyAngles.LeftUpperLeg) &&
                _leftLowerLeg.Equals(lowerBodyAngles.LeftLowerLeg) &&
                _rightUpperLeg.Equals(lowerBodyAngles.RightUpperLeg) &&
                _rightLowerLeg.Equals(lowerBodyAngles.RightLowerLeg))
            {
                return;
            }
            _waist = lowerBodyAngles._waist;
            _leftUpperLeg = lowerBodyAngles._leftUpperLeg;
            _leftLowerLeg = lowerBodyAngles._leftLowerLeg;
            _rightUpperLeg = lowerBodyAngles._rightUpperLeg;
            _rightLowerLeg = lowerBodyAngles._rightLowerLeg;

            RaiseLowerBodyAnglesChangedEvent();
        }

        /// <summary>
        /// Applies angles to the upper body all at once.
        /// </summary>
        /// <param name="waist"></param>
        /// <param name="leftUpperLeg"></param>
        /// <param name="leftLowerLeg"></param>
        /// <param name="rightUpperLeg"></param>
        /// <param name="rightLowerLeg"></param>
        public void SetLowerBodyAngles(
            T waist,
            T leftUpperLeg,
            T leftLowerLeg,
            T rightUpperLeg,
            T rightLowerLeg)
        {
            if (_waist.Equals(waist) &&
                _leftUpperLeg.Equals(leftUpperLeg) &&
                _leftLowerLeg.Equals(leftLowerLeg) &&
                _rightUpperLeg.Equals(rightUpperLeg) &&
                _rightLowerLeg.Equals(rightLowerLeg))
            {
                return;
            }
            _waist = waist;
            _leftUpperLeg = leftUpperLeg;
            _leftLowerLeg = leftLowerLeg;
            _rightUpperLeg = rightUpperLeg;
            _rightLowerLeg = rightLowerLeg;

            RaiseLowerBodyAnglesChangedEvent();
        }

        /// <summary>
        /// Applies angles to the entire body all at once.
        /// </summary>
        /// <param name="fullBodyAngles"></param>
        public void SetFullBodyAngles(HumanoidAngles<T> fullBodyAngles)
        {
            SetUpperBodyAngles(fullBodyAngles);
            SetLowerBodyAngles(fullBodyAngles);
        }

        /// <summary>
        /// Applies angles to the entire body all at once.
        /// </summary>
        /// <param name="chest"></param>
        /// <param name="leftUpperArm"></param>
        /// <param name="leftLowerArm"></param>
        /// <param name="rightUpperArm"></param>
        /// <param name="rightLowerArm"></param>
        /// <param name="waist"></param>
        /// <param name="leftUpperLeg"></param>
        /// <param name="leftLowerLeg"></param>
        /// <param name="rightUpperLeg"></param>
        /// <param name="rightLowerLeg"></param>
        public void SetFullBodyAngles(
            T chest,
            T leftUpperArm,
            T leftLowerArm,
            T rightUpperArm,
            T rightLowerArm,
            T waist,
            T leftUpperLeg,
            T leftLowerLeg,
            T rightUpperLeg,
            T rightLowerLeg)
        {
            SetUpperBodyAngles(chest, leftUpperArm, leftLowerArm, rightUpperArm, rightLowerArm);
            SetLowerBodyAngles(waist, leftUpperLeg, leftLowerLeg, rightUpperLeg, rightLowerLeg);
        }

        public override string ToString()
        {
            return
                string.Format(
                    "(Chest: {0}, LeftUpperArm: {1}, LeftLowerArm: {2}, RightUpperArm: {3}, RightLowerArm: {4}, Waist: {5}, LeftUpperLeg: {6}, LeftLowerLeg: {7}, RightUpperLeg: {8}, RightLowerLeg: {9})",
                    Chest,
                    LeftUpperArm,
                    LeftLowerArm,
                    RightUpperArm,
                    RightLowerArm,
                    Waist,
                    LeftUpperLeg,
                    LeftLowerLeg,
                    RightUpperLeg,
                    RightLowerLeg
                );
        }

        private void RaiseUpperBodyAnglesChangedEvent()
        {
            var handler = UpperBodyAnglesChanged;
            if (handler != null)
            {
                handler(this);
            }
        }

        private void RaiseLowerBodyAnglesChangedEvent()
        {
            var handler = LowerBodyAnglesChanged;
            if (handler != null)
            {
                handler(this);
            }
        }
    }
}