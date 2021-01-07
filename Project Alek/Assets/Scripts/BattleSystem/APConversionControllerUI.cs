using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace BattleSystem
{
    public class APConversionControllerUI : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        public Unit unit;
        [SerializeField] private List<GameObject> arrows;
        
        void Start()
        {
            GameEventsManager.AddListener(this);
        }

        private void AdjustConversionLevel()
        {
            for (var i = 0; i < arrows.Count; i++)
            {
                if (i >= unit.conversionLevel)
                {
                    arrows[i].SetActive(false);
                    continue;
                }
                
                arrows[i].SetActive(true);
            }
        }

        private void OnDisable()
        {
            GameEventsManager.RemoveListener(this);
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.APConversion && eventType._character == unit.parent)
            {
                AdjustConversionLevel();
            }
        }
    }
}
