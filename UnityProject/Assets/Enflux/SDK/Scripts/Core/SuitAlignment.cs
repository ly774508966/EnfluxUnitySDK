using UnityEngine;
using System;
using Enflux.SDK.Alignment;
using Enflux.SDK.Utils;

namespace Enflux.SDK.Core
{    
    public class SuitAlignment 
    {
        private EnfluxSuitStream _absoluteAnglesStream;        

        private bool _isSubscribed = false;
        private bool _isAligned = false;      
        
        private readonly SensorAlignment _sensorAlignment = new SensorAlignment();
        private readonly ImuOrientations _imuOrientation = new ImuOrientations();                

        private Module _upperModule = null;
        private Module _lowerModule = null;

        private AlignmentQuaternions<Quaternion> _upperAlignment = null;
        private AlignmentQuaternions<Quaternion> _lowerAlignment = null;       

        public event Action<AlignmentQuaternions<Quaternion>> UpperAlignmentCompleted;
        public event Action<AlignmentQuaternions<Quaternion>> LowerAlignmentCompleted;

        public SuitAlignment(EnfluxSuitStream s)
        {
            _absoluteAnglesStream = s;
            _upperAlignment.SetAlignment(
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity);

            _lowerAlignment.SetAlignment(
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity);
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

        public float AlignmentProgress()
        {
            var upper = (_upperModule != null) ?
                _upperModule.PercentDone : 1;

            var lower = (_lowerModule != null) ?
                _lowerModule.PercentDone : 1;

            return upper * lower;
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

        private void CheckProgress()
        {
            var progress = AlignmentProgress();

            if(Mathf.Approximately(progress, 1.0f))
            {
                // stop collecting data before doing calculations
                UnsubscribeFromEvents();
                AlignFullBodySensors();
            }
        }        

        private void RaiseUpperAlignmentCompletedEvent()
        {
            var handler = UpperAlignmentCompleted;
            if(handler != null)
            {
                handler(_upperAlignment);
            }
        }

        private void RaiseLowerAlignmentCompletedEvent()
        {
            var handler = LowerAlignmentCompleted;
            if (handler != null)
            {
                handler(_lowerAlignment);
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

                CheckProgress();
            }
        }

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

        private void AlignFullBodySensors()
        {
            AlignUpperBodySensors();
            AlignLowerBodySensors();
        }

        private void AlignUpperBodySensors()
        {
            if(_upperModule != null)
            {
                _upperAlignment = _sensorAlignment.UpperBodyAlignment(
                    _upperModule.InitialCenter,
                    _upperModule.Center,
                    _upperModule.LeftUpper,
                    _upperModule.LeftLower,
                    _upperModule.RightUpper,
                    _upperModule.RightLower);

                RaiseUpperAlignmentCompletedEvent();
            }

            // discard module
            _upperModule = null;
        }

        private void AlignLowerBodySensors()
        {
            if(_upperModule != null)
            {
                _lowerAlignment = _sensorAlignment.LowerBodyAlignment(
                    _lowerModule.InitialCenter,
                    _lowerModule.Center,
                    _lowerModule.LeftUpper,
                    _lowerModule.LeftLower,
                    _lowerModule.RightUpper,
                    _lowerModule.RightLower);

                RaiseLowerAlignmentCompletedEvent();
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

            private const int _numSamples = 350;

            public float PercentDone
            {
                get { return QuatCounter / _numSamples; }
            }            
        }
    }
}

