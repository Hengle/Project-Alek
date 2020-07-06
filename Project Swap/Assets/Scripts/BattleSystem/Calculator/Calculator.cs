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
                    case DamageType.Str: //Logger.Log("On Str Switch");
                        dealerDamage = (int) damageDealer.strength2.Value * damageDealer.weaponMight * damageDealer.Unit.currentAbility.damageMultiplier;
                        targetDefense = (int) target.defense2.Value * (target.level / 2);
                        break;
                    
                    case DamageType.Mag: //Logger.Log("On Mag Switch");
                        dealerDamage = (int) damageDealer.magic2.Value * damageDealer.magicMight * damageDealer.Unit.currentAbility.damageMultiplier;
                        targetDefense = (int) target.resistance2.Value * (target.Unit.level / 2);
                        break;
                }
            }

            else
            {
                //Logger.Log("Not an ability??");
                dealerDamage = (int) damageDealer.strength2.Value * damageDealer.weaponMight;
                targetDefense = (int) target.defense2.Value * (target.Unit.level / 2);
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
        
        private static bool CalculateCritChance(UnitBase damageDealer)
        {
            var critChance = damageDealer.criticalChance2.Value / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }

        private static bool CalculateAccuracy(UnitBase damageDealer, UnitBase target)
        {
            target.Unit.attackerHasMissed = false;
            var hitChance = (damageDealer.accuracy2.Value + damageDealer.weaponAccuracy - target.initiative2.Value) / 100;
            var randomValue = Random.value;
            
            return randomValue <= hitChance;
        }
    }
}