using System.Runtime.CompilerServices;
using Characters;
using Characters.StatusEffects;
using Kryz.CharacterStats;
using UnityEngine;
using Type = Characters.Type;

namespace StatusEffects.StatChange
{
    public enum AffectedStat { None, MaxHP, Strength, Magic, Accuracy, Initiative, CriticalChance, Defense, Resistance }
    public enum Multiplier { None, Slight, Moderate, Significant}
    [CreateAssetMenu(fileName = "Stat Effect", menuName = "Status Effect/Stat Change")]
    public class StatChange : StatusEffect
    {
        public AffectedStat buffedStat;
        public Multiplier buffMultiplier;
        
        public AffectedStat debuffedStat;
        public Multiplier debuffMultiplier;

        private const float SlightBuff = 0.05f;
        private const float SlightDebuff = -0.05f;

        private const float ModerateBuff = 0.10f;
        private const float ModerateDebuff = -0.10f;

        private const float SignificantBuff = 0.20f;
        private const float SignificantDebuff = -0.20f;
        
        private float BuffMultiplier
        {
            get
            {
                switch (buffMultiplier)
                {
                    case Multiplier.Slight: return SlightBuff;
                    case Multiplier.Moderate: return ModerateBuff;
                    case Multiplier.Significant: return SignificantBuff;
                    case Multiplier.None: return 1;
                    default: return 1;
                }
            }
        }
        private float DebuffMultiplier
        {
            get
            {
                switch (debuffMultiplier)
                {
                    case Multiplier.Slight: return SlightDebuff;
                    case Multiplier.Moderate: return ModerateDebuff;
                    case Multiplier.Significant: return SignificantDebuff;
                    case Multiplier.None: return 1;
                    default: return 1;
                }
            }
        }

        private void Awake() => effectType = EffectType.StatChange;

        public override void InflictStatus(UnitBase unitBase) { /*Do nothing*/ }
        
        public override void OnAdded(UnitBase target) {
            Logger.Log(target.characterName + " is inflicted with " + name);
            AddModifiers(buffedStat, target, true, false);
            AddModifiers(debuffedStat, target, false, false);
            target.onStatusEffectReceived?.Invoke(this);
        }
        
        public override void OnRemoval(UnitBase unitBase)
        {
            Logger.Log("Stat Change has been removed from " + unitBase);
            RemoveModifier(buffedStat, unitBase);
            unitBase.onStatusEffectRemoved?.Invoke(this);
        }

        // Refactor all this
        private void AddModifiers(AffectedStat stat, UnitBase unit, bool isBuff, bool removing)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP:
                    // float maxHealth = unit.maxHealthRef;
                    // if (removing) maxHealth /= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // else maxHealth *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.maxHealthRef = (int) maxHealth;
                    break;
                
                case AffectedStat.Strength:
                    unit.strength2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Strength: {unit.strength2.Value}");
                    // float strength = unit.currentStrength;
                    // if (removing) { strength /= isBuff ? BuffMultiplier : DebuffMultiplier; strength++; }
                    // else strength *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentStrength = (int) strength;
                    break;
                
                case AffectedStat.Magic:
                    unit.magic2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Magic: {unit.magic2.Value}");
                    // float magic = unit.currentMagic;
                    // if (removing) { magic /= isBuff ? BuffMultiplier : DebuffMultiplier; magic++; }
                    // else magic *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentMagic = (int) magic;
                    break;
                
                case AffectedStat.Accuracy:
                    unit.accuracy2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Accuracy: {unit.accuracy2.Value}");
                    // float accuracy = unit.currentAccuracy;
                    // if (removing) { accuracy /= isBuff ? BuffMultiplier : DebuffMultiplier; accuracy++; }
                    // else accuracy *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentAccuracy = (int) accuracy;
                    break;
                
                case AffectedStat.Initiative:
                    unit.initiative2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Initiative: {unit.initiative2.Value}");
                    // float init = unit.currentInitiative;
                    // if (removing) { init /= isBuff ? BuffMultiplier : DebuffMultiplier; init++; }
                    // else init *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentInitiative = (int) init;
                    break;
                
                case AffectedStat.CriticalChance:
                    unit.criticalChance2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Critical Chance: {unit.criticalChance2.Value}");
                    // float crit = unit.currentCrit;
                    // if (removing) { crit /= isBuff ? BuffMultiplier : DebuffMultiplier; crit++; }
                    // else crit *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentCrit = (int) crit;
                    break;
                
                case AffectedStat.Defense:
                    unit.defense2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Defense: {unit.defense2.Value}");
                    // float defense = unit.currentDefense;
                    // if (removing) { defense /= isBuff ? BuffMultiplier : DebuffMultiplier; defense++; }
                    // else defense *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentDefense = (int) defense;
                    break;
                
                case AffectedStat.Resistance:
                    unit.resistance2.AddModifier(isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this));
                    Logger.Log($"Resistance: {unit.resistance2.Value}");
                    // float resistance = unit.currentResistance;
                    // if (removing) { resistance /= isBuff ? BuffMultiplier : DebuffMultiplier; resistance++; }
                    // else resistance *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentResistance = (int) resistance;
                    break;
                case AffectedStat.None: break;
                default: Logger.Log("Could not find the stat to remove");
                    break;
            }
        }

        private void RemoveModifier(AffectedStat stat, UnitBase unit)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP:
                    // float maxHealth = unit.maxHealthRef;
                    // if (removing) maxHealth /= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // else maxHealth *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.maxHealthRef = (int) maxHealth;
                    break;
                
                case AffectedStat.Strength:
                    unit.strength2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Strength: {unit.strength2.Value}");
                    // float strength = unit.currentStrength;
                    // if (removing) { strength /= isBuff ? BuffMultiplier : DebuffMultiplier; strength++; }
                    // else strength *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentStrength = (int) strength;
                    break;
                
                case AffectedStat.Magic:
                    unit.magic2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Magic: {unit.magic2.Value}");
                    // float magic = unit.currentMagic;
                    // if (removing) { magic /= isBuff ? BuffMultiplier : DebuffMultiplier; magic++; }
                    // else magic *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentMagic = (int) magic;
                    break;
                
                case AffectedStat.Accuracy:
                    unit.accuracy2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Accuracy: {unit.accuracy2.Value}");
                    // float accuracy = unit.currentAccuracy;
                    // if (removing) { accuracy /= isBuff ? BuffMultiplier : DebuffMultiplier; accuracy++; }
                    // else accuracy *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentAccuracy = (int) accuracy;
                    break;
                
                case AffectedStat.Initiative:
                    unit.initiative2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Initiative: {unit.initiative2.Value}");
                    // float init = unit.currentInitiative;
                    // if (removing) { init /= isBuff ? BuffMultiplier : DebuffMultiplier; init++; }
                    // else init *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentInitiative = (int) init;
                    break;
                
                case AffectedStat.CriticalChance:
                    unit.criticalChance2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Critical Chance: {unit.criticalChance2.Value}");
                    // float crit = unit.currentCrit;
                    // if (removing) { crit /= isBuff ? BuffMultiplier : DebuffMultiplier; crit++; }
                    // else crit *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentCrit = (int) crit;
                    break;
                
                case AffectedStat.Defense:
                    unit.defense2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Defense: {unit.defense2.Value}");
                    // float defense = unit.currentDefense;
                    // if (removing) { defense /= isBuff ? BuffMultiplier : DebuffMultiplier; defense++; }
                    // else defense *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentDefense = (int) defense;
                    break;
                
                case AffectedStat.Resistance:
                    unit.resistance2.RemoveAllModifiersFromSource(this);
                    Logger.Log($"Resistance: {unit.resistance2.Value}");
                    // float resistance = unit.currentResistance;
                    // if (removing) { resistance /= isBuff ? BuffMultiplier : DebuffMultiplier; resistance++; }
                    // else resistance *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    // unit.currentResistance = (int) resistance;
                    break;
                case AffectedStat.None: break;
                default: Logger.Log("Could not find the stat to remove");
                    break;
            }
        }
    }
}