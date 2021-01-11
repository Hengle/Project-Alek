using UnityEngine;

namespace Characters.StatusEffects
{
    // TODO: This could probably inherit from a base Timer script since i will probably need different types of timers
    public class StatusEffectTimer : MonoBehaviour, IGameEventListener<BattleEvents>
    {
        [SerializeField] private int timer;

        private UnitBase targetUnit;
        private StatusEffect statusEffect;

        public void SetTimer(StatusEffect effect, UnitBase unit)
        {
            targetUnit = unit;
            statusEffect = effect;
            timer = statusEffect.turnDuration;
            
            GameEventsManager.AddListener(this);
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
 
            targetUnit.onDeath -= RemoveTimerAndEffect;
            targetUnit.Unit.recoveredFromOverexertion -= RemoveTimerAndEffect;
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            switch (eventType._battleEventType)
            {
                case BattleEventType.NewRound: DecrementTimer();
                    break;
                case BattleEventType.LostBattle: RemoveTimerAndEffect(targetUnit);
                    break;
                case BattleEventType.WonBattle: RemoveTimerAndEffect(targetUnit);
                    break;
            }
        }
    }
}