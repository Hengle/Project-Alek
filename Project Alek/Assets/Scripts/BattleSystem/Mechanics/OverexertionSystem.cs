using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem.Mechanics
{
    public class OverexertionSystem : MonoBehaviour, IGameEventListener<BattleEvents>, IGameEventListener<CharacterEvents>
    {
        private Unit unit;
        [ShowInInspector] private bool isRecovered;
        [ShowInInspector] private int currentApExerted;
        [ShowInInspector] private int initialApExerted;

        private void Start()
        {
            unit = GetComponent<Unit>();
            currentApExerted = 0;

            unit.borrowAP += BorrowAP;
            unit.parent.onDeath += OnDeath;
            
            GameEventsManager.AddListener<BattleEvents>(this);
            GameEventsManager.AddListener<CharacterEvents>(this);
        }

        private void BorrowAP(int amount)
        {
            if (amount > BattleManager.Instance.globalVariables.maxLoanAmount) return;
            currentApExerted = amount;
            initialApExerted = amount;
            unit.status = Status.Overexerted;
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
            if (initialApExerted == BattleManager.Instance.globalVariables.maxLoanAmount)
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
            
            GameEventsManager.RemoveListener<BattleEvents>(this);
            GameEventsManager.RemoveListener<CharacterEvents>(this);
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            if (eventType._battleEventType != BattleEventType.NewRound) return;
            if (unit.status == Status.Overexerted) Recover();
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.EndOfTurn && eventType._character == unit.parent &&
                unit.status == Status.Overexerted)
            {
                unit.parent.onApValChanged?.Invoke(currentApExerted);
            }
            if (eventType._eventType != CEventType.CharacterTurn) return;
            if (eventType._character != unit.parent) return;
            if (isRecovered) GiveRecoverBenefits();
        }
    }
}
