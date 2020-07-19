using System.Collections.Generic;
using System.Linq;
using MEC;

namespace Characters.StatusEffects
{
    public static class InflictStatus
    {
        // TODO: Update so that if there are multiple BeforeEveryAction effects, it will break after the first one triggers
        public static IEnumerator<float> OnThisUnit(UnitBase unitBase, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            if (unitBase.IsDead) yield break;
            
            foreach (var statusEffect in from statusEffect in unitBase.Unit.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate) select statusEffect)
            {
                if (!delayAfterInfliction) yield return Timing.WaitForSeconds(delay);
                
                statusEffect.InflictStatus(unitBase);
                
                if (delayAfterInfliction) yield return Timing.WaitForSeconds(delay);
                if (unitBase.IsDead) break;
            }
        }
        
        public static IEnumerator<float> OnTargetsOf(UnitBase attacker, RateOfInfliction rate, float delay, bool delayAfterInfliction)
        {
            if (attacker.Unit.isAbility && attacker.Unit.currentAbility.isMultiTarget)
            {
                foreach (var target in attacker.Unit.multiHitTargets.
                    Where(target => !target.Unit.attackerHasMissed && !target.IsDead))
                {
                    foreach (var statusEffect in from statusEffect in target.Unit.statusEffects
                        where statusEffect.rateOfInfliction.Contains(rate) select statusEffect)
                    {
                        if (attacker.IsDead) break;
                        if (!delayAfterInfliction) yield return Timing.WaitForSeconds(delay);
                
                        statusEffect.InflictStatus(target);
                
                        if (delayAfterInfliction) yield return Timing.WaitForSeconds(delay);
                        if (attacker.IsDead) break;
                    }
                }
                yield break;
            }

            if (attacker.Unit.currentTarget.Unit.attackerHasMissed || attacker.Unit.currentTarget.IsDead) yield break;
            
            foreach (var statusEffect in from statusEffect in attacker.Unit.currentTarget.Unit.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate) select statusEffect)
            {
                if (attacker.Unit.currentTarget.IsDead) break;
                if (!delayAfterInfliction) yield return Timing.WaitForSeconds(delay);
                
                statusEffect.InflictStatus(attacker.Unit.currentTarget);
                
                if (delayAfterInfliction) yield return Timing.WaitForSeconds(delay);
                if (attacker.Unit.currentTarget.IsDead) break;
            }
        }
    }
}