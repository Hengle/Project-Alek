using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.Abilities;
using Random = UnityEngine.Random;

namespace BattleSystem.Calculator
{
    public static class Calculator
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
                    case DamageType.Physical:
                        dealerDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight * damageDealer.Unit.currentAbility.damageMultiplier;
                        targetDefense = (int) target.defense.Value * (target.level / 2);
                        break;
                    
                    case DamageType.Magic:
                        dealerDamage = (int) damageDealer.magic.Value * damageDealer.magicMight * damageDealer.Unit.currentAbility.damageMultiplier;
                        targetDefense = (int) target.resistance.Value * (target.Unit.level / 2);
                        break;
                }
            }

            else
            {
                //Logger.Log("Not an ability??");
                dealerDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight;
                targetDefense = (int) target.defense.Value * (target.Unit.level / 2);
            }
            
            //Logger.Log($"{dealerDamage} - {targetDefense}");

            var totalDamage = (int) dealerDamage - targetDefense;

            var critical = CalculateCritChance(damageDealer);
            if (!critical) return totalDamage < 0 ? 0 : Random.Range((int) (0.95f * totalDamage), (int) (1.05f * totalDamage));
            
            totalDamage = (int)(totalDamage * 1.75f);
            target.Unit.targetHasCrit = true;
            damageDealer.Unit.isCrit = true;

            return totalDamage < 0 ? 0 : Random.Range((int)(0.95f * totalDamage), (int)(1.05f * totalDamage));
        }

        private static int CalculateDamageWithElemental(UnitBase damageDealer, UnitBase target)
        {
            if (damageDealer.Unit == target.Unit) return 0;
            
            var hitChance = CalculateAccuracy(damageDealer, target);
            if (!hitChance) { target.Unit.attackerHasMissed = true; return -1; }

            float normalDamage = 0;
            float elementalDamage;
            float totalDamage;
            
            var targetDefense = 0;
            var ability = damageDealer.Unit.currentAbility;
            
            switch (ability.damageType)
            {
                case DamageType.Physical:
                    normalDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight;
                    
                    elementalDamage =
                        (int) damageDealer.magic.Value * damageDealer.weaponMight * 
                        (target._elementalResistances.ContainsKey(ability.elementalType)? 
                            ability.ElementalScaler * ( 1 - (float) target._elementalResistances.Single
                                (s => s.Key == ability.elementalType).Value / 100) :
                            ability.ElementalScaler);
                    
                    targetDefense = (int) target.defense.Value * (target.level / 2);
                    
                    totalDamage = (normalDamage + elementalDamage) * damageDealer.Unit.currentAbility.damageMultiplier;
                    break;
                    
                case DamageType.Magic:
                    normalDamage =
                        (int) damageDealer.magic.Value * damageDealer.weaponMight +
                        damageDealer.magic.Value * damageDealer.weaponMight * damageDealer.Unit.currentAbility.ElementalScaler
                        * damageDealer.Unit.currentAbility.damageMultiplier;
                    targetDefense = (int) target.resistance.Value * (target.Unit.level / 2);
                    break;
            }

            return 0;
        }
        
        private static bool CalculateCritChance(UnitBase damageDealer)
        {
            var critChance = damageDealer.criticalChance.Value / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }

        private static bool CalculateAccuracy(UnitBase damageDealer, UnitBase target)
        {
            target.Unit.attackerHasMissed = false;
            var hitChance = (damageDealer.accuracy.Value + damageDealer.weaponAccuracy - target.initiative.Value) / 100;
            var randomValue = Random.value;
            
            return randomValue <= hitChance;
        }
    }
}