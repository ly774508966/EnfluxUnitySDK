// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;
using Enflux.SDK.Attributes;
using UnityEngine;
using Enflux.SDK.Alignment;

namespace Enflux.SDK.Core
{
    /// <summary>
    /// Defines an Enflux suit (pair of Enflux shirt+pants). 
    /// Provides absolute sensor orientations (relative to real-world directions), the starting absolute orientation of the suit, and outputs events for device state, device notifications, and device errors.
    /// </summary>
    public abstract class EnfluxSuitStream : MonoBehaviour
    {
        [SerializeField, Readonly] private DeviceState _shirtState = DeviceState.Disconnected;
        [SerializeField, Readonly] private DeviceState _pantsState = DeviceState.Disconnected;

        private AlignmentState _alignState = AlignmentState.Unaligned;

        private Vector3 _shirtBaseOrientation;
        private Vector3 _pantsBaseOrientation;

        private SuitAlignment _suitAlign = null;
        private AlignmentQuaternions _shirtAlignment = null;
        private AlignmentQuaternions _pantAlignment = null;

        private readonly HumanoidAngles<Vector3> _absoluteAngles = new HumanoidAngles<Vector3>();

        public event Action<StateChange<DeviceState>> ShirtStateChanged;
        public event Action<StateChange<DeviceState>> PantsStateChanged;
        public event Action<DeviceNotification> ShirtReceivedNotification;
        public event Action<DeviceNotification> PantsReceivedNotification;
        public event Action<DeviceError> ShirtReceivedError;
        public event Action<DeviceError> PantsReceivedError;
        public event Action<AlignmentState> AlignmentStateChanged;

        public AlignmentQuaternions ShirtAlignment
        {
            get
            {
                if(_shirtAlignment == null)
                {
                    _shirtAlignment = new AlignmentQuaternions();
                }
                return _shirtAlignment;
            }
        }

        public AlignmentQuaternions PantAlignment
        {
            get
            {
                if(_pantAlignment == null)
                {
                    _pantAlignment = new AlignmentQuaternions();
                }
                return _pantAlignment;
            }
        }

        public AlignmentState AlignState
        {
            get { return _alignState; }
        }

        public DeviceState ShirtState
        {
            get { return _shirtState; }
            protected set
            {
                if (_shirtState == value)
                {
                    return;
                }
                var previous = _shirtState;
                _shirtState = value;
                var handler = ShirtStateChanged;
                if (handler != null)
                {
                    handler.Invoke(new StateChange<DeviceState>(previous, _shirtState));
                }
            }
        }

        public DeviceState PantsState
        {
            get { return _pantsState; }
            protected set
            {
                if (_pantsState == value)
                {
                    return;
                }
                var previous = _pantsState;
                _pantsState = value;
                var handler = PantsStateChanged;
                if (handler != null)
                {
                    handler.Invoke(new StateChange<DeviceState>(previous, _pantsState));
                }
            }
        }

        /// <summary>
        /// The suit's limb angles in absolute real-world coordinates. For example, a rotation's y component of 0 corresponds to a yaw pointing north in real-world coordinates, 180 corresponds to south.
        /// </summary>
        public HumanoidAngles<Vector3> AbsoluteAngles
        {
            get { return _absoluteAngles; }
        }

        /// <summary>
        /// The base real-world orientation of the core module of the shirt.
        /// </summary>
        /// <param name="value">A y component of 0 corresponds to a yaw pointing north in real-world coordinates, 180 corresponds to south.</param>
        public Vector3 ShirtBaseOrientation
        {
            get { return _shirtBaseOrientation; }
            set
            {
                _shirtBaseOrientation = value;
                RaiseShirtNotificationEvent(DeviceNotification.ResetOrientation);
            }
        }

        /// <summary>
        /// The base real-world orientation of the waist module of the pants.
        /// </summary>
        /// <param name="value">A y component of 0 corresponds to a yaw pointing north in real-world coordinates, 180 corresponds to south.</param>
        public Vector3 PantsBaseOrientation
        {
            get { return _pantsBaseOrientation; }
            set
            {
                _pantsBaseOrientation = value;
                RaisePantsNotificationEvent(DeviceNotification.ResetOrientation);
            }
        }

        protected void RaiseShirtNotificationEvent(DeviceNotification shirtNotification)
        {
            var handler = ShirtReceivedNotification;
            if (handler != null)
            {
                handler(shirtNotification);
            }
        }

        protected void RaisePantsNotificationEvent(DeviceNotification pantsNotification)
        {
            var handler = PantsReceivedNotification;
            if (handler != null)
            {
                handler(pantsNotification);
            }
        }

        protected void RaiseShirtErrorEvent(DeviceError shirtError)
        {
            var handler = ShirtReceivedError;
            if (handler != null)
            {
                handler(shirtError);
            }
        }

        protected void RaisePantsErrorEvent(DeviceError pantsError)
        {
            var handler = PantsReceivedError;
            if (handler != null)
            {
                handler(pantsError);
            }
        }

        protected void RaiseAlignmentStateEvent(AlignmentState state)
        {
            var handler = AlignmentStateChanged;
            if(handler != null)
            {
                handler(state);
            }
        }

        public void SetUpperAlignment(AlignmentQuaternions align)
        {           
            _shirtAlignment = align;
        }

        public void SetLowerAlignment(AlignmentQuaternions align)
        {
            _pantAlignment = align;
        }

        public void SetAlignmentCompleted(AlignmentState state)
        {
            if(state == AlignmentState.Aligned)
            {
                _suitAlign = null;
                _alignState = AlignmentState.Aligned;
                RaiseAlignmentStateEvent(_alignState);
            }           
        }       

        public void AlignSensorsToUser()
        {
            if(_suitAlign == null)
            {
                _suitAlign = new SuitAlignment(this);
                _suitAlign.InitiateAlignment();
            }
            else
            {
                // fire off some sort of event
                RaiseAlignmentStateEvent(AlignmentState.InProgress);
            }
        }

        /// <summary>
        /// Sets ShirtBaseOrientation to the current orientation of the shirt's chest module.
        /// </summary>
        public void ResetShirtBaseOrientation()
        {
            ShirtBaseOrientation = AbsoluteAngles.Chest;
            if (_alignState == AlignmentState.Unaligned)
            {
                _shirtAlignment = new SuitAlignment().
                    SetUpperInitialAlignment(ShirtBaseOrientation);
            }
        }

        /// <summary>
        /// Sets PantsBaseOrientation to the current orientation of the pant's waist module.
        /// </summary>
        public void ResetPantsBaseOrientation()
        {
            PantsBaseOrientation = AbsoluteAngles.Waist;

            if (_alignState == AlignmentState.Unaligned)
            {
                _pantAlignment = new SuitAlignment().
                    SetLowerInitialAlignment(PantsBaseOrientation);
            }
        }

        /// <summary>
        /// Sets ShirtBaseOrientation to the current orientation of the shirt's chest module and PantsBaseOrientation to the current orientation of the pant's waist module.
        /// </summary>
        public void ResetFullBodyBaseOrientation()
        {
            ResetShirtBaseOrientation();
            ResetPantsBaseOrientation();          
        }       
    }
}