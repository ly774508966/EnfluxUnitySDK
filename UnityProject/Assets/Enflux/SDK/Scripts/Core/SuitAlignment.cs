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
        private AlignmentAngles<Quaternion> _alignment = new AlignmentAngles<Quaternion>();

        private float time = 5.0f;
        private IEnumerator _co_alignTimer = null;

        private bool _isSubscribed = false;
        private bool _isAligned = false;
        
        private readonly SensorAlignment _sensorAlignment = new SensorAlignment();
        private readonly ImuOrientations _imuOrientation = new ImuOrientations();                

        private Module _upperModule = null;
        private Module _lowerModule = null;                       

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

        public AlignmentAngles<Quaternion> Alignment
        {
            get { return _alignment; }
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

        private void SetInitialAlignment()
        {

        }

        // used to get initial alignment w/ no calculations
        private Vector3 ChestBaseOrientation
        {
            get
            {                
                return 
                    (AbsoluteAnglesStream != null) ? 
                    AbsoluteAnglesStream.ShirtBaseOrientation : Vector3.zero;
            }
        }

        // used to get initial alignment w/ no calculations
        private Vector3 WaistBaseOrientation
        {
            get
            {
                return 
                    (AbsoluteAnglesStream != null) ? 
                    AbsoluteAnglesStream.PantsBaseOrientation : Vector3.zero;
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
            while(time > 0.0f)
            {
                // emit an event here for status of alignment
                yield return new WaitForSeconds(1.0f);
            }

            // Stop listening for data
            UnsubscribeFromEvents();

            // reset timer
            _co_alignTimer = null;

            // fire off alignment calculations
            AlignFullBodySensors();                                    
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

