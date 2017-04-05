using UnityEngine;
using System;
using System.Collections;
using Enflux.SDK.Alignment;
using Enflux.SDK.Utils;

namespace Enflux.SDK.Core
{
    // Designed to be attached to a rig
    public class SuitAlignment : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private EnfluxSuitStream _absoluteAnglesStream;
        private AlignmentQuaternions<Quaternion> _alignment = new AlignmentQuaternions<Quaternion>();

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

        /// <summary>
        /// The source of the absolute angles used to calculate local angles for each limb.
        /// </summary>
        public EnfluxSuitStream AbsoluteAnglesStream
        {
            get { return _absoluteAnglesStream; }
            set
            {
                if (_absoluteAnglesStream == value)
                {
                    return;
                }
                _absoluteAnglesStream = value;                
            }
        }       

        // may need to use this to get initial orientation
        private void Awake()
        {
            if (!_isAligned)
            {

            }            
        }

        private void OnDestroy()
        {
            if (_isSubscribed)
            {
                UnsubscribeFromEvents();
            }
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

        }

        // Subscribe to main data stream
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

        //TODO: needs to stop the coroutine if connection is lost
        private void QueueAlignSensors()
        {
            if(_co_alignTimer != null)
            {
                StopCoroutine(_co_alignTimer);
            }

            _co_alignTimer = Co_QueueAlignSensors();
            StartCoroutine(_co_alignTimer);
        }

        private IEnumerator Co_QueueAlignSensors()
        {
            var time = _alignmentTime;

            while(time > 0)
            {
                // emit an event here for status of alignment
                RaiseAlignmentTimingEvent(time--);
                yield return new WaitForSeconds(1.0f);
            }

            // Stop listening for data
            UnsubscribeFromEvents();

            // reset timer
            _co_alignTimer = null;

            // fire off alignment calculations
            AlignFullBodySensors();                                    
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

