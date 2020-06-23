using System;
using Abilities;
using Animations;
using Characters;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Calculator
{
    public static class DamageCalculator
    {
        // Make this the base function that is called for everything and delegate to correct function based on action type
        public static int CalculateAttackDamage(UnitBase damageDealer)
        {
            var dealerUnit = damageDealer.unit;
            var targetUnit = damageDealer.unit.currentTarget;
            if (damageDealer.unit == targetUnit) return 0;

            float dealerDamage = 0;
            int targetDefense = 0;
            
            if (damageDealer.unit.isAbility)
            {
                switch (damageDealer.unit.currentAbility.damageType)
                {
                    case DamageType.Str:
                        dealerDamage = dealerUnit.currentStrength * dealerUnit.weaponMT * dealerUnit.currentAbility.damageMultiplier;
                        targetDefense = targetUnit.currentDefense * (targetUnit.level / 2);
                        break;
                    
                    case DamageType.Mag: 
                        dealerDamage = dealerUnit.currentMagic * dealerUnit.weaponMT * dealerUnit.currentAbility.damageMultiplier;
                        targetDefense = targetUnit.currentResistance * (targetUnit.level / 2);
                        break;
                }
            }

            else
            {
                dealerDamage = dealerUnit.currentStrength * dealerUnit.weaponMT;
                targetDefense = targetUnit.currentDefense * (targetUnit.level / 2);
            }

            var totalDamage = (int) dealerDamage - targetDefense;

            var critical = CalculateCritChance(damageDealer);
            if (!critical) return totalDamage < 0 ? 0 : Random.Range((int) (0.95f * totalDamage), (int) (1.05f * totalDamage));

            dealerUnit.anim.SetInteger(AnimationHandler.PhysAttackState, 1);
            totalDamage = (int)(totalDamage * 1.75f);
            dealerUnit.isCrit = true;

            return totalDamage < 0 ? 0 : Random.Range((int)(0.95f * totalDamage), (int)(1.05f * totalDamage));
        }
        
        private static bool CalculateCritChance(UnitBase damageDealer)
        {
            var critChance = (float) damageDealer.unit.currentCrit / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }
    }
}