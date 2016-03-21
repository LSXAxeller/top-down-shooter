// (c) Copyright Cleverous 2015. All rights reserved.

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Network;

namespace Deftly
{
    [AddComponentMenu("Deftly/Subject")]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Subject : NetworkedMonoBehavior
    {
        #region Variable Definitions
        // public variables
        public SubjectStats Stats = new SubjectStats();
        public PlayerControllerStats ControlStats = new PlayerControllerStats();

        public List<GameObject> WeaponListEditor;
        public List<GameObject> WeaponListRuntime; // the list of physical weapons
        public AudioClip SwapSound;

        public bool LogDebug = false;
        public GameObject WeaponRestPoint; // floating point created at runtime.
        [NetSync]
        public bool IsDead;
        public int LastDamage;
        public Subject LastAttacker;

        // subscribe to the events below for callbacks.
        public delegate void SwitchedWeapon(GameObject currentWeapon);
        public event SwitchedWeapon OnSwitchedWeapon;
        public delegate void Reload();
        public event Reload OnReload;
        public delegate void Fire(Subject whoFired); // potentially way too expensive. untested. use with caution.
        public event Fire OnFire;
        public delegate void Death();
        public event Death OnDeath;
        public delegate void Attacked();
        public event Attacked OnAttacked;
        public delegate void MagazineEmpty();
        public event MagazineEmpty OnMagazineEmpty;
        public delegate void HealthChanged();
        public event HealthChanged OnHealthChanged;
        public delegate void GetPowerup();
        public event GetPowerup OnGetPowerup;
        public delegate void LevelChange();
        public event LevelChange OnLevelChange;

        public bool UnlimitedAmmo;
        public bool GodMode;
        [NetSync]
        public bool InputPermission;

        // fixing IK stuff
        public bool ShowGunDebug;
        public bool InvertHandForward;
        public Vector3 ThumbDirection = new Vector3(0, 0, -1);
        public Vector3 PalmDirection = new Vector3(0, 1, 0);

        public Vector3 DominantHandPosCorrection = new Vector3(0,0,0);
        public Vector3 DominantHandRotCorrection = new Vector3(90, -90, 0);
        public Vector3 NonDominantHandPosCorrection = new Vector3(0,0,0);
        public Vector3 NonDominantHandRotCorrection = new Vector3(0,0,0);
        public Vector3 WeaponPositionInHandCorrection = new Vector3(0,0,0);

        public int RightHandIkLayer = 0;
        public int LeftHandIkLayer = 1;

        private Rigidbody2D _myRigidbody;
        private int _currentWeapon;
        private PlayerController _myControls;
        private bool _isControllable;
        private Transform _hand;
        private bool _switchingWeapons;
        private Intellect _intellect;
        private bool _initialized;
        [NetSync]
        public int Armor
        {
            get { return (int)Stats.Armor.Actual; }
            set
            {
                if (GodMode) return;
                Stats.Armor.Actual = value;
                if (LogDebug) Debug.Log("Armor After Damage: " + Health);
                if (Armor < Stats.Armor.Min) Stats.Armor.Actual = Stats.Armor.Min;
                if (Armor > Stats.Armor.Max) Stats.Armor.Actual = Stats.Armor.Max;
                if (OnHealthChanged != null) OnHealthChanged();
            }
        }
        [NetSync]
        public int Health
        {
            get { return (int)Stats.Health.Actual; }
            set
            {
                if (GodMode) return;
                Stats.Health.Actual = value;
                if (LogDebug) Debug.Log("Health After Damage: " + Health);
                if (Health <= Stats.Health.Min) Die();
                if (Health > Stats.Health.Max) Stats.Health.Actual = Stats.Health.Max;
                if (OnHealthChanged != null) OnHealthChanged();
                GUIManager.Instance.SetHealthBar(value, Stats.Health.Max);
            }
        }

        #endregion

        public static void ResetCharacterStatValues(SubjectStats group)
        {
            //                          Base    Min     Max     PerLvl  Actual
            group.Level =       new Stat(1,     0,      100,      1,        0 );
            group.Experience =  new Stat(0,     0,      50,       50,       0 );
            group.XpReward =    new Stat(25,    25,     100,      25,       25);

            group.Health =      new Stat(100,   15,     100,      8,        15);
            group.Armor =       new Stat(100,   0,      100,      1,        0 );
            group.Agility =     new Stat(0,     1,      100,      1.25f,    1 );
            group.Dexterity =   new Stat(0,     1,      100,      1.5f,     1 );
            group.Endurance =   new Stat(0,     1,      100,      2,        1 );
            group.Strength =    new Stat(0,     1,      100,      2.5f,     1 );
        }
        void Reset()
        {
            WeaponListEditor = new List<GameObject>();

            Stats.Title = "Peon";
            Stats.TeamId = 0;
            Stats.SubjectSkin = 0;
            Stats.HitSound = null;

            ResetCharacterStatValues(Stats);

            Stats.CrippledTime = 8.0f;
            Stats.CorpseTime = 15.0f;
            Stats.CharacterScale = 1f;
            
            ThumbDirection = new Vector3(0,0,-1);
            PalmDirection = new Vector3(0,1,0);
            DominantHandPosCorrection = new Vector3(0,0,0);
            DominantHandRotCorrection = new Vector3(90, -90, 0);
            NonDominantHandPosCorrection = new Vector3(0,0,0);
            NonDominantHandRotCorrection = new Vector3(0,0,0);
            WeaponPositionInHandCorrection = new Vector3(0,0,0);

            Stats.UseLeftHandIk = true;
            Stats.UseRightHandIk = true;
            Stats.UseWeaponIk = true;
            RightHandIkLayer = 0;
            LeftHandIkLayer = 1;

            GodMode = false;
            UnlimitedAmmo = false;

            ControlStats.AnimatorMoveSpeed = "MovementSpeed";
            ControlStats.AnimatorHorizontal = "horizontal";
            ControlStats.AnimatorVertical = "vertical";
            ControlStats.AnimatorDie = "die";
            ControlStats.AnimatorRevive = "revive";
            ControlStats.AnimatorReload = "reload";
            ControlStats.AnimatorSwap = "swap";
            ControlStats.AnimatorWeaponType = "IsMelee";
            ControlStats.TurnSpeed = 5;
            ControlStats.MoveSpeed = 5;
        }
        void Awake()
        {          
            _myRigidbody = GetComponent<Rigidbody2D>();
            InputPermission = true;
            IsDead = false;
            _myControls = GetComponent<PlayerController>();
            WeaponListRuntime = new List<GameObject>();

            if (Stats.SubjectGroup == SubjectGroup.Intellect) _intellect = GetComponent<Intellect>();
            if (_myControls != null) _isControllable = true;
        }
        
        void OnEnable()
        {
            if (Stats.SpawnFx)
            {
                StaticUtil.Spawn(Stats.SpawnFx, transform.position, transform.rotation);
            }

            // Pooling... ?
        }

        private void Start()
        {
            isPlayer = true;
            serializePosition = SerializeVector3Properties.XY;
            serializeRotation = SerializeVector3Properties.Z;
            serializeScale = SerializeVector3Properties.None;
            StartCoroutine(InputLoop());

            if ((int)Stats.Level.Actual == 0) StaticUtil.GiveXp((int)Stats.Experience.Max, this);

            // TODO Cleanup
            // Different (terrible) formatting to save vertical space... Was getting pointlessly long.
            _currentWeapon = 0;
            bool weaponSetupIsOkay = true;

            if (Stats.WeaponMountPoint != null)
            {
                _hand = Stats.WeaponMountPoint.transform;
            }
            else
            {
                weaponSetupIsOkay = false;
                if (WeaponListEditor.Count != 0)
                {
                    Debug.LogWarning("No Weapon Mount Point is specified! Assign it on the Subject.");
                }
            }

            // Initialize if there were no problems.
            if (!weaponSetupIsOkay) return;
            if (LogDebug)
            {
                Debug.Log(".... Weapon setup is okay");
            }

            // Create the predefined weapons, if any
            if (WeaponListEditor.Count == 0) return;
            foreach (GameObject boomboom in WeaponListEditor) PickupWeapon(boomboom);
            StartCoroutine(ChangeWeaponToSlot(0));
            _initialized = true;
        }

        private IEnumerator InputLoop()
        {
            if (GetComponent<Intellect>() != null) yield break; // this is only relevant for local Players
            else if (IsOwner == false) yield break;
            while (true)
            {
                while (!InputPermission) yield return null;

                if (!IsDead && _isControllable && WeaponListRuntime.Count > 0)
                {
                    float changeWeapon = _myControls.InputChangeWeapon;
                    if (changeWeapon > 0 && !_switchingWeapons) yield return StartCoroutine(ChangeWeaponToSlot(_currentWeapon + 1));
                    if (changeWeapon < 0 && !_switchingWeapons) yield return StartCoroutine(ChangeWeaponToSlot(_currentWeapon - 1));
                    while ((int) _myControls.InputChangeWeapon != 0) yield return null;
                    if (_myControls.GetInputDropWeapon && !_switchingWeapons) StartCoroutine(DropCurrentWeapon());
                }
                yield return null;
            }
        }

        public void LevelUp()
        {
            if (Stats.Level.Actual >= Stats.Level.Max) return;

            if (OnLevelChange != null) OnLevelChange();
            if (Stats.LevelUpFx) StaticUtil.Spawn(Stats.LevelUpFx, transform.position, Quaternion.identity);
            SetLevelAndStats((int)Stats.Level.Actual+1);
        }
        public void SetLevelAndStats(int level)
        {
            // Set the level and handle excess experience by looping here.
            int carryOverXp = (int)(Stats.Experience.Actual - Stats.Experience.Max);
            Stats.Experience.Actual = Stats.Experience.Min + carryOverXp;
            Stats.Level.Actual = level;
            Stats.Experience.Max = StaticUtil.StatShouldBe(Stats.Experience, (int)Stats.Level.Actual);
            if (Stats.Experience.Actual > Stats.Experience.Max) LevelUp();

            // The level is correct, now calculate all of the stats.
            Stats.XpReward.Actual = StaticUtil.StatShouldBe(Stats.XpReward, (int)Stats.Level.Actual);
            Stats.Health.Max = StaticUtil.StatShouldBe(Stats.Health, (int)Stats.Level.Actual);
            Stats.Armor.Max = StaticUtil.StatShouldBe(Stats.Armor, (int)Stats.Level.Actual);

            Stats.Agility.Actual = StaticUtil.StatShouldBe(Stats.Agility, (int)Stats.Level.Actual);
            Stats.Dexterity.Actual = StaticUtil.StatShouldBe(Stats.Dexterity, (int)Stats.Level.Actual);
            Stats.Endurance.Actual = StaticUtil.StatShouldBe(Stats.Endurance, (int)Stats.Level.Actual);
            Stats.Strength.Actual = StaticUtil.StatShouldBe(Stats.Strength, (int)Stats.Level.Actual);
        }

        /// <summary> 
        /// Starts the pickup sequence 
        /// </summary>
        public void PickupWeapon(GameObject obj)
        {
            if (LogDebug) Debug.Log("Picking up weapon " + obj.name);

            CreateNewWeapon(obj);
            if ((Options.Data.WeaponPickupAutoSwitch || WeaponListRuntime.Count == 1) && _initialized) StartCoroutine(ChangeWeaponToSlot(WeaponListRuntime.Count-1));
        }

        /// <summary> 
        /// Starts the pickup sequence 
        /// </summary>
        void CreateNewWeapon(GameObject weaponPrefab)
        {
            if (weaponPrefab == null) return;
            if (_hand == null)
            {
                Debug.LogError("Incorrect Rig Type on " + gameObject.name +"! Must be Humanoid. Check the Import Settings for this model and correct the type. Also confirm the Avatar Configuration has no errors.");
                return;
            }

            GameObject newToy = (GameObject) StaticUtil.Spawn(weaponPrefab, _hand.position, _hand.rotation);
            Weapon wx = newToy.GetComponent<Weapon>();
            wx.Owner = this;

            WeaponListRuntime.Add(newToy);
            newToy.transform.SetParent(gameObject.transform);
            newToy.SetActive(false);

            if (LogDebug) Debug.Log("Adding Weapon to Subject " + newToy.name + "...");
        }

        /// <summary> 
        /// Drops the current Weapon into the scene 
        /// </summary>
        public IEnumerator DropCurrentWeapon()
        {
            _switchingWeapons = true;
            if (LogDebug) Debug.Log("Dropping current weapon in slot " + _currentWeapon);
            if (GetCurrentWeaponGo() == null || GetCurrentWeaponComponent().PickupReference == null)
            {
                Debug.Log("Either there is no weapon equippped or the weapon doesn't have a Pickup Reference defined.");
                _switchingWeapons = false;
                yield break;
            }
            StaticUtil.SpawnLoot(WeaponListRuntime[_currentWeapon].GetComponent<Weapon>().PickupReference, transform.position, true);

            GameObject droppingThis = WeaponListRuntime[_currentWeapon];
            WeaponListRuntime.RemoveAt(_currentWeapon);
            if (WeaponListRuntime.Count == _currentWeapon) _currentWeapon -= 1;

            yield return StartCoroutine(ChangeWeaponToSlot(_currentWeapon));
            _switchingWeapons = true;
            
            StaticUtil.DeSpawn(droppingThis);
            while (_myControls.GetInputDropWeapon) yield return null;
            _switchingWeapons = false;
        }
        
        /// <summary> 
        /// Cycles directly to a specific weapon slot 
        /// </summary>
        public IEnumerator ChangeWeaponToSlot(int index)
        {
            _switchingWeapons = true;
            if (LogDebug)
            {
                Debug.Log("trying to switch weapon to slot " + index + " from "+_currentWeapon);
                Debug.Log("...Current Weapon Slot: " + _currentWeapon + " ... Weapon Count: " + WeaponListRuntime.Count);
            }
            if (WeaponListRuntime.Count == 0)
            {
                if (LogDebug) Debug.Log("No weapons left to switch to. Disabling IK, sending null switch.");
                _currentWeapon = 0;
                if (OnSwitchedWeapon != null) OnSwitchedWeapon(null);
                _switchingWeapons = false;
                yield break;
            }

            // turn off current weapon (old)
            GameObject oldWeapon = WeaponListRuntime[_currentWeapon];

            // 1. target is under the first weapon, go to last weapon.
            // 2. target is over the last weapon, go to first weapon.
            // 3. target is somewhere in between, go to the desired index.
            if (index < 0) _currentWeapon = WeaponListRuntime.Count - 1;
            else if (index >= WeaponListRuntime.Count) _currentWeapon = 0;
            else _currentWeapon = index;

            // tell everybody what just happened.
            if (OnSwitchedWeapon != null) OnSwitchedWeapon(WeaponListRuntime[_currentWeapon]);
            Weapon newWeapon = WeaponListRuntime[_currentWeapon].GetComponent<Weapon>();
           
            // halfway through the swap, flip them on/off
            yield return new WaitForSeconds(newWeapon.Stats.SwapTime / 2);
            oldWeapon.SetActive(false);
            newWeapon.gameObject.SetActive(true);
            yield return new WaitForSeconds(newWeapon.Stats.SwapTime / 2);
            
            // TODO blend IK into position

            _switchingWeapons = false;
        }

        /// <summary> 
        /// Begins the Subject's death. Subjects can be 'down-but-not-out' for a period of time, then completely die. 
        /// </summary>
        void Die()
        {
            // Deny controls, reset velocity, turn off ai, turn off collider
            if (_isControllable)
            {
                SetInputPermission(false, false, false);
            }

            _myRigidbody.velocity = Vector3.zero;
            _myRigidbody.angularVelocity = 0f;

            if (GetComponent<Intellect>() != null) GetComponent<Agent>().enabled = false;

            GetComponent<Collider>().enabled = false;
            
            if (Stats.DeathFx) StaticUtil.Spawn(Stats.DeathFx, transform.position, Quaternion.identity);
            if (OnDeath != null) OnDeath();

            // Update stats
            Stats.Deaths++;
            LastAttacker.Stats.Kills++;
            StaticUtil.GiveXp((int)Stats.XpReward.Actual, LastAttacker);
            //

            IsDead = true;
            StartCoroutine(CrippleAndDie());
        }

        /// <summary> 
        /// Timed period of downage (mostly dead). Could be revived. DeSpawn after timer ends - then totally dead. 
        /// </summary>
        private IEnumerator CrippleAndDie()
        {
            // Drop to a crippled state and wait.
            if (Stats.CrippledTime > 0) yield return new WaitForSeconds(Stats.CrippledTime);
            if (Stats.DeathFx) StaticUtil.Spawn(Stats.DeathFx, transform.position, transform.rotation);
            // Detatch the corpse model and give it an expiration timer.
            Stats.AnimatorHostObj.transform.SetParent(null);
            Lifetimer c = Stats.AnimatorHostObj.AddComponent<Lifetimer>();
            c.Lifetime = Stats.CorpseTime;

            DeSpawn();
        }

        /// <summary> 
        /// Inflict damage to this subject 
        /// </summary>
        public void DoDamage(int damage, Subject dealer)
        {
            if (IsDead) return;
            if (!Options.Data.FriendlyFire && StaticUtil.SameTeam(this, dealer)) return;

            LastAttacker = dealer;
            if (Stats.HitSound != null) AudioSource.PlayClipAtPoint(Stats.HitSound, transform.position);
            int damageProcessed = damage;

            #region #### Armor Calculation Block
            if (Armor > 0)
            {
                if (Stats.ArmorType == ArmorType.Absorb)
                {
                    Armor = Armor - damage; // Absorb damage
                    if (Armor < 0) // If any armor is left, count as excess
                    {
                        damageProcessed = -Armor;
                        Armor = 0;
                    }
                    else damageProcessed = 0;
                }
                else
                {
                    damageProcessed = damage - Armor; // Nullify damage
                    if (Armor < 0) // if any is left, count as excess
                    {
                        damageProcessed = -Armor;
                        Armor = 0;
                    }
                }
            }
            #endregion

            if (damageProcessed < 0) damageProcessed = 0;
            else if (damageProcessed > Stats.Health.Actual)
            {
                damageProcessed = (int)Stats.Health.Actual;
            }
            Health -= damageProcessed;
            LastDamage = damageProcessed;

            Stats.DamageTaken += damageProcessed;
            LastAttacker.Stats.DamageDealt += damageProcessed;
            LastAttacker.Stats.ShotsConnected++;

            StaticUtil.SpawnFloatingText(this, transform.position, damageProcessed.ToString());

            if (OnAttacked != null) OnAttacked();
        }

        /// <summary> 
        /// Broadcast Weapon has fired. 
        /// </summary>
        public void DoWeaponFire()
        {
            if (OnFire != null) OnFire(this);
        }

        /// <summary>
        /// Broadcast Weapon has reloaded
        /// </summary>
        public void DoWeaponReload()
        {
            if (UnlimitedAmmo)
            {
                Weapon weapon = GetCurrentWeaponComponent();
                weapon.Stats.CurrentMagazines = weapon.Stats.StartingMagazines;
            }
            if (OnReload != null) OnReload();
        }

        /// <summary>
        /// Broadcast Weapon magazine is empty.
        /// </summary>
        public void DoMagazineIsEmpty()
        {
            if (OnMagazineEmpty != null) OnMagazineEmpty();
        }

        /// <summary>
        /// Undeath! </summary>
        public void Revive(int newHealth)
        {
            if (!IsDead) return;

            Health = newHealth;
            if (GetComponent<Intellect>() != null)
                GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<CapsuleCollider>().enabled = true;

            if (GetCurrentWeaponGo() != null)
            {
                GetCurrentWeaponGo().transform.SetParent(transform);
            }
            
            IsDead = false;
        }

        /// <summary>
        /// Broadcast Subject has claimed a powerup.
        /// </summary>
        public void DoGrabPowerup()
        {
            if (OnGetPowerup != null) OnGetPowerup();
        }

        /// <summary>
        /// Returns the current weapon as a GameObject
        /// </summary>
        public GameObject GetCurrentWeaponGo()
        {
            if (WeaponListRuntime.Count == 0) return null;
            GameObject foo = WeaponListRuntime[_currentWeapon];
            return foo;
        }

        /// <summary>
        /// Returns the current weapon as a [Weapon] Component
        /// </summary>
        public Weapon GetCurrentWeaponComponent()
        {
            if (WeaponListRuntime.Count == 0) return null;
            Weapon foo = WeaponListRuntime[_currentWeapon].GetComponent<Weapon>();
            return foo;
        }

        /// <summary>
        /// Returns the weapon's projectile spawn point
        /// </summary>
        public GameObject GetCurrentWeaponSpawnPt()
        {
            if (WeaponListRuntime.Count == 0) return null;
            GameObject foo = WeaponListRuntime[_currentWeapon].GetComponent<Weapon>().Stats.ProjectileSpawnPoints[0];
            return foo;
        }

        /// <summary>
        /// Returns the Subject's [PlayerController] component
        /// </summary>
        public PlayerController GetController()
        {
            if (!_isControllable) return null;
            return _myControls;
        }

        public Intellect GetIntellect()
        {
            return _intellect ? _intellect : null;
        }

        /// <summary>
        /// Defines the permissions of a Subject's interaction. Cycle Weapons, Move, Fire.
        /// </summary>
        public void SetInputPermission(bool allowWeaponCycling, bool allowMovement, bool allowFiring)
        {
            InputPermission = allowWeaponCycling;
            if (_myControls != null) _myControls.InputPermission = allowMovement;
            if (GetCurrentWeaponComponent() != null) GetCurrentWeaponComponent().InputPermission = allowFiring;
        }

        public void Spawn()
        {
            // Pooling TBD
            gameObject.SetActive(true);
        }
        public void DeSpawn()
        {
            // Pooling TBD
            StaticUtil.DeSpawn(gameObject);
        }
    }
}