using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Characters;
using Characters.Abilities;
using Characters.Animations;
using Characters.CharacterExtensions;
using Characters.PartyMembers;
using Characters.StatusEffects;
using MEC;

namespace BattleSystem
{
    public class BattleFunctions : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        #region FieldsAndProperties
        
        private Vector3 originPosition, targetPosition;
        private AnimationHandler animHandler;
        private UnitBase unitBase;
        private UnitBase currentTarget;
        private Unit unit;
        
        #endregion

        private void Start() => GameEventsManager.AddListener(this);

        private void GetCommand(UnitBase unitBaseParam)
        {
            BattleManager._performingAction = true;
            
            unitBase = unitBaseParam;
            unit = unitBase.Unit;
            currentTarget = unitBase.Unit.currentTarget;
            animHandler = unitBase.Unit.animationHandler;
            
            StartCoroutine(unitBase.Unit.commandActionName);
        }

        #region ImplicitFunctions
        
        [UsedImplicitly] private IEnumerator UniversalAction()
        {
            unitBase.Unit.isAbility = false;
            
            switch (unitBase.Unit.commandActionOption)
            {
                case 1: Timing.RunCoroutine(CloseRangeAttack());
                    yield break;
                case 2: // TODO: Item. May not need this depending on how the inventory system integration goes
                    yield break;
                case 3: // TODO: Flee
                    yield break;
            }
        }
        
        [UsedImplicitly] private IEnumerator AbilityAction()
        {
            unit.currentAbility = unitBase.GetAndSetAbility(unit.commandActionOption);
            unit.isAbility = true;

            switch (unit.currentAbility.abilityType)
            {
                case AbilityType.CloseRange: Timing.RunCoroutine(CloseRangeAttack());
                    yield break;
                case AbilityType.Ranged: Timing.RunCoroutine(RangedAttack());
                    yield break;
                case AbilityType.NonAttack: Logger.Log("Non-Attack: " + unit.currentAbility.name);
                    BattleManager._performingAction = false;
                    yield break;
                default: Logger.Log("This message should not be displaying...");
                    yield break;
            }
        }
        
        #endregion

        #region AttackCoroutines
        
        private IEnumerator<float> CloseRangeAttack()
        {
            unitBase.GetDamageValues();

            yield return Timing.WaitUntilDone(MoveToTargetPosition());

            yield return Timing.WaitUntilDone(ExecuteAttack());

            yield return Timing.WaitUntilDone(MoveBackToOriginPosition());

            BattleManager._performingAction = false;
        }
        
        private IEnumerator<float> RangedAttack()
        {
            unitBase.GetDamageValues();

            var originalRotation = unitBase.LookAtTarget();

            yield return Timing.WaitUntilDone(ExecuteAttack());

            unit.transform.rotation = originalRotation;

            BattleManager._performingAction = false;
        }

        private IEnumerator<float> ExecuteAttack()
        {
            TimeManager._slowTimeCrit = unitBase.Unit.isCrit;
            
            unit.anim.SetInteger(AnimationHandler.PhysAttackState, unit.isAbility?
                unit.currentAbility.attackState : 0);
            unit.anim.SetTrigger(AnimationHandler.AttackTrigger);

            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitUntilFalse(() => animHandler.isAttacking);

            TimeManager._slowTime = false;
            TimeManager._slowTimeCrit = false;

            yield return Timing.WaitUntilDone(unitBase.InflictOnTargets
                (Rate.AfterAttacked, 0.5f, false));

            // TODO: Move this out of this function
            unit.multiHitTargets = new List<UnitBase>();
            unit.damageValueList = new List<int>();
        }
        
        #endregion

        #region MovementCoroutines

        private IEnumerator<float> MoveToTargetPosition()
        {
            var parent = unit.transform.parent.transform;
            originPosition = parent.position;
            
            var position = currentTarget.Unit.transform.position;

            targetPosition = unitBase.id == CharacterType.PartyMember? 
                new Vector3(position.x, originPosition.y, position.z - 2) :
                new Vector3(position.x, position.y, position.z + 2);

            while (parent.position != targetPosition)
            {
                parent.position = Vector3.MoveTowards
                    (parent.position, targetPosition,
                    TimeManager._moveSpeed * Time.deltaTime);
                
                yield return Timing.WaitForOneFrame;
            }
            
            if (unit.isCrit) CriticalCamController._onCritical(unitBase);
        }

        private IEnumerator<float> MoveBackToOriginPosition()
        {
            CriticalCamController._disableCam(unitBase);
            
            var parent = unit.transform.parent.transform;
            
            while (parent.position != originPosition)
            {
                parent.position = Vector3.MoveTowards
                    (parent.position, originPosition,
                    TimeManager._moveSpeed * Time.deltaTime);
                
                yield return Timing.WaitForOneFrame;
            }
        }
        
        #endregion

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.NewCommand)
            {
                GetCommand(eventType._character as UnitBase);
            }
        }
    }
}
