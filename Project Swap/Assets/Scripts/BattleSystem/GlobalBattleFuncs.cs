﻿using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Abilities;
using Abilities.Ranged_Attacks;
using Animations;
using Calculator;
using Characters;
using Characters.PartyMembers;
using DG.Tweening;
using StatusEffects;
using Type = Characters.Type;

// This script stores all of the battle functions that are shared between every party member and/or enemy
namespace BattleSystem
{
    public class GlobalBattleFuncs : MonoBehaviour
    {
        public int moveSpeed = 45;
        public int swapSpeed = 25;
        
        private bool specialSwap;
        public static bool slowTime;
        public static bool slowTimeCrit;
        
        private Vector3 originPosition, targetPosition;
        
        private AnimationHandler animHandler;
        
        private UnitBase unitBase;
        private Unit unit, currentTarget;
        private UnitBase iUnitCharacterSwappingPosition;
        private Unit characterSwappingPositionUnit, currentSwapTarget;

        private void Start() => DOTween.Init();

        private void Update()
        {
            if (slowTime)
            {
                Time.timeScale = 0.05f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;

                swapSpeed = 150;
                moveSpeed = 20;
            }
            
            else if (slowTimeCrit) {
                Time.timeScale = 0.50f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
            }
            
            else
            {
                Time.timeScale = 1;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                
                swapSpeed = 30;
                moveSpeed = 45;
            }
        }
        
        public void GetCommand(UnitBase unitBaseParam)
        {
            unitBase = unitBaseParam;
            unit = unitBaseParam.unit;
            currentTarget = unit.currentTarget;
                
            animHandler = unitBaseParam.unit.animationHandler;
            StartCoroutine(unit.commandActionName);
        }

        // Called from GetCommand with unit.commandActionName
        [UsedImplicitly] private IEnumerator UniversalAction()
        {
            unit.isAbility = false;
            
            switch (unit.commandActionOption)
            {
                case 1: StartCoroutine(PhysicalAttack());
                    yield break;
                case 2: // Item
                    yield break;
                case 3: // Flee
                    yield break;
            }
        }
        
        [UsedImplicitly] private IEnumerator AbilityAction()
        {
            unit.currentAbility = unitBase.GetAndSetAbility(unit.commandActionOption);
            unit.isAbility = true;

            switch (unit.currentAbility.abilityType)
            {
                case AbilityType.Physical: StartCoroutine(PhysicalAttack());
                    yield break;;
                case AbilityType.Ranged: StartCoroutine(RangedAttack());
                    yield break;;
                case AbilityType.NonAttack: Logger.Log("Non-Attack: " + unit.currentAbility.name);
                    yield break;;
                default: Logger.Log("This message should not be displaying...");
                    yield break;;
            }
        }

        private void SetupSpecialSwap()
        {
            currentTarget.isSwapping = false;
            slowTime = true;

            iUnitCharacterSwappingPosition = currentTarget.unitRef;
            characterSwappingPositionUnit = iUnitCharacterSwappingPosition.unit;
            
            currentSwapTarget = BattleHandler.partySwapTarget;
            characterSwappingPositionUnit.currentTarget = currentSwapTarget;

            specialSwap = true;
            StartCoroutine(Swap());
                    
            currentTarget = currentSwapTarget;
            unit.currentTarget = currentSwapTarget;

            unit.currentDamage = DamageCalculator.CalculateAttackDamage(unitBase, currentTarget);
        }

        private IEnumerator Swap()
        {
            currentSwapTarget = iUnitCharacterSwappingPosition.CheckTargetStatus();
            
            yield return characterSwappingPositionUnit.spriteParentObject.transform.SwapPosition
                (currentSwapTarget.spriteParentObject.transform, swapSpeed);
            
            slowTime = false;

            if (characterSwappingPositionUnit.id == Type.PartyMember && characterSwappingPositionUnit != currentSwapTarget)
                characterSwappingPositionUnit.characterPanelRef.SwapSiblingIndex(currentSwapTarget.characterPanelRef);

            if (specialSwap) yield break;
            BattleHandler.performingAction = false;
        }
        
        private IEnumerator MoveToTargetPosition()
        {
            originPosition = unit.spriteParentObject.transform.position;
            var position = currentTarget.transform.position;
            
            targetPosition = unit.id == Type.PartyMember ? 
                new Vector3(position.x, originPosition.y, position.z - 2) :
                new Vector3(position.x, position.y, position.z + 2);
            
            while (unit.spriteParentObject.transform.position != targetPosition)
            {
                if (currentTarget.isSwapping && BattleHandler.partySwapTarget.status != Status.Dead) SetupSpecialSwap();
                
                unit.spriteParentObject.transform.position = Vector3.MoveTowards
                    (unit.spriteParentObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
          
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);
            if (unit.id == Type.PartyMember && unit.isCrit) unit.closeUpCamCrit.SetActive(true);
        }

        private IEnumerator MoveBackToOriginPosition()
        {
            if (unit.id == Type.PartyMember) unit.closeUpCamCrit.SetActive(false);
            
            while (unit.spriteParentObject.transform.position != originPosition)
            {
                unit.spriteParentObject.transform.position = Vector3.MoveTowards
                    (unit.spriteParentObject.transform.position, originPosition, moveSpeed * Time.deltaTime);
  
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1);
        }

        private IEnumerator ExecuteAttack()
        {
            if (unit.isCrit && unit.id == Type.PartyMember) slowTimeCrit = true;
            if (unit.isAbility) unit.anim.SetInteger(AnimationHandler.PhysAttackState, unit.currentAbility.attackState);
            
            unit.anim.SetTrigger(AnimationHandler.AttackTrigger);
            yield return new WaitForEndOfFrame();
            while (animHandler.isAttacking) yield return null;
            
            slowTime = false;
            slowTimeCrit = false;
            
            if (unit.missed) yield break;
            
            var coroutine = StartCoroutine
                (StatusEffectManager.Trigger
                (currentTarget, RateOfInfliction.AfterAttacked, 0.25f, false));
            
            yield return coroutine;
        }
        
        private IEnumerator PhysicalAttack()
        {
            currentTarget = unitBase.CheckTargetStatus();
            unit.currentDamage = DamageCalculator.CalculateAttackDamage(unitBase, currentTarget);

            var move = StartCoroutine(MoveToTargetPosition());
            yield return move;

            var attack = StartCoroutine(ExecuteAttack());
            yield return attack;

            var moveBack = StartCoroutine(MoveBackToOriginPosition());
            yield return moveBack;

            BattleHandler.performingAction = false;
        }

        
        // Gonna have to update this to account for multi-target attacks
        private IEnumerator RangedAttack()
        {
            currentTarget = unitBase.CheckTargetStatus();
            
            if (unit.currentAbility.isMultiHit)
            {
                switch (unit.currentAbility.targetOptions)
                {
                    case 0: foreach (var enemy in BattleHandler.enemiesForThisBattle)
                            unit.damageValueList.Add(DamageCalculator.CalculateAttackDamage(unitBase, enemy.unit));
                        break;
                    case 1: foreach (var member in BattleHandler.membersForThisBattle)
                            unit.damageValueList.Add(DamageCalculator.CalculateAttackDamage(unitBase, member.unit));
                        break;
                    case 2: break;
                }
            }
            
            else unit.currentDamage = DamageCalculator.CalculateAttackDamage(unitBase, currentTarget);
            
            var originalRotation = unit.transform.rotation;
            var lookAtPosition = currentTarget.transform.position;

            var rangeAbility = (RangedAttack) unit.currentAbility;
            if (rangeAbility.lookAtTarget) {
                unit.transform.LookAt(lookAtPosition);
                unit.transform.rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.forward); 
            }

            var attack = StartCoroutine(ExecuteAttack());
            yield return attack;

            unit.transform.rotation = originalRotation;

            BattleHandler.performingAction = false;
        }
    }
}