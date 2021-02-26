using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ScriptableObjectSingleton<T> : SerializedScriptableObject where T : ScriptableObject
{
    public static T Instance { get; private set; }
    
    protected static void Init(string path = "")
    {
        var results = Resources.LoadAll<T>(path);
        
        if (results.Length == 0)
        {
            Debug.LogError($"ScriptableObjectSingleton: results length is 0 of {typeof(T)}");
            Instance = null;
            return;
        }

        Instance = results[0];
    }
}