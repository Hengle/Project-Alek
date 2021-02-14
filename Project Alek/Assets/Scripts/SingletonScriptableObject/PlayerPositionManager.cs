using System.Collections;
using Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Player Position Manager", menuName = "Singleton SO/Player Position Manager")]
    public class PlayerPositionManager : ScriptableObjectSingleton<PlayerPositionManager>
    {
        [SerializeField] [ReadOnly] 
        private Vector3 position;

        [Button] private void ResetPosition() => 
            position = Vector3.zero;
        
        [ValueDropdown(nameof(GetAllSpawnPointsInScene))]
        [SerializeField] private string spawnId = "0";

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }
        
        public string SpawnId
        {
            get => spawnId;
            set => spawnId = value;
        }

        private IEnumerable GetAllSpawnPointsInScene()
        {
            var spawnAreas = FindObjectsOfType<PlayerSpawnArea>();
            if (spawnAreas.Length == 0) yield break;
            
            foreach (var area in spawnAreas)
            {
                yield return new ValueDropdownItem($"{area.gameObject.name} ({area.Id})", area.Id);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}