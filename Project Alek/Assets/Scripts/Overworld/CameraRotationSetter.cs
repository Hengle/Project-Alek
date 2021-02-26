using ScriptableObjectArchitecture;
using UnityEngine;

namespace Overworld
{
    public class CameraRotationSetter : MonoBehaviour, IGameEventListener<Quaternion>
    {
        [SerializeField] private QuaternionGameEvent setSceneRotationEvent;
        [SerializeField] private GameEvent newRotationSetEvent;
        [SerializeField] private QuaternionVariable cVCamRotation;
        [SerializeField] private Quaternion sceneRotation;

        private void Awake()
        {
            cVCamRotation.Value = SceneRotationManager.CurrentRotation != Quaternion.identity ?
                SceneRotationManager.CurrentRotation : sceneRotation;

            SceneRotationManager.CurrentRotation = Quaternion.identity;
            newRotationSetEvent.Raise();
        }

        private void SetSceneRotation(Quaternion rotation)
        {
            cVCamRotation.Value = rotation;
            newRotationSetEvent.Raise();
        }

        public void OnEventRaised(Quaternion value) => SetSceneRotation(value);
        
        private void OnEnable() => setSceneRotationEvent.AddListener(this);

        private void OnDisable()
        {
            SceneRotationManager.CurrentRotation = cVCamRotation.Value;
            setSceneRotationEvent.RemoveListener(this);
        }
    }
}