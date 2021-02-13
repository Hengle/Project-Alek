using System.Linq;
using Characters;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace BattleSystem.Mechanics
{
    public enum UnitStates { Normal, Susceptible, Weakened, Checkmate }
    public class BreakSystem : MonoBehaviour, IGameEventListener<BattleEvent>, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        [SerializeField] private BattleGameEvent battleEvent;
        [SerializeField] private CharacterGameEvent enemyTurnEvent;
        
        private int maxShieldCount;
        private int currentShieldCount;

        // Weakened state resets once a new round passes and then at the start of the enemy's turn
        private bool newRoundCondition;
        private bool enemyTurnCondition;
        
        private UnitStates currentState;
        private UnitBase unitBase;
        
        public void Init(UnitBase unit, int count)
        {
            unitBase = unit;
            maxShieldCount = count;
            currentShieldCount = count;

            currentState = UnitStates.Normal;
            unitBase.Unit.currentState = currentState;

            unitBase.onWeaponDamageTypeReceived += EvaluateState;
            unitBase.onElementalDamageReceived += EvaluateState;
            unitBase.onStatusEffectReceived += EvaluateState;
            unitBase.onStatusEffectRemoved += EvaluateStateOnRemoval;
            
            battleEvent.AddListener(this);
            enemyTurnEvent.AddListener(this);
        }

        private void OnDisable()
        {
            battleEvent.RemoveListener(this);
            enemyTurnEvent.RemoveListener(this);
        }

        private void RestoreShield()
        {
            currentState = UnitStates.Normal;
            unitBase.Unit.currentState = UnitStates.Normal;

            currentShieldCount = maxShieldCount;
            newRoundCondition = false;
            enemyTurnCondition = false;
            
            unitBase.onShieldRestored?.Invoke();
            unitBase.onShieldValueChanged?.Invoke(currentShieldCount);
        }

        private void Checkmate()
        {
            TimeManager.SlowMotionSequence(0.3f, 0.8f);
            currentState = UnitStates.Checkmate;
            unitBase.Unit.currentState = currentState;
            unitBase.Unit.status = Status.UnableToPerformAction;

            unitBase.OnNewState(currentState);
        }
        
        private void EvaluateStateOnRemoval(StatusEffect effect)
        {
            if (currentState == UnitStates.Susceptible)
            {
                if (effect as Susceptible == null) return;
                RestoreShield();
            }

            if (currentState != UnitStates.Checkmate) return;
            if (effect as Checkmate == null) return;
            RestoreShield();
        }
        
        private void EvaluateState(StatusEffect effect)
        {
            switch (currentState)
            {
                case UnitStates.Checkmate: return;
                
                case UnitStates.Normal when effect as Susceptible != null:
                    currentState = UnitStates.Susceptible;
                    unitBase.Unit.currentState = currentState;

                    unitBase.OnNewState(currentState);
                    return;
                
                case UnitStates.Weakened when effect as Checkmate != null: Checkmate();
                    return;
                
                case UnitStates.Normal: return;
            }
        }
        
        private void EvaluateState(ElementalType elementalType)
        {
            if (currentState == UnitStates.Checkmate) return;

            var tryGetElement = unitBase._elementalWeaknesses.Any
                (kvp => kvp.Key._type == elementalType);
            
            if (tryGetElement) EvaluateShield();
        }

        private void EvaluateState(WeaponDamageType damageType)
        {
            if (currentState == UnitStates.Checkmate) return;
            
            var tryGetType = unitBase._damageTypeWeaknesses.ContainsKey(damageType);
            if (tryGetType) EvaluateShield();
        }
        
        private void EvaluateShield()
        {
            if (currentState == UnitStates.Weakened || currentState == UnitStates.Checkmate) return;

            currentShieldCount -= 1;
            unitBase.onShieldValueChanged?.Invoke(currentShieldCount);

            if (currentShieldCount > 0) return;

            unitBase.onShieldBroken?.Invoke();
            currentState = UnitStates.Weakened;
            unitBase.Unit.currentState = currentState;
            unitBase.OnNewState(currentState);
        }

        public void OnEventRaised(BattleEvent value)
        {
            if (value == BattleEvent.NewRound)
            {
                if (currentState == UnitStates.Weakened) newRoundCondition = true;
            }

            if (currentState != UnitStates.Weakened) return;
            if (newRoundCondition && enemyTurnCondition) RestoreShield();
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == enemyTurnEvent && value1 == unitBase)
            {
                if (currentState == UnitStates.Weakened) enemyTurnCondition = true;
            }
            
            if (currentState != UnitStates.Weakened) return;
            if (newRoundCondition && enemyTurnCondition) RestoreShield();
        }
    }
}