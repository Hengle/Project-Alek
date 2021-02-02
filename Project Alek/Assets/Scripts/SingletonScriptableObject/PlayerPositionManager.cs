using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Player Position Manager")]
    public class PlayerPositionManager : SingletonScriptableObject<PlayerPositionManager>
    {
        [SerializeField] private Vector3 position;
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}