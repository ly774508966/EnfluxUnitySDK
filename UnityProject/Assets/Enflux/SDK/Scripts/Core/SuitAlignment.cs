﻿using UnityEngine;
using Enflux.SDK.Alignment;
using Enflux.SDK.Utils;

namespace Enflux.SDK.Core
{
    public class SuitAlignment
    {
        private readonly EnfluxSuitStream _absoluteAnglesStream;

        private bool _isSubscribed;

        private readonly SensorAlignment _sensorAlignment = new SensorAlignment();
        private readonly ImuOrientations _imuOrientation = new ImuOrientations();

        private Module _upperModule;
        private Module _lowerModule;

        private AlignmentQuaternions _upperAlignment;
        private AlignmentQuaternions _lowerAlignment;

        public SuitAlignment()
        {
            /* empty constructor */
        }

        public SuitAlignment(EnfluxSuitStream s)
        {
            _absoluteAnglesStream = s;
        }

        // call to start data polling 
        public void InitiateAlignment()
        {
            if (!_isSubscribed)
            {
                SubscribeToEvents();
            }
        }

        public void CancelAlignment()
        {
            if (_isSubscribed)
            {
                UnsubscribeFromEvents();
            }
        }

        private float AlignmentProgress()
        {
            var upper = (_upperModule != null) ?
                CheckUpperProgress() : 1;

            var lower = (_lowerModule != null) ?
                CheckLowerProgress() : 1;

            var progress = upper * lower;

            _absoluteAnglesStream.SetAlignmentProgress(progress);

            return progress;
        }

        private float CheckLowerProgress()
        {
            var lower = _lowerModule.PercentDone;

            if (Mathf.Approximately(lower, 1.0f))
            {
                UnsubscribeLower();
            }

            return lower;
        }

        private float CheckUpperProgress()
        {
            var upper = _upperModule.PercentDone;

            if (Mathf.Approximately(upper, 1.0f))
            {
                UnsubscribeUpper();
            }

            return upper;
        }

        private void CheckProgress()
        {
            var progress = AlignmentProgress();

            if (Mathf.Approximately(progress, 1.0f))
            {
                // stop collecting data before doing calculations
                UnsubscribeFromEvents();
                AlignFullBodySensors();
            }
        }

        public AlignmentQuaternions SetUpperInitialAlignment(Vector3 angles)
        {
            _upperAlignment = new AlignmentQuaternions();

            // alignment should not do anything to IMU heading axis                
            angles.z = 0;
            _upperAlignment.CenterAlignment =
                Quaternion.Inverse(_imuOrientation.BaseOrientation(angles));

            return _upperAlignment;
        }

        public AlignmentQuaternions SetLowerInitialAlignment(Vector3 angles)
        {
            _lowerAlignment = new AlignmentQuaternions();

            // alignment should not do anything to IMU heading axis                
            angles.z = 0;
            _lowerAlignment.CenterAlignment =
                Quaternion.Inverse(_imuOrientation.BaseOrientation(angles));

            return _lowerAlignment;
        }

        private void CompleteSuitAlignment()
        {
            var state = AlignmentState.Aligned;
            UnsubscribeFromEvents();
            if (_lowerAlignment != null && _lowerAlignment.Error < 0)
                state = AlignmentState.ErrorAligning;


            if (_upperAlignment != null && _upperAlignment.Error < 0)
                state = AlignmentState.ErrorAligning;

            _absoluteAnglesStream.SetAlignmentCompleted(state);
        }

        private void CompleteUpperAlignment()
        {
            _absoluteAnglesStream.SetUpperAlignment(_upperAlignment);
        }

        private void CompleteLowerAlginment()
        {
            _absoluteAnglesStream.SetLowerAlignment(_lowerAlignment);
        }

        private void OnUpperBodyAnglesChanged(HumanoidAngles<Vector3> absoluteAngles)
        {
            if (_upperModule == null)
            {
                _upperModule = new Module();
                _upperModule.FirstQuat = _imuOrientation.BaseOrientation(absoluteAngles.Chest);
            }
            else
            {
                _upperModule.Center =
                    _imuOrientation.BaseOrientation(absoluteAngles.Chest);
                _upperModule.LeftUpper =
                    _imuOrientation.LeftOrientation(absoluteAngles.LeftUpperArm);
                _upperModule.LeftLower =
                    _imuOrientation.LeftOrientation(absoluteAngles.LeftLowerArm);
                _upperModule.RightUpper =
                    _imuOrientation.RightOrientation(absoluteAngles.RightUpperArm);
                _upperModule.RightLower =
                    _imuOrientation.RightOrientation(absoluteAngles.RightLowerArm);

                _upperModule.InitialCenter = QuaternionUtils.AverageQuaternion(
                    ref _upperModule.QuatComponents,
                    _upperModule.Center,
                    _upperModule.FirstQuat,
                    ++_upperModule.QuatCounter);

                CheckProgress();
            }
        }

        private void OnLowerBodyAnglesChanged(HumanoidAngles<Vector3> absoluteAngles)
        {
            if (_lowerModule == null)
            {
                _lowerModule = new Module();
                _lowerModule.FirstQuat = _imuOrientation.BaseOrientation(absoluteAngles.Waist);
            }
            else
            {
                _lowerModule.Center =
                    _imuOrientation.BaseOrientation(absoluteAngles.Waist);
                _lowerModule.LeftUpper =
                    _imuOrientation.LeftOrientation(absoluteAngles.LeftUpperLeg);
                _lowerModule.LeftLower =
                    _imuOrientation.LeftOrientation(absoluteAngles.LeftLowerLeg);
                _lowerModule.RightUpper =
                    _imuOrientation.RightOrientation(absoluteAngles.RightUpperLeg);
                _lowerModule.RightLower =
                    _imuOrientation.RightOrientation(absoluteAngles.RightLowerLeg);

                _lowerModule.InitialCenter = QuaternionUtils.AverageQuaternion(
                    ref _lowerModule.QuatComponents,
                    _lowerModule.Center,
                    _lowerModule.FirstQuat,
                    ++_lowerModule.QuatCounter);

                CheckProgress();
            }
        }

        private void SubscribeToEvents()
        {
            if (_absoluteAnglesStream == null)
            {
                return;
            }
            _isSubscribed = true;
            _absoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged +=
                OnUpperBodyAnglesChanged;
            _absoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged +=
                OnLowerBodyAnglesChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_absoluteAnglesStream != null)
            {
                _absoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged -=
                    OnUpperBodyAnglesChanged;
                _absoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged -=
                    OnLowerBodyAnglesChanged;
            }
            _isSubscribed = false;
        }

        private void UnsubscribeUpper()
        {
            if (_absoluteAnglesStream == null)
            {
                return;
            }
            _absoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged -=
                OnUpperBodyAnglesChanged;
        }

        private void UnsubscribeLower()
        {
            if (_absoluteAnglesStream == null)
            {
                return;
            }
            _absoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged -=
                OnLowerBodyAnglesChanged;
        }

        private void AlignFullBodySensors()
        {
            AlignUpperBodySensors();
            AlignLowerBodySensors();
            CompleteSuitAlignment();
        }

        private void AlignUpperBodySensors()
        {
            if (_upperModule != null)
            {
                _upperAlignment = _sensorAlignment.UpperBodyAlignment(
                    _upperModule.InitialCenter,
                    _upperModule.Center,
                    _upperModule.LeftUpper,
                    _upperModule.LeftLower,
                    _upperModule.RightUpper,
                    _upperModule.RightLower);

                CompleteUpperAlignment();
            }

            // discard module
            _upperModule = null;
        }

        private void AlignLowerBodySensors()
        {
            if (_lowerModule != null)
            {
                _lowerAlignment = _sensorAlignment.LowerBodyAlignment(
                    _lowerModule.InitialCenter,
                    _lowerModule.Center,
                    _lowerModule.LeftUpper,
                    _lowerModule.LeftLower,
                    _lowerModule.RightUpper,
                    _lowerModule.RightLower);

                CompleteLowerAlginment();
            }

            // discard module
            _lowerModule = null;
        }

        private class Module
        {
            public Quaternion Center;
            public Quaternion LeftUpper;
            public Quaternion LeftLower;
            public Quaternion RightUpper;
            public Quaternion RightLower;

            public Quaternion InitialCenter;
            public Quaternion FirstQuat;
            public Vector4 QuatComponents;
            public int QuatCounter;

            private const float _numSamples = 350;

            public float PercentDone
            {
                get { return QuatCounter / _numSamples; }
            }
        }
    }
}

