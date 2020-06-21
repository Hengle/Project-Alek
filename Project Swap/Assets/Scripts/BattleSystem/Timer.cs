using Characters;
using StatusEffects;
using UnityEngine;

namespace BattleSystem
{
    public class Timer
    {
        private int timer;
        private readonly Unit targetUnit;
        private readonly StatusEffect statusEffect;
        
        public Timer(StatusEffect effect, Unit unit)
        {
            targetUnit = unit;
            statusEffect = effect;
            timer = statusEffect.duration;
        }

        public void DecrementTimer()
        {
            if (timer > 0) timer--;
            Debug.Log($"{targetUnit.unitName} has {timer} rounds left for {statusEffect.name.ToLower()} damage");

            if (timer != 0) return;
            targetUnit.RemoveStatusEffect(statusEffect);
            BattleHandler.newRound.RemoveListener(DecrementTimer);
        }
    }
}