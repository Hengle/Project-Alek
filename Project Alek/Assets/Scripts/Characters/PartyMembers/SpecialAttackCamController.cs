using System;
using Cinemachine;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class SpecialAttackCamController : MonoBehaviour
    {
        public static Action<UnitBase> _onSpecialAttack;
        public static Action<UnitBase> _onLevelUpCloseUp;
        public static Action<UnitBase> _disableCam;
        
        private CinemachineVirtualCamera cvCamera;
        private Unit unit;

        private void Awake()
        {
            cvCamera = GetComponent<CinemachineVirtualCamera>();
            unit = transform.parent.GetComponentInChildren<Unit>();
            
            _onSpecialAttack += OnSpecialAttack;
            _onLevelUpCloseUp += OnLevelUpCloseUp;
            _disableCam += DisableCam;
            cvCamera.enabled = false;
        }

        private void DisableCam(UnitBase unitBase)
        {
            if (unitBase.Unit != unit) return;
            cvCamera.enabled = false;
        }

        private void OnSpecialAttack(UnitBase unitBase)
        {
            if (unitBase.Unit != unit) return;
            cvCamera.enabled = true;
        }

        private void OnLevelUpCloseUp(UnitBase unitBase)
        {
            if (unitBase.Unit != unit) return;
            var transposer = cvCamera.GetCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset = new Vector3(2, 1, -3);
            cvCamera.enabled = true;
        }

    }
}