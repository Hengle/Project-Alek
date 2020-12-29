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
        [ShowInInspector] public readonly int _maxLoan = 4;

        private void Start()
        {
            unit = GetComponent<Unit>();
            currentLoan = 0;

            unit.borrowAP += BorrowAP;
            GameEventsManager.AddListener<BattleEvents>(this);
            GameEventsManager.AddListener<CharacterEvents>(this);
        }

        private void BorrowAP(int amount)
        {
            if (amount > _maxLoan) return;
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
            
            if (initialLoan == _maxLoan)
            {
                unit.recoveredFromOverexertion?.Invoke(unit.parent);
            }
            
            initialLoan = 0;
            isRecovered = false;
        }

        private void OnDisable()
        {
            unit.borrowAP = BorrowAP;
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
