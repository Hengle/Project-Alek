using System.Collections.Generic;
using System.Linq;
using Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "Enemy Database", menuName = "Singleton SO/Enemy Database")]
    public class EnemyDatabase : ScriptableObjectSingleton<EnemyDatabase>
    {
        [SerializeField] private bool debug;
        
        [SerializeField] private List<Enemy> enemies = new List<Enemy>();
        
        [ShowInInspector] [DictionaryDrawerSettings(KeyLabel = "ID", ValueLabel = "Enemies", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public readonly Dictionary<OverworldAreaId, List<Enemy>> _overworldAreaDictionary =
            new Dictionary<OverworldAreaId, List<Enemy>>();

        public IEnumerable<Enemy> GetAvailableEnemiesForThisArea(OverworldAreaId id)
        {
            return DatabaseContainsId(id) ? _overworldAreaDictionary[id] : null;
        }

        public IEnumerable<Spawnable> GetAvailableSpawnablesForThisArea(OverworldAreaId id)
        {
            return DatabaseContainsId(id) ? _overworldAreaDictionary[id].
                Select(enemy => enemy.overworldPrefab.GetComponent<Spawnable>()).ToList() : null;
        }

        private bool DatabaseContainsId(OverworldAreaId id)
        {
            if (_overworldAreaDictionary.ContainsKey(id)) return true;
            PrintError($"The ID {id} does not exist in the Overworld Area Dictionary!");
            return false;
        }
        
        private void PrintMessage(string message) { if (debug) Debug.Log(message); }

        private void PrintWarning(string message) { if (debug) Debug.LogWarning(message); }
        
        private void PrintError(string message) { if (debug) Debug.LogError(message); }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}