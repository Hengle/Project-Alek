using UnityEngine;

public class MonoBehaviorSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        var results = FindObjectsOfType<T>();
        if (results.Length == 0)
        {
            Debug.LogError($"There are no instances of {typeof(T)} in scene");
            Instance = null;
            return;
        }

        if (results.Length > 1)
        {
            Debug.LogError($"There are multiple instances of {typeof(T)} in scene");
            Instance = null;
            return;
        }
        
        Instance = results[0];
    }
}