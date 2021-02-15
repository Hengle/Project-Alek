using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using MEC;

namespace Overworld
{
    public class CameraRotationHandler : MonoBehaviour, IGameEventListener
    {
        [SerializeField] private QuaternionVariable cVCamRotation;
        [SerializeField] private GameEvent newRotationSetEvent;

        private void Start() => transform.rotation = cVCamRotation;

        private IEnumerator<float> RotateCoroutine()
        {
            while (transform.rotation != cVCamRotation)
            {
                transform.rotation = Quaternion.RotateTowards
                    (transform.rotation, cVCamRotation, Time.deltaTime * 50);

                yield return Timing.WaitForOneFrame;
            }
        }
        
        public void OnEventRaised() => Timing.RunCoroutine(RotateCoroutine().CancelWith(gameObject));
        
        private void OnEnable() => newRotationSetEvent.AddListener(this);
        private void OnDisable() => newRotationSetEvent.RemoveListener(this);
    }
}