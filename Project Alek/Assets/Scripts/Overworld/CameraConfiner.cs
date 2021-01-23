using Cinemachine;
using UnityEngine;

namespace Overworld
{
    public class CameraConfiner : MonoBehaviour
    {
        [SerializeField] private BoxCollider boxCollider;
        private BoxCollider mainSceneCollider;
        private CinemachineConfiner vCamera;

        private void Awake()
        {
            mainSceneCollider = GameObject.FindWithTag("MainSceneCamCollider").GetComponent<BoxCollider>();
            vCamera = GameObject.FindWithTag("VirtualCamera").GetComponent<CinemachineConfiner>();
        }

        private void OnTriggerEnter(Collider other) => vCamera.m_BoundingVolume = boxCollider;

        private void OnTriggerExit(Collider other) => vCamera.m_BoundingVolume = mainSceneCollider;
    }
}
