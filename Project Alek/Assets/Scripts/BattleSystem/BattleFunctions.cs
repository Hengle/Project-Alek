﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Characters;
using Characters.Abilities;
using Characters.Animations;
using Characters.CharacterExtensions;
using Characters.PartyMembers;
using Characters.StatusEffects;
using DG.Tweening;
using MEC;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;

namespace BattleSystem
{
    public class BattleFunctions : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        [SerializeField] private CharacterGameEvent commandEvent;

        private void Start() => commandEvent.AddListener(this);
        
        private void OnDisable() => commandEvent.RemoveListener(this);

        private void GetCommand(UnitBase unit)
        {
            BattleEngine.Instance.performingAction = true;
            StartCoroutine(unit.Unit.commandActionName, unit);
        }

        #region ImplicitFunctions
        
        [UsedImplicitly] private IEnumerator UniversalAction(UnitBase dealer)
        {
            switch (dealer.Unit.commandActionOption)
            {
                case 1: Timing.RunCoroutine(CloseRangeAttack(dealer));
                    yield break;
                case 2: Timing.RunCoroutine(SpecialAttack(dealer));
                    yield break;
                case 3: // TODO: Flee
                    yield break;
            }
        }
        
        [UsedImplicitly] private IEnumerator AbilityAction(UnitBase dealer)
        {
            if (dealer.CurrentAbility == null) dealer.CurrentAbility =
                dealer.GetAndSetAbility(dealer.Unit.commandActionOption);
            
            dealer.Unit.isAbility = true;
            
            switch (dealer.CurrentAbility.abilityType)
            {
                case AbilityType.CloseRange: Timing.RunCoroutine(CloseRangeAttack(dealer));
                    yield break;
                case AbilityType.Ranged: Timing.RunCoroutine(RangedAttack(dealer));
                    yield break;
                case AbilityType.NonAttack: BattleEngine.Instance.performingAction = false;
                    yield break;
                default: Logging.Instance.Log("This message should not be displaying...");
                    yield break;
            }
        }

        #endregion

        #region AttackCoroutines
        
        private static IEnumerator<float> CloseRangeAttack(UnitBase dealer)
        {
            dealer.GetDamageValues(false);
            
            var parent = dealer.Unit.transform.parent.transform;
            var originPosition = parent.position;
            
            var position = dealer.CurrentTarget.Unit.transform.position;

            var targetPosition = dealer.id == CharacterType.PartyMember? 
                new Vector3(position.x, originPosition.y, position.z - 2) :
                new Vector3(position.x, position.y, position.z + 2);

            yield return Timing.WaitUntilDone(MoveToPosition(parent, targetPosition));
            yield return Timing.WaitUntilDone(ExecuteAttack(dealer));
            
            if (dealer.IsDead)
            {
                dealer.Unit.DestroyGO();
                BattleEngine.Instance.performingAction = false;
                SetBackToOriginPosition(parent, originPosition);
                yield break;
            }
            
            yield return Timing.WaitUntilDone(MoveToPosition(parent, originPosition));

            BattleEngine.Instance.performingAction = false;
        }
        
        private static IEnumerator<float> RangedAttack(UnitBase dealer)
        {
            dealer.GetDamageValues(false);
            
            var originalRotation = dealer.LookAtTarget();

            yield return Timing.WaitUntilDone(ExecuteAttack(dealer));

            dealer.Unit.transform.rotation = originalRotation;
            
            BattleEngine.Instance.performingAction = false;
        }

        private static IEnumerator<float> SpecialAttack(UnitBase dealer)
        {
            SuperCloseupCamController._onSpecialAttack?.Invoke(dealer);
            dealer.Unit.onSpecialAttack?.Invoke();
            dealer.GetDamageValues(true);
            
            var parent = dealer.Unit.transform.parent.transform;
            var originPosition = parent.position;
            
            var position = dealer.CurrentTarget.Unit.transform.position;

            var targetPosition = dealer.id == CharacterType.PartyMember? 
                new Vector3(position.x, originPosition.y, position.z - 2) :
                new Vector3(position.x, position.y, position.z + 2);

            yield return Timing.WaitUntilDone(MoveToPosition(parent, targetPosition));
            yield return Timing.WaitUntilDone(ExecuteSpecialAttack(dealer));
            yield return Timing.WaitUntilDone(MoveToPosition(parent, originPosition));

            BattleEngine.Instance.performingAction = false;
        }

        private static IEnumerator<float> ExecuteSpecialAttack(UnitBase dealer)
        {
            dealer.Unit.anim.SetTrigger(AnimationHandler.SpecialAttackTrigger);
            
            yield return Timing.WaitForOneFrame;
            
            yield return Timing.WaitUntilFalse(() => dealer.AnimationHandler.performingSpecial);
            
            SuperCloseupCamController._disableCam?.Invoke(dealer);
        }

        private static IEnumerator<float> ExecuteAttack(UnitBase dealer, bool isSpell = false)
        {
            if (!dealer.Unit.isAbility) dealer.Unit.anim.SetTrigger(AnimationHandler.AttackTrigger);
            else dealer.Unit.anim.Play(isSpell ? $"Spell {dealer.CurrentAbility.attackState}" :
                $"Ability {dealer.CurrentAbility.attackState}", 0);

            yield return Timing.WaitForOneFrame;

            yield return Timing.WaitUntilFalse(() => dealer.AnimationHandler.isAttacking);
            yield return Timing.WaitUntilFalse(() => dealer.Unit.isCountered);
            
            if (!dealer.IsDead) yield return Timing.WaitUntilDone(dealer.InflictOnTargets
                (Rate.AfterAttacked, 0.5f, false));
        }
        
        #endregion

        #region MovementCoroutines

        private static IEnumerator<float> MoveToPosition(Transform parent, Vector3 targetPosition)
        {
            while (parent.position != targetPosition)
            {
                parent.position = Vector3.MoveTowards(parent.position, targetPosition, 
                    TimeManager._moveSpeed * Time.deltaTime);
                
                yield return Timing.WaitForOneFrame;
            }
        }

        private static void SetBackToOriginPosition(Transform parent, Vector3 originPosition)
        {
            parent.position = originPosition;
        }
        
        #endregion
        
        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == commandEvent) GetCommand(value1);
        }
    }
}
