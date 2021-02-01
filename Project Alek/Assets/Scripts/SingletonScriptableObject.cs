using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    public static T Instance { get; private set; }
    
    protected static void Init()
    {
        var results = Resources.LoadAll<T>("");
        
        if (results.Length == 0)
        {
            Debug.LogError($"SingletonScriptableObject: results length is 0 of {typeof(T)}");
            Instance = null;
            return;
        }
        
        Debug.Log($"SingletonScriptableObject of type {typeof(T)} has been found!");
        
        Instance = results[0];
    }
}