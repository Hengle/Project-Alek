using System.Collections.Generic;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Overworld
{
    public class EnemySpawnArea : Spawner
    {
        [SerializeField] private SpawnEvent spawnEvent;
        
        [SerializeField] [Tooltip("How far the spawn area can detect the player from")]
        [OnValueChanged(nameof(UpdateBoxCollider))]
        private float spawnTriggerRange;
        
        [SerializeField] [Tooltip("The range that an enemy can be spawned in")]
        private float spawnRange;
        
        [SerializeField] private float maxSpawns;
        
        [ShowInInspector] private readonly List<Spawnable> enemiesSpawned = new List<Spawnable>();

        [SerializeField] private BoxCollider boxCollider;

        [SerializeField] private bool onCooldown;
        public override bool CanSpawn => enemiesSpawned.Count < maxSpawns && !onCooldown;

        private void UpdateBoxCollider() => boxCollider.size = new Vector3
            (spawnTriggerRange, boxCollider.size.y, spawnTriggerRange);

        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position, spawnRange);

        private void OnTriggerStay(Collider other)
        {
            if (onCooldown || !CanSpawn || !other.transform.CompareTag("Player")) return;
            
            spawnEvent.Raise(this);
            onCooldown = true;
            Invoke(nameof(ResetCooldown), 2);
        }

        private void ResetCooldown() => onCooldown = false;

        public override Vector3 GetSpawnPoint()
        {
            var randomZ = Random.Range(-spawnRange, spawnRange);
            var randomX = Random.Range(-spawnRange, spawnRange);
            var position = transform.position;
            
            return new Vector3(position.x + randomX, position.y, position.z + randomZ);
        }

        public override Spawnable Spawn(Spawnable spawnable, Vector3 position, Transform parent)
        {
            var spawnGO = Instantiate(spawnable, position, spawnable.transform.rotation);
            spawnGO.transform.SetParent(parent);
            enemiesSpawned.Add(spawnGO);
            spawnGO.OnDestroyAction += () => enemiesSpawned.Remove(spawnGO);
            
            return spawnGO;
        }
    }
}