using Cinemachine;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class CloseUpCamController : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        private CinemachineVirtualCamera cvCamera;
        private Unit unit;

        private void Awake()
        {
            cvCamera = GetComponent<CinemachineVirtualCamera>();
            unit = transform.parent.GetComponentInChildren<Unit>();
            cvCamera.enabled = false;
            GameEventsManager.AddListener(this);
        }

        private void ToggleCloseUpCam(CEventType eventType) => cvCamera.enabled = eventType == CEventType.CharacterTurn;
        
        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterTurn &&
                eventType._eventType != CEventType.ChoosingTarget && 
                eventType._eventType != CEventType.EndOfTurn) return;
            
            if (eventType._character.GetType() != typeof(PartyMember)) return;
            
            var character = (PartyMember) eventType._character;
            if (character.Unit == unit) ToggleCloseUpCam(eventType._eventType);
        }
    }
}