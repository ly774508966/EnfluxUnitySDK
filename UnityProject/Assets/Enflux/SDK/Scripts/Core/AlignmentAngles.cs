using System;
using UnityEngine;
using System.Collections;

namespace Enflux.SDK.Core
{
    [Serializable]
    public class AlignmentAngles<T>
    {
        private T _chestAlignment;
        private T _leftUpperArmAlignment;
        private T _leftLowerArmAlignment;
        private T _rightUpperArmAlignment;
        private T _rightLowerArmAlignment;
        private T _waistAlignment;
        private T _leftUpperLegAlignment;
        private T _leftLowerLegAlignment;
        private T _rightUpperLegAlignment;
        private T _rightLowerLegAlignment;
        
        public T ChestAlignment
        {
            get { return _chestAlignment; }
            set
            {
                if (_chestAlignment.Equals(value))
                {
                    return;
                }
                _chestAlignment = value;
            }
        }

        public T LeftUpperArmAlignment
        {
            get { return _leftUpperArmAlignment; }
            set
            {
                if (_leftUpperArmAlignment.Equals(value))
                {
                    return;
                }
                _leftUpperArmAlignment = value;
            }
        }

        public T LeftLowerArmAlignment
        {
            get { return _leftLowerArmAlignment; }
            set
            {
                if (_leftLowerArmAlignment.Equals(value))
                {
                    return;
                }
                _leftLowerArmAlignment = value;
            }
        }

        public T RightUpperArmAlignment
        {
            get { return _rightUpperArmAlignment; }
            set
            {
                if (_rightUpperArmAlignment.Equals(value))
                {
                    return;
                }
                _rightUpperArmAlignment = value;
            }
        }

        public T RightLowerArmAlignment
        {
            get { return _rightLowerArmAlignment; }
            set
            {
                if (_rightLowerArmAlignment.Equals(value))
                {
                    return;
                }
                _rightLowerArmAlignment = value;
            }
        }
    }
}

