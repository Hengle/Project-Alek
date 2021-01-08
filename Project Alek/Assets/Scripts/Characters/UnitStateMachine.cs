using Characters.ElementalTypes;
using Characters.StatusEffects;
using MoreMountains.InventoryEngine;

namespace Characters
{
    public enum UnitStates { Normal, Susceptible, Weakened, Checkmate }
    public class UnitStateMachine : IGameEventListener<BattleEvents>, IGameEventListener<CharacterEvents>
    {
        private readonly int maxShieldCount;
        private int currentShieldCount;

        // Weakened state resets once a new round passes and then at the start of the enemy's turn
        private bool newRoundCondition;
        private bool enemyTurnCondition;
        
        private UnitStates currentState;
        
        private readonly UnitBase unitBase;

        public UnitStateMachine(UnitBase unit, int count)
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
            
            GameEventsManager.AddListener<BattleEvents>(this);
            GameEventsManager.AddListener<CharacterEvents>(this);
        }

        private void Reset()
        {
            currentState = UnitStates.Normal;
            unitBase.Unit.currentState = currentState;

            currentShieldCount = maxShieldCount;
            newRoundCondition = false;
            enemyTurnCondition = false;
            
            unitBase.onShieldValueChanged?.Invoke(currentShieldCount);
            Logger.Log($"{unitBase.characterName}'s shield has been reset");
        }
        
        private void EvaluateStateOnRemoval(StatusEffect effect)
        {
            if (currentState == UnitStates.Susceptible)
            {
                if (effect as Susceptible == null) return;
                Reset();
            }

            if (currentState != UnitStates.Checkmate) return;
            if (effect as Checkmate == null) return;
            Reset();
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
                
                case UnitStates.Weakened when effect as Checkmate != null:
                    currentState = UnitStates.Checkmate;
                    unitBase.Unit.currentState = currentState;
                    unitBase.Unit.status = Status.UnableToPerformAction;

                    unitBase.OnNewState(currentState);
                    Logger.Log($"{unitBase.characterName} is in checkmate!");
                    return;
                
                case UnitStates.Normal: return;
            }
        }
        
        private void EvaluateState(ElementalType elementalType)
        {
            if (currentState == UnitStates.Checkmate) return;

            var tryGetElement = unitBase._elementalWeaknesses.ContainsKey(elementalType);
            if (tryGetElement) EvaluateShield();
        }

        private void EvaluateState(WeaponDamageType damageType)
        {
            if (currentState == UnitStates.Checkmate) return;

            var tryGetType = unitBase.damageTypeWeaknesses.Contains(damageType);
            if (tryGetType) EvaluateShield();
        }
        
        private void EvaluateShield()
        {
            if (currentState == UnitStates.Weakened || currentState == UnitStates.Checkmate) return;

            currentShieldCount -= 1;
            unitBase.onShieldValueChanged?.Invoke(currentShieldCount);

            Logger.Log($"{unitBase.characterName}'s shield has been lowered! Shield count: {currentShieldCount}");

            if (currentShieldCount > 0) return;
            
            Logger.Log($"{unitBase.characterName}'s shield has been broken!");

            currentState = UnitStates.Weakened;
            unitBase.Unit.currentState = currentState;
            unitBase.OnNewState(currentState);
        }
        
        public void OnGameEvent(BattleEvents eventType)
        {
            if (eventType._battleEventType == BattleEventType.NewRound)
            {
                if (currentState == UnitStates.Weakened) newRoundCondition = true;
            }

            if (currentState != UnitStates.Weakened) return;
            if (newRoundCondition && enemyTurnCondition) Reset();
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.EnemyTurn)
            {
                if (currentState == UnitStates.Weakened) enemyTurnCondition = true;
            }
            
            if (currentState != UnitStates.Weakened) return;
            if (newRoundCondition && enemyTurnCondition) Reset();
        }
    }
}