using System;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Overworld
{
    public class CameraRotationTrigger : MonoBehaviour
    {
        [SerializeField] private GameEvent resetCamRotationEvent;
        [SerializeField] private QuaternionGameEvent setSceneRotationEvent;
        [SerializeField] private Quaternion areaRotation;

        private void OnTriggerEnter(Collider other)
        {
            setSceneRotationEvent.Raise(areaRotation);

            // var hasEnteredArea = other.transform.InverseTransformPoint(transform.position).z < 0;
            // if (hasEnteredArea)
            // {
            //     print("Has entered area");
            //     setSceneRotationEvent.Raise(areaRotation);
            // }
            // else
            // {
            //     print("Has exited area");
            //     resetCamRotationEvent.Raise();
            // }
            
            //print(other.transform.InverseTransformPoint(transform.position));
        }
    }
}