using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Add AudioTypes to this class that are called from scripts that are attached to multiple different prefabs. This way
    /// You do not have to put the reference for the AudioType for each prefab. Make sure these sounds are on
    /// the AudioController prefab for that scene
    /// </summary>
    public class CommonAudioTypes : MonoBehaviour
    {
        public AudioType hitWindow;

        private static CommonAudioTypes instance;

        public static CommonAudioTypes Instance {
            get { if (instance == null) Debug.LogError("CommonAudioTypes is null");
                return instance; }
        }

        private void Awake() => instance = this;
    }
}