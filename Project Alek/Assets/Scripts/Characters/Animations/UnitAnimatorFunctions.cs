﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Random = UnityEngine.Random;
using BattleSystem;
using Characters.ElementalTypes;

namespace Characters.Animations
{
    // DO NOT CHANGE THE NAMES OF ANY ANIMATOR FUNCTIONS UNLESS YOU WANT TO MANUALLY UPDATE EVERY ANIMATION EVENT
    public class UnitAnimatorFunctions : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        private Unit unit;
        private UnitBase unitBase;

        private bool windowOpen;
        private bool missedWindow;
        private bool hitWindow;

        private ElementalType ElementalCondition => unit != null && unit.isAbility && unit.currentAbility.hasElemental?
            unit.currentAbility.elementalType : null;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            GameEventsManager.AddListener(this);
        }
        
        private void OnEnable() => BattleInput._controls.Battle.Parry.performed += ctx => OnParry();

        private void OnParry()
        {
            if (missedWindow || hitWindow) return;
            if (!windowOpen && !unit.animationHandler.isAttacking) return;
            if (!windowOpen && unit.animationHandler.isAttacking) {
                missedWindow = true;
                Logger.Log("You missed the parry window...");
                return;
            }

            if (unit.currentAbility.isMultiTarget)
            {
                hitWindow = true;
                unit.multiHitTargets.ForEach(t => t.Unit.parry = true);
                Logger.Log("The whole party hit the parry window!");
                return;
            }
            
            unit.currentTarget.Unit.parry = true;
            hitWindow = true;
            windowOpen = false;
            Logger.Log("Hit the parry window!");
            
        }
        
        [UsedImplicitly] private void OpenParryWindow() => windowOpen = true;

        [UsedImplicitly] private void TryToInflictStatusEffect()
        {
            if (!unit.isAbility || !unit.currentAbility.hasStatusEffect) return;
            
            if (!unit.currentAbility.isMultiTarget)
            {
                if (unit.currentTarget.Unit.attackerHasMissed || unit.currentTarget.IsDead) return;

                foreach (var effect in from effect in unit.currentAbility.statusEffects
                    where !(from statusEffect in unit.currentTarget.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any()
                    
                    let randomValue = Random.value
                    let modifier = effect.StatusEffectModifier(unit.currentTarget)

                    where !(randomValue > unit.currentAbility.chanceOfInfliction * modifier) select effect)
                {
                    effect.OnAdded(unit.currentTarget);
                    unit.currentTarget.Unit.statusEffects.Add(effect);
                }
            }
            
            foreach (var target in unit.multiHitTargets.Where(target => !target.Unit.attackerHasMissed && !target.IsDead))
            {
                foreach (var effect in from effect in unit.currentAbility.statusEffects
                    where !(from statusEffect in target.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any() 
                    
                    let randomValue = Random.value
                    let modifier = effect.StatusEffectModifier(target)
                    
                    where !(randomValue > unit.currentAbility.chanceOfInfliction * modifier) select effect)
                {
                    effect.OnAdded(target);
                    target.Unit.statusEffects.Add(effect);
                }
            }
        }

        [UsedImplicitly] private void TargetTakeDamage()
        {
            windowOpen = false;

            if (!unit.isAbility || !unit.currentAbility.isMultiTarget) 
            {
                unit.currentTarget.TakeDamage(unit.currentDamage, ElementalCondition);
                unit.isCrit = false;
                return;
            }
            
            unit.multiHitTargets.ForEach(t => t.TakeDamage
                (unit.damageValueList[unit.multiHitTargets.IndexOf(t)], ElementalCondition));

            unit.isCrit = false;
            hitWindow = false;
            missedWindow = false;
        }

        [UsedImplicitly] private void RecalculateDamage() 
        {
            if (unit.isAbility && unit.currentAbility.isMultiTarget)
            {
                unit.damageValueList = new List<int>();
                
                unit.multiHitTargets.ForEach(t => unit.damageValueList.Add
                    (Calculator.CalculateAttackDamage(unitBase, t)));

                return;
            }
            
            unit.currentDamage = Calculator.CalculateAttackDamage(unitBase, unit.currentTarget);
            
            if (unitBase.id != CharacterType.PartyMember || !unit.isCrit) return;
            TimeManager._slowTimeCrit = true;
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterAttacking) return;

            var character = (UnitBase) eventType._character;
            if (character.Unit != unit) return;

            unitBase = character;
        }
    }
}