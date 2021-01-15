using System.Linq;
using BattleSystem.Mechanics;
using Characters;
using Characters.Abilities;
using Random = UnityEngine.Random;

namespace BattleSystem.Calculators
{
    public static class Calculator
    {
        // TODO: Make this the base function that is called for everything and delegate to correct function based on action type
        // TODO: OVERHAUL THE CRAP OUT OF THIS CRAPPY CODE!!!!!!!!!!!!!!!
        public static int CalculateAttackDamage(UnitBase damageDealer, UnitBase target)
        {
            if (damageDealer.Unit == target.Unit) return 0;

            var hitChance = CalculateAccuracy(damageDealer, target);
            if (!hitChance) { target.Unit.attackerHasMissed = true; return -1; }

            if (damageDealer.Unit.isAbility) return CalculateAbilityDamage(damageDealer, target);
            
            float dealerDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight;
            var targetDefense = (int) target.defense.Value * (target.level / 2);

            var totalDamage = (int) dealerDamage - targetDefense;

            totalDamage = CalculateBoostFactor(damageDealer, target, totalDamage);
            totalDamage = CalculateConversionFactor(damageDealer, totalDamage);
            totalDamage = CalculateShieldFactor(damageDealer, target, totalDamage);

            var critical = CalculateCritChance(damageDealer);
            
            return CalculateFinalDamageAmount(damageDealer, target, totalDamage, critical);
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

            var tryGetRes = target._elementalResistances.Any
                (kvp => kvp.Key._type == ability.elementalType);
            var tryGetWeakness = target._elementalWeaknesses.Any
                (kvp => kvp.Key._type == ability.elementalType);

            if (tryGetRes)
            {
                Logger.Log($"{target.characterName} resists {ability.elementalType.name}!");
                var resistanceScalar = 1 - (float) target._elementalResistances.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= resistanceScalar;
            }
                    
            else if (tryGetWeakness)
            {
                Logger.Log($"{target.characterName} is weak to {ability.elementalType.name}!");
                var weaknessScalar = (float) target._elementalWeaknesses.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= weaknessScalar;
            }
            
            totalDamage = (int) ((normalDamage + elementalDamage) * damageDealer.Unit.currentAbility.damageMultiplier) - targetDefense;
            Logger.Log($"Elemental Damage: {elementalDamage} \t Total Damage: {totalDamage}");
            
            SkipElemental:
            totalDamage = CalculateBoostFactor(damageDealer, target, totalDamage);
            totalDamage = CalculateConversionFactor(damageDealer, totalDamage);
            totalDamage = CalculateShieldFactor(damageDealer, target, totalDamage);
            
            var critical = CalculateCritChance(damageDealer);
            return CalculateFinalDamageAmount(damageDealer, target, totalDamage, critical);
        }
        
        private static bool CalculateCritChance(UnitBase damageDealer)
        {
            var critChance = damageDealer.criticalChance.Value / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }

        private static int CalculateFinalDamageAmount(UnitBase damageDealer, UnitBase target, int totalDamage, bool isCritical)
        {
            if (!isCritical) return totalDamage < 0 ? 0 : Random.Range((int) (0.97f * totalDamage), (int) (1.03f * totalDamage));

            totalDamage = (int)(totalDamage * BattleManager.Instance.globalVariables.criticalDamageFactor);
            target.Unit.targetHasCrit = true;
            //damageDealer.Unit.isCrit = true;
            
            return totalDamage < 0 ? 0 : Random.Range((int)(0.97f * totalDamage), (int)(1.03f * totalDamage));
        }

        private static bool CalculateAccuracy(UnitBase damageDealer, UnitBase target)
        {
            target.Unit.attackerHasMissed = false;
            var hitChance = (damageDealer.accuracy.Value + damageDealer.weaponAccuracy - target.initiative.Value) / 100;
            var randomValue = Random.value;
            
            return randomValue <= hitChance;
        }

        private static int CalculateShieldFactor(UnitBase damageDealer, UnitBase target, int totalDamage)
        {
            if (damageDealer.id == CharacterType.Enemy) return totalDamage;
            return (int) (totalDamage * target.Unit.ShieldFactor);
        }

        private static int CalculateBoostFactor(UnitBase damageDealer, UnitBase target, int totalDamage)
        {
            if (damageDealer.id == CharacterType.PartyMember)
            {
                var damageVal = damageDealer.Unit.gameObject.GetComponent<BoostSystem>().FinalDamageBoostVal;
                return (int)(totalDamage * damageVal);
            }

            var defVal = target.Unit.gameObject.GetComponent<BoostSystem>().FinalDefenseBoostVal;
            return (int)(totalDamage * defVal);
        }
        
        private static int CalculateConversionFactor(UnitBase damageDealer, int totalDamage) =>
            (int) (totalDamage * damageDealer.Unit.conversionFactor);
    }
}