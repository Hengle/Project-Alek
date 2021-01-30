using ScriptableObjectArchitecture;
using UnityEngine;

namespace Characters.StatusEffects
{
    // TODO: This could probably inherit from a base Timer script since i will probably need different types of timers
    public class StatusEffectTimer : MonoBehaviour, IGameEventListener<BattleEvent>
    {
        [SerializeField] private BattleGameEvent battleEvent;
        [SerializeField] private int timer;

        private UnitBase targetUnit;
        private StatusEffect statusEffect;

        public void SetTimer(StatusEffect effect, UnitBase unit)
        {
            targetUnit = unit;
            statusEffect = effect;
            timer = statusEffect.turnDuration;
    
            battleEvent.AddListener(this);
            
            targetUnit.onDeath += RemoveTimerAndEffect;
            targetUnit.Unit.recoveredFromOverexertion += RemoveTimerAndEffect;
        }

        private void DecrementTimer()
        {
            if (targetUnit.Unit.status == Status.Dead)
            {
                RemoveTimerAndEffect(targetUnit);
                return;
            }
            
            if (timer > 0) timer--;
            if (timer != 0) return;
            
            RemoveTimerAndEffect(targetUnit);
        }

        private void RemoveTimerAndEffect(UnitBase target)
        {
            if (target != targetUnit) return;
            if (!target.Unit.statusEffects.Contains(statusEffect)) return;

            target.Unit.statusEffects.Remove(statusEffect);
            statusEffect.OnRemoval(target);
        }

        private void OnDisable()
        {
            if (targetUnit == null) return;
 
            battleEvent.RemoveListener(this);
            targetUnit.onDeath -= RemoveTimerAndEffect;
            targetUnit.Unit.recoveredFromOverexertion -= RemoveTimerAndEffect;
        }

        public void OnEventRaised(BattleEvent value)
        {
            switch (value)
            {
                case BattleEvent.NewRound: DecrementTimer();
                    break;
                case BattleEvent.LostBattle: RemoveTimerAndEffect(targetUnit);
                    break;
                case BattleEvent.WonBattle: RemoveTimerAndEffect(targetUnit);
                    break;
            }
        }
    }
}