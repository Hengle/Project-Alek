using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Overworld
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        
        [SerializeField] private int maxSpawns;
        
        [SerializeField] private List<Spawnable> spawnables = new List<Spawnable>();
        [ReadOnly] public List<Spawnable> currentEnemiesSpawned = new List<Spawnable>();

        [ShowInInspector] private bool CanSpawnMoreEnemies => currentEnemiesSpawned.Count < maxSpawns;

        private void Awake()
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        private void Update()
        {
            if (!CanSpawnMoreEnemies) return;
            
            foreach (var spawner in Spawner.Instances)
            {
                if (!spawner.CanSpawn || !CanSpawnMoreEnemies || !(Math.Abs
                    (spawner.transform.position.x - player.position.x) < 20f)) continue;
                
                var spawnable = spawnables[Random.Range(0, spawnables.Count-1)];
                var position = spawner.GetSpawnPoint();
                var spawnableGO = spawner.Spawn(spawnable, position, spawner.transform);
                
                currentEnemiesSpawned.Add(spawnableGO);
                spawnableGO.OnDestroyAction += () => currentEnemiesSpawned.Remove(spawnableGO);
            }
        }
    }
}