using System.Collections.Generic;
using Characters;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace BattleSystem.UI
{
    public class APConversionControllerUI : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        public Unit unit;
        [SerializeField] private CharacterGameEvent conversionEvent;
        [SerializeField] private List<GameObject> arrows;

        private void Start() => conversionEvent.AddListener(this);
        
        private void AdjustConversionLevel()
        {
            for (var i = 0; i < arrows.Count; i++)
            {
                if (i >= unit.conversionLevel) { arrows[i].SetActive(false); continue; }
                
                arrows[i].SetActive(true);
            }
        }

        private void OnDisable() => conversionEvent.RemoveListener(this);

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value1 == unit.parent && value2 == conversionEvent)
            {
                AdjustConversionLevel();
            }
        }
    }
}
