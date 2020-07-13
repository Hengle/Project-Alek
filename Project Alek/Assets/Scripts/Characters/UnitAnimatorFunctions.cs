using System.Collections.Generic;
using System.Linq;
using BattleSystem.Calculator;
using Characters.ElementalTypes;
using JetBrains.Annotations;
using UnityEngine;

namespace Characters
{
    public class UnitAnimatorFunctions : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        private Unit unit;
        private UnitBase unitBase;

        private ElementalType ElementalCondition => unit != null && unit.isAbility && unit.currentAbility.hasElemental ?
            unit.currentAbility.elementalType : null;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            GameEventsManager.AddListener(this);
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterAttacking) return;

            var character = (UnitBase) eventType._character;
            if (character.Unit != unit) return;

            unitBase = character;
        }
        
        [UsedImplicitly] public void TryToInflictStatusEffect()
        {
            if (!unit.isAbility || !unit.currentAbility.hasStatusEffect) return;
            
            if (!unit.currentAbility.isMultiTarget)
            {
                if (unit.currentTarget.Unit.attackerHasMissed || unit.currentTarget.IsDead) return;

                foreach (var effect in from effect in unit.currentAbility.statusEffects
                    where !(from statusEffect in unit.currentTarget.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any()
                    
                    let randomValue = Random.value 
                    where !(randomValue > unit.currentAbility.chanceOfInfliction) select effect)
                {
                    effect.OnAdded(unit.currentTarget);
                    unit.currentTarget.Unit.statusEffects.Add(effect);
                    // unit.currentTarget.EvaluateState();
                }
            }
            
            foreach (var target in unit.multiHitTargets.Where(target => !target.Unit.attackerHasMissed && !target.IsDead))
            {
                foreach (var effect in from effect in unit.currentAbility.statusEffects
                    where !(from statusEffect in target.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any() 
                    
                    let randomValue = Random.value 
                    where !(randomValue > unit.currentAbility.chanceOfInfliction) select effect)
                {
                    effect.OnAdded(target);
                    target.Unit.statusEffects.Add(effect);
                }
            }
        }

        [UsedImplicitly] public void TargetTakeDamage()
        {
            if (!unit.isAbility || !unit.currentAbility.isMultiTarget) {
                unit.currentTarget.TakeDamage(unit.currentDamage, ElementalCondition);
                unit.isCrit = false;
                return;
            }

            for (var i = 0; i < unit.multiHitTargets.Count; i++)
                unit.multiHitTargets[i].TakeDamage(unit.damageValueList[i], ElementalCondition);

            unit.isCrit = false;
        }

        [UsedImplicitly] public void RecalculateDamage() 
        {
            if (unit.isAbility && unit.currentAbility.isMultiTarget)
            {
                unit.damageValueList = new List<int>();
                foreach (var target in unit.multiHitTargets) 
                    unit.damageValueList.Add(Calculator.CalculateAttackDamage(unitBase, target));
                return;
            }
            
            unit.currentDamage = Calculator.CalculateAttackDamage(unitBase, unit.currentTarget);
            if (unitBase.id != CharacterType.PartyMember || !unit.isCrit) return;
            TimeManager._slowTimeCrit = true;
        }
    }
}