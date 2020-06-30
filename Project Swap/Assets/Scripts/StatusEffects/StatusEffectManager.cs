using System;
using System.Collections;
using System.Linq;
using BattleSystem;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    public class StatusEffectManager : MonoBehaviour
    {

        // Update so that if there are multiple BeforeEveryAction effects, it will break after the first one triggers
        public static IEnumerator TriggerOnThisUnit(UnitBase unitBase, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            foreach (var statusEffect in from statusEffect in unitBase.Unit.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate)
                select statusEffect)
            {
                if (!delayAfterInfliction) yield return new WaitForSeconds(delay);
                
                statusEffect.InflictStatus(unitBase);
                
                if (delayAfterInfliction) yield return new WaitForSeconds(delay);
                if (unitBase.Unit.status == Status.Dead) break;
            }
        }

        public static IEnumerator TriggerOnTargetsOfUnit(UnitBase attacker, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            if (attacker.Unit.isAbility && attacker.Unit.currentAbility.isMultiTarget)
            {
                foreach (var target in attacker.Unit.multiHitTargets.Where(target => !target.Unit.attackerHasMissed))
                {
                    foreach (var statusEffect in from statusEffect in target.Unit.statusEffects
                        where statusEffect.rateOfInfliction.Contains(rate)
                        select statusEffect)
                    {
                        if (!delayAfterInfliction) yield return new WaitForSeconds(delay);
                
                        statusEffect.InflictStatus(target);
                
                        if (delayAfterInfliction) yield return new WaitForSeconds(delay);
                        if (attacker.Unit.status == Status.Dead) break;
                    }
                }

                yield break;
            }

            if (attacker.Unit.currentTarget.Unit.attackerHasMissed) yield break;
            foreach (var statusEffect in from statusEffect in attacker.Unit.currentTarget.Unit.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate)
                select statusEffect)
            {
                if (!delayAfterInfliction) yield return new WaitForSeconds(delay);
                
                statusEffect.InflictStatus(attacker.Unit.currentTarget);
                
                if (delayAfterInfliction) yield return new WaitForSeconds(delay);
                if (attacker.Unit.status == Status.Dead) break;
            }
        }
    }
}