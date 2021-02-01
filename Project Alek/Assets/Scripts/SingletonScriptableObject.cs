using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance != null) return instance;
            
            var results = Resources.FindObjectsOfTypeAll<T>();
            if (results.Length == 0)
            {
                Debug.LogError($"SingletonScriptableObject: results length is 0 of {typeof(T)}");
                return null;
            }

            if (results.Length > 1)
            {
                Debug.LogError($"SingletonScriptableObject: results length is greater than 1 of {typeof(T)}");
                return null;
            }

            instance = results[0];
            instance.hideFlags = HideFlags.DontUnloadUnusedAsset;

            return instance;
        }
    }
}