using Cinemachine;
using UnityEngine;

namespace Overworld
{
    public class CameraConfiner : MonoBehaviour
    {
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private bool resetOnExit = true;
        private BoxCollider mainSceneCollider;
        private CinemachineConfiner vCamera;

        private void Awake()
        {
            mainSceneCollider = GameObject.FindWithTag("MainSceneCamCollider").GetComponent<BoxCollider>();
            vCamera = GameObject.FindWithTag("VirtualCamera").GetComponent<CinemachineConfiner>();
        }

        private void OnTriggerEnter(Collider other) => vCamera.m_BoundingVolume = boxCollider;

        private void OnTriggerExit(Collider other)
        {
            if (resetOnExit) vCamera.m_BoundingVolume = mainSceneCollider;
        }
    }
}
