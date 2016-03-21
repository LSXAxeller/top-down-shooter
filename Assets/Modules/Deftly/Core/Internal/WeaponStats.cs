// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Deftly
{        
    public enum FireStyle { FullAuto, SemiAuto };
    public enum MountPivot { LowerSpine, Shoulder };
    public enum WeaponType { Ranged, Melee };
    public enum Hand { Right, Left };

    [System.Serializable]
    public class WeaponStats
    {
        // BASIC
        [SerializeField] public string Title;
        [SerializeField] public WeaponType WeaponType = WeaponType.Ranged;
        [SerializeField] public Sprite UiImage;
        [SerializeField] public FireStyle FireStyle = FireStyle.FullAuto;
        [SerializeField] public float EffectiveRange;
        [SerializeField] public float SwapTime;
        [SerializeField] public int TypeId;

        // IK
        [SerializeField] public Transform NonDominantHandGoal;
        [SerializeField] public Vector3 NonDominantHandPos;
        [SerializeField] public Vector3 NonDominantHandRot;
        [SerializeField] public Transform WeaponInHandOffset;
        [SerializeField] public Vector3 NonDominantElbowOffset;
        [SerializeField] public Vector3 DominantElbowOffset;
        [SerializeField] public bool UseElbowHintL;
        [SerializeField] public bool UseElbowHintR;

        [SerializeField] public MountPivot MountPivot;
        [SerializeField] public Vector3 PositionOffset;
        [SerializeField] public Vector3 RotationOffset;
        [SerializeField] public Hand WeaponHeldInHand;

        // PROJECTILES
        [SerializeField] public AudioClip FireSound;
        [SerializeField] public GameObject FiresProjectile;
        [SerializeField] public List<GameObject> ProjectileSpawnPoints;
        
        // AMMO
        [SerializeField] public AudioClip NoAmmoSound;
        [SerializeField] public AudioClip ReloadSound;
        [SerializeField] public ParticleSystem AmmoEjector;
        [SerializeField] public float ReloadTime;
        [SerializeField] public int AmmoCost;
        [SerializeField] public int StartingMagazines;
        [SerializeField] public int MagazineSize;
        [SerializeField] public int CurrentMagazines;
        [SerializeField] public int CurrentAmmo;
        [SerializeField] public bool AutoReload;

        [SerializeField] public float TimeToCooldown;
        [SerializeField] public float Accuracy;

        // MELEE ONLY
        [SerializeField] public bool CanHitMultiples;
        [SerializeField] public bool CanAttackAndMove;
        [SerializeField] public GameObject WeaponTrail;
    }
}