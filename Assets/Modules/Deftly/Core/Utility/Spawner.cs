using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Deftly
{
    public class Spawner : MonoBehaviour
    {
        public enum SpawnerStyle { FixedCount, Destroyable }
        public SpawnerStyle SpawnerType;
        public SpawnTrigger Owner;
        public int FixedWaveCount;
        public float TimeBetweenWaves;

        public List<GameObject> EnemyPrefabs;
        public List<int> EnemyCounts;
 
        public Dictionary<GameObject, int> EnemyReferences;
        public bool Completed;
        public List<GameObject> Positions;

        private int _curWave;
        private WaitForSeconds _waveGap;

        void Awake()
        {
            EnemyReferences = new Dictionary<GameObject, int>();
            for (int i = 0; i < EnemyPrefabs.Count; i++) EnemyReferences.Add(EnemyPrefabs[i], EnemyCounts[i]);
            _waveGap = new WaitForSeconds(TimeBetweenWaves);
        }
        void Start()
        {
            StartCoroutine(SpawnerLoop());
        }

        public void ActivateSpawner()
        {
            StartCoroutine(SpawnerLoop());
        }
        IEnumerator SpawnerLoop()
        {
            while (!Completed)
            {
                _curWave++;
                SpawnWave();

                if (SpawnerType == SpawnerStyle.FixedCount && _curWave >= FixedWaveCount) { StaticUtil.DeSpawn(gameObject); }
                yield return _waveGap;
            }
        }

        public void SpawnWave()
        {
            foreach (KeyValuePair<GameObject, int> key in EnemyReferences)
            {
                for (int i = 0; i < key.Value; i++)
                {
                    Transform t = Positions[Random.Range(0, (Positions.Count))].transform;
                    if (Owner) Owner.AddToEnemies(StaticUtil.Spawn(key.Key, t.position, t.rotation) as GameObject);
                }
            }
        }
    }
}