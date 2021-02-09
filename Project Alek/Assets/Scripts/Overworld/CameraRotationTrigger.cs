using ScriptableObjectArchitecture;
using UnityEngine;

namespace Overworld
{
    public class CameraRotationTrigger : MonoBehaviour
    {
        [SerializeField] private QuaternionGameEvent setSceneRotationEvent;
        [SerializeField] private Quaternion areaRotation;
        [SerializeField] private Quaternion exitRotation;

        public Quaternion AreaRotation => areaRotation;
        public Quaternion ExitRotation => exitRotation;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            var thisTransform = transform;
            var toOther = other.transform.position - thisTransform.position;
            var hasEnteredArea = Vector3.Dot(thisTransform.right, toOther) > 0;

            setSceneRotationEvent.Raise(hasEnteredArea ? areaRotation : exitRotation);
        }
    }
}