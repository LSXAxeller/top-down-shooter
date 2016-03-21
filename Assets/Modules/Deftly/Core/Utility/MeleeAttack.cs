// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    public class MeleeAttack : MonoBehaviour
    {
        // TODO Input Reference Fix
        public int Damage;
        public float Cooldown;
        public bool IsVeryFast;
        public float TriggerStartAt;
        public float TriggerEndAt;
        public string InputReference;
        public string AnimatorTriggerName;
        public float AnimatonSpeed;
        public string AnimatorAttackSpeed;

        public void Reset()
        {
            Damage = 0;
            Cooldown = 0.0f;
            IsVeryFast = false;
            TriggerStartAt = 0.2f;
            TriggerEndAt = 0.8f;
            InputReference = "Fire1";
            AnimatorTriggerName = "Melee1";
            AnimatonSpeed = 1f;
            AnimatorAttackSpeed = "AttackSpeed";
        }
    }
}