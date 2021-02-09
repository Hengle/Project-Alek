using System;
using System.Collections.Generic;
using MEC;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Overworld
{
    public class CameraRotationHandler : MonoBehaviour, IGameEventListener
    {
        [SerializeField] private QuaternionVariable cVCamRotation;
        [SerializeField] private GameEvent newRotationSetEvent;

        private void Start()
        {
            transform.rotation = cVCamRotation;
        }

        private void OnEnable() => newRotationSetEvent.AddListener(this);
        private void OnDisable() => newRotationSetEvent.RemoveListener(this);

        private void UpdateRotation() => Timing.RunCoroutine(RotateCoroutine());

        private IEnumerator<float> RotateCoroutine()
        {
            while (transform.rotation != cVCamRotation)
            {
                transform.rotation = Quaternion.RotateTowards
                    (transform.rotation, cVCamRotation, Time.deltaTime * 25);

                yield return Timing.WaitForOneFrame;
            }
        }

        public void OnEventRaised()
        {
            UpdateRotation();
        }
    }
}