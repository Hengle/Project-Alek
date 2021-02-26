using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Overworld
{
    [CreateAssetMenu(fileName = "Player Position Manager", menuName = "Singleton SO/Player Position Manager")]
    public class PlayerPositionManager : ScriptableObjectSingleton<PlayerPositionManager>
    {
        [SerializeField] [ReadOnly] private Vector3 position;

        [Button] private void ResetPosition() => position = Vector3.zero;
        
        [ValueDropdown(nameof(GetAllSpawnPointsInScene))]
        [SerializeField] private string spawnId = "0";

        public static Vector3 Position
        {
            get => Instance.position;
            set => Instance.position = value;
        }
        
        public static string SpawnId
        {
            get => Instance.spawnId;
            set => Instance.spawnId = value;
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