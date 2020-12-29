using System;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public class APConversionSystem : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        private Unit unit;
        [ShowInInspector] private bool thisUnitTurn;
        private int maxConversionAmount = 4;

        private float ConversionFactor
        {
            get
            {
                switch (unit.conversionAmount)
                {
                    case 1: return 1.2f;
                    case 2: return 1.4f;
                    case 3: return 1.8f;
                    case 4: return 2.25f;
                    default: return 1.0f;
                }
            }
        }

        private bool CanDecreaseConversion =>
            BattleInput._controls.Menu.LeftSelect.triggered
            && unit.conversionAmount > 0;
        
        private bool CanIncreaseConversion =>
            BattleInput._controls.Menu.RightSelect.triggered
            && unit.conversionAmount < maxConversionAmount;

        void Start()
        {
            unit = GetComponent<Unit>();
            GameEventsManager.AddListener(this);
        }

        void Update()
        {
            if (!thisUnitTurn) return;
            if (!BattleManager._choosingAbility) return;
            if (CanDecreaseConversion) AdjustConversionAmount(false);
            if (CanIncreaseConversion) AdjustConversionAmount(true);
        }

        private void AdjustConversionAmount(bool increase)
        {
            if (increase) unit.conversionAmount += 1;
            else unit.conversionAmount -= 1;

            unit.conversionFactor = ConversionFactor;
        }

        private void ResetConversion()
        {
            unit.conversionAmount = 0;
            unit.conversionFactor = 1;
        }

        private void OnDisable() => GameEventsManager.RemoveListener(this);

        public void OnGameEvent(CharacterEvents eventType)
        {
            switch (eventType._eventType)
            {
                case CEventType.CharacterTurn when eventType._character == unit.parent:
                    thisUnitTurn = true;
                    ResetConversion();
                    break;
                case CEventType.EndOfTurn when eventType._character == unit.parent:
                    thisUnitTurn = false;
                    ResetConversion();
                    break;
            }
        }
    }
}
