using Characters;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace BattleSystem
{
    public class OverexertionSystem : MonoBehaviour, IGameEventListener<BattleEvent>,IGameEventListener<UnitBase,CharacterGameEvent>
    {
        private Unit unit;
        [ShowInInspector] private bool isRecovered;
        [ShowInInspector] private int currentApExerted;
        [ShowInInspector] private int initialApExerted;
        
        [SerializeField] private BattleGameEvent battleEvent;
        [SerializeField] private CharacterGameEvent characterTurnEvent;
        [SerializeField] private CharacterGameEvent endOfTurnEvent;

        private void Start()
        {
            unit = GetComponent<Unit>();
            currentApExerted = 0;

            unit.borrowAP += BorrowAP;
            unit.parent.onDeath += OnDeath;
            
            battleEvent.AddListener(this);
            characterTurnEvent.AddListener(this);
            endOfTurnEvent.AddListener(this);
        }

        private void BorrowAP(int amount)
        {
            if (amount > GlobalVariables.Instance.maxLoanAmount) return;
            currentApExerted = amount;
            initialApExerted = amount;
            unit.isBorrowing = true;
        }

        private void Recover()
        {
            if (!isRecovered) currentApExerted -= 2;
            if (currentApExerted < 0) currentApExerted = 0;
            
            unit.parent.onApValChanged?.Invoke(currentApExerted);
            if (currentApExerted != 0) return;

            if (!isRecovered)
            {
                isRecovered = true;
                return;
            }
            
            unit.status = Status.Normal;
        }

        private void GiveRecoverBenefits()
        {
            unit.recoveredFromOverexertion?.Invoke(unit.parent);
            if (initialApExerted == GlobalVariables.Instance.maxLoanAmount)
                unit.recoveredFromMaxOverexertion?.Invoke(unit.parent);
            
            initialApExerted = 0;
            isRecovered = false;
        }

        private void OnDeath(UnitBase unitBase)
        {
            currentApExerted = 0;
        }

        private void OnDisable()
        {
            if (!unit) return;
            unit.borrowAP -= BorrowAP;
            unit.parent.onDeath -= OnDeath;
            
            battleEvent.RemoveListener(this);
            characterTurnEvent.RemoveListener(this);
            endOfTurnEvent.RemoveListener(this);
        }
    
        public void OnEventRaised(BattleEvent value)
        {
            if (value != BattleEvent.NewRound) return;
            if (unit.status == Status.Overexerted) Recover();
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == endOfTurnEvent && value1 == unit.parent && unit.isBorrowing)
            {
                unit.status = Status.Overexerted;
                unit.parent.onApValChanged?.Invoke(currentApExerted);
            }
            
            if (value2 != characterTurnEvent) return;
            if (value1 != unit.parent) return;
            if (isRecovered) GiveRecoverBenefits();
            else if (unit.isBorrowing) unit.isBorrowing = false;
        }
    }
}
