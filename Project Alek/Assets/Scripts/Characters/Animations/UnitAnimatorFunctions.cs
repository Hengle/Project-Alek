using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Audio;
using JetBrains.Annotations;
using Random = UnityEngine.Random;
using BattleSystem;
using Characters.PartyMembers;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Utils;

namespace Characters
{
    // DO NOT CHANGE THE NAMES OF ANY ANIMATOR FUNCTIONS UNLESS YOU WANT TO MANUALLY UPDATE EVERY ANIMATION EVENT
    public class UnitAnimatorFunctions : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        [SerializeField] private CharacterGameEvent characterAttackEvent;
        
        [SerializeField] private Unit unit;
        [SerializeField] private UnitBase unitBase;

        [ShowInInspector] private bool thisCharacterTurn;
        [ShowInInspector] private bool windowOpen;
        [ShowInInspector] private bool missedWindow;
        [ShowInInspector] private bool hitWindow;
        [ShowInInspector] private bool isOverride;

        private int timedAttackCount;

        private ElementalType ElementalCondition => unit != null && unit.isAbility && unit.currentAbility.hasElemental?
            unit.currentAbility.elementalType : null;

        private WeaponDamageType WeaponDamageTypeCondition =>
            unit != null && unit.parent.id == CharacterType.PartyMember && unit.isAbility
                ? ((PartyMember) unit.parent).equippedWeapon.damageType
                : null;

        private bool CheckmateCondition => 
            timedAttackCount >= 3 &&
            unit.currentTarget.CurrentState == UnitStates.Weakened && 
            !unit.currentTarget.StatusEffects.Contains(unit.currentAbility.statusEffects[0]);

        private Material originalMaterial;
        private Material flashMaterial;
        private SpriteRenderer spriteRenderer;

        public void OverrideUnit(Unit newUnit, Material material)
        {
            unit = newUnit;
            originalMaterial = material;
            isOverride = true;
        }
        
        private void Awake()
        {
            unit = GetComponent<Unit>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalMaterial = spriteRenderer.material;
            flashMaterial = GlobalVariables.Instance.flashMaterial;
        }

        private void OnEnable()
        {
            BattleInput._controls.Battle.Confirm.performed += OnTimedButtonPress;
            BattleInput._controls.Battle.Confirm.performed += OnTimedButtonPressSpecialAttack;
            characterAttackEvent.AddListener(this);
        }

        private void OnDisable()
        {
            BattleInput._controls.Battle.Confirm.performed -= OnTimedButtonPress;
            BattleInput._controls.Battle.Confirm.performed -= OnTimedButtonPressSpecialAttack;
            characterAttackEvent.RemoveListener(this);
        }

        private void OnTimedButtonPress(InputAction.CallbackContext ctx)
        {
            if (!thisCharacterTurn) return;
            if (unit.animationHandler.performingSpecial) return;
            if (missedWindow || hitWindow) return;
            
            var handler = isOverride ? unit.currentSummon.summonHandler : unit.animationHandler;
            if (!windowOpen && !handler.isAttacking) return;
            if (!windowOpen && handler.isAttacking) {
                missedWindow = true;
                SendTimedButtonEventResult(false);
                windowOpen = false;
                return;
            }
            
            SendTimedButtonEventResult(true);
            hitWindow = true;
            windowOpen = false;
            AudioController.PlayAudio(CommonAudioTypes.HitWindow);
        }

        private void OnTimedButtonPressSpecialAttack(InputAction.CallbackContext ctx)
        {
            if (!thisCharacterTurn) return;
            if (!unit.animationHandler.performingSpecial) return;
            if (!windowOpen) return;

            timedAttackCount += 1;
            AudioController.PlayAudio(CommonAudioTypes.HitWindow);
        }

        private void SendTimedButtonEventResult(bool result)
        {
            if (unit.parent.id == CharacterType.Enemy)
            {
                if (unit.currentAbility != null && unit.currentAbility.isMultiTarget)
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
        [UsedImplicitly] private void OpenParryWindow()
        {
            windowOpen = true;
            spriteRenderer.material = flashMaterial;
        }

        [UsedImplicitly] private void CloseParryWindow()
        {
            windowOpen = false;
            spriteRenderer.material = originalMaterial;
            unit.currentTarget.Unit.anim.SetTrigger(AnimationHandler.HurtTrigger);
        }

        [UsedImplicitly] private void TryToInflictStatusEffect()
        {
            if (!unit.isAbility || !unit.currentAbility.hasStatusEffect) return;
            
            if (!unit.currentAbility.isMultiTarget)
            {
                if (unit.currentTarget.Unit.attackerHasMissed || unit.currentTarget.IsDead) return;
                
                foreach (var effect in from effect in unit.currentAbility.statusEffects
                    where !(from statusEffect in unit.currentTarget.StatusEffects
                    where statusEffect.name == effect.name select statusEffect).Any()
                    
                    let randomValue = Random.value
                    let modifier = effect.StatusEffectModifier(unit.currentTarget)
                    let chance = unit.currentAbility.chanceOfInfliction * modifier + unit.inflictionChanceBoost

                    where !(randomValue > chance) select effect)
                {
                    effect.OnAdded(unit.currentTarget);
                    unit.currentTarget.StatusEffects.Add(effect);
                }
            }
            
            foreach (var target in unit.multiHitTargets.Where(target => !target.Unit.attackerHasMissed && !target.IsDead))
            {
                foreach (var effect in from effect in unit.currentAbility.statusEffects
                    where !(from statusEffect in target.StatusEffects
                    where statusEffect.name == effect.name select statusEffect).Any() 
                    
                    let randomValue = Random.value
                    let modifier = effect.StatusEffectModifier(target)
                    let chance = unit.currentAbility.chanceOfInfliction * modifier + unit.inflictionChanceBoost
                    
                    where !(randomValue > chance) select effect)
                {
                    effect.OnAdded(target);
                    target.StatusEffects.Add(effect);
                }
            }
        }

        [UsedImplicitly] private void TargetTakeDamage()
        {
            windowOpen = false;
            spriteRenderer.material = originalMaterial;
            
            if (!missedWindow && !hitWindow)
            {
                SendTimedButtonEventResult(false);
            }

            if (!unit.isAbility || !unit.currentAbility.isMultiTarget)
            {
                if (unit.currentTarget == null) return;
                if (unit.currentTarget.Unit.isCountered) RecalculateDamage();
                unit.currentTarget.TakeDamage(unit.currentDamage, unit.overrideElemental ?
                    unit.overrideAbility.elementalType : ElementalCondition, WeaponDamageTypeCondition);
                return;
            }
            
            unit.multiHitTargets.ForEach(t => t.TakeDamage
                (unit.damageValueList[unit.multiHitTargets.IndexOf(t)],unit.overrideElemental ?
                unit.overrideAbility.elementalType : ElementalCondition, WeaponDamageTypeCondition));
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
            Logging.Log($"Timed Attack Modifier: {timedAttackModifier}");
            
            var apUsedModifier = 1.0f + unit.specialAttackAP * 5f / 100f;
            Logging.Log($"AP Modifier: {apUsedModifier}");

            var finalDamageAmt = unit.currentDamage * timedAttackModifier;
            finalDamageAmt *= apUsedModifier;
            
            Logging.Log($"Final Damage: {(int)finalDamageAmt}");

            unit.currentTarget.TakeDamageSpecial((int)finalDamageAmt);

            if (CheckmateCondition)
            {
                var checkmate = unit.currentAbility.statusEffects[0];
                checkmate.OnAdded(unit.currentTarget);
                unit.currentTarget.StatusEffects.Add(checkmate);
            }
            
            timedAttackCount = 0;
        }

        [UsedImplicitly] private void ActivateSpell()
        {
            windowOpen = false;
            spriteRenderer.material = originalMaterial;
            if (!missedWindow && !hitWindow) SendTimedButtonEventResult(false);

            var spell = (Spell) unit.currentAbility;
            var prefab = spell.effectPrefab;
            var position = unit.currentTarget.Unit.transform.position;
            var newPosition = new Vector3(position.x + spell.offset.x, position.y + spell.offset.y, position.z);
            var effectGo = Instantiate(prefab, newPosition, prefab.transform.rotation);
            effectGo.GetComponent<SpellAnimationEvents>().Setup(unit, ElementalCondition);
        }
        
        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 != characterAttackEvent) return;
            if (value1.Unit != unit) { thisCharacterTurn = false; return; }
            if (value1.Unit.hasSummon && !isOverride)
            {
                thisCharacterTurn = false;
                return;
            }
            
            missedWindow = false;
            hitWindow = false;

            thisCharacterTurn = true;
            unitBase = value1;
        }
    }
}