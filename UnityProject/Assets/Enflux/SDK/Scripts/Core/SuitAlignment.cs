using UnityEngine;
using System;
using System.Collections;
using Enflux.SDK.Alignment;
using Enflux.SDK.Utils;

namespace Enflux.SDK.Core
{
    
    public class SuitAlignment 
    {
        private EnfluxSuitStream _absoluteAnglesStream;        

        private int _alignmentTime = 5;
        private IEnumerator _co_alignTimer = null;

        private bool _isSubscribed = false;
        private bool _isAligned = false;
        
        private readonly SensorAlignment _sensorAlignment = new SensorAlignment();
        private readonly ImuOrientations _imuOrientation = new ImuOrientations();                

        private Module _upperModule = null;
        private Module _lowerModule = null;

        private AlignmentQuaternions<Quaternion> _upperAlignment = null;
        private AlignmentQuaternions<Quaternion> _lowerAlignment = null;       

        public event Action<int> AlignmentTimeRemaining;

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

                // start a timer
            }
            else
            {
                // fire event that already initiated
            }
        }

        private void SetInitialAlignment(Vector3 upper, Vector3 lower)
        {
            // only set if there is not already an alignment
            if (_upperAlignment == null)
            {
                _upperAlignment = new AlignmentQuaternions<Quaternion>();

                // alignment should not do anything to IMU heading axis                
                upper.z = 0;
                _upperAlignment.CenterAlignment = _imuOrientation.BaseOrientation(upper);               
            }
        }       

        private void OnUpperBodyAnglesChanged(HumanoidAngles<Vector3> absoluteAngles)
        {
            if(_upperModule == null)
            {
                _upperModule = new Module();
                _upperModule.FirstQuat = _imuOrientation.BaseOrientation(absoluteAngles.Chest);
            }
            else
            {
                _upperModule.Center = _imuOrientation.BaseOrientation(absoluteAngles.Chest);
                _upperModule.LeftUpper = _imuOrientation.LeftOrientation(absoluteAngles.LeftUpperArm);
                _upperModule.LeftLower = _imuOrientation.LeftOrientation(absoluteAngles.LeftLowerArm);
                _upperModule.RightUpper = _imuOrientation.RightOrientation(absoluteAngles.RightUpperArm);
                _upperModule.RightLower = _imuOrientation.RightOrientation(absoluteAngles.RightLowerArm);

                _upperModule.InitialCenter = QuaternionUtils.AverageQuaternion(
                    ref _upperModule.QuatComponents,
                    _upperModule.Center,
                    _upperModule.FirstQuat,
                    ++_upperModule.QuatCounter);
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
                _lowerModule.Center = _imuOrientation.BaseOrientation(absoluteAngles.Waist);
                _lowerModule.LeftUpper = _imuOrientation.LeftOrientation(absoluteAngles.LeftUpperLeg);
                _lowerModule.LeftLower = _imuOrientation.LeftOrientation(absoluteAngles.LeftLowerLeg);
                _lowerModule.RightUpper = _imuOrientation.RightOrientation(absoluteAngles.RightUpperLeg);
                _lowerModule.RightLower = _imuOrientation.RightOrientation(absoluteAngles.RightLowerLeg);

                _lowerModule.InitialCenter = QuaternionUtils.AverageQuaternion(
                    ref _lowerModule.QuatComponents,
                    _lowerModule.Center,
                    _lowerModule.FirstQuat,
                    ++_lowerModule.QuatCounter);
            }
        }

        // Subscribe to main data stream
        private void SubscribeToEvents()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            _isSubscribed = true;
            _absoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged += OnUpperBodyAnglesChanged;
            _absoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged += OnLowerBodyAnglesChanged;           
        }

        private void UnsubscribeFromEvents()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            _isSubscribed = false;
            _absoluteAnglesStream.AbsoluteAngles.UpperBodyAnglesChanged -= OnUpperBodyAnglesChanged;
            _absoluteAnglesStream.AbsoluteAngles.LowerBodyAnglesChanged -= OnLowerBodyAnglesChanged;
        }        

        private void RaiseAlignmentTimingEvent(int remainingTime)
        {
            var handler = AlignmentTimeRemaining;
            if(handler != null)
            {
                handler(remainingTime);
            }
        }

        private void AlignFullBodySensors()
        {
            AlignUpperBodySensors();
            AlignLowerBodySensors();
        }

        private void AlignUpperBodySensors()
        {
            // may want to return something from this function
            _sensorAlignment.UpperBodyAlignment(
                    _upperModule.InitialCenter,
                    _upperModule.Center,
                    _upperModule.LeftUpper,
                    _upperModule.LeftLower,
                    _upperModule.RightUpper,
                    _upperModule.RightLower);

            // discard module
            _upperModule = null;
        }

        private void AlignLowerBodySensors()
        {
            // may want to return something from this function
            _sensorAlignment.LowerBodyAlignment(
                    _lowerModule.InitialCenter,
                    _lowerModule.Center,
                    _lowerModule.LeftUpper,
                    _lowerModule.LeftLower,
                    _lowerModule.RightUpper,
                    _lowerModule.RightLower);

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
        }
    }
}

