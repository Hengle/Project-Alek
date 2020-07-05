using System.Collections;
using System.Linq;
using UnityEngine;

namespace Characters.StatusEffects
{
    public class InflictStatus : MonoBehaviour
    {

        // Update so that if there are multiple BeforeEveryAction effects, it will break after the first one triggers
        
        /// <summary>
        /// Inflict a status effect on this unit
        /// </summary>
        /// <param name="unitBase"></param>
        /// <param name="rate"></param>
        /// <param name="delay"></param>
        /// <param name="delayAfterInfliction"></param>
        /// <returns></returns>
        public static IEnumerator OnThisUnit(UnitBase unitBase, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            //var inhibited = false;
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
        
        /// <summary>
        /// Inflict a status effect on the targets of a unit
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="rate"></param>
        /// <param name="delay"></param>
        /// <param name="delayAfterInfliction"></param>
        /// <returns></returns>
        public static IEnumerator OnTargetsOf(UnitBase attacker, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            if (attacker.Unit.isAbility && attacker.Unit.currentAbility.isMultiTarget)
            {
                foreach (var target in attacker.Unit.multiHitTargets.Where(target => !target.Unit.attackerHasMissed && !target.IsDead))
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

            if (attacker.Unit.currentTarget.Unit.attackerHasMissed || attacker.Unit.currentTarget.IsDead) yield break;
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