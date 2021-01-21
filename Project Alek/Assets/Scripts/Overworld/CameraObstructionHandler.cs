using UnityEngine;
using UnityEngine.Rendering;

namespace Overworld
{
    public class CameraObstructionHandler : MonoBehaviour
    {
        private Transform player, obstruction;

        private void Awake()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
            obstruction = player;
        }

        private void LateUpdate() => ViewObstructed();

        private void ViewObstructed()
        {
            if (!Physics.Raycast(transform.position, Vector3.forward, out var hit)) return;
            
            if (hit.transform.CompareTag("Wall"))
            {
                obstruction = hit.transform;
                obstruction.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            else if (obstruction != player)
                obstruction.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
        }
    }
}
