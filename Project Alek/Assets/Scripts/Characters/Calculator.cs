using System.Linq;
using Characters.Abilities;
using Random = UnityEngine.Random;

namespace Characters
{
    public static class Calculator
    {
        // TODO: Make this the base function that is called for everything and delegate to correct function based on action type
        public static int CalculateAttackDamage(UnitBase damageDealer, UnitBase target)
        {
            if (damageDealer.Unit == target.Unit) return 0;
            
            var hitChance = CalculateAccuracy(damageDealer, target);
            if (!hitChance) { target.Unit.attackerHasMissed = true; return -1; }

            float dealerDamage = 0;
            var targetDefense = 0;

            if (damageDealer.Unit.isAbility) return CalculateAbilityDamage(damageDealer, target);
            
            dealerDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight;
            targetDefense = (int) target.defense.Value * (target.Unit.level / 2);

            var totalDamage = (int) dealerDamage - targetDefense;

            var critical = CalculateCritChance(damageDealer);
            if (!critical) return totalDamage < 0 ? 0 : Random.Range((int) (0.95f * totalDamage), (int) (1.05f * totalDamage));
            
            totalDamage = (int)(totalDamage * 1.75f);
            target.Unit.targetHasCrit = true;
            damageDealer.Unit.isCrit = true;

            return totalDamage < 0 ? 0 : Random.Range((int)(0.95f * totalDamage), (int)(1.05f * totalDamage));
        }

        private static int CalculateAbilityDamage(UnitBase damageDealer, UnitBase target)
        {
            var ability = damageDealer.Unit.currentAbility;

            var normalDamage = ability.damageType == DamageType.Physical
                ? (int) damageDealer.strength.Value * damageDealer.weaponMight
                : (int) damageDealer.magic.Value * damageDealer.magicMight;

            var targetDefense = (ability.damageType == DamageType.Physical?
                (int) target.defense.Value : (int) target.resistance.Value) * (target.level / 2);
            
            int totalDamage;
            
            if (!ability.hasElemental)
            {
                totalDamage = (int) (normalDamage * damageDealer.Unit.currentAbility.damageMultiplier) - targetDefense;
                goto SkipElemental;
            }
            
            var elementalDamage = (int) damageDealer.magic.Value * damageDealer.weaponMight * ability.ElementalScalar;

            var tryGetRes = target._elementalResistances.ContainsKey(ability.elementalType);
            var tryGetWeakness = target._elementalWeaknesses.ContainsKey(ability.elementalType);

            if (tryGetRes)
            {
                Logger.Log($"{target.characterName} resists {ability.elementalType.name}!");
                var resistanceScalar = 1 - (float) target._elementalResistances.Single
                    (s => s.Key == ability.elementalType).Value / 100;

                elementalDamage *= resistanceScalar;
            }
                    
            else if (tryGetWeakness)
            {
                Logger.Log($"{target.characterName} is weak to {ability.elementalType.name}!");
                var weaknessScalar = (float) target._elementalWeaknesses.Single
                    (s => s.Key == ability.elementalType).Value / 100;

                elementalDamage *= weaknessScalar;
            }
            
            totalDamage = (int) ((normalDamage + elementalDamage) * damageDealer.Unit.currentAbility.damageMultiplier) - targetDefense;
            Logger.Log($"Elemental Damage: {elementalDamage} \t Total Damage: {totalDamage}");
            
            SkipElemental:
            var critical = CalculateCritChance(damageDealer);
            if (!critical) return totalDamage < 0 ? 0 : Random.Range((int) (0.95f * totalDamage), (int) (1.05f * totalDamage));

            totalDamage = (int)(totalDamage * 1.75f);
            target.Unit.targetHasCrit = true;
            damageDealer.Unit.isCrit = true;

            return totalDamage < 0 ? 0 : Random.Range((int)(0.95f * totalDamage), (int)(1.05f * totalDamage));
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