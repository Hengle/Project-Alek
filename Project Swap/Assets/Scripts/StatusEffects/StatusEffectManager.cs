using System.Collections;
using System.Linq;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    public class StatusEffectManager : MonoBehaviour
    {
        public static IEnumerator TriggerStatusEffects(Unit unit, RateOfInfliction rate, WaitForSeconds delay, bool delayAfterInfliction)
        {
            foreach (var statusEffect in from statusEffect in unit.statusEffects
                where statusEffect.rateOfInfliction.Contains(rate)
                select statusEffect)
            {
                if (!delayAfterInfliction) yield return delay;
                
                statusEffect.InflictStatus(unit);
                
                if (delayAfterInfliction) yield return delay;
                if (unit.status == Status.Dead) break;
            }
        }
    }
}