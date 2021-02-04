using ScriptableObjectArchitecture;
using UnityEngine;

namespace Overworld
{
    public class CameraRotationHandler : MonoBehaviour
    {
        [SerializeField] private QuaternionVariable cVCamRotation;
        private void Update() => gameObject.transform.rotation = cVCamRotation;
    }
}