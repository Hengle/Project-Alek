using System;
using BattleSystem;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    // This could probably inherit from a base Timer script since i will probably need different types of timers
    public class StatusEffectTimer : MonoBehaviour, IGameEventListener<BattleEvent>
    {
        [SerializeField] private int timer;

        private UnitBase targetUnit;
        private StatusEffect statusEffect;

        public void SetTimer(StatusEffect effect, UnitBase unit)
        {
            targetUnit = unit;
            statusEffect = effect;
            timer = statusEffect.duration;
            GameEventsManager.AddListener(this);
            //BattleManager.NewRound += DecrementTimer;
            targetUnit.onDeath += RemoveTimerAndEffect;
        }

        private void DecrementTimer()
        {
            Logger.Log("Decrementing timer for " + targetUnit.characterName);
            if (targetUnit.Unit.status == Status.Dead) {
                RemoveTimerAndEffect(targetUnit);
                return;
            }
            if (timer > 0) timer--;
            if (timer != 0) return;
            
            RemoveTimerAndEffect(targetUnit);
        }

        private void RemoveTimerAndEffect(UnitBase target) {
            if (target != targetUnit) return;
            Logger.Log($"Removing {statusEffect.name} for " + target.characterName);
            targetUnit.RemoveStatusEffect(statusEffect);
            //BattleManager.NewRound -= DecrementTimer;
            GameEventsManager.RemoveListener(this);
        }

        public void OnGameEvent(BattleEvent eventType)
        {
            Logger.Log("Status Effect Timer has been notified of new round event");
            DecrementTimer();
        }
    }
}