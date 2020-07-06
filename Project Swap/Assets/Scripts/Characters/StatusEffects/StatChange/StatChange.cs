using Characters;
using Characters.StatusEffects;
using Kryz.CharacterStats;
using UnityEngine;

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

        private StatModifier modifier;
        
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
            AddModifiers(buffedStat, target, true);
            AddModifiers(debuffedStat, target, false);
            target.onStatusEffectReceived?.Invoke(this);
            CharacterEvents.Trigger(CEventType.StatChange, target);
        }
        
        public override void OnRemoval(UnitBase unitBase)
        {
            Logger.Log("Stat Change has been removed from " + unitBase.characterName);
            RemoveModifier(buffedStat, unitBase);
            RemoveModifier(debuffedStat, unitBase);
            unitBase.onStatusEffectRemoved?.Invoke(this);
            CharacterEvents.Trigger(CEventType.StatChange, unitBase);
        }
        
        private void AddModifiers(AffectedStat stat, UnitBase unit, bool isBuff)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP:
                    break;
                
                case AffectedStat.Strength: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.strength2.AddModifier(modifier);
                    break;
                
                case AffectedStat.Magic: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.magic2.AddModifier(modifier);
                    break;
                
                case AffectedStat.Accuracy: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.accuracy2.AddModifier(modifier);
                    break;
                
                case AffectedStat.Initiative: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.initiative2.AddModifier(modifier);
                    break;
                
                case AffectedStat.CriticalChance: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.criticalChance2.AddModifier(modifier);
                    break;
                
                case AffectedStat.Defense: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.defense2.AddModifier(modifier);
                    break;
                
                case AffectedStat.Resistance: modifier = isBuff? new StatModifier(BuffMultiplier, StatModType.PercentAdd, this)
                        : new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.resistance2.AddModifier(modifier);
                    break;
                
                case AffectedStat.None: break;
                
                default: Logger.Log("Could not find the stat to modify");
                    break;
            }
        }

        private void RemoveModifier(AffectedStat stat, UnitBase unit)
        {
            Logger.Log("Removing Stat Mod");
            switch (stat)
            {
                case AffectedStat.MaxHP:
                    break;
                
                case AffectedStat.Strength: unit.strength2.RemoveModifier(modifier);
                    break;
                
                case AffectedStat.Magic: unit.magic2.RemoveAllModifiersFromSource(this);
                    break;
                
                case AffectedStat.Accuracy: unit.accuracy2.RemoveAllModifiersFromSource(this);
                    break;
                
                case AffectedStat.Initiative: unit.initiative2.RemoveAllModifiersFromSource(this);
                    break;
                
                case AffectedStat.CriticalChance: unit.criticalChance2.RemoveAllModifiersFromSource(this);
                    break;
                
                case AffectedStat.Defense: unit.defense2.RemoveAllModifiersFromSource(this);
                    break;
                
                case AffectedStat.Resistance: unit.resistance2.RemoveAllModifiersFromSource(this);
                    break;
                case AffectedStat.None: break;
                default: Logger.Log("Could not find the stat to remove");
                    break;
            }
        }
    }
}