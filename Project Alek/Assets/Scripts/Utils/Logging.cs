using UnityEngine;

namespace Utils
{
    [CreateAssetMenu(fileName = "Logger", menuName = "Singleton SO/Logger")]
    public class Logging : ScriptableObjectSingleton<Logging>
    {
        [SerializeField] private bool showLogs = true;
        
        public static void Log(string message)
        { 
            if (Instance.showLogs) Debug.Log(message);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}