using System.Linq;
using BattleSystem.Mechanics;
using Characters;
using Characters.Abilities;
using SingletonScriptableObject;
using Random = UnityEngine.Random;

namespace BattleSystem.Calculators
{
    public static class Calculator
    {
        public static int CalculateAttackDamage(UnitBase damageDealer, UnitBase target)
        {
            if (damageDealer.Unit == target.Unit) return 0;

            var type = damageDealer.Unit.isAbility ? damageDealer.CurrentAbility.GetType() : null;
            
            if (type != typeof(Spell))
            {
                var hitChance = CalculateAccuracy(damageDealer, target);
                if (!hitChance)
                {
                    target.Unit.attackerHasMissed = true;
                    return -1;
                }
            }

            if (type == typeof(Spell)) return CalculateSpellDamage(damageDealer, target);
            if (damageDealer.Unit.isAbility) return CalculateAbilityDamage(damageDealer, target);
            
            float dealerDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight;
            var targetDefense = (int) target.defense.Value * (target.level / 2);

            var totalDamage = (int) dealerDamage - targetDefense;

            if (damageDealer.Unit.overrideElemental) totalDamage += CalculateOverrideElementalDamage(damageDealer, target);
            totalDamage = CalculateBoostFactor(damageDealer, target, totalDamage);
            totalDamage = CalculateConversionFactor(damageDealer, totalDamage);
            totalDamage = CalculateShieldFactor(damageDealer, target, totalDamage);

            var critical = CalculateCritChance(damageDealer);
            
            return CalculateFinalDamageAmount(target, totalDamage, critical);
        }

        public static int CalculateSpecialAttackDamage(UnitBase damageDealer, UnitBase target)
        {
            float dealerDamage = (int) damageDealer.strength.Value * damageDealer.weaponMight;
            var targetDefense = (int) target.defense.Value * (target.level / 2);

            var totalDamage = (int) (dealerDamage * damageDealer.Unit.currentAbility.damageMultiplier) - targetDefense;

            totalDamage = CalculateBoostFactor(damageDealer, target, totalDamage);
            
            return CalculateFinalDamageAmount(target, totalDamage, false);
        }

        private static int CalculateSpellDamage(UnitBase damageDealer, UnitBase target)
        {
            var ability = damageDealer.Unit.currentAbility;

            var normalDamage = (int) damageDealer.magic.Value * damageDealer.magicMight;

            var targetDefense = (int) target.resistance.Value * (target.level / 2);
            
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
                Logging.Instance.Log($"{target.characterName} resists {ability.elementalType.name}!");
                var resistanceScalar = 1 - (float) target._elementalResistances.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= resistanceScalar;
            }
                    
            else if (tryGetWeakness)
            {
                Logging.Instance.Log($"{target.characterName} is weak to {ability.elementalType.name}!");
                var weaknessScalar = (float) target._elementalWeaknesses.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= weaknessScalar;
            }
            
            totalDamage = (int) ((normalDamage + elementalDamage) * damageDealer.Unit.currentAbility.damageMultiplier) - targetDefense;
            Logging.Instance.Log($"Elemental Damage: {elementalDamage} \t Total Damage: {totalDamage}");
            
            SkipElemental:
            totalDamage = CalculateBoostFactor(damageDealer, target, totalDamage);
            totalDamage = CalculateConversionFactor(damageDealer, totalDamage);
            totalDamage = CalculateShieldFactor(damageDealer, target, totalDamage);
            
            var critical = CalculateCritChance(damageDealer);
            return CalculateFinalDamageAmount(target, totalDamage, critical);
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
                Logging.Instance.Log($"{target.characterName} resists {ability.elementalType.name}!");
                var resistanceScalar = 1 - (float) target._elementalResistances.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= resistanceScalar;
            }
                    
            else if (tryGetWeakness)
            {
                Logging.Instance.Log($"{target.characterName} is weak to {ability.elementalType.name}!");
                var weaknessScalar = (float) target._elementalWeaknesses.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= weaknessScalar;
            }
            
            totalDamage = (int) ((normalDamage + elementalDamage) * damageDealer.Unit.currentAbility.damageMultiplier) - targetDefense;
            //Logging.Instance.Log($"Elemental Damage: {elementalDamage} \t Total Damage: {totalDamage}");
            
            SkipElemental:
            if (damageDealer.Unit.overrideElemental) totalDamage += CalculateOverrideElementalDamage(damageDealer, target);
            totalDamage = CalculateBoostFactor(damageDealer, target, totalDamage);
            totalDamage = CalculateConversionFactor(damageDealer, totalDamage);
            totalDamage = CalculateShieldFactor(damageDealer, target, totalDamage);
            if (damageDealer.Unit.hasSummon) totalDamage = CalculateSummonBonus(damageDealer, totalDamage);
            
            var critical = CalculateCritChance(damageDealer);
            return CalculateFinalDamageAmount(target, totalDamage, critical);
        }

        private static int CalculateOverrideElementalDamage(UnitBase dealer, UnitBase target)
        {
            var ability = dealer.Unit.overrideAbility;
            
            var elementalDamage = (int) dealer.magic.Value * dealer.weaponMight * ability.ElementalScalar;
            
            var tryGetRes = target._elementalResistances.Any
                (kvp => kvp.Key._type == ability.elementalType);
            var tryGetWeakness = target._elementalWeaknesses.Any
                (kvp => kvp.Key._type == ability.elementalType);

            if (tryGetRes)
            {
                Logging.Instance.Log($"{target.characterName} resists {ability.elementalType.name}!");
                var resistanceScalar = 1 - (float) target._elementalResistances.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= resistanceScalar;
            }
                    
            else if (tryGetWeakness)
            {
                Logging.Instance.Log($"{target.characterName} is weak to {ability.elementalType.name}!");
                var weaknessScalar = (float) target._elementalWeaknesses.Single
                    (s => s.Key._type == ability.elementalType).Key._scalar / 100;

                elementalDamage *= weaknessScalar;
            }

            Logging.Instance.Log($"Elemental Damage: {(int)elementalDamage}");
            return (int) elementalDamage;
        }
        
        private static bool CalculateCritChance(UnitBase damageDealer)
        {
            var critChance = damageDealer.criticalChance.Value / 100;
            var randomValue = Random.value;

            return randomValue <= critChance;
        }

        private static int CalculateSummonBonus(UnitBase damageDealer, float totalDamage)
        {
            totalDamage *= damageDealer.Unit.currentSummon.MasterDamageModifier;
            return (int) totalDamage;
        }

        private static int CalculateFinalDamageAmount(UnitBase target, int totalDamage, bool isCritical)
        {
            if (!isCritical) return totalDamage < 0 ? 0 : Random.Range((int) (0.97f * totalDamage), (int) (1.03f * totalDamage));
            
            totalDamage = (int)(totalDamage * GlobalVariables.Instance.criticalDamageFactor);
            target.Unit.targetHasCrit = true;

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
            if (damageDealer.id == CharacterType.Enemy && !damageDealer.Unit.hasSummon) return totalDamage;
            return (int) (totalDamage * target.Unit.ShieldFactor);
        }

        private static int CalculateBoostFactor(UnitBase damageDealer, UnitBase target, int totalDamage)
        {
            if (damageDealer.id == CharacterType.PartyMember || damageDealer.Unit.hasSummon)
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