using System.Collections.Generic;
using System.Linq;
using Characters.Enemies;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Overworld
{
    public class EnemySpawnManager : MonoBehaviorSingleton<EnemySpawnManager>, IGameEventListener<Spawner>
    {
        [SerializeField] private OverworldAreaId id;
        
        [SerializeField] private SpawnEvent spawnEvent;
        
        [SerializeField] private int maxSpawns;
        
        [SerializeField] private List<Enemy> enemiesInArea = new List<Enemy>();
        
        [SerializeField] private List<Spawnable> spawnables = new List<Spawnable>();
        [ReadOnly] public List<Spawnable> currentEnemiesSpawned = new List<Spawnable>();

        [ShowInInspector] private bool CanSpawnMoreEnemies =>
            currentEnemiesSpawned.Count < maxSpawns;

        private bool delaySpawning;

        protected override void Awake()
        {
            base.Awake();
            enemiesInArea = new List<Enemy>(EnemyDatabase.Instance.GetAvailableEnemiesForThisArea(id));
            spawnables = new List<Spawnable>(EnemyDatabase.Instance.GetAvailableSpawnablesForThisArea(id));
            delaySpawning = true;
            Invoke(nameof(StartSpawning), 2);
            spawnEvent.AddListener(this);
        }
        
        private void StartSpawning() => delaySpawning = false;

        private void OnDisable() => spawnEvent.RemoveListener(this);

        public void SetRandomEnemiesForBattle(Spawnable instigator)
        {
            var index = spawnables.IndexOf(spawnables.Single(s => s.name == instigator.name));
            var firstEnemy = enemiesInArea[index];
            var numberOfEnemies = Random.Range(1, 5);
            var list = new List<Enemy> {firstEnemy};
            
            for (var i = 1; i < numberOfEnemies; i++)
            {
                var enemyToAdd = enemiesInArea[Random.Range(0, enemiesInArea.Count)];
                list.Add(enemyToAdd);
            }
            
            EnemyManager.SetBattleEnemies(list);
        }
        
        public void OnEventRaised(Spawner spawner)
        {
            if (delaySpawning) return;
            if (!CanSpawnMoreEnemies) return;
            
            var spawnable = spawnables[Random.Range(0, spawnables.Count)];
            var position = spawner.GetSpawnPoint();
            var spawnableGO = spawner.Spawn(spawnable, position, spawner.transform);
            spawnableGO.name = spawnable.name;
                
            currentEnemiesSpawned.Add(spawnableGO);
            spawnableGO.OnDestroyAction += () => currentEnemiesSpawned.Remove(spawnableGO);
        }
    }
}