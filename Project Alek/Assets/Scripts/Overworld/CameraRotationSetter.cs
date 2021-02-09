using System;
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

        private void Awake() => cVCamRotation.Value = sceneRotation;
        
        public void ResetRotation()
        {
            cVCamRotation.Value = sceneRotation;
            newRotationSetEvent.Raise();
        }

        private void SetSceneRotation(Quaternion rotation)
        {
            if (cVCamRotation.Value == rotation) { ResetRotation(); return; }
            cVCamRotation.Value = rotation;
            newRotationSetEvent.Raise();
        }

        public void OnEventRaised(Quaternion value)
        {
            SetSceneRotation(value);
        }

        private void OnEnable() => setSceneRotationEvent.AddListener(this);
        private void OnDisable() => setSceneRotationEvent.RemoveListener(this);
    }
}