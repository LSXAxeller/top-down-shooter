// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections;
using UnityEngine;

namespace Deftly
{
    public class IkProxy : MonoBehaviour
    {
        // Subject Information
        public Transform SubjectTr;
        public Subject Subject;
        private Animator Animator;
        private bool UseLeftIk;
        private bool UseRightIk;
        private int RightHandLayer;
        private int LeftHandLayer;
        private float CharScaleMultiplier;

        // Weapon Information
        private Weapon _weapon;
        private Transform _weaponTransform;
        private WeaponType _weaponType;
        private MountPivot _weaponPivot;
        private Transform _weaponOriginalNonDomGoal;
        private Hand DominantHand = Hand.Right;
        private bool _cantProcessIk;

        private bool _transitioning;
        private GameObject _nonDomWorldTarget;

        void CreateNonDomWorldTarget()
        {
            _nonDomWorldTarget = new GameObject
            {
                hideFlags = HideFlags.HideInHierarchy,
                name = "_NonDomHandTarget"
            };
        }
        void Awake()
        {
            Animator = GetComponent<Animator>();
        }
        void OnEnable()
        {
            CreateNonDomWorldTarget();
        }        
        void OnDestroy()
        {
            Destroy(_nonDomWorldTarget); // cleanup
        }
        void Start()
        {
            CharScaleMultiplier = Subject.Stats.CharacterScale;
            Subject.OnReload += DoReload;
        }

        public void UseLeftHandIk(bool status) { UseLeftIk = status; }
        public void UseRightHandIk(bool status) { UseRightIk = status; }
        public void DisableHandIk()
        {
            UseLeftIk = false;
            UseRightIk = false;
        }
        public void EnableHandIk()
        {
            UseLeftIk = true;
            UseRightIk = true;
        }
        public void ResetHandIk()
        {
            UseLeftIk = Subject.Stats.UseLeftHandIk;
            UseRightIk = Subject.Stats.UseRightHandIk;
        }
        public void SetupWeapon(GameObject weapon)
        {
            // TODO blend IK into position
            // Cache relevant stuff
            _weaponTransform = weapon.transform;
            _weapon = _weaponTransform.GetComponent<Weapon>();
            _weaponType = _weapon.Stats.WeaponType;
            _weaponPivot = _weapon.Stats.MountPivot;
            _weaponOriginalNonDomGoal = _weapon.Stats.NonDominantHandGoal;
            if (_weaponOriginalNonDomGoal && _nonDomWorldTarget == null) { CreateNonDomWorldTarget(); }

            // Set Weapon Parent
            _weaponTransform.SetParent(GetRightHandBone());

            // Set Weapon IN HAND Position and IN HAND Rotation
            _weaponTransform.localPosition = Vector3.zero + SubjectTr.TransformVector(Subject.WeaponPositionInHandCorrection);
            _weaponTransform.rotation = FindWeaponOrientation();

            // Turn on/off ik per the Subject
            ResetHandIk();

            // Get Layer Information
            LeftHandLayer = Subject.LeftHandIkLayer;
            RightHandLayer = Subject.RightHandIkLayer;

            // Tell the Animator its Type ID
            if (Subject.ControlStats.AnimatorWeaponType != "") Animator.SetInteger(Subject.ControlStats.AnimatorWeaponType, _weapon.Stats.TypeId);

            // Fire the event to do the swap animation.
            DoSwitchedWeapon();
        }
        
        public void DoReload() { StartCoroutine(ReloadTransition()); }
        public void DoSwitchedWeapon() { StartCoroutine(WeaponTransition()); }

        public IEnumerator ReloadTransition()
        {
            _transitioning = true;

            if (Subject.ControlStats.AnimatorReload != "") Animator.SetBool(Subject.ControlStats.AnimatorReload, true);
            yield return new WaitForSeconds(_weapon.Stats.ReloadTime);
            if (Subject.ControlStats.AnimatorReload != "") Animator.SetBool(Subject.ControlStats.AnimatorReload, false);

            _transitioning = false;
        }
        public IEnumerator WeaponTransition()
        {
            if (Subject.LogDebug) Debug.Log("Swapping.");
            _transitioning = true;

            if (Subject.ControlStats.AnimatorSwap != "") Animator.SetBool(Subject.ControlStats.AnimatorSwap, true);
            yield return new WaitForSeconds(_weapon.Stats.SwapTime);
            if (Subject.ControlStats.AnimatorSwap != "") Animator.SetBool(Subject.ControlStats.AnimatorSwap, false);

            _transitioning = false;
            if (Subject.LogDebug) Debug.Log("Done Swapping.");
        }

        void Update()
        {
            _cantProcessIk = !_weapon || Subject.IsDead || _transitioning || _weaponType == WeaponType.Melee;
            if (_cantProcessIk) return;

            if (UseLeftIk && _weaponOriginalNonDomGoal != null)
            {
                _nonDomWorldTarget.transform.position = _weaponOriginalNonDomGoal.position;
                _nonDomWorldTarget.transform.rotation = _weaponOriginalNonDomGoal.rotation;
            }
        }
        void OnAnimatorIK(int layerIndex)
        {            
            if (_cantProcessIk) return;

            UseLeftIk = Subject.Stats.UseLeftHandIk;
            UseRightIk = Subject.Stats.UseRightHandIk;

            _weaponTransform.rotation = FindWeaponOrientation();

            if (layerIndex == RightHandLayer && UseRightIk) ApplyRightIk();
            if (layerIndex == LeftHandLayer && UseLeftIk && _weaponOriginalNonDomGoal != null) ApplyLeftIk();
        }

        private void ApplyRightIk()
        {
            SetIkPositionWeight(AvatarIKGoal.RightHand, 1);
            SetIkPosition(AvatarIKGoal.RightHand, FindDominantHandPosition());
            if (_weapon.Stats.UseElbowHintR)
            {
                SetIkHintWeight(AvatarIKHint.RightElbow, 1);
                SetIkHintPosition(AvatarIKHint.RightElbow, FindDominantElbowHintPosition());
            }
        }
        private void ApplyLeftIk()
        {
            SetIkPositionWeight(AvatarIKGoal.LeftHand, 1);
            SetIkRotationWeight(AvatarIKGoal.LeftHand, 1);
            SetIkPosition(AvatarIKGoal.LeftHand, FindNonDominantHandPosition());
            SetIkRotation(AvatarIKGoal.LeftHand, FindNonDominantHandRotation());
            if (_weapon.Stats.UseElbowHintL)
            {
                SetIkHintWeight(AvatarIKHint.LeftElbow, 1);
                SetIkHintPosition(AvatarIKHint.LeftElbow, FindNonDominantElbowHintPosition());
            }
        }

        void LateUpdate()
        {
            if (_cantProcessIk) return;
            
            // Fix the broken rotation
            if (UseRightIk) GetRightHandBone().rotation = FindDominantHandRotation();

            // cache the correct position non-dom goal data
            if (_nonDomWorldTarget && _weaponOriginalNonDomGoal)
            {
                _nonDomWorldTarget.transform.position = _weaponOriginalNonDomGoal.position;
                _nonDomWorldTarget.transform.rotation = _weaponOriginalNonDomGoal.rotation;
            }

            // show any debugs
            if (Subject.ShowGunDebug)
            {
                Debug.DrawRay(GetRightHandBone().position, Subject.InvertHandForward ? -GetRightHandBone().right*2 : GetRightHandBone().right, Color.red);
                Debug.DrawRay(GetRightHandBone().position, SubjectTr.forward * 0.5f, Color.green);
            }
        }

        private Quaternion FindWeaponOrientation()
        {
            Transform bone = GetRightHandBone();
            return Quaternion.LookRotation(
                DominantHand == Hand.Right && !Subject.InvertHandForward ? bone.right : -bone.right, 
                bone.TransformDirection(DominantHand == Hand.Right ? Subject.ThumbDirection : -Subject.ThumbDirection));
        }

        private Vector3 FindDominantHandPosition()
        {
            Vector3 a = _weaponPivot == MountPivot.LowerSpine ? FindSpinePosition() : FindShoulderPosition();
            Vector3 b = SubjectTr.TransformVector(_weapon.Stats.PositionOffset + Subject.DominantHandPosCorrection * CharScaleMultiplier);
            return a + b;
        }
        private Quaternion FindDominantHandRotation()
        {
            return Quaternion.LookRotation(SubjectTr.forward) * Quaternion.Euler(Subject.DominantHandRotCorrection);
        }
        private Vector3 FindDominantElbowHintPosition()
        {
            Vector3 pos = Animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position + SubjectTr.TransformVector(_weapon.Stats.DominantElbowOffset * CharScaleMultiplier);
#if UNITY_EDITOR
            Debug.DrawRay(pos, Vector3.up * 0.1f, Color.green);
            Debug.DrawRay(pos, Vector3.left * 0.1f, Color.green);
            Debug.DrawRay(pos, Vector3.right * 0.1f, Color.green);
            Debug.DrawRay(pos, Vector3.down * 0.1f, Color.green);
#endif
            return pos;
        }

        private Vector3 FindNonDominantHandPosition()
        {
            // The original goal will always be wrong until UT fixes the SetIKRotation() bug.
            // The Internal Animation pass is done and Mecanim gets the rotation wrong every time on the right hand. (which breaks the left hand)
            // I correct the rotation manually in LateUpdate() and cache the position for the LH Goal while its correct.
            // I use that goal here. So everything is 1 frame behind for the left hand.

            return _nonDomWorldTarget.transform.position + SubjectTr.InverseTransformVector(Subject.NonDominantHandPosCorrection * CharScaleMultiplier);

            /*
            Transform goal = CurrentWeapon.Stats.NonDominantHandGoal;
            if (goal) 
            {
                //if (_nonDomCached) 
                return goal.position; // + SubjectTr.TransformVector(Subject.NonDominantHandPosCorrection);
            }
            return CurrentWeaponTr.position + CurrentWeapon.Stats.NonDominantHandPos;
             */
        }
        private Quaternion FindNonDominantHandRotation()
        {
            return _nonDomWorldTarget.transform.rotation * Quaternion.Euler(Subject.NonDominantHandRotCorrection);

            /*
            Transform goal = CurrentWeapon.Stats.NonDominantHandGoal;
            if (goal)
            {
                //if (_nonDomCached) 
                return Quaternion.LookRotation(goal.forward, goal.up) * Quaternion.Euler(Subject.NonDominantHandRotCorrection);
            }

            Quaternion a = CurrentWeaponTr.rotation;
            Quaternion b = Quaternion.Euler(CurrentWeapon.Stats.NonDominantHandRot);
            return a * b;
             */
        }
        private Vector3 FindNonDominantElbowHintPosition()
        {
            Vector3 pos = Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position + SubjectTr.TransformVector(_weapon.Stats.NonDominantElbowOffset * CharScaleMultiplier);
#if UNITY_EDITOR
            Debug.DrawRay(pos, Vector3.up * 0.1f, Color.magenta);
            Debug.DrawRay(pos, Vector3.left * 0.1f, Color.magenta);
            Debug.DrawRay(pos, Vector3.right * 0.1f, Color.magenta);
            Debug.DrawRay(pos, Vector3.down * 0.1f, Color.magenta);
#endif
            return pos;
        }

        private Vector3 FindSpinePosition() { return Animator.GetBoneTransform(HumanBodyBones.Spine).position; }
        private Vector3 FindShoulderPosition() { return Animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position; }

        private Transform GetRightHandBone() { return Animator.GetBoneTransform(HumanBodyBones.RightHand); }
        private Transform GetLeftHandBone() { return Animator.GetBoneTransform(HumanBodyBones.LeftHand); }
        private static Quaternion GetDelta(Quaternion targetRotation, Quaternion currentRotation)
        {
            return targetRotation*Quaternion.Inverse(currentRotation);
        }

        // You can call these, but Mecanim requires it to come from OnAnimatorIK()
        // They'll be overidden anyway, so I need some sort of queue system for handling overrides that is analyzed during OnAnimatorIK()
        // The issue with Mecanim's rotation errors is holding back proper implementation of this as well.
        //
        // UPDATE: Mecanim's innacuracy can be adjusted in the Avatar Configuration, you can adjust individual bone rotation. I can't rely on it. 
        // UT has no intention of changing this workflow so we're stuck with overriding it with LateUpdate anyway until they realize the current workflow is useless (years, probably).
        // Likely FinalIK support will be in Deftly's future and this legacy Mecanim IK implementation will stay mostly as-is.
        //
        public void SetIkPositionWeight(AvatarIKGoal armature, float weight) { Animator.SetIKPositionWeight(armature, weight); }
        public void SetIkRotationWeight(AvatarIKGoal armature, float weight) { Animator.SetIKRotationWeight(armature, weight); }
        public void SetIkPosition(AvatarIKGoal armature, Vector3 position) { Animator.SetIKPosition(armature, position); }
        public void SetIkRotation(AvatarIKGoal armature, Quaternion rotation) { Animator.SetIKRotation(armature, rotation); }
        public void SetIkHintPosition(AvatarIKHint hint, Vector3 position) { Animator.SetIKHintPosition(hint, position);}
        public void SetIkHintWeight(AvatarIKHint hint, float weight) { Animator.SetIKHintPositionWeight(hint, weight);}
    }
}