using System.Collections.Generic;
using Characters.Enemies;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Enemy Manager")]
    public class EnemyManager : SingletonScriptableObject<EnemyManager>
    {
        public List<Enemy> enemies = new List<Enemy>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}