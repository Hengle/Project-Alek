using System.Collections;
using System.Linq;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    public class StatusEffectManager : MonoBehaviour
    {
        public static IEnumerator TriggerOnThisUnit(Unit unit, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            foreach (var statusEffect in from statusEffect in unit.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate)
                select statusEffect)
            {
                if (!delayAfterInfliction) yield return new WaitForSeconds(delay);
                
                statusEffect.InflictStatus(unit);
                
                if (delayAfterInfliction) yield return new WaitForSeconds(delay);
                if (unit.status == Status.Dead) break;
            }
        }

        public static IEnumerator TriggerOnTargetsOfUnit(Unit attacker, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            if (attacker.isAbility && attacker.currentAbility.isMultiHit)
            {
                foreach (var target in attacker.multiHitTargets)
                {
                    foreach (var statusEffect in from statusEffect in target.unit.statusEffects
                        where statusEffect.rateOfInfliction.Contains(rate)
                        select statusEffect)
                    {
                        if (!delayAfterInfliction) yield return new WaitForSeconds(delay);
                
                        statusEffect.InflictStatus(target.unit);
                
                        if (delayAfterInfliction) yield return new WaitForSeconds(delay);
                        if (attacker.status == Status.Dead) break;
                    }
                }

                yield break;
            }

            foreach (var statusEffect in from statusEffect in attacker.currentTarget.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate)
                select statusEffect)
            {
                if (!delayAfterInfliction) yield return new WaitForSeconds(delay);
                
                statusEffect.InflictStatus(attacker.currentTarget);
                
                if (delayAfterInfliction) yield return new WaitForSeconds(delay);
                if (attacker.status == Status.Dead) break;
            }
        }
    }
}