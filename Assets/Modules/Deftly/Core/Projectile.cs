// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deftly
{
    [AddComponentMenu("Deftly/Projectile")]
    public class Projectile : MonoBehaviour
    {
        public ProjectileStats Stats;
        public LayerMask Mask;

        public GameObject AttackEffect;

        public List<string> ImpactTagNames = new List<string>();
        public List<AudioClip> ImpactSounds = new List<AudioClip>();
        public List<GameObject> ImpactEffects = new List<GameObject>();

        public enum ImpactType { ReflectOffHit, HitPointNormal, InLineWithShot }
        public ImpactType ImpactStyle = ImpactType.InLineWithShot;

        public Subject Owner;
        public GameObject DetachOnDestroy;

        private GameObject _go;
        private Vector3 _startPoint;
        private Vector3 _endPoint;
        private Vector3 _endNormal;
        private Subject _victim;
        private GameObject _victimGo;

        private List<Collider2D> _myColliders;
        private bool _despawning;
        private bool _firstRun = true;
        public bool LogDebug = false;

        void Awake()
        {
            if (_firstRun || Owner.IsDead) return;
            _despawning = false;
            // IgnoreCollision() kindly resets itself after being deactivated+reactivated... Safe for pooling. =]
            foreach (Collider2D c in _myColliders)
            {
                if (c != null) Physics2D.IgnoreCollision(c, Owner.GetComponent<Collider2D>());
            }

            Lifetimer.AddTimer(gameObject, Stats.Lifetime, true);
            Fire(_go.transform.position);
        }
        void Reset()
        {
            Stats = new ProjectileStats
            {
                Title = "Pewpew",
                weaponType = ProjectileStats.WeaponType.Standard,
                LineRenderer = gameObject.GetComponent<LineRenderer>(),
                Speed = 40f,
                Damage = 10,
                MaxDistance = 10f,
                Lifetime = 4f,
                Bouncer = false,
                UsePhysics = true,
                ConstantForce = true,
                CauseAoeDamage = false,
                AoeRadius = 5,
                AoeForce = 50
            };
            ImpactSounds = new List<AudioClip>();
            AttackEffect = null;
            ImpactEffects = new List<GameObject>();
        }
        void Start()
        {
            _myColliders = GetComponentsInChildren<Collider2D>().ToList();
            _go = gameObject;
            _firstRun = false;
            Awake();
        }
        void OnCollisionEnter2D(Collision2D col) // Handles hits for Standard Type.
        {
            if (!Stats.CauseAoeDamage) // I cause damage to what I collided into.
            {
                _victimGo = col.gameObject;
                _victim = _victimGo.GetComponent<Subject>();

                if (StaticUtil.LayerMatchTest(Mask, _victimGo))
                {
                    if (_victim != null) DoDamageToVictim();

                    _endPoint = col.contacts[0].point;
                    SetupImpactNormal(col.contacts[0].normal);
                    PopFx(GetCorrectFx(col.collider.gameObject));
                    FinishImpact();
                }
                else
                {
                    foreach (Collider2D z in _myColliders) Physics2D.IgnoreCollision(z, col.collider);
                }
            }
            else if (Stats.CauseAoeDamage && !Stats.Bouncer) DoDamageAoe(); // I cause AoE immediately when I hit something.
        }

        private void Fire(Vector3 fromPos)
        {
            _startPoint = fromPos;
            DoMuzzleFlash();

            #region Standard Type
            if (Stats.weaponType == ProjectileStats.WeaponType.Standard)
            {
                Mover.AddMover(gameObject, Stats.UsePhysics, Stats.Speed, Stats.ConstantForce, Owner, Mask);
            }
            #endregion

            #region Raycast Type
            // TODO build solution: How should Raycast type work? Ouput start/end for a 3rd party script? Include elaborate trail system?
            if (Stats.weaponType == ProjectileStats.WeaponType.Raycast)
            {
                Vector3 dir = _go.transform.TransformDirection(Vector3.right);

                RaycastHit2D hit = Physics2D.Raycast(_startPoint, dir, Stats.MaxDistance, Mask);
                if (hit.collider != null)
                {
                    // This is a hit.
                    _victimGo = hit.collider.gameObject;
                    _victim = _victimGo.GetComponent<Subject>();
                    if (_victim != null) DoDamageToVictim(); // only Subject's can be damaged.

                    _endNormal = hit.normal;
                    _endPoint = hit.point;
                    _endPoint.z = 0;
                    if (LogDebug)
                    {
                        Debug.Log(name + " registered a Hit on " + _victimGo.name);
                    }
                }
                else
                {
                    // This is a miss.
                    _endPoint = _startPoint + dir*Stats.MaxDistance;
                    _endPoint.z = 0;
                    Owner.Stats.ShotsMissed++;
                    if (LogDebug)
                    {
                        Debug.Log(name + " registered a Miss");
                    }
                }

                DrawRayFx();
                FinishImpact();
            }
            #endregion
        }
        private void DoDamageToVictim()
        {
            _victim.DoDamage(Stats.Damage, Owner);
            Owner.Stats.DamageDealt += Stats.Damage;
        }
        private void DoDamageAoe()
        {
            // could use foo.SendMessage, but it is sloppy... Rather pay for GetComponent instead.
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, Stats.AoeRadius, Vector2.up, 0.1f, Mask);
            foreach (RaycastHit2D thisHit in hits)
            {
                _victimGo = thisHit.collider.gameObject;
                _victim = _victimGo.GetComponent<Subject>();

                if (Stats.AoeForce > 0)
                {
                   Rigidbody2D rb = _victimGo.GetComponent<Rigidbody2D>();
                    Vector3 dir = (rb.transform.position - transform.position);
                    float wearoff = 1 - (dir.magnitude / Stats.AoeRadius);
                    if (rb != null) rb.AddForce(dir.normalized * Stats.AoeForce * wearoff);
                }

                if (_victim != null)
                {
                    _victim.DoDamage(Stats.Damage, Owner);
                    Owner.Stats.DamageDealt += Stats.Damage;
                }

                // TODO Hit FX
                // Hit FX per AoE contact not yet working.
                //
                // _endPoint = thisHit.point;
                // SetupImpactNormal(thisHit.normal);
                // PopFx(GetCorrectFx(thisHit.collider.gameObject));

                FinishImpact();
            }

            if (Stats.AoeEffect != null) StaticUtil.Spawn(Stats.AoeEffect, transform.position, Quaternion.identity);
        }

        private void DoMuzzleFlash()
        {
            // TODO should muzzle flash be on the projectile or the weapon? Poll users for suggestions.
            StaticUtil.Spawn(AttackEffect, transform.position, transform.rotation);
        }
        private void DrawRayFx() // TODO decide how how handle the Raycast Type's behavior.
        {
            LineRenderer line = GetComponent<LineRenderer>();
            if (line == null) return;

            Stats.LineRenderer.SetPosition(0, _startPoint);
            Stats.LineRenderer.SetPosition(1, _endPoint);
        }

        private void SetupImpactNormal(Vector3 hitNormal)
        {
            switch (ImpactStyle)
            {
                case (ImpactType.InLineWithShot):
                    _endNormal = -transform.right;                                                // standard or raycast
                    break;
                case (ImpactType.HitPointNormal):
                    _endNormal = (Stats.weaponType == ProjectileStats.WeaponType.Standard)
                        ? hitNormal                                                                 // standard
                        : _endNormal;                                                               // raycast
                    break;
                case (ImpactType.ReflectOffHit):
                    _endNormal = (Stats.weaponType == ProjectileStats.WeaponType.Standard)
                        ? Vector3.Reflect(transform.right, hitNormal)                             // standard
                        : Vector3.Reflect(transform.right, _endNormal);                           // raycast
                    break;
            }
        }
        private int GetCorrectFx(GameObject victim)
        {
            if (ImpactEffects.Count <= 1 || victim == null) return 0;
            for (int i = 0; i < ImpactTagNames.Count; i++)
            {
                if (victim.CompareTag(ImpactTagNames[i])) return i;
            }
            return 0;
        }
        private void PopFx(int index)
        {
            if (ImpactSounds[index] != null) AudioSource.PlayClipAtPoint(ImpactSounds[index], _endPoint);
            else Debug.LogWarning(gameObject.name + " cannot spawn Impact sound because it is null. Check the Impact Tag List.");

            if (ImpactEffects[index] != null) StaticUtil.Spawn(ImpactEffects[index], _endPoint, Quaternion.LookRotation(_endNormal));
            else Debug.LogWarning(gameObject.name + " cannot spawn Impact effect because it is null. Check the Impact Tag List.");
        }
        private void FinishImpact()
        {
            // TODO how is this supposed to work with pooling? Do I need to insanely nest pool the detachable? :S
            if (DetachOnDestroy != null) DetachOnDestroy.transform.SetParent(null);

            // Only for Standard Type, relying on Lifetimer to despawn Raycast Type. (because trails, etc..)
            if (Stats.weaponType == ProjectileStats.WeaponType.Standard)
            {
                DeSpawn();
            }
        }

        public void Spawn()
        {
            // Pooling TBD
            gameObject.SetActive(true);
        }
        public void DeSpawn()
        {
            // using DeSpawn() to apply aoe dmg?... not sure if that is okay...
            if (Stats.CauseAoeDamage && Stats.Bouncer && !_despawning)
            {
                _despawning = true;
                DoDamageAoe();
            }
            StaticUtil.DeSpawn(gameObject);
        }
    }
}