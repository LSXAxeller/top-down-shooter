// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deftly
{
    [AddComponentMenu("Deftly/Intellect")]
    [RequireComponent(typeof(Subject))]
    [RequireComponent(typeof(Agent))]
    public class Intellect : MonoBehaviour
    {
        #region ### Variable Definitions

        // DATA STUFF
        public bool LogDebug;
        public bool DrawGizmos;
        public int IgnoreUpdates; // how many *Frames* to skip before running the **Process**.
        public int SenseFrequency; // how many **Processes** to skip between sensory updates.

        // PROVOKING AND ALLIES
        public enum ProvokeType { TargetIsInRange, TargetAttackedMe }
        public enum ThreatPriority { Nearest, MostDamage }
        public ProvokeType MyProvokeType;
        public ThreatPriority MyThreatPriority;
        public float ProvokeDelay;
        public bool HelpAllies;
        public int MaxAllyCount = 10;
        public bool Provoked;
        public int FleeHealthThreshold;
        public float AlertTime;

        public int RetargetDamageThreshold;

        // JUKE SETUP
        public float JukeTime;
        public float JukeFrequency;
        public float JukeFrequencyRandomness;
        private bool _juking;
        private Vector3 _jukeHeading;

        // RANGES AND MASKS
        public float FieldOfView;
        public float SightRange;
        public float PursueRange;
        public float AttackRange;
        public float WanderRange;
        public float EngageThreshold;
        public LayerMask SightMask;
        public LayerMask ThreatMask;

        // UNIMPLEMENTED
        public List<GameObject> PatrolPoints;

        // NAVIGATION
        public float MaxDeviation;
        internal Agent Agent;
        public string AnimatorDirection;
        public string AnimatorSpeed;

        // LIVE PUBLIC VARIABLES
        public Dictionary<Subject, int> ThreatList;
        public List<Subject> AllyList;
        public Subject Target;
        public enum Mood { Idle, Patrol, Wander, Flee, Alert, Combat, Dead }

        // PRIVATE VARIABLES USED/CACHED INTERNALLY
        private GameObject _go;
        private bool _hasWeapons;
        private Weapon _weapon;
        private float _weaponSpeed;
        private bool _needToReload;
        private Transform _startTransform;
        private float _distanceToTarget;
        private bool _onPatrol;
        private bool _waiting;
        private int _scanClock;
        private static float _editorGizmoSpin;
        private Vector3 _victimLastPos;
        private Rigidbody2D _rb;
        private CircleCollider2D _collider;
        private Vector3 _myCurPos;

        protected bool NoAmmoLeft;
        protected Subject ThisSubject;
        protected bool Fleeing;
        protected bool MoodSwitch;
        protected Mood MyMood;
        protected Mood MyLastMood;
        private UnityTileMap.TileMapBehaviour _map;
        private bool _patrolling;
        private int _curPatrol;
        #endregion

        // Init and Editor stuff
        void Reset()
        {
            LogDebug = false;
            IgnoreUpdates = 2;
            SenseFrequency = 2;
            
            MyProvokeType = ProvokeType.TargetIsInRange;
            ProvokeDelay = 1f;
            HelpAllies = true;

            JukeTime = 2f;
            JukeFrequency = 0f;
            JukeFrequencyRandomness = 1f;

            FieldOfView = 30f;
            SightRange = 12f;
            PursueRange = 10f;
            WanderRange = 3f;
            EngageThreshold = 1f;

            FleeHealthThreshold = 0;

            SightMask = -1;
            ThreatMask = -1;

            MaxDeviation = 15f;
            AlertTime = 3f;

            AnimatorSpeed = "speed";
            AnimatorDirection = "direction";

            MaxAllyCount = 10;
        }
        void OnEnable()
        {
            ThreatList = new Dictionary<Subject, int>();
            AllyList = new List<Subject>();
        }
        void Awake()
        {
            _map = FindObjectOfType<UnityTileMap.TileMapBehaviour>();
            _rb = GetComponent<Rigidbody2D>();
            _rb.angularDrag = 1f;
            _rb.drag = 1f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            //_rb.mass = 1f;
            //_rb.constraints = (RigidbodyConstraints) 84;

            _startTransform = transform;
            ThreatList = new Dictionary<Subject, int>();
            AllyList =  new List<Subject>();
            ThisSubject = GetComponent<Subject>();
            Agent = AgentGetComponent;
        }
        void Start()
        {
            _go = gameObject;
            _collider = GetComponent<CircleCollider2D>();

            //_curPatrol = 0;
            CheckIfHasWeapons();

            Fleeing = false;
            MyMood = Mood.Combat;
            
            ThisSubject.OnDeath += Die;
            ThisSubject.OnAttacked += Attacked;
            ThisSubject.OnSwitchedWeapon += UpdateWeapon;
            CheckIfHasWeapons();
            if (_hasWeapons) UpdateWeapon(ThisSubject.GetCurrentWeaponGo());

            StartCoroutine(StateMachine());
        }

        public void CheckIfHasWeapons()
        {
            _hasWeapons = (ThisSubject.WeaponListRuntime.Count > 0);
        }

        void OnDrawGizmos()
        {
            _editorGizmoSpin+=0.02f;
            if (_editorGizmoSpin > 360) _editorGizmoSpin = 0;
        }
        void OnDrawGizmosSelected()
        {
            if (!DrawGizmos) return;
            // SIGHT range
            Gizmos.color = Color.grey;
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin)*new Vector3(0, SightRange, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin)*new Vector3(0, -SightRange, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin)*new Vector3(SightRange, 0, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin)*new Vector3(-SightRange, 0, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin + 45)*new Vector3(0, SightRange, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin + 45)*new Vector3(0, -SightRange, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin + 45)*new Vector3(SightRange, 0, 0));
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, _editorGizmoSpin + 45)*new Vector3(-SightRange, 0, 0));
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, SightRange);

            // ATTACK range
            // Gizmos.color = Color.red;
            // Gizmos.DrawWireSphere(transform.position, _weapon.Stats.EffectiveRange);

            // WANDER range
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, WanderRange);

            // FOV
            Gizmos.color = Color.cyan;
            Vector3 dirR = Quaternion.AngleAxis(FieldOfView, Vector3.forward)*transform.right;
            Gizmos.DrawRay(transform.position, dirR*SightRange);
            Vector3 dirL = Quaternion.AngleAxis(-FieldOfView, Vector3.forward)*transform.right;
            Gizmos.DrawRay(transform.position, dirL*SightRange);

            // Allies
            if (AllyList.Count > 0)
            {
                foreach (var ally in AllyList)
                {
                    Debug.DrawLine(transform.position, ally.transform.position, Color.green);
                }
            }

            if (Provoked)
            {
                Debug.DrawLine(transform.position, Target.transform.position, Color.red);
            }

            if (ThreatList != null && ThreatList.Count > 0)
            {
                foreach (var threat in ThreatList)
                {
                    Debug.DrawLine(transform.position, threat.Key.transform.position, Color.yellow);
                }
            }
            #if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
            #endif
        }

        // Primary loop and context/condition analysis
        void DoLocomotion()
        {
            CheckIfHasWeapons();
            // aim either at the target or toward our path.
            if (Provoked && _hasWeapons && Target != null)
            {
                if (TargetCanBeSeen(Target.gameObject))
                {
                    LeadTarget(Target.gameObject);
                }
                else if (AgentDesiredVelocity != Vector3.zero)
                {
                    LookAt(transform.position + AgentDesiredVelocity);
                }
            }        
        }
        private IEnumerator StateMachine()
        {
            if (ThisSubject.WeaponListRuntime.Count > 0) UpdateWeapon(ThisSubject.GetCurrentWeaponGo()); // Intellect does not get the first weapon update call so we Init here.
            int i = IgnoreUpdates;
            while (true)
            {
                while (ThisSubject.IsDead) { yield return null; }
                DoLocomotion();
                while (i > 0)
                {
                    i--;
                    yield return null;
                }

                i = IgnoreUpdates;

                yield return StartCoroutine(ProcessConditions());

                switch (MyMood)
                {
                    case (Mood.Idle):
                        StartCoroutine(Idle());
                        break;
                    case (Mood.Patrol):
                        StartCoroutine(Patrol());
                        break;
                    case (Mood.Wander):
                        StartCoroutine(Wander());
                        break;
                    case (Mood.Alert):
                        StartCoroutine(Alert());
                        break;
                    case (Mood.Flee):
                        StartCoroutine(Flee());
                        break;
                    case (Mood.Combat):
                        StartCoroutine(Combat());
                        break;
                    case (Mood.Dead):
                        StartCoroutine(Dead());
                        break;
                }
            }
        }
        private IEnumerator ProcessConditions()
        {
            CheckIfHasWeapons();
            if (ThisSubject.IsDead) yield break;
            if (MyMood != Mood.Combat && _hasWeapons) _weapon.Attacking = false;
            if (MyProvokeType == ProvokeType.TargetAttackedMe) Fleeing = (ThisSubject.Health <= FleeHealthThreshold && ThisSubject.LastAttacker);
            if (MyProvokeType == ProvokeType.TargetIsInRange) Fleeing = (ThisSubject.Health <= FleeHealthThreshold && Target);
            if (_scanClock >= SenseFrequency)
            {
                ScanForAllSubjects();
                Target = FindThreat().Key;
            }
            _scanClock++;

            // NOTE: Order of Mood processing matters here. Upper evaluations have priority over the ones below.
            // If a Mood condition is met then no other moods are processed.

            #region ### Mood Conditions
            if (ThisSubject.IsDead)
            {
                Provoked = false;
                yield return MyMood = Mood.Dead;
                yield break;
            }
            // TODO since we could have non-dangerous threats, this needs to look at the biggest threat value instead.
            if (MyMood == Mood.Combat && ThreatList.Count == 0)
            {
                // just dropped out of combat and there's no dangerous threats, be Alert
                Provoked = false;
                MyLastMood = MyMood;
                yield return MyMood = Mood.Alert;
                yield break;
            }
            if (_waiting) yield return null;
            if (Fleeing)
            {
                Provoked = true;
                MyLastMood = MyMood;
                yield return MyMood = Mood.Flee;
                yield break;
            }
            if (Provoked || (MyProvokeType == ProvokeType.TargetIsInRange && Target))
            {
                if (MyLastMood != Mood.Combat)
                {
                    StartCoroutine(DelayProvoke());
                }

                Provoked = true;
                MyLastMood = MyMood;
                yield return MyMood = Mood.Combat;
                yield break;
            }
            // provoked moods above, force unprovoked if required (Alert)
            // unprovoked moods below
            Provoked = false;
            if (PatrolPoints.Count > 1)
            {
                MyLastMood = MyMood;
                yield return MyMood = Mood.Patrol;
                yield break;
            }
            if (WanderRange > 0f)
            {
                MyLastMood = MyMood;
                yield return MyMood = Mood.Wander;
                yield break;
            }
            if (!Provoked)
            {
                MyLastMood = MyMood;
                yield return MyMood = Mood.Idle;
            }
            #endregion
        }

        // Possible AI states
        private IEnumerator Idle()
        {
            if (Vector3.Distance(AgentDestination, _startTransform.position) > 0.1)
            {
                MoveTo(_startTransform.position);
            }
            else
            {
                AgentResume();
            }
            if (LogDebug) Debug.Log(_go.name + " Mood: Idle.");

            yield return null;
        }
        private IEnumerator Patrol()
        {
            
            if (LogDebug) Debug.Log(_go.name + " Mood: Patrol.");
            
            if (_patrolling) yield break;

            AgentStoppingDistance = 0.05f;
            MoveTo(PatrolPoints[_curPatrol].transform.position);

            _patrolling = true;
            if (LogDebug) Debug.Log(_go.name + " Patrolling to " + PatrolPoints[_curPatrol]);
            while (AgentRemainingDistance >= AgentStoppingDistance) yield return null;
            _patrolling = false;

            _curPatrol++;
            if (_curPatrol > PatrolPoints.Count - 1) _curPatrol = 0;
            
            yield return null;
        }
        private IEnumerator Wander()
        {
            if (LogDebug) Debug.Log(_go.name + " Mood: Wander.");

            yield return null;
        }
        private IEnumerator Flee()
        {
            if (ThisSubject.IsDead) yield break;
            CheckIfHasWeapons();
            if (_hasWeapons) _weapon.Attacking = false;
            AgentStoppingDistance = 0;
            if (DistToTarget >= SightRange)
            {
                if (LogDebug) Debug.Log("I seem to be safe from the attacker here.");
                yield break;
            }
            if (AgentIsPathStale | AgentRemainingDistance < 1)
            {
                if (LogDebug) Debug.Log("Destination is stale, repathing.");
                // TODO better algorithm to decide flee destination
                int rng = Random.Range(0, 10);
                if (rng > 5) GetPosNearby(SightRange);
                else GetPosFleeing(SightRange);
                yield break;
            }
            if (AgentRemainingDistance < 1)
            {
                if (LogDebug) Debug.Log("Reached Destination, repathing.");
                GetPosNearby(2f);
                yield break;
            }
            if (Vector3.Distance(AgentDestination, Target.transform.position) <= 5f)
            {
                if (LogDebug) Debug.Log("Destination is too close to the Attacker, repathing.");
                GetPosNearby(SightRange);
                yield break;
            }

            yield return null;
        }
        private IEnumerator Alert()
        {
            if (LogDebug) Debug.Log(_go.name + " Mood: Alert.");

            if (_waiting) yield break;
            _waiting = true;
            yield return new WaitForSeconds(AlertTime);
            _waiting = false;

            AgentStoppingDistance = 0.05f;
            MoveTo(_startTransform.position);
        }
        private IEnumerator Combat()
        {
            if (LogDebug) Debug.Log(_go.name + " Mood: Combat!...");
            /*CheckIfHasWeapons();
            if (!_hasWeapons) yield break;
            if (_waiting) yield break;
            if (_needToReload && !_weapon.IsReloading) yield return _weapon.StartCoroutine("Reload"); // TODO is there a better way to call this so it can refactor clean?
            if (ThisSubject.IsDead) yield break;*/
            if (!Target)
            {
                _weapon.Attacking = false;
                yield break;
            }
            if (Target.IsDead)
            {
                AgentStoppingDistance = AttackRange + EngageThreshold;
                _weapon.Attacking = false;
                yield break;
            }
            if (!CanFire())
            {
                _weapon.Attacking = false;
                yield break;
            }
            // At this point, I know I *could* fire my weapon.
            // If I'm in range, look at the target, if I'm not, Move to it.
            _distanceToTarget = DistToTarget; // this is caching a variable used by other routines.
            if (TargetCanBeSeen(Target.gameObject) || TargetIsInRange())
            {
                if (JukeTime > 0f && !_juking) StartCoroutine(Juke());
                _weapon.Attacking = _weapon.Stats.WeaponType == WeaponType.Melee || TargetIsInFov();
                AgentStoppingDistance = AttackRange;
            }
            else // move in
            {
                AgentStoppingDistance = 0.1f;
                MoveTo(Target.transform.position);
                _weapon.Attacking = false;
            }
        }
        private IEnumerator Dead()
        {
            if (LogDebug) Debug.Log("Given the circumstances, I have decided that I am dead.");
            yield break;
        }

        // Callback responses
        public void Die()                                   // callback when we are dead
        {
            // method called from Subject.
            _rb.velocity = (Vector3.zero);
            if (_hasWeapons) _weapon.Attacking = false;
            AgentEnabled(false);
        }
        public void Revive()                                // callback at ressurection
        {
            // inverse the relevant actions of Die() here.
            AgentEnabled(true);
        }
        void Attacked()                                     // callback when we are hit
        {
            // If its not in the Threat List and it is not an Ally...
            if (!ThreatList.ContainsKey(ThisSubject.LastAttacker) && !AllyList.Contains(ThisSubject.LastAttacker))
            {
                DefineAsThreat(ThisSubject.LastAttacker, ThisSubject.LastDamage);
            }
            // Its already in the Threat List, so just increase its threat level by how much damage it has done.
            else if (!AllyList.Contains(ThisSubject.LastAttacker))
            {
                AddThreatValue(ThisSubject.LastAttacker, ThisSubject.LastDamage);
            }

            if (LogDebug) Debug.Log(gameObject.name + ": " + ThisSubject.LastAttacker.name + " has provoked me by attacking, dealing " + ThisSubject.LastDamage + " damage.");
            Provoked = true;
        }

        // List Management and Queries
        void DefineAsThreat(Subject target, int threat)     // add a subject to the threat list
        {
            if (ThreatList.ContainsKey(target))
            {
                if (target.IsDead) RemoveThreat(target);
                else return;
            }

            ThreatList.Add(target, threat);
            if (LogDebug) Debug.Log(target.name + " was flagged as a Threat with " + threat + " influence.");
            
        }
        void DefineAsAlly(Subject target)                   // add a subject to the ally list
        {
            if (AllyList.Count >= MaxAllyCount)
            {
                if (LogDebug) Debug.Log("Too many allies, discarding.");
                return;
            }
            if (!AllyList.Contains(target))
            {
                if (LogDebug) Debug.Log(target.name + " was flagged as an Ally");
                AllyList.Add(target);
            }
            else
            {
                if (LogDebug) Debug.Log("Ally already flagged.");
            }
        }
        void RemoveThreat(Subject target)                   // remove a subject from the threat list
        {
            ThreatList.Remove(target);
            if (LogDebug) Debug.Log(target.name + " was removed from the Threat List.");
        }
        void AddThreatValue(Subject target, int threat)     // add threat to a specific subject
        {
            ThreatList[target] += threat;
            if (LogDebug) Debug.Log(target.name + "'s threat influence increased by: " + threat);
        }
        void RemoveAlly(Subject target)                     // remove an Ally from the AllyList
        {
            if (LogDebug) Debug.Log(target.name + " [ally] was removed from list.");
            AllyList.Remove(target);
        }                   

        void ScanForAllSubjects()                           // get all subjects in the Sight Range
        {
            _scanClock = 0;
            Collider2D[] scanHits = Physics2D.OverlapCircleAll(transform.position, SightRange, ThreatMask);

            if (LogDebug) Debug.Log("Scanning " + scanHits.Length + " hits.");
            foreach (Collider2D thisHit in scanHits)
            {
                if (thisHit.gameObject == gameObject) continue; // is it me?
                Subject otherSubject = thisHit.GetComponentInParent<Subject>(); // TODO this is unfortunately frequent...
                if (!otherSubject) continue; // is it null?
                if (AllyList.Contains(otherSubject) || ThreatList.ContainsKey(otherSubject)) continue; // is it a duplicate?

                // none of that? then sort the new entry as Ally or Threat.
                if (StaticUtil.SameTeam(ThisSubject, otherSubject)) DefineAsAlly(otherSubject);
                else DefineAsThreat(otherSubject, (MyProvokeType == ProvokeType.TargetIsInRange) ? 1 : 0);
            }

            CleanLists();
        }
        void CleanLists()                                   // remove null entries in the lists
        {
            if (ThreatList.Count > 0)
            {
                List<Subject> removals = (from entry in ThreatList where !entry.Key || entry.Key.IsDead select entry.Key).ToList();
                foreach (Subject trash in removals) RemoveThreat(trash);
            }

            if (AllyList.Count > 0)
            {
                List<Subject> removals = (from entry in AllyList where !entry || entry.IsDead select entry).ToList();
                foreach (Subject trash in removals) RemoveAlly(trash);
            }
        }
        KeyValuePair<Subject, int> FindThreat()             // look in the threat list for something to kill
        {
            // no threats and not helping allies?
            if (!ThreatList.Any() && !HelpAllies) return new KeyValuePair<Subject, int>();

            // grab the local threatlist
            Dictionary<Subject, int> allThreats = ThreatList.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
            // grab the ally's threats
            if (HelpAllies)
            {
                Dictionary<Subject, int> myFriendsThreats = new Dictionary<Subject, int>();

                // look at each ally
                if (AllyList.Count > 0)
                {
                    foreach (var ally in AllyList)
                    {
                        Intellect friend = ally.GetIntellect();

                        if (friend)
                        {
                            // look at each threat in that ally
                            foreach (var threat in friend.ThreatList)
                            {
                                // add that threat to this local list
                                if (!myFriendsThreats.ContainsKey(threat.Key))
                                {
                                    myFriendsThreats.Add(threat.Key, threat.Value);
                                }
                            }
                        }
                    }
                }
                // put any threats from allies into the full list of threats
                if (myFriendsThreats.Any())
                {
                    foreach (KeyValuePair<Subject, int> kvp in myFriendsThreats.Where(kvp => !allThreats.ContainsKey(kvp.Key)))
                    {
                        allThreats.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            // do i want the closest or the biggest threat?
            KeyValuePair<Subject, int> final = (MyThreatPriority == ThreatPriority.Nearest
                ? GetNearestThreat(allThreats, transform)
                : GetHighestThreat(allThreats));

            if (final.Value > 0 || (HelpAllies && AllyList.Count > 0 && AllyList.Where(ally => ally.GetIntellect() != null).Any(ally => ally.GetIntellect().Provoked))) Provoked = true;
            return final;
        }
        static KeyValuePair<Subject, int> GetNearestThreat(Dictionary<Subject, int> listOfThreats, Transform fromThis)     // find the nearest threat
        {
            if (listOfThreats.Count == 0) return new KeyValuePair<Subject, int>();
            float[] curNearestDistance = {1000f};

            KeyValuePair<Subject, int>[] nearest = {listOfThreats.First()};
            foreach (KeyValuePair<Subject, int> kvp in listOfThreats
                .Where(kvp => Vector3.Distance(kvp.Key.transform.position, fromThis.position) < curNearestDistance[0]))
            {
                curNearestDistance[0] = Vector3.Distance(kvp.Key.transform.position, fromThis.position);
                nearest[0] = kvp;
            }
            return nearest[0];
        }
        static KeyValuePair<Subject, int> GetHighestThreat(Dictionary<Subject, int> listOfThreats)                         // find the highest threat
        {
            if (listOfThreats.Count == 0) return new KeyValuePair<Subject, int>();

            KeyValuePair<Subject, int>[] biggestThreat = {listOfThreats.First()};
            foreach (KeyValuePair<Subject, int> threat in listOfThreats.Where
                (threat => threat.Value > biggestThreat[0].Value))
            {
                biggestThreat[0] = threat;
            }

            return biggestThreat[0];
        }
        public int GetTargetThreatValue()
        {
            return Target != null ? ThreatList[Target] : 0;
        }

        // Commands
        void UpdateWeapon(GameObject newWeapon)             // callback when switched weapons
        {
            _weapon = newWeapon.GetComponent<Weapon>();
            _weaponSpeed = _weapon.Stats.WeaponType == WeaponType.Ranged ? _weapon.Stats.FiresProjectile.GetComponent<Projectile>().Stats.Speed : 1000;
            AttackRange = _weapon.Stats.EffectiveRange;
        }
        void MoveTo(Vector3 position)                       // pathfind to a position
        {
            if (ThisSubject.IsDead) return;
            AgentDestination = position;
            AgentResume();
        }
        public void LookAt(Vector3 position)                // look at a specific position
        {
            if (Target.IsDead) return;
            Agent.RotateTowards(position);
        }
        public void LeadTarget(GameObject victim)           // lead the target, compensating for their trajectory and projectile speed
        {
            // TODO find a way to compensate for the horizontal offset of the weapon without screwing with the character orientation

            // Get the velocity of the Subject. We need to know *direction* and *speed*.
            Vector3 victimVelocty = (victim.transform.position - _victimLastPos) * Time.deltaTime;
            Vector3 intercept = victim.transform.position + (victimVelocty*(_distanceToTarget/_weaponSpeed)); 
                //+ victim.transform.TransformVector(new Vector3((_weapon.Stats.MountPivot == MountPivot.RightShoulder ? 1f : _weapon.Stats.PositionOffset.x),0,0)));

            _victimLastPos = victim.transform.position; // Plug in the last known position (first calc is always wrong)
            LookAt(intercept);
        }
        private IEnumerator Juke()                          // Execute a juke maneuver
        {
            // juke has to do something every frame until it reaches its point or cant reach it
            if (_juking) yield break;
            if (LogDebug) Debug.Log("Perform Juke");
            _juking = true;
            
            // tell the agent to stay still
            AgentStoppingDistance = AttackRange;

            _jukeHeading = GetJukeHeading();

            // setup the time before the next Juke
            float wait = Random.Range(
                    JukeFrequency - JukeFrequencyRandomness,
                    JukeFrequency + JukeFrequencyRandomness);

            bool yieldToJukeTime = true;
            float timer = 0;
            while (yieldToJukeTime)
            {
                _rb.MovePosition(transform.position + _jukeHeading * .01f);
                timer += Time.deltaTime;
                if (timer >= JukeTime)
                {
                    yieldToJukeTime = false;
                }
                yield return null;
            }

            yield return new WaitForSeconds(wait);
            _juking = false;
        }
        private Vector3 GetJukeHeading()
        {
            // setup the distance to juke per frame
            bool r = (Random.value < 0.5);
            _jukeHeading = (r && !Physics.Raycast(transform.position + Vector3.up, Vector3.right, 0.2f, SightMask)
                ? transform.TransformDirection(Vector3.right * ThisSubject.ControlStats.MoveSpeed)
                : transform.TransformDirection(Vector3.left * ThisSubject.ControlStats.MoveSpeed));
            return _jukeHeading;
        }

        // Logic queries
        bool CanFire()                                      // check if the weapon can be fired
        {
            if (MagHasAmmo())
            {
                _needToReload = false;
                return true;
            }
            NoAmmoLeft = (_weapon.Stats.CurrentMagazines <= 0 && !MagHasAmmo());
            if (NoAmmoLeft) return false;

            _needToReload = true;
            return false;
        }
        bool MagHasAmmo()                                   // is there enough ammo in the mag to fire?
        {
            return _weapon.Stats.CurrentAmmo >= _weapon.Stats.AmmoCost;
        }
        bool TargetCanBeSeen(GameObject interest)           // raycast to the Target and check for a hit
        {
            bool inSight = false;
            Vector3 direction = (interest.transform.position - transform.position).normalized;
            Vector3 origin = transform.position;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, SightRange, SightMask);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == interest) inSight = true;
            }

            return inSight;
        }
        bool TargetIsInFov()                                // check the FOV area for the target
        {
            Vector3 direction = Target.gameObject.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.right);
            return angle < FieldOfView * 0.5f;
        }
        bool TargetIsInRange()                              // check if distance to target is less than vision range.
        {
            return _distanceToTarget < AttackRange;
        }
        
        private void GetPosNearby(float area)               // find a random nearby position
        {
            Vector3 waypoint = transform.position + Random.insideUnitSphere * area;
            waypoint.z = 0f;
            Vector3 localOrigin = _map.transform.TransformPoint(Vector3.zero);
            waypoint.x = Mathf.Clamp(waypoint.x, localOrigin.x, _map.MeshSettings.TilesX * _map.MeshSettings.TileSize);
            waypoint.y = Mathf.Clamp(waypoint.y, localOrigin.y, _map.MeshSettings.TilesY * _map.MeshSettings.TileSize);
            
            MoveTo(waypoint);
            if (LogDebug)
                Debug.Log("Moving to " + waypoint.ToString());
        }
        private void GetPosFleeing(float area)              // find a random nearby position relative to the Target
        {
            AgentStoppingDistance = 0;
            Vector3 waypoint = Vector3.Scale
                (transform.position,
                (Target.transform.position - transform.position).normalized * area)
                + Random.insideUnitSphere * 2;
            waypoint.z = 0f;
            Vector3 localOrigin = _map.transform.TransformPoint(Vector3.zero);
            waypoint.x = Mathf.Clamp(waypoint.x, localOrigin.x, _map.MeshSettings.TilesX * _map.MeshSettings.TileSize);
            waypoint.y = Mathf.Clamp(waypoint.y, localOrigin.y, _map.MeshSettings.TilesY * _map.MeshSettings.TileSize);

            MoveTo(waypoint);
            if (LogDebug)
                Debug.Log("Fleeing to " + waypoint.ToString());
        }

        public float DistToTarget                           // find the distance to the target
        {
            get
            {
                return Target.IsDead ? 0f : Vector3.Distance(Target.transform.position, transform.position);
            }
        }

        // Miscellaneous
        public string GetTargetName()
        {
            return Target != null ? Target.gameObject.name : "";
        }
        private IEnumerator DelayProvoke()
        {
            if (_waiting) yield break;
            _waiting = true;
            yield return new WaitForSeconds(ProvokeDelay);
            _waiting = false;
        }

        // Agent accessors 
        internal Agent AgentGetComponent
        {
            get
            {
                return GetComponent<Agent>();
            }
        }

        public void AgentResume()
        {
            Agent.CalculatePath();
            Agent.Resume();
        }
        public void AgentStop()
        {
            Agent.Stop();
        }
        public void AgentEnabled(bool status)
        {
            Agent.enabled = status;
        }
        public float AgentRemainingDistance
        {
            get { return Agent.remainingDistance; }
        }
        public float AgentStoppingDistance
        {
            get { return Agent.stoppingDistance; }
            set { Agent.stoppingDistance = value; }
        }
        public Vector3 AgentDesiredVelocity
        {
            get { return Agent.desiredVelocity; }
        }
        public Vector3 AgentDestination
        {
            get { return Agent.destination; } 
            set { Agent.destination = value; Agent.CalculatePath(); }
        }
        public bool AgentIsPathStale
        {
            get { return Agent.isPathStale; }
        }
    }
}