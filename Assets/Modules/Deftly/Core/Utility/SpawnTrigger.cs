// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deftly
{
    public class SpawnTrigger : MonoBehaviour
    {
        [Space]
        public LayerMask CanUseThis;
        [Header("Doors are Opened at Game Start")]
        public Door[] DoorsToAffect;
        public GameObject[] SpawnPoints;
        public List<GameObject> SpawnerPrefabs;

        private List<Spawner> _remainingSpawners;
        private List<Subject> _remainingEnemies;
        private bool _started;
        private WaitForSeconds _cleanWait;

        void Reset()
        {
            CanUseThis = 0;
            DoorsToAffect = null;
            SpawnPoints = null;
        }
        void Awake()
        {
            _cleanWait = new WaitForSeconds(0.5f);
            _remainingEnemies = new List<Subject>();
            _remainingSpawners = new List<Spawner>();
        }
        void Start()
        {
            foreach (Door door in DoorsToAffect)
            {
                if (!door.StartClosed)
                {
                    door.Locked = false;
                    door.Open();
                }
            }
        }
        void OnTriggerEnter(Collider col)
        {
            if (_started || !StaticUtil.LayerMatchTest(CanUseThis, col.gameObject)) return;
            _started = true;

            int i = 0;
            if (SpawnPoints.Length == 0)
            {
                Debug.LogWarning("Spawn Trigger Warning: No spawn points are specified so this trigger effectively does nothing.");
                return;
            }
            foreach (GameObject point in SpawnPoints)
            {
                if (!SpawnerPrefabs[i])
                {
                    Debug.LogError("Spawn Trigger Error: You specify a spawn point, but no Spawner Prefab. Can't proceed without a valid Prefab of the Spawner that you to create.");
                    break;
                }
                GameObject s = (GameObject)StaticUtil.Spawn(SpawnerPrefabs[i], point.transform.position, point.transform.rotation);
                Spawner x = s.GetComponent<Spawner>();
                _remainingSpawners.Add(x);
                x.Owner = this;
                i++;
            }

            StartCoroutine(CloseDoors());
            StartCoroutine(VictoryLoop());
        }

        public IEnumerator VictoryLoop()
        {
            while (true)
            {
                // Clean Lists
                for (int i = 0; i < _remainingEnemies.Count; i++)
                {
                    if (!_remainingEnemies[i] || _remainingEnemies[i].IsDead)
                    {
                        _remainingEnemies.RemoveAt(i);
                    }
                }

                for (int i = 0; i < _remainingSpawners.Count; i++)
                {
                    if (!_remainingSpawners[i])
                    {
                        _remainingSpawners.RemoveAt(i);
                    }
                }

                // Check for victory
                if (_remainingSpawners.All(s => !s) && _remainingEnemies.Count == 0)
                {
                    StartCoroutine(OpenDoors());
                    yield break;
                }

                yield return _cleanWait;
            }
        }
        public IEnumerator CloseDoors()
        {
            foreach (Door door in DoorsToAffect)
            {
                door.Locked = true;
                door.Close();
            }
            yield break;
        }
        public IEnumerator OpenDoors()
        {
            foreach (Door door in DoorsToAffect)
            {
                door.Locked = false;
                door.Open();
            }
            yield break;
        }
        public void AddToEnemies(GameObject go)
        {
            _remainingEnemies.Add(go.GetComponent<Subject>());
        }
    }
}