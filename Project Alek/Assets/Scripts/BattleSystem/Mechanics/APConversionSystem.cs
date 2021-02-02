using Characters;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.Mechanics
{
    public class APConversionSystem : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        private Unit unit;
        [SerializeField] private CharacterGameEvent characterTurnEvent;
        [SerializeField] private CharacterGameEvent endOfTurnEvent;
        [SerializeField] private CharacterGameEvent conversionEvent;
        
        [ShowInInspector] private bool thisUnitTurn;
        private const int MaxConversionAmount = 4;

        private float ConversionFactor
        {
            get
            {
                switch (unit.conversionLevel)
                {
                    case 1: return GlobalVariables.Instance.conversionFactorLvl1;
                    case 2: return GlobalVariables.Instance.conversionFactorLvl2;
                    case 3: return GlobalVariables.Instance.conversionFactorLvl3;
                    case 4: return GlobalVariables.Instance.conversionFactorLvl4;
                    default: return 1.0f;
                }
            }
        }

        private bool CanDecreaseConversion =>
            thisUnitTurn && BattleEngine.Instance.choosingAbility && BattleInput._controls.Battle.LeftShoulder.triggered
            && unit.conversionLevel > 0;

        private bool CanIncreaseConversion =>
            thisUnitTurn && BattleEngine.Instance.choosingAbility && BattleInput._controls.Battle.RightShoulder.triggered
            && unit.conversionLevel < MaxConversionAmount;

        private bool CancelCondition =>
            thisUnitTurn && BattleEngine.Instance.choosingAbility && BattleInput._controls.Battle.Move.triggered && unit.conversionLevel > 0;

        private void Start()
        {
            unit = GetComponent<Unit>();
        }

        private void OnEnable()
        {
            BattleInput._controls.Battle.LeftShoulder.performed += AdjustConversionAmount;
            BattleInput._controls.Battle.RightShoulder.performed += AdjustConversionAmount;
            BattleInput._controls.Battle.Move.performed += AdjustConversionAmount;
            characterTurnEvent.AddListener(this);
            endOfTurnEvent.AddListener(this);
        }

        private void AdjustConversionAmount(InputAction.CallbackContext ctx)
        {
            if (CancelCondition) ResetConversion();
            if (CanIncreaseConversion) unit.conversionLevel += 1;
            else if (CanDecreaseConversion) unit.conversionLevel -= 1;
            else return;
            
            unit.conversionFactor = ConversionFactor;
            conversionEvent.Raise(unit.parent, conversionEvent);
        }

        private void ResetConversion()
        {
            unit.conversionLevel = 0;
            unit.conversionFactor = 1;
            
            conversionEvent.Raise(unit.parent, conversionEvent);
        }

        private void OnDisable()
        {
            BattleInput._controls.Battle.LeftShoulder.performed -= AdjustConversionAmount;
            BattleInput._controls.Battle.RightShoulder.performed -= AdjustConversionAmount;
            
            characterTurnEvent.RemoveListener(this);
            endOfTurnEvent.RemoveListener(this);
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == characterTurnEvent && value1 == unit.parent)
            {
                thisUnitTurn = true;
                if (unit.conversionLevel > 0) ResetConversion();
            }
            else if (value2 == endOfTurnEvent && value1 == unit.parent)
            {
                thisUnitTurn = false;
                if (unit.conversionLevel > 0) ResetConversion();
            }
        }
    }
}
