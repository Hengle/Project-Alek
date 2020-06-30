using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Abilities;
using Animations;
using Characters;
using Characters.PartyMembers;
using StatusEffects;
using Type = Characters.Type;

namespace BattleSystem
{
    public class GlobalBattleFuncs : MonoBehaviour
    {
        private Vector3 originPosition, targetPosition;
        private AnimationHandler animHandler;
        private UnitBase unitBase;
        private UnitBase currentTarget;

        public void GetCommand(UnitBase unitBaseParam)
        {
            unitBase = unitBaseParam;
            currentTarget = unitBase.Unit.currentTarget;
            animHandler = unitBase.Unit.animationHandler;
            
            StartCoroutine(unitBase.Unit.commandActionName);
        }

        // Called from GetCommand with unit.commandActionName
        [UsedImplicitly] private IEnumerator UniversalAction()
        {
            unitBase.Unit.isAbility = false;
            
            switch (unitBase.Unit.commandActionOption)
            {
                case 1: StartCoroutine(CloseRangeAttack());
                    yield break;
                case 2: // Item
                    yield break;
                case 3: // Flee
                    yield break;
            }
        }
        
        [UsedImplicitly] private IEnumerator AbilityAction()
        {
            unitBase.Unit.currentAbility = unitBase.GetAndSetAbility(unitBase.Unit.commandActionOption);
            unitBase.Unit.isAbility = true;

            switch (unitBase.Unit.currentAbility.abilityType)
            {
                case AbilityType.Physical: StartCoroutine(CloseRangeAttack());
                    yield break;;
                case AbilityType.Ranged: StartCoroutine(RangedAttack());
                    yield break;;
                case AbilityType.NonAttack: Logger.Log("Non-Attack: " + unitBase.Unit.currentAbility.name);
                    yield break;;
                default: Logger.Log("This message should not be displaying...");
                    yield break;;
            }
        }

        private IEnumerator CloseRangeAttack()
        {
            unitBase.GetDamageValues();

            var move = StartCoroutine(MoveToTargetPosition());
            yield return move;

            var attack = StartCoroutine(ExecuteAttack());
            yield return attack;

            var moveBack = StartCoroutine(MoveBackToOriginPosition());
            yield return moveBack;

            BattleManager.performingAction = false;
        }
        
        private IEnumerator RangedAttack()
        {
            unitBase.GetDamageValues();

            var originalRotation = unitBase.LookAtTarget();

            var attack = StartCoroutine(ExecuteAttack());
            yield return attack;

            unitBase.Unit.transform.rotation = originalRotation;

            BattleManager.performingAction = false;
        }

        private IEnumerator ExecuteAttack()
        {
            TimeManager.slowTimeCrit = unitBase.Unit.isCrit;
            
            if (unitBase.Unit.isAbility) unitBase.Unit.anim.SetInteger
                (AnimationHandler.PhysAttackState, unitBase.Unit.currentAbility.attackState);
            
            unitBase.Unit.anim.SetTrigger(AnimationHandler.AttackTrigger);
            
            yield return new WaitForEndOfFrame();
            while (animHandler.isAttacking) yield return null;
            
            TimeManager.slowTime = false;
            TimeManager.slowTimeCrit = false;

            var coroutine = StartCoroutine(StatusEffectManager.TriggerOnTargetsOfUnit
                (unitBase, RateOfInfliction.AfterAttacked, 0.25f, false));
            
            yield return coroutine;
        }

        private IEnumerator MoveToTargetPosition()
        {
            originPosition = unitBase.Unit.parent.transform.position;
            var position = currentTarget.Unit.transform.position;
            
            targetPosition = unitBase.id == Type.PartyMember ? 
                new Vector3(position.x, originPosition.y, position.z - 2) :
                new Vector3(position.x, position.y, position.z + 2);

            var parent = unitBase.Unit.parent.transform;
            while (parent.position != targetPosition)
            {
                parent.position = Vector3.MoveTowards
                    (parent.position, targetPosition, TimeManager.moveSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(0.5f);
            if (unitBase.Unit.isCrit) CriticalCamController.onCritical(unitBase);
        }

        private IEnumerator MoveBackToOriginPosition()
        {
            CriticalCamController.disableCam(unitBase);
            
            var parent = unitBase.Unit.parent.transform;
            while (parent.position != originPosition)
            {
                parent.position = Vector3.MoveTowards
                    (parent.position, originPosition, TimeManager.moveSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1);
        }
    }
}
