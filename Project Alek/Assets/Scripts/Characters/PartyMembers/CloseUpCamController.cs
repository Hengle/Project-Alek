using Cinemachine;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class CloseUpCamController : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        [SerializeField] private CharacterGameEvent characterTurnEvent;

        private CinemachineVirtualCamera cvCamera;
        private Unit unit;

        private void Awake()
        {
            cvCamera = GetComponent<CinemachineVirtualCamera>();
            unit = transform.parent.GetComponentInChildren<Unit>();
            cvCamera.enabled = false;
            characterTurnEvent.AddListener(this);
        }

        private void OnDisable()
        {
            characterTurnEvent.RemoveListener(this);
        }
        
        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == characterTurnEvent && value1.Unit == unit)
            {
                cvCamera.enabled = true;
            }
        }
    }
}