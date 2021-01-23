using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Overworld
{
    public class EnemySpawnArea : Spawner
    {
        [SerializeField] private float spawnRange;
        [SerializeField] private float maxSpawns;
        [ShowInInspector] private readonly List<Spawnable> enemiesSpawned = new List<Spawnable>();

        public override bool CanSpawn => enemiesSpawned.Count < maxSpawns;

        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position, spawnRange);

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