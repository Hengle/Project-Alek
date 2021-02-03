using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Overworld
{
    public class PlayerSpawnArea : MonoBehaviour
    {
        [SerializeField] [ReadOnly]
        private string id;
        public string Id => id;

        [Button] private void GenerateId() =>
            id = Random.Range(1, 1000).ToString();
    }
}