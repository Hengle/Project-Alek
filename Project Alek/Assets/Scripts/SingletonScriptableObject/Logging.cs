﻿using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Logger")]
    public class Logging : SingletonScriptableObject<Logging>
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