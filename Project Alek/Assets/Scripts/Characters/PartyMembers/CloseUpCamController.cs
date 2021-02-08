using Cinemachine;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class CloseUpCamController : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        [SerializeField] private CharacterGameEvent characterTurnEvent;

        private CinemachineVirtualCamera cvCamera;
        private CinemachineTransposer cvTransposer;
        private Unit unit;

        private void Awake()
        {
            cvCamera = GetComponent<CinemachineVirtualCamera>();
            cvTransposer = cvCamera.GetCinemachineComponent<CinemachineTransposer>();
            unit = transform.parent.GetComponentInChildren<Unit>();
            cvCamera.enabled = false;
            SetDistance();
            characterTurnEvent.AddListener(this);
        }

        private void SetDistance()
        {
            var aspectRatio = Screen.width + " x " + Screen.height;

            if (aspectRatio == "720 x 480")
            {
                cvTransposer.m_FollowOffset.z = -9.5f;
            }
            else cvTransposer.m_FollowOffset.z = -8;
        }

        private void OnDisable()
        {
            characterTurnEvent.RemoveListener(this);
        }
        
        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == characterTurnEvent && value1.Unit == unit)
            {
                SetDistance();
                cvCamera.enabled = true;
            }
        }
    }
}