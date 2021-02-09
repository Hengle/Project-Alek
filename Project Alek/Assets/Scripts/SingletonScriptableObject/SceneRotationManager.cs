using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Scene Rotation Manager")]
    public class SceneRotationManager : SingletonScriptableObject<SceneRotationManager>
    {
        [SerializeField] private Quaternion currentRotation = Quaternion.identity;
        public Quaternion CurrentRotation
        {
            get => currentRotation;
            set => currentRotation = value;
        }
  
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}