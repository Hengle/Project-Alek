using System.Collections.Generic;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using StatusEffects.InhibitingEffect;
using UnityEngine;

namespace Characters
{
    public enum UnitStates { Normal, Susceptible, Intermediate, Checkmate }
    public enum TransitionRequirements { StatusEffect, ElementalDamage }
    public class UnitStateMachine : IGameEventListener<BattleEvents>
    {
        private UnitBase unitBase;
        private SortedList<ScriptableObject, TransitionRequirements> checkmateRequirements;
        private int requirementIndex;

        private TransitionRequirements currentRequirement;

        private UnitStates currentState;
        
        private readonly Stack<UnitStates> states = new Stack<UnitStates>();

        public UnitStateMachine(UnitBase unit, SortedList<ScriptableObject, TransitionRequirements> requirements)
        {
            unitBase = unit;
            checkmateRequirements = requirements;
            
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

            unitBase.Unit.currentState = UnitStates.Normal;
            currentState = states.Pop();
            
            Logger.Log($"{unitBase.characterName}'s current state is {currentState}. Stack count: {states.Count}");

            requirementIndex = 0;
            currentRequirement = checkmateRequirements.Values[0];
        }

        private void EvaluateStateOnRemoval(StatusEffect effect)
        {
            if (currentState != UnitStates.Susceptible) return;
            if (effect.GetType() != typeof(Susceptible)) return;
            
            states.Clear();
            InitializeStack();
        }

        private void EvaluateState(StatusEffect effect)
        {
            switch (currentState)
            {
                case UnitStates.Checkmate: return;
                
                case UnitStates.Normal when effect.GetType() == typeof(Susceptible):
                    currentState = states.Pop();
                    unitBase.onNewState?.Invoke(currentState);
                    unitBase.Unit.currentState = currentState;
                    Logger.Log($"{unitBase.characterName}'s current state is {currentState}. Stack count: {states.Count}");
                    return;
                
                case UnitStates.Normal: return;
            }

            if (currentRequirement != TransitionRequirements.StatusEffect) return;

            var tryGetEffect = checkmateRequirements.Keys[requirementIndex] as StatusEffect;

            if (tryGetEffect == null || !unitBase.Unit.statusEffects.Contains(tryGetEffect)) return;
            
            RequirementMet();
        }

        private void EvaluateState(ElementalType elementalType)
        {
            if (currentState == UnitStates.Checkmate || currentState == UnitStates.Normal) return;
            
            if (currentRequirement != TransitionRequirements.ElementalDamage) return;
            
            var tryGetElement = checkmateRequirements.Keys[requirementIndex] as ElementalType;

            if (tryGetElement == null || tryGetElement != elementalType) return;
            
            RequirementMet();
        }

        private void RequirementMet()
        {
            requirementIndex++;
            currentRequirement = checkmateRequirements.Values[requirementIndex];
            
            currentState = states.Pop();
            unitBase.onNewState?.Invoke(currentState);
            unitBase.Unit.currentState = currentState;
                
            Logger.Log($"{unitBase.characterName}'s current state is {currentState}. Stack count: {states.Count}");
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            if (eventType._battleEventType != BattleEventType.NewRound) return;

            if (currentState != UnitStates.Checkmate) return;
            
            states.Clear();
            InitializeStack();
        }
    }
}