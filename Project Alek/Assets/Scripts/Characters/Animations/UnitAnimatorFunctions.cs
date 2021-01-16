using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Random = UnityEngine.Random;
using BattleSystem;
using BattleSystem.Calculators;
using BattleSystem.Mechanics;
using Characters.ElementalTypes;
using Characters.PartyMembers;
using Characters.StatusEffects;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

namespace Characters.Animations
{
    // DO NOT CHANGE THE NAMES OF ANY ANIMATOR FUNCTIONS UNLESS YOU WANT TO MANUALLY UPDATE EVERY ANIMATION EVENT
    public class UnitAnimatorFunctions : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        [SerializeField] private Unit unit;
        [SerializeField] private UnitBase unitBase;

        [ShowInInspector] private bool thisCharacterTurn;
        [ShowInInspector] private bool windowOpen;
        [ShowInInspector] private bool missedWindow;
        [ShowInInspector] private bool hitWindow;

        private int timedAttackCount;

        private ElementalType ElementalCondition => unit != null && unit.isAbility && unit.currentAbility.hasElemental?
            unit.currentAbility.elementalType : null;

        private WeaponDamageType WeaponDamageTypeCondition =>
            unit != null && unit.parent.id == CharacterType.PartyMember && unit.isAbility
                ? ((PartyMember) unit.parent).equippedWeapon.damageType
                : null;

        private bool CheckmateCondition => 
            timedAttackCount == 5 &&
            unit.currentTarget.CurrentState == UnitStates.Weakened && 
            !unit.currentTarget.StatusEffects.Contains(unit.currentAbility.statusEffects[0]);

        private void Awake()
        {
            unit = GetComponent<Unit>();
            GameEventsManager.AddListener(this);
        }

        private void OnEnable()
        {
            BattleInput._controls.Battle.Parry.performed += OnTimedButtonPress;
            BattleInput._controls.Battle.Parry.performed += OnTimedButtonPressSpecialAttack;
        }

        private void OnDisable()
        {
            BattleInput._controls.Battle.Parry.performed -= OnTimedButtonPress;
            BattleInput._controls.Battle.Parry.performed -= OnTimedButtonPressSpecialAttack;
            GameEventsManager.RemoveListener(this);
        }

        private void Update()
        {
            if (windowOpen || unit.animationHandler.isAttacking) return;
            missedWindow = false;
            hitWindow = false;
        }

        private void OnTimedButtonPress(InputAction.CallbackContext ctx)
        {
            if (!thisCharacterTurn) return;
            if (unit.animationHandler.performingSpecial) return;
            if (missedWindow || hitWindow) return;
            if (!windowOpen && !unit.animationHandler.isAttacking) return;
            if (!windowOpen && unit.animationHandler.isAttacking) {
                missedWindow = true;
                SendTimedButtonEventResult(false);
                windowOpen = false;
                return;
            }

            SendTimedButtonEventResult(true);
            hitWindow = true;
            windowOpen = false;
        }

        private void OnTimedButtonPressSpecialAttack(InputAction.CallbackContext ctx)
        {
            if (!thisCharacterTurn) return;
            if (!unit.animationHandler.performingSpecial) return;
            if (!windowOpen) return;

            timedAttackCount += 1;
            Logger.Log("Hit Window! Count: " + timedAttackCount);
        }

        private void SendTimedButtonEventResult(bool result)
        {
            if (unit.parent.id == CharacterType.Enemy)
            {
                if (unit.currentAbility.isMultiTarget)
                {
                    unit.multiHitTargets.ForEach(t => t.Unit.parry = result);

                    if (result == false) unit.multiHitTargets.ForEach
                        (t => t.Unit.onTimedDefense?.Invoke(false));

                    return;
                }
                
                unit.currentTarget.Unit.parry = result;
                if (result == false) unit.currentTarget.Unit.onTimedDefense?.Invoke(false);
            }
            else
            {
                if (unit.currentAbility != null && unit.currentAbility.isMultiTarget)
                {
                    unit.multiHitTargets.ForEach(t => t.Unit.timedAttack = result);
                    unit.onTimedAttack?.Invoke(result);
                }
                else
                {
                    unit.currentTarget.Unit.timedAttack = result;
                    if (!unit.currentTarget.Unit.isCountered) unit.onTimedAttack?.Invoke(result);
                }
            }
        }
        
        // TODO: Separate window into a normal window and a perfect parry window (only for timed defense)
        [UsedImplicitly] private void OpenParryWindow() => windowOpen = true;

        [UsedImplicitly]
        private void CloseParryWindow()
        {
            windowOpen = false;
            unit.currentTarget.Unit.anim.SetTrigger(AnimationHandler.HurtTrigger);
        }

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
                    //if (effect as Checkmate && unit.currentTarget.Unit.currentState != UnitStates.Weakened) continue;
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

            if (!missedWindow && !hitWindow) SendTimedButtonEventResult(false);

            if (!unit.isAbility || !unit.currentAbility.isMultiTarget)
            {
                if (unit.currentTarget == null) return;
                if (unit.currentTarget.Unit.isCountered) RecalculateDamage();
                unit.currentTarget.TakeDamage(unit.currentDamage, ElementalCondition, WeaponDamageTypeCondition);
                return;
            }
            
            unit.multiHitTargets.ForEach(t => t.TakeDamage
                (unit.damageValueList[unit.multiHitTargets.IndexOf(t)], ElementalCondition, WeaponDamageTypeCondition));
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
            
            if (unit.currentTarget == null) return;
            unit.currentDamage = Calculator.CalculateAttackDamage(unitBase, unit.currentTarget);
        }

        [UsedImplicitly] private void PerformSpecialAttack()
        {
            double timedAttackModifier = 1 + timedAttackCount / 10f;
            Logger.Log($"Timed Attack Modifier: {timedAttackModifier}");
            
            var apUsedModifier = 1.0f + unit.specialAttackAP * 5f / 100f;
            Logger.Log($"AP Modifier: {apUsedModifier}");

            var finalDamageAmt = unit.currentDamage * timedAttackModifier;
            finalDamageAmt *= apUsedModifier;
            
            Logger.Log($"Final Damage: {(int)finalDamageAmt}");

            unit.currentTarget.TakeDamageSpecial((int)finalDamageAmt);

            if (CheckmateCondition)
            {
                var checkmate = unit.currentAbility.statusEffects[0];
                checkmate.OnAdded(unit.currentTarget);
                unit.currentTarget.StatusEffects.Add(checkmate);
            }
            
            timedAttackCount = 0;
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterAttacking) return;

            var character = (UnitBase) eventType._character;

            if (character.Unit != unit) { thisCharacterTurn = false; return;}

            thisCharacterTurn = true;
            unitBase = character;
        }
    }
}