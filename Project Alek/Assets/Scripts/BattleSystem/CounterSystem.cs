using System.Collections.Generic;
using Characters;
using Characters.Animations;
using Characters.CharacterExtensions;
using Characters.StatusEffects;
using MEC;
using UnityEngine;

namespace BattleSystem
{
    public class CounterSystem : MonoBehaviour
    {
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
            if (unit.currentHP == 0) return;

            CharacterEvents.Trigger(CEventType.CharacterAttacking, unit.parent);
            unit.currentTarget = BattleManager.Instance.activeUnit;
            unit.currentTarget.Unit.isCountered = true;
            isCountering = true;

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
            unit.anim.SetInteger(AnimationHandler.PhysAttackState,  0);
            unit.anim.SetTrigger(AnimationHandler.AttackTrigger);

            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitUntilFalse(() => animHandler.isAttacking);
            yield return Timing.WaitUntilDone(unit.parent.InflictOnTargets
                (Rate.AfterAttacked, 0.5f, false));
        }

        private static void SlowTime() => TimeManager._slowTimeCounter = true;
        
        private static void RestoreTime() => TimeManager._slowTimeCounter = false;
        
        private void OnDisable() => unit.onTimedDefense -= SetupCounterAttack;
        
    }
}
