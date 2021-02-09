using System.Collections.Generic;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Overworld
{
    //TODO: Make this a singleton
    public class EnemySpawnManager : MonoBehaviour, IGameEventListener<Spawner>
    {
        [SerializeField] private SpawnEvent spawnEvent;
        
        [SerializeField] private int maxSpawns;
        
        [SerializeField] private List<Spawnable> spawnables = new List<Spawnable>();
        [ReadOnly] public List<Spawnable> currentEnemiesSpawned = new List<Spawnable>();

        [ShowInInspector] private bool CanSpawnMoreEnemies => currentEnemiesSpawned.Count < maxSpawns;

        private void Awake() => spawnEvent.AddListener(this);
        
        private void OnDisable() => spawnEvent.RemoveListener(this);
        
        public void OnEventRaised(Spawner spawner)
        {
            if (!CanSpawnMoreEnemies) return;
            
            var spawnable = spawnables[Random.Range(0, spawnables.Count)];
            var position = spawner.GetSpawnPoint();
            var spawnableGO = spawner.Spawn(spawnable, position, spawner.transform);
                
            currentEnemiesSpawned.Add(spawnableGO);
            spawnableGO.OnDestroyAction += () => currentEnemiesSpawned.Remove(spawnableGO);
        }
    }
}