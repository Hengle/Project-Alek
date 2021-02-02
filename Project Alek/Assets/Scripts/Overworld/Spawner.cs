using System.Collections.Generic;
using UnityEngine;

namespace Overworld
{
    public abstract class Spawner : MonoBehaviour
    {
        public abstract Vector3 GetSpawnPoint();
        
        public virtual bool CanSpawn { get; }

        public virtual Spawnable Spawn(Spawnable spawnable, Vector3 position, Transform parent) { return null; }
        
        private static readonly HashSet<Spawner> InstanceList = new HashSet<Spawner>();
        
        public static IEnumerable<Spawner> Instances => new HashSet<Spawner>(InstanceList);
        
        protected virtual void Awake() => InstanceList.Add(this); 

        protected virtual void OnDestroy() => InstanceList.Remove(this);
    }
}