using Characters;
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

        private const float SlightBuff = 1.05f;
        private const float SlightDebuff = 0.95f;

        private const float ModerateBuff = 1.10f;
        private const float ModerateDebuff = 0.90f;

        private const float SignificantBuff = 1.20f;
        private const float SignificantDebuff = 0.80f;

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

        public override void InflictStatus(Unit unit) { /*Do nothing*/ }
        
        public override void OnAdded(Unit target) {
            Logger.Log(target.unitName + " is inflicted with " + name);
            AffectThisStat(buffedStat, target, true, false);
            AffectThisStat(debuffedStat, target, false, false);
            SetIconAndTimer(target);
        }
        
        public override void OnRemoval(Unit unit)
        {
            Logger.Log("Stat Change has been removed from " + unit);
            AffectThisStat(buffedStat, unit, true, true);
            AffectThisStat(debuffedStat, unit, false, true);
            
            if (unit.statusBox == null) return;
            
            var iconGO = unit.id == Type.Enemy ? unit.statusBox.GetChild(0).Find(name) : unit.statusBox.Find(name);
            if (iconGO != null) iconGO.gameObject.SetActive(false);
        }

        private void AffectThisStat(AffectedStat stat, Unit unit, bool isBuff, bool removing)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP:
                    float maxHealth = unit.maxHealthRef;
                    if (removing) maxHealth /= isBuff ? BuffMultiplier : DebuffMultiplier;
                    else maxHealth *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.maxHealthRef = (int) maxHealth;
                    break;
                
                case AffectedStat.Strength:
                    float strength = unit.currentStrength;
                    if (removing) { strength /= isBuff ? BuffMultiplier : DebuffMultiplier; strength++; }
                    else strength *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentStrength = (int) strength;
                    break;
                
                case AffectedStat.Magic:
                    float magic = unit.currentMagic;
                    if (removing) { magic /= isBuff ? BuffMultiplier : DebuffMultiplier; magic++; }
                    else magic *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentMagic = (int) magic;
                    break;
                
                case AffectedStat.Accuracy:
                    float accuracy = unit.currentAccuracy;
                    if (removing) { accuracy /= isBuff ? BuffMultiplier : DebuffMultiplier; accuracy++; }
                    else accuracy *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentAccuracy = (int) accuracy;
                    break;
                
                case AffectedStat.Initiative:
                    float init = unit.currentInitiative;
                    if (removing) { init /= isBuff ? BuffMultiplier : DebuffMultiplier; init++; }
                    else init *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentInitiative = (int) init;
                    break;
                
                case AffectedStat.CriticalChance:
                    float crit = unit.currentCrit;
                    if (removing) { crit /= isBuff ? BuffMultiplier : DebuffMultiplier; crit++; }
                    else crit *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentCrit = (int) crit;
                    break;
                
                case AffectedStat.Defense:
                    float defense = unit.currentDefense;
                    if (removing) { defense /= isBuff ? BuffMultiplier : DebuffMultiplier; defense++; }
                    else defense *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentDefense = (int) defense;
                    break;
                
                case AffectedStat.Resistance:
                    float resistance = unit.currentResistance;
                    if (removing) { resistance /= isBuff ? BuffMultiplier : DebuffMultiplier; resistance++; }
                    else resistance *= isBuff ? BuffMultiplier : DebuffMultiplier;
                    unit.currentResistance = (int) resistance;
                    break;
                case AffectedStat.None: break;
                default: Logger.Log("Could not find the stat to remove");
                    break;
            }
        }
    }
}