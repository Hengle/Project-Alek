﻿using Random = UnityEngine.Random;
using Abilities;
using Animations;
using Characters;

namespace Calculator
{
    public static class DamageCalculator
    {
        private const int WeaponAccPlaceholder = 90;
        // Make this the base function that is called for everything and delegate to correct function based on action type
        public static int CalculateAttackDamage(UnitBase damageDealer, UnitBase target)
        {
            if (damageDealer.Unit == target.Unit) return 0;
            
            var hitChance = CalculateAccuracy(damageDealer, target);
            if (!hitChance) { target.Unit.attackerHasMissed = true; return -1; }

            float dealerDamage = 0;
            var targetDefense = 0;
            
            if (damageDealer.Unit.isAbility)
            {
                switch (damageDealer.Unit.currentAbility.damageType)
                {
                    case DamageType.Str:
                        dealerDamage = damageDealer.Unit.currentStrength * damageDealer.Unit.weaponMT * damageDealer.Unit.currentAbility.damageMultiplier;
                        targetDefense = target.Unit.currentDefense * (target.Unit.level / 2);
                        break;
                    
                    case DamageType.Mag: 
                        dealerDamage = damageDealer.Unit.currentMagic * damageDealer.Unit.weaponMT * damageDealer.Unit.currentAbility.damageMultiplier;
                        targetDefense = target.Unit.currentResistance * (target.Unit.level / 2);
                        break;
                }
            }

            else {
                dealerDamage = damageDealer.Unit.currentStrength * damageDealer.Unit.weaponMT;
                targetDefense = target.Unit.currentDefense * (target.Unit.level / 2);
            }

            var totalDamage = (int) dealerDamage - targetDefense;

            var critical = CalculateCritChance(damageDealer);
            if (!critical) return totalDamage < 0 ? 0 : Random.Range((int) (0.95f * totalDamage), (int) (1.05f * totalDamage));
            
            totalDamage = (int)(totalDamage * 1.75f);
            target.Unit.targetHasCrit = true;
            damageDealer.Unit.isCrit = true;

            return totalDamage < 0 ? 0 : Random.Range((int)(0.95f * totalDamage), (int)(1.05f * totalDamage));
        }
        
        private static bool CalculateCritChance(UnitBase damageDealer)
        {
            var critChance = (float) damageDealer.Unit.currentCrit / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }

        private static bool CalculateAccuracy(UnitBase damageDealer, UnitBase target)
        {
            target.Unit.attackerHasMissed = false;
            var hitChance = 
                (float) (damageDealer.Unit.currentAccuracy + WeaponAccPlaceholder - target.Unit.currentInitiative) / 100;
            var randomValue = Random.value;
            
            return randomValue <= hitChance;
        }
    }
}