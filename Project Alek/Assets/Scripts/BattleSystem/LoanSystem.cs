using System;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public class LoanSystem : MonoBehaviour, IGameEventListener<BattleEvents>, IGameEventListener<CharacterEvents>
    {
        private Unit unit;
        [ShowInInspector] private bool isRecovered;
        [ShowInInspector] private int currentLoan;
        [ShowInInspector] private int initialLoan;

        private void Start()
        {
            unit = GetComponent<Unit>();
            currentLoan = 0;

            unit.borrowAP += BorrowAP;
            unit.parent.onDeath += OnDeath;
            
            GameEventsManager.AddListener<BattleEvents>(this);
            GameEventsManager.AddListener<CharacterEvents>(this);
        }

        private void BorrowAP(int amount)
        {
            if (amount > BattleManager.Instance.globalVariables.maxLoanAmount) return;
            currentLoan = amount;
            initialLoan = amount;
            unit.status = Status.Overexerted;
        }

        private void Recover()
        {
            if (!isRecovered) currentLoan -= 2;
            if (currentLoan < 0) currentLoan = 0;
            if (currentLoan != 0) return;

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
            if (initialLoan == BattleManager.Instance.globalVariables.maxLoanAmount) unit.recoveredFromMaxOverexertion?.Invoke(unit.parent);
            
            initialLoan = 0;
            isRecovered = false;
        }

        private void OnDeath(UnitBase unit)
        {
            currentLoan = 0;
        }

        private void OnDisable()
        {
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
            if (eventType._eventType != CEventType.CharacterTurn) return;
            if (eventType._character != unit.parent) return;
            if (isRecovered) GiveRecoverBenefits();
        }
    }
}
