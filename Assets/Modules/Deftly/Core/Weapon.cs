// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Deftly
{
    [AddComponentMenu("Deftly/Weapon")]
    public class Weapon : MonoBehaviour
    {
        #region Variable Definitions

        public WeaponStats Stats;
        public GameObject[] ProjectileSpawnPoints;
        public List<AudioClip> FireSounds = new List<AudioClip>();

        // melee only
        public List<GameObject> AttackPrefabs;
        public List<MeleeAttack> Attacks;
        public List<string> ImpactTagNames = new List<string>();
        public List<AudioClip> ImpactSounds = new List<AudioClip>();
        public List<GameObject> ImpactEffects = new List<GameObject>();
        public LayerMask Mask;
        //

        public GameObject PickupReference;
        public Subject Owner;
        private PlayerController _thisUsersControls;
        private ParticleSystem _ejector;
 
        private int _nextSpawnPt;
        public bool IsReloading;

        public bool Attacking;
        public bool Attacking2;
        public bool DoingMeleeSwing;
        private MeleeAttack _thisAttack;

        private bool _thisIsPlayer;
        private int _currentAttack;

        private Collider _myCollider;
        private List<Collider> _hitsDuringThisSwing;
        private GameObject _victimGameObject;
        private Subject _victimSubject;
        private string _firingInputName = "Fire1";
        public bool InputPermission = true;

        #endregion

        void Reset()
        {
            AttackPrefabs = new List<GameObject>();
            FireSounds = new List<AudioClip>();

            ImpactSounds = new List<AudioClip>();
            ImpactEffects = new List<GameObject>();

            Stats = new WeaponStats
            {
                Title = "Derpzooka",
                WeaponType = WeaponType.Ranged,
                FireStyle = FireStyle.FullAuto,

                FireSound = null,
                FiresProjectile = null,
                ProjectileSpawnPoints = new List<GameObject>(),

                PositionOffset = new Vector3(0f, 0f, 0.5f),
                NonDominantHandPos = new Vector3(0f, 0f, 0f),
                NonDominantHandRot = new Vector3(0f, 0f, 0f),
                NonDominantElbowOffset = new Vector3(0,-0.1f,0.1f),
                DominantElbowOffset = new Vector3(0, -0.1f, 0.1f),

                NoAmmoSound = null,
                ReloadSound = null,
                AmmoCost = 1,
                StartingMagazines = 8,
                MagazineSize = 6,
                CurrentMagazines = 8,
                CurrentAmmo = 6, // TODO add editor field for Current Ammo
                TimeToCooldown = 0.2f,
                Accuracy = 1.0f,

                CanHitMultiples = true,
                CanAttackAndMove = false,
                WeaponTrail = null
            };
        }
        void Awake()
        {
            _thisAttack = null;
            Attacks = new List<MeleeAttack>();
            _hitsDuringThisSwing = new List<Collider>();
            _myCollider = GetComponent<Collider>(); // disabled for Melee in Start()

            foreach (GameObject go in AttackPrefabs)
            {
                Attacks.Add(go.GetComponent<MeleeAttack>());
            }
            Stats.CurrentAmmo = Stats.MagazineSize;
        }
        void OnEnable()
        {
            _currentAttack = 0;
            _nextSpawnPt = 0;
            Attacking = false;

            while (Owner == null) return;
            if (Owner.Stats.SubjectGroup == SubjectGroup.Player)
            {
                _thisUsersControls = Owner.GetComponent<PlayerController>();
                if (_thisUsersControls != null) _thisIsPlayer = true;
                else Debug.LogWarning(this + " could not initialize! Subject is Player, but is missing a PlayerController script for input relays.");
            }
            else { _thisIsPlayer = false; }
            
            _ejector = Stats.AmmoEjector;

            // Start the correct control loop for this Weapon Type.
            StartCoroutine(Stats.WeaponType == WeaponType.Ranged
                ? WeaponLoopRanged()
                : WeaponLoopMelee());
        }
        void Start()
        {
            if (Stats.WeaponType == WeaponType.Melee)
            {
                Physics.IgnoreCollision(Owner.GetComponent<Collider>(), GetComponent<Collider>());
                _myCollider.enabled = false;

                if (Stats.WeaponTrail != null)
                {
                    Stats.WeaponTrail.SetActive(false);
                }
            }
        }

        // Ranged
        private IEnumerator WeaponLoopRanged()
        {
            while (true)
            {
                while (IsReloading || Owner == null || !InputPermission) yield return null;
                if (_thisIsPlayer)
                {
                    if (Owner != null && !Owner.IsDead && _thisUsersControls.GetInputReload) yield return StartCoroutine(Reload());
                    Attacking = (_thisUsersControls.GetInputFire1 > 0);
                }

                if (Attacking) yield return StartCoroutine(FireRanged());
                yield return null;
            }
        }
        private IEnumerator FireRanged()
        {
            if (Stats.CurrentAmmo >= Stats.AmmoCost)
            {
                DoFireEffect();

                Vector3 pos = Stats.ProjectileSpawnPoints[_nextSpawnPt].transform.position;
                Quaternion rot = Stats.ProjectileSpawnPoints[_nextSpawnPt].transform.rotation;

                if (Stats.Accuracy < 1.0f)
                {
                    float a = Mathf.Clamp(Stats.Accuracy, 0, 1) - 1;
                    rot.x += Random.Range(-0.2f*a, 0.2f*a);
                    rot.y += Random.Range(-0.2f*a, 0.2f*a);
                    rot.z += Random.Range(-0.2f*a, 0.2f*a);
                }

                GameObject thisBullet = StaticUtil.Spawn(Stats.FiresProjectile, pos, rot) as GameObject;
                if (thisBullet != null)
                    thisBullet.GetComponent<Projectile>().Owner = Owner; // assign the bullet owner, for points, etc.

                if (_ejector != null) _ejector.Emit(1); // if there is a shell casing ejector, use it.
                if (Stats.ProjectileSpawnPoints.Count > 1) // if using multiple spawn points, iterate through them.
                {
                    _nextSpawnPt++;
                    if (_nextSpawnPt > Stats.ProjectileSpawnPoints.Count - 1)
                    {
                        _nextSpawnPt = 0;
                    }
                }

                Stats.CurrentAmmo -= Stats.AmmoCost;

                Owner.DoWeaponFire();
                yield return StartCoroutine(FireCooldown());
                yield break;
            }

            if (MagIsEmpty() && Stats.AutoReload)
            {
                yield return StartCoroutine(Reload());
                yield break;
            }

            // the IFs failed [and did not yield break], so this code is reached and we are indeed out of ammo.
            yield return StartCoroutine(NoAmmo());
        }

        private IEnumerator NoAmmo()
        {
            Owner.DoMagazineIsEmpty();
            if (Stats.NoAmmoSound != null)
            {
                AudioSource.PlayClipAtPoint(Stats.NoAmmoSound, transform.position);
            }

            yield return StartCoroutine(FireCooldown());
        }
        public IEnumerator Reload()
        {
            if (HasMags() && !MagIsFull())
            {
                Stats.CurrentMagazines--;
                Stats.CurrentAmmo = Stats.MagazineSize;
                Owner.DoWeaponReload();

                if (Stats.ReloadSound != null)
                {
                    AudioSource.PlayClipAtPoint(Stats.ReloadSound, transform.position);
                }
                yield return StartCoroutine(ReloadCooldown());
            }
            else
            {
                yield return StartCoroutine(NoAmmo());
                yield return StartCoroutine(FireCooldown());
                
            }
        }
        private IEnumerator ReloadCooldown()
        {
            IsReloading = true;
            yield return new WaitForSeconds(Stats.ReloadTime);
            IsReloading = false;
        }

        // Melee
        void OnTriggerEnter(Collider theColliderWeHit)
        {
            // Add hits to a list to prevent damage stacking in the same swing. 
            // Empty the list when swing finishes.
            if (_hitsDuringThisSwing.Contains(theColliderWeHit))
            {
                return;
            }

            _victimGameObject = theColliderWeHit.gameObject;
            _victimSubject = _victimGameObject.GetComponent<Subject>();

            Vector3 impactPos = theColliderWeHit.transform.position + Vector3.up; // would be nice to get more accurate with this.

            // if what we hit is on a layer in our mask, plow into it.
            if (!StaticUtil.LayerMatchTest(Mask, _victimGameObject)) return;
            if (_victimSubject != null) DoMeleeDamage(theColliderWeHit.GetComponent<Subject>());

            DoMeleeDamage(theColliderWeHit.GetComponent<Subject>());
            DoImpactEffects(impactPos);
            _hitsDuringThisSwing.Add(theColliderWeHit);
        }
        private IEnumerator WeaponLoopMelee()
        {
            while (true)
            {
                if (Owner == null || !InputPermission) yield return null;
                if (_thisIsPlayer)
                {
                    if (Stats.FireStyle == FireStyle.SemiAuto)
                    {
                        while (_thisUsersControls.GetInputFire1 > 0.1f || _thisUsersControls.GetInputFire2 > 0.1f) yield return null;
                    }

                    Attacking = _thisUsersControls.GetInputFire1 > 0.1f;
                    Attacking2 = _thisUsersControls.GetInputFire2 > 0.1f;
                }

                if (Attacking || Attacking2)
                {
                    _firingInputName = Attacking ? "Fire1" : "Fire2";
                    if (DoingMeleeSwing) break;

                    DoingMeleeSwing = true;
                    yield return StartCoroutine(FireMelee(_firingInputName));
                    DoingMeleeSwing = false;
                }

                Attacking = false;
                Attacking2 = false;
                yield return null;
            }
        }
        private IEnumerator FireMelee(string controlInput)
        {
            GetMeleeAttackByInput(controlInput);

            MeleeStart(); // start

            MeleeTriggerToggle(false);

            MeleeCleanup(); // finish

            if (_thisAttack.Cooldown > 0) yield return StartCoroutine(FireCooldown());
            yield return null;
        }
        private void GetMeleeAttackByInput(string input)
        {
            ///// Find a match for the inputs
            for (int i = 0; i < Attacks.Count; i++)
            {
                if (Attacks[i].InputReference == input)
                {
                    _currentAttack = i;
                    break;
                }
            }

            _thisAttack = Attacks[_currentAttack];
        }
        private void MeleeStart()
        {
            ///// Deny Weapon Cycling and/or Movement
            Owner.SetInputPermission(false, Stats.CanAttackAndMove, true);

            if (Stats.WeaponTrail != null)
            {
                Stats.WeaponTrail.SetActive(true);
            }

            DoFireEffect();
            Owner.DoWeaponFire();
        }
        private void MeleeTriggerToggle(bool status)
        {
            _myCollider.enabled = status;
        }
        private void MeleeCleanup()
        {
            _hitsDuringThisSwing.Clear();
            Owner.SetInputPermission(true, !Stats.CanAttackAndMove, true);

            if (Stats.CanAttackAndMove && _thisUsersControls != null) 
                _thisUsersControls.InputPermission = true;
            if (Stats.WeaponTrail != null) 
                Stats.WeaponTrail.SetActive(false);
        }
        private void DoMeleeDamage(Subject toThis)
        {
            if (toThis == null) return;
            toThis.DoDamage(Attacks[_currentAttack].Damage, Owner);
        }

        // Shared
        private IEnumerator FireCooldown()
        {
            if (Stats.WeaponType == WeaponType.Melee) yield return new WaitForSeconds(Attacks[_currentAttack].Cooldown);
            else
            {
                yield return new WaitForSeconds(Stats.TimeToCooldown);
                if (Stats.FireStyle == FireStyle.SemiAuto)
                {
                    if (_thisUsersControls == null) yield break;
                    while (_thisUsersControls.GetInputFire1 > 0) yield return null;
                }
            }
        }
        private void DoFireEffect()
        {
            if (FireSounds.Count <= 0) return;
            int rng = FireSounds.Count > 0
                ? Random.Range(0, FireSounds.Count)
                : 0;
            if (FireSounds[rng] != null)
            {
                AudioSource.PlayClipAtPoint(FireSounds[rng], transform.position);
            }
        }
        private void DoImpactEffects(Vector3 position)
        {
            if (ImpactEffects.Count > 1)
            {
                if (_victimGameObject == null) return;

                for (int i = 0; i < ImpactTagNames.Count; i++)
                {
                    // check if the tag matches
                    if (_victimGameObject.CompareTag(ImpactTagNames[i]))
                    {
                        // if it does, we're done here.
                        PopFx(i, position);
                        break;
                    }

                    // if its the last entry and no match was found yet, default to the first entry.
                    if (i == ImpactEffects.Count) PopFx(0, position);
                }
            }
            else PopFx(0, position);
        }
        private void PopFx(int entry, Vector3 position)
        {
            if (ImpactSounds[entry] != null) AudioSource.PlayClipAtPoint(ImpactSounds[entry], position);
            else Debug.LogWarning(gameObject.name + " cannot spawn Impact sound because it is null. Check the Impact Tag List.");

            if (ImpactEffects[entry] != null) StaticUtil.Spawn(ImpactEffects[entry], position, Quaternion.identity);
            else Debug.LogWarning(gameObject.name + " cannot spawn Impact effect because it is null. Check the Impact Tag List.");
        }


        // Public
        public bool MagIsFull()
        {
            return Stats.CurrentAmmo >= Stats.MagazineSize;
        }
        public bool MagIsEmpty()
        {
            return Stats.CurrentAmmo <= 0;
        }
        public bool HasMags()
        {
            return Stats.CurrentMagazines > 0;
        }
    }
}