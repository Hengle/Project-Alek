using Characters;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.Mechanics
{
    public class APConversionSystem : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        private Unit unit;
        [ShowInInspector] private bool thisUnitTurn;
        private const int MaxConversionAmount = 4;

        private float ConversionFactor
        {
            get
            {
                switch (unit.conversionLevel)
                {
                    case 1: return BattleManager.Instance.globalVariables.conversionFactorLvl1;
                    case 2: return BattleManager.Instance.globalVariables.conversionFactorLvl2;
                    case 3: return BattleManager.Instance.globalVariables.conversionFactorLvl3;
                    case 4: return BattleManager.Instance.globalVariables.conversionFactorLvl4;
                    default: return 1.0f;
                }
            }
        }

        private bool CanDecreaseConversion =>
            thisUnitTurn && BattleManager.Instance.choosingAbility && BattleInput._controls.Menu.LeftSelect.triggered
            && unit.conversionLevel > 0;

        private bool CanIncreaseConversion =>
            thisUnitTurn && BattleManager.Instance.choosingAbility && BattleInput._controls.Menu.RightSelect.triggered
            && unit.conversionLevel < MaxConversionAmount;

        private void Start()
        {
            unit = GetComponent<Unit>();

            BattleInput._controls.Menu.LeftSelect.performed += AdjustConversionAmount;
            BattleInput._controls.Menu.RightSelect.performed += AdjustConversionAmount;
            GameEventsManager.AddListener(this);
        }

        private void AdjustConversionAmount(InputAction.CallbackContext ctx)
        {
            if (CanIncreaseConversion) unit.conversionLevel += 1;
            else if (CanDecreaseConversion) unit.conversionLevel -= 1;
            else return;
            
            unit.conversionFactor = ConversionFactor;
            
            CharacterEvents.Trigger(CEventType.APConversion, unit.parent);
        }

        private void ResetConversion()
        {
            unit.conversionLevel = 0;
            unit.conversionFactor = 1;
            
            CharacterEvents.Trigger(CEventType.APConversion, unit.parent);
        }

        private void OnDisable()
        {
            BattleInput._controls.Menu.LeftSelect.performed -= AdjustConversionAmount;
            BattleInput._controls.Menu.RightSelect.performed -= AdjustConversionAmount;
            GameEventsManager.RemoveListener(this);
        }

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
