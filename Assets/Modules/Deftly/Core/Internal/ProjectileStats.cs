// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    [System.Serializable]
    public class ProjectileStats
    {
        public enum ProjectileLocomotion {Translation, Physics}
        public enum WeaponType {Standard, Raycast}

        [SerializeField] public string Title;
        [SerializeField] public WeaponType weaponType = WeaponType.Standard;
        [SerializeField] public LineRenderer LineRenderer;
        [SerializeField] public float Speed; // Raycast type ignores this.
        [SerializeField] public int Damage;
        [SerializeField] public float MaxDistance;
        [SerializeField] public float Lifetime;
        [SerializeField] public bool Bouncer;
        [SerializeField] public bool UsePhysics;
        [SerializeField] public bool ConstantForce;
        [SerializeField] public ProjectileLocomotion MoveStyle = ProjectileLocomotion.Physics;
        [SerializeField] public bool CauseAoeDamage;
        [SerializeField] public GameObject AoeEffect;
        [SerializeField] public float AoeRadius;
        [SerializeField] public float AoeForce;
    }
}