using System.Collections.Generic;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using UnityEngine;

namespace Characters
{
    public enum UnitStates { Normal, Susceptible, Intermediate, Checkmate }
    public enum TransitionRequirements { StatusEffect, ElementalDamage }
    public class UnitStateMachine : IGameEventListener<BattleEvents>
    {
        private int requirementIndex;
        
        private TransitionRequirements currentRequirement;
        private UnitStates currentState;
        
        private readonly UnitBase unitBase;

        private readonly List<KeyValuePair<ScriptableObject, TransitionRequirements>> checkmateRequirements =
            new List<KeyValuePair<ScriptableObject, TransitionRequirements>>();

        private readonly Stack<UnitStates> states = new Stack<UnitStates>();

        public UnitStateMachine(UnitBase unit, IReadOnlyList<ScriptableObject> objects, IReadOnlyList<TransitionRequirements> requirements)
        {
            unitBase = unit;
            
            for (var i = 0; i < objects.Count; i++)
            {
                checkmateRequirements.Add(new KeyValuePair<ScriptableObject, TransitionRequirements>(objects[i], requirements[i]));
            }
            
            InitializeStack();

            unitBase.onElementalDamageReceived += EvaluateState;
            unitBase.onStatusEffectReceived += EvaluateState;
            unitBase.onStatusEffectRemoved += EvaluateStateOnRemoval;
            
            GameEventsManager.AddListener(this);
        }

        private void InitializeStack()
        {
            states.Push(UnitStates.Checkmate);

            for (var i = 0; i < checkmateRequirements.Count; i++)
            {
                states.Push(UnitStates.Intermediate);
            }
            
            states.Push(UnitStates.Susceptible);
            states.Push(UnitStates.Normal);
            
            currentState = states.Pop();
            unitBase.Unit.currentState = currentState;

            requirementIndex = 0;
            currentRequirement = checkmateRequirements[requirementIndex].Value;
        }

        private void EvaluateStateOnRemoval(StatusEffect effect)
        {
            if (currentState != UnitStates.Susceptible) return;
            if (effect as Susceptible == null) return;
            
            states.Clear();
            InitializeStack();
        }
        
        private void EvaluateState(StatusEffect effect)
        {
            switch (currentState)
            {
                case UnitStates.Checkmate: return;
                
                case UnitStates.Normal when effect as Susceptible != null:
                    currentState = states.Pop();
                    unitBase.onNewState?.Invoke(currentState);
                    unitBase.Unit.currentState = currentState;
                    return;
                
                case UnitStates.Normal: return;
            }
            
            if (currentRequirement != TransitionRequirements.StatusEffect) return;

            var tryGetEffect = checkmateRequirements[requirementIndex].Key as StatusEffect;

            if (tryGetEffect != null && tryGetEffect == effect) RequirementMet();
        }

        private void EvaluateState(ElementalType elementalType)
        {
            if (currentState == UnitStates.Checkmate || currentState == UnitStates.Normal) return;
            if (currentRequirement != TransitionRequirements.ElementalDamage) return;
            
            var tryGetElement = checkmateRequirements[requirementIndex].Key as ElementalType;

            if (tryGetElement != null && tryGetElement == elementalType) RequirementMet();
        }

        private void RequirementMet()
        {
            currentState = states.Pop();

            Logger.Log($"Requirement met for {unitBase.characterName}! Stack count: {states.Count}");

            if (states.Peek() != UnitStates.Checkmate)
            {
                requirementIndex++;
                currentRequirement = checkmateRequirements[requirementIndex].Value;
                
                unitBase.Unit.currentState = currentState;
                unitBase.onNewState?.Invoke(currentState);
                return;
            }
            
            currentState = states.Pop();
            unitBase.Unit.currentState = currentState;
            unitBase.onNewState?.Invoke(currentState);
            unitBase.Unit.status = Status.UnableToPerformAction;

            var checkmate = ScriptableObject.CreateInstance<Checkmate>();
            checkmate.name = "Checkmate";
            checkmate.turnDuration = 3;
            checkmate.OnAdded(unitBase);
            unitBase.Unit.statusEffects.Add(checkmate);
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            if (eventType._battleEventType != BattleEventType.NewRound) return;
            if (currentState != UnitStates.Checkmate) return;
            
            // Prevent unit from being checkmated again
            states.Clear();
            
            unitBase.onElementalDamageReceived -= EvaluateState;
            unitBase.onStatusEffectReceived -= EvaluateState;
            unitBase.onStatusEffectRemoved -= EvaluateStateOnRemoval;
            
            GameEventsManager.RemoveListener(this);
            
            unitBase.Unit.status = Status.Normal;
        }
    }
}