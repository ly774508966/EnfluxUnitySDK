using System.Collections;
using Enflux.SDK.Animation;
using UnityEngine;

namespace Enflux.SDK.Experimental
{
    [ExecuteInEditMode]
    public class AnimatorRigMapper : RigMapper
    {
        //TODO: Make a public property and handle logic to regenerate waist
        [SerializeField] private Animator _animator;

        public Transform Hips { get; private set; }

        // World basis vectors for Animator rig's 3D model
        private Vector3 _rigUp;
        private Vector3 _rigForward;
        private Vector3 _rigRight;


        protected override void Reset()
        {
            base.Reset();
            _animator = GetComponent<Animator>();
            if (IsAnimatorConfigured())
            {
                RegenerateRigDirections();
            }
        }

        private void OnValidate()
        {
            if (!IsAnimatorConfigured(true))
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _animator = _animator ?? GetComponent<Animator>();
            if (!IsAnimatorConfigured(true))
            {
                enabled = false;
            }
        }

        private IEnumerator Start()
        {
            if (!Application.isPlaying)
            {
                yield break;
            }
            while (Waist == null)
            {
                RegenerateWaistOnRig();
                yield return null;
            }
            RegenerateRigDirections();
        }

        private void OnDrawGizmos()
        {
            if (!IsAnimatorConfigured())
            {
                return;
            }

            var hipsTransform = _animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hipsTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(hipsTransform.position, _rigUp);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hipsTransform.position, _rigForward);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(hipsTransform.position, _rigRight);
            }
        }

        public override void ApplyHumanoidToRig()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || Waist == null || Humanoid == null)
            {
                return;
            }

            // TODO: Investigate handling null joints! Null values may not be possible at this point b/c of the Unity's Animator rig validation.
            Hips.localRotation = Quaternion.identity;

            Core.localRotation = Humanoid.LocalAngles.Chest;
            LeftUpperArm.localRotation = Humanoid.LocalAngles.LeftUpperArm;
            LeftLowerArm.localRotation = Humanoid.LocalAngles.LeftLowerArm;
            RightUpperArm.localRotation = Humanoid.LocalAngles.RightUpperArm;
            RightLowerArm.localRotation = Humanoid.LocalAngles.RightLowerArm;

            Waist.localRotation = Humanoid.LocalAngles.Waist;
            LeftUpperLeg.localRotation = Humanoid.LocalAngles.LeftUpperLeg;
            LeftLowerLeg.localRotation = Humanoid.LocalAngles.LeftLowerLeg;
            RightUpperLeg.localRotation = Humanoid.LocalAngles.RightUpperLeg;
            RightLowerLeg.localRotation = Humanoid.LocalAngles.RightLowerLeg;

            var coreWorldRotation = Core.rotation;
            var leftUpperArmWorldRotation = LeftUpperArm.rotation;
            var leftLowerArmWorldRotation = LeftLowerArm.rotation;
            var rightUpperArmWorldRotation = RightUpperArm.rotation;
            var rightLowerArmWorldRotation = RightLowerArm.rotation;

            var waistWorldRotation = Waist.rotation;
            var leftUpperLegWorldRotation = LeftUpperLeg.rotation;
            var leftLowerLegWorldRotation = LeftLowerLeg.rotation;
            var rightUpperLegWorldRotation = RightUpperLeg.rotation;
            var rightLowerLegWorldRotation = RightLowerLeg.rotation;

            // Prefer to blend closer to the waist rotation than the core rotation
            Hips.rotation = Quaternion.Lerp(coreWorldRotation, waistWorldRotation, 0.25f);

            Core.rotation = coreWorldRotation;
            LeftUpperArm.rotation = leftUpperArmWorldRotation;
            LeftLowerArm.rotation = leftLowerArmWorldRotation;
            RightUpperArm.rotation = rightUpperArmWorldRotation;
            RightLowerArm.rotation = rightLowerArmWorldRotation;

            Waist.rotation = waistWorldRotation;
            LeftUpperLeg.rotation = leftUpperLegWorldRotation;
            LeftLowerLeg.rotation = leftLowerLegWorldRotation;
            RightUpperLeg.rotation = rightUpperLegWorldRotation;
            RightLowerLeg.rotation = rightLowerLegWorldRotation;
        }

        private void RegenerateWaistOnRig()
        {
            var hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            var leftUpperLeg = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            var rightUpperLeg = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);

            if (hips == null)
            {
                Debug.Log(gameObject.name + ", hips are null!");
                _animator.Rebind();
            }

            Core = _animator.GetBoneTransform(HumanBodyBones.Chest);
            LeftUpperArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            LeftLowerArm = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            RightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            RightLowerArm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

            Hips = hips;
            LeftUpperLeg = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            LeftLowerLeg = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            RightUpperLeg = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            RightLowerLeg = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

            var oldWaist = Waist;
            var root = new GameObject("Enflux Waist");
            Waist = root.transform;
            Waist.SetParent(hips, false);
            Waist.localPosition = Vector3.zero; // TODO: Verify that set initial transform values is even necessary

            if (leftUpperLeg != null)
            {
                leftUpperLeg.SetParent(Waist, true);
            }
            if (rightUpperLeg != null)
            {
                rightUpperLeg.SetParent(Waist, true);
            }
            if (oldWaist != null)
            {
                Destroy(oldWaist);
            }
            _animator.Rebind();
        }

        private void RegenerateRigDirections()
        {
            var hipsTransform = _animator.GetBoneTransform(HumanBodyBones.Hips);
            var headTransform = _animator.GetBoneTransform(HumanBodyBones.Head);
            if (hipsTransform == null)
            {
                Debug.LogError(gameObject.name + " - Animator does not contain a bone transform for hips!");
                return;
            }
            if (headTransform == null)
            {
                Debug.LogError(gameObject.name + " - Animator does not contain a bone transform for head!");
                return;
            }

            _rigUp = (headTransform.position - hipsTransform.position).normalized;
            _rigForward = hipsTransform.forward.normalized;
            _rigRight = (Quaternion.AngleAxis(90f, _rigUp)*_rigForward).normalized;
        }

        private bool IsAnimatorConfigured(bool printConfigErrors)
        {
            if (_animator == null)
            {
                if (printConfigErrors)
                {
                    Debug.LogError(gameObject.name + " - Animator is null! Please assign this.");
                }
                return false;
            }
            if (_animator.avatar == null)
            {
                if (printConfigErrors)
                {
                    Debug.LogError(gameObject.name + " - Animator's avatar is null! Please assign this.");
                }
                return false;
            }
            if (!_animator.avatar.isHuman)
            {
                if (printConfigErrors)
                {
                    Debug.LogError(gameObject.name +
                                   " - Animator's avatar is not properly configured as a humanoid! Please fix this.");
                }
                return false;
            }
            if (!_animator.avatar.isValid)
            {
                if (printConfigErrors)
                {
                    Debug.LogError(gameObject.name + " - Animator's avatar is not valid! Please fix this.");
                }
                return false;
            }
            if (!_animator.hasTransformHierarchy)
            {
                if (printConfigErrors)
                {
                    Debug.LogError(gameObject.name +
                                   " - Animator's avatar is optimized! Avatars without transform hierarchies aren't supported. Please disable \"Optimize Game Objects\" in the rig import settings.");
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Has the animator and its avatar been properly configured to work with Enflux clothing?
        /// </summary>
        /// <returns>A boolean indicating proper configuration.</returns>
        public bool IsAnimatorConfigured()
        {
            return IsAnimatorConfigured(false);
        }
    }
}