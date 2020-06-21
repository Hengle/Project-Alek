using Animations;
using Characters;
using UnityEngine;

namespace Calculator
{
    public static class DamageCalculator
    {
        // Make this the base function that is called for everything and delegate to correct function based on action type
        public static int CalculateAttackDamage(IUnitBase damageDealer)
        {
            var dealerUnit = damageDealer.GetUnit();
            var targetUnit = dealerUnit.currentTarget;

            if (dealerUnit == targetUnit) return 0;
            
            // This needs to be changed so that it calls a function on the units that returns what stat is supposed to be used to determine damage
            var dealerDamage = dealerUnit.currentStrength * dealerUnit.weaponMT;
            // Needs to be changed to check what type of damage is being applied and set targetDefense to a function call that determines stats to use
            var targetDefense = targetUnit.currentDefense * (targetUnit.level / 2);
            
            var totalDamage = dealerDamage - targetDefense;

            var critical = CalculateCritChance(damageDealer);
            if (!critical) return totalDamage < 0 ? 0 : Random.Range((int) (0.95f * totalDamage), (int) (1.05f * totalDamage));

            dealerUnit.anim.SetInteger(AnimationHandler.PhysAttackState, 1);
            totalDamage = (int)(totalDamage * 1.75f);
            dealerUnit.isCrit = true;

            return totalDamage < 0 ? 0 : Random.Range((int)(0.95f * totalDamage), (int)(1.05f * totalDamage));
        }

        private static bool CalculateCritChance(IUnitBase damageDealer)
        {
            var critChance = (float) damageDealer.GetCriticalChance() / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }
    }
}