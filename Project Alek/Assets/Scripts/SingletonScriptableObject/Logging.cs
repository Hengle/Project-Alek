using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Logger", menuName = "Singleton SO/Logger")]
    public class Logging : ScriptableObjectSingleton<Logging>
    {
        [SerializeField] private bool showLogs = true;
        
        public void Log(string message)
        { 
            if (showLogs) Debug.Log(message);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}