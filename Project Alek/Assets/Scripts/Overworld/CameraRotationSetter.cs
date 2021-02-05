using ScriptableObjectArchitecture;
using UnityEngine;

namespace Overworld
{
    public class CameraRotationSetter : MonoBehaviour
    {
        [SerializeField] private QuaternionVariable cVCamRotation;
        [SerializeField] private Quaternion sceneRotation;

        private void Awake() => cVCamRotation.Value = sceneRotation;
    }
}