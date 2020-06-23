using System;
using System.Collections;
using System.Linq;
using Abilities;
using JetBrains.Annotations;
using Animations;
using Calculator;
using Characters;
using StatusEffects;
using UnityEngine;

// This script stores all of the battle functions that are shared between every party member and/or enemy
namespace BattleSystem
{
    public class GlobalBattleFuncs : MonoBehaviour
    {
        public int moveSpeed = 45;
        public int swapSpeed = 25;

        private static bool isMoving;
        private bool endFunction;
        private bool specialSwap;
        private bool executing;
        //private bool isAbility;
        public bool slowTime;
        public bool slowTimeCrit;
        
        private Vector3 originPosition, targetPosition;
        private AnimationHandler animHandler;
        private Ability ability;

        private UnitBase unitBase;
        private Unit unit, currentTarget;
        private UnitBase iUnitCharacterSwappingPosition;
        private Unit characterSwappingPositionUnit, currentSwapTarget;
        
        private void Update()
        {
            if (slowTime)
            {
                Time.timeScale = 0.05f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;

                swapSpeed = 80;
                moveSpeed = 20;
            }
            else if (slowTimeCrit)
            {
                Time.timeScale = 0.50f;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
            }
            else
            {
                Time.timeScale = 1;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                
                swapSpeed = 25;
                moveSpeed = 45;
            }
        }
        
        public void GetCommand(UnitBase unitBaseParam, bool isSwapping)
        {
            if (!isSwapping)
            {
                unitBase = unitBaseParam;
                unit = unitBaseParam.unit;
                currentTarget = unit.currentTarget;
                
                animHandler = unitBaseParam.unit.animationHandler;
                StartCoroutine(unit.commandActionName);
                return;
            }
            
            iUnitCharacterSwappingPosition = unitBaseParam;
            characterSwappingPositionUnit = unitBaseParam.unit;
            characterSwappingPositionUnit.currentTarget = BattleHandler.partySwapTarget;
            currentSwapTarget = characterSwappingPositionUnit.currentTarget;
                
            animHandler = unitBaseParam.unit.animationHandler;
            StartCoroutine(Swap());
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
            ability = unitBase.GetAndSetAbility(unit.commandActionOption);
            unit.isAbility = true;

            switch (ability.abilityType)
            {
                case AbilityType.Physical: StartCoroutine(PhysicalAttack());
                    break;
                case AbilityType.Ranged: StartCoroutine(RangedAttack());
                    break;
                case AbilityType.NonAttack: Debug.Log("Non-Attack: " + ability.name);
                    break;
                default: Debug.Log("This message should not be displaying...");
                    break;
            }

            yield return new WaitForSeconds(1);
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

            unit.currentDamage = DamageCalculator.CalculateAttackDamage(unitBase);
        }

        private IEnumerator Swap()
        {
            if (!specialSwap) isMoving = true;
            currentSwapTarget = iUnitCharacterSwappingPosition.CheckTargetStatus(true);
            
            var originalTargetPosition1 = characterSwappingPositionUnit.spriteParentObject.transform.position;
            var originalTargetPosition2 = currentSwapTarget.spriteParentObject.transform.position;
            
            var offsetPosition1 = new Vector3(originalTargetPosition1.x, originalTargetPosition1.y, originalTargetPosition1.z + 4);
            var offsetPosition2 = new Vector3(originalTargetPosition2.x, originalTargetPosition2.y, originalTargetPosition2.z - 4);

            var targetPosition1 = offsetPosition1;
            var targetPosition2 = offsetPosition2;
            
            while (characterSwappingPositionUnit.spriteParentObject.transform.position != targetPosition2 
                   && currentSwapTarget.spriteParentObject.transform.position != targetPosition1)
            {
                var characterSwapping = characterSwappingPositionUnit.spriteParentObject.transform;
                var currentSwapTarg = currentSwapTarget.spriteParentObject.transform;
                
                if (Mathf.Abs((characterSwapping.position - currentSwapTarg.position).x) <= 0.2f)
                {
                    targetPosition1 = originalTargetPosition1;
                    targetPosition2 = originalTargetPosition2;
                }
                
                characterSwapping.position = Vector3.MoveTowards
                    (characterSwapping.position, targetPosition2, swapSpeed * Time.deltaTime);


                currentSwapTarg.position = Vector3.MoveTowards
                    (currentSwapTarg.position, targetPosition1, swapSpeed * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            }
            
            slowTime = false;
            
            if (characterSwappingPositionUnit.id == 1 && characterSwappingPositionUnit != currentSwapTarget)
            {
                var unitIndex = characterSwappingPositionUnit.characterPanelRef.GetSiblingIndex();
                var targetIndex = currentSwapTarget.characterPanelRef.GetSiblingIndex();
                
                characterSwappingPositionUnit.characterPanelRef.SetSiblingIndex(targetIndex);
                currentSwapTarget.characterPanelRef.SetSiblingIndex(unitIndex);
            }

            if (specialSwap) yield break;
            isMoving = false; BattleHandler.performingAction = false;
        }
        
        private IEnumerator MoveToTargetPosition()
        {
            isMoving = true;
            
            originPosition = unit.spriteParentObject.transform.position;
            var position = currentTarget.transform.position;
            
            targetPosition = unit.id == 1 ? 
                new Vector3(position.x, originPosition.y, position.z - 2) : new Vector3(position.x, position.y, position.z + 2);
            
            while (unit.spriteParentObject.transform.position != targetPosition)
            {
                if (currentTarget.isSwapping && BattleHandler.partySwapTarget.status != Status.Dead) SetupSpecialSwap();
                
                unit.spriteParentObject.transform.position = Vector3.MoveTowards
                    (unit.spriteParentObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
          
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);
            if (unit.id == 1 && unit.isCrit) unit.closeUpCamCrit.SetActive(true);
            
            isMoving = false;
        }

        private IEnumerator MoveBackToOriginPosition()
        {
            isMoving = true;

            if (unit.id == 1) unit.closeUpCamCrit.SetActive(false);
            
            while (unit.spriteParentObject.transform.position != originPosition)
            {
                unit.spriteParentObject.transform.position = Vector3.MoveTowards
                    (unit.spriteParentObject.transform.position, originPosition, moveSpeed * Time.deltaTime);
  
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1);
            isMoving = false;
        }

        private IEnumerator ExecuteAttack()
        {
            executing = true;
            if (unit.isCrit && unit.id == 1) slowTimeCrit = true;
            
            if (unit.isAbility) unit.anim.SetInteger(AnimationHandler.PhysAttackState, ability.attackState);
            unit.anim.SetTrigger(AnimationHandler.AttackTrigger);
            yield return new WaitForEndOfFrame();
            while (animHandler.isAttacking) yield return null;
            
            slowTime = false;
            slowTimeCrit = false;
            
            foreach (var statusEffect in from statusEffect in currentTarget.statusEffects
                where statusEffect.rateOfInfliction == RateOfInfliction.AfterAttacked
                select statusEffect) statusEffect.InflictStatus(currentTarget);

            executing = false;
        }
        
        private IEnumerator PhysicalAttack()
        {
            currentTarget = unitBase.CheckTargetStatus(false);
            unit.currentDamage = DamageCalculator.CalculateAttackDamage(unitBase);

            StartCoroutine(MoveToTargetPosition());
            while (isMoving) yield return null;

            StartCoroutine(ExecuteAttack());
            while (executing) yield return null;

            StartCoroutine(MoveBackToOriginPosition());
            while (isMoving) yield return null;

            BattleHandler.performingAction = false;
        }

        
        // Gonna have to update this to account for multi-target attacks
        private IEnumerator RangedAttack()
        {
            currentTarget = unitBase.CheckTargetStatus(false);
            unit.currentDamage = DamageCalculator.CalculateAttackDamage(unitBase);
            
            var originalRotation = unit.transform.rotation;
            var lookAtPosition = currentTarget.transform.position;

            var rangeAbility = (RangedAttack) ability;
            if (rangeAbility.lookAtTarget) 
            {
                unit.transform.LookAt(lookAtPosition);
                unit.transform.rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.forward); 
            }

            StartCoroutine(ExecuteAttack());
            while (executing) yield return null;
            
            unit.transform.rotation = originalRotation;

            BattleHandler.performingAction = false;
        }
    }
}
