using UnityEngine;

namespace Overworld
{
    public class ChestManager : MonoBehaviour
    {
        private static ChestManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this) Destroy(gameObject);
        }
    }
}