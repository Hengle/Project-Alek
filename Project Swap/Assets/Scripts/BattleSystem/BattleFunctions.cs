using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Characters;
using Characters.Abilities;
using Characters.Animations;
using Characters.PartyMembers;
using Characters.StatusEffects;
using Type = Characters.Type;

namespace BattleSystem
{
    public class BattleFunctions : MonoBehaviour
    {
        private Vector3 originPosition, targetPosition;
        private AnimationHandler animHandler;
        private UnitBase unitBase;
        private UnitBase currentTarget;
        private Unit unit;

        public void GetCommand(UnitBase unitBaseParam)
        {
            unitBase = unitBaseParam;
            unit = unitBase.Unit;
            currentTarget = unitBase.Unit.currentTarget;
            animHandler = unitBase.Unit.animationHandler;
            
            StartCoroutine(unitBase.Unit.commandActionName);
        }
        
        [UsedImplicitly] private IEnumerator UniversalAction()
        {
            unitBase.Unit.isAbility = false;
            
            switch (unitBase.Unit.commandActionOption)
            {
                case 1: StartCoroutine(CloseRangeAttack());
                    yield break;
                case 2: // Item. May not need this depending on how the inventory system integration goes
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
                case AbilityType.Physical: StartCoroutine(CloseRangeAttack());
                    yield break;
                case AbilityType.Ranged: StartCoroutine(RangedAttack());
                    yield break;
                case AbilityType.NonAttack: Logger.Log("Non-Attack: " + unit.currentAbility.name);
                    BattleManager._performingAction = false;
                    yield break;
                default: Logger.Log("This message should not be displaying...");
                    yield break;
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

            BattleManager._performingAction = false;
        }
        
        private IEnumerator RangedAttack()
        {
            unitBase.GetDamageValues();

            var originalRotation = unitBase.LookAtTarget();

            var attack = StartCoroutine(ExecuteAttack());
            yield return attack;

            unit.transform.rotation = originalRotation;

            BattleManager._performingAction = false;
        }

        private IEnumerator ExecuteAttack()
        {
            TimeManager._slowTimeCrit = unitBase.Unit.isCrit;
            
            unit.anim.SetInteger(AnimationHandler.PhysAttackState, unit.isAbility? unit.currentAbility.attackState : 0);
            
            unit.anim.SetTrigger(AnimationHandler.AttackTrigger);
            
            yield return new WaitForEndOfFrame();
            while (animHandler.isAttacking) yield return null;
            
            TimeManager._slowTime = false;
            TimeManager._slowTimeCrit = false;

            var coroutine = StartCoroutine(InflictStatus.OnTargetsOf
                (unitBase, RateOfInfliction.AfterAttacked, 0.5f, false));
            
            yield return coroutine;
        }

        private IEnumerator MoveToTargetPosition()
        {
            var parent = unit.transform.parent.transform;
            originPosition = parent.position;
            
            var position = currentTarget.Unit.transform.position;

            targetPosition = unitBase.id == Type.PartyMember ? 
                new Vector3(position.x, originPosition.y, position.z - 2) :
                new Vector3(position.x, position.y, position.z + 2);

            while (parent.position != targetPosition)
            {
                parent.position = Vector3.MoveTowards
                    (parent.position, targetPosition, TimeManager._moveSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(0.5f);
            if (unit.isCrit) CriticalCamController._onCritical(unitBase);
        }

        private IEnumerator MoveBackToOriginPosition()
        {
            CriticalCamController._disableCam(unitBase);
            
            var parent = unit.transform.parent.transform;
            while (parent.position != originPosition)
            {
                parent.position = Vector3.MoveTowards
                    (parent.position, originPosition, TimeManager._moveSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1);
        }
    }
}
