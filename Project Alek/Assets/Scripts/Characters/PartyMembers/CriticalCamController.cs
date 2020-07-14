using System;
using Cinemachine;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class CriticalCamController : MonoBehaviour
    {
        public static Action<UnitBase> _onCritical;
        public static Action<UnitBase> _disableCam;
        
        private CinemachineVirtualCamera cvCamera;
        private Unit unit;

        private void Awake()
        {
            cvCamera = GetComponent<CinemachineVirtualCamera>();
            unit = transform.parent.GetComponentInChildren<Unit>();
            
            _onCritical += OnCritical;
            _disableCam += DisableCam;
            cvCamera.enabled = false;
        }

        private void DisableCam(UnitBase unitBase)
        {
            if (unitBase.Unit != unit) return;
            cvCamera.enabled = false;
        }

        private void OnCritical(UnitBase unitBase)
        {
            if (unitBase.Unit != unit) return;
            cvCamera.enabled = true;
        }
        
    }
}