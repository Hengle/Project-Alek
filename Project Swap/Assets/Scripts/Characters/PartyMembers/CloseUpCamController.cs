using Cinemachine;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class CloseUpCamController : MonoBehaviour
    {
        private CinemachineVirtualCamera cvCamera;
        private Unit unit;

        private void Awake() {
            cvCamera = GetComponent<CinemachineVirtualCamera>();
            unit = transform.parent.GetComponentInChildren<Unit>();
        }

        private void Update() => cvCamera.enabled =
            unit.battlePanelRef.activeSelf &&
            unit.battlePanelRef.transform.GetChild(1).gameObject.activeSelf;
    }
}