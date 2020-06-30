using BattleSystem;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    // This could probably inherit from a base Timer script since i will probably need different types of timers
    public class StatusEffectTimer : MonoBehaviour
    {
        [SerializeField] private int timer;

        private UnitBase targetUnit;
        private StatusEffect statusEffect;

        public void SetTimer(StatusEffect effect, UnitBase unit)
        {
            targetUnit = unit;
            statusEffect = effect;
            timer = statusEffect.duration;
            BattleManager.NewRound += DecrementTimer;
        }

        private void DecrementTimer()
        {
            if (targetUnit.Unit.status == Status.Dead) {
                RemoveTimerAndEffect();
                return;
            }
            if (timer > 0) timer--;
            if (timer != 0) return;
            
            RemoveTimerAndEffect();
        }

        private void RemoveTimerAndEffect() {
            targetUnit.RemoveStatusEffect(statusEffect);
            BattleManager.NewRound -= DecrementTimer;
        }
    }
}