using System;
using BattleSystem;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    public class StatusEffectTimer : MonoBehaviour // How will this work for buff/debuffs with no icon?
    {
        [SerializeField] private int timer;

        private Unit targetUnit;
        private StatusEffect statusEffect;

        public void SetTimer(StatusEffect effect, Unit unit)
        {
            targetUnit = unit;
            statusEffect = effect;
            timer = statusEffect.duration;
            BattleHandler.newRound.AddListener(DecrementTimer);
        }

        private void DecrementTimer()
        {
            if (timer > 0) timer--;
            if (timer != 0) return;
            targetUnit.RemoveStatusEffect(statusEffect);
            BattleHandler.newRound.RemoveListener(DecrementTimer);
        }
    }
}