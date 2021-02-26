﻿using System.Collections.Generic;
using Characters;
using MEC;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace BattleSystem
{
    public class CounterSystem : MonoBehaviour
    {
        [SerializeField] private CharacterGameEvent characterAttackEvent;
        private Unit unit;
        private Animator animator;
        private AnimationHandler animHandler;
        
        private bool isCountering;

        private void Start()
        {
            unit = GetComponent<Unit>();
            animator = GetComponent<Animator>();
            animHandler = unit.animationHandler;

            unit.onTimedDefense += SetupCounterAttack;
        }

        private void Update() => animator.updateMode = Time.timeScale < 1 && isCountering ?
            AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;

        private void SetupCounterAttack(bool hitWindow)
        {
            if (!hitWindow) return;
            if (unit.currentHP == 0 || unit.status == Status.Dead) return;

            unit.currentTarget = Battle.Engine.activeUnit;
            unit.currentTarget.Unit.isCountered = true;
            isCountering = true;
            characterAttackEvent.Raise(unit.parent, characterAttackEvent);

            SlowTime();
            Timing.RunCoroutine(CounterAttack());
        }

        private IEnumerator<float> CounterAttack()
        {
            yield return Timing.WaitForSeconds(0.15f);
            yield return Timing.WaitUntilDone(ExecuteAttack());
            
            unit.currentTarget.Unit.isCountered = false;
            isCountering = false;
            
            RestoreTime();
        }
    
        private IEnumerator<float> ExecuteAttack()
        {
            if (unit.anim.GetBool(AnimationHandler.DontTranToIdle)) yield break;
            unit.anim.SetTrigger(AnimationHandler.AttackTrigger);

            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitUntilFalse(() => animHandler.isAttacking);
            yield return Timing.WaitUntilDone(unit.parent.InflictOnTargets
                (Rate.AfterAttacked, 0.25f, false));
        }

        private static void SlowTime() => TimeManager.SlowTime(0.35f);
        
        private static void RestoreTime() => TimeManager.ResumeTime();

        private void OnDisable()
        {
            if (!unit) return;
            unit.onTimedDefense -= SetupCounterAttack;
        }

    }
}
