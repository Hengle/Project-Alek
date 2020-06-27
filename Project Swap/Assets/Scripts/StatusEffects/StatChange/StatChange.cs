using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace StatusEffects.StatChange
{
    public enum AffectedStat { None, MaxHP, Strength, Magic, Accuracy, Initiative, CriticalChance, Defense, Resistance }
    public enum Multiplier { None, Slight, Moderate, Significant}
    [CreateAssetMenu(fileName = "Stat Effect", menuName = "Status Effect/Stat Change")]
    public class StatChange : StatusEffect
    {
        // Needs to be controlled by abilities
        public AffectedStat buffedStat;
        public Multiplier buffMultiplier;
        
        public AffectedStat debuffedStat;
        public Multiplier debuffMultiplier;


        // public List<AffectedStat> buffedStats = new List<AffectedStat>();
        // public List<AffectedStat> debuffedStats = new List<AffectedStat>();

        private const float SlightBuff = 1.10f;
        private const float SlightDebuff = 0.90f;

        private const float ModerateBuff = 1.20f;
        private const float ModerateDebuff = 0.80f;

        private const float SignificantBuff = 1.35f;
        private const float SignificantDebuff = 0.65f;

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

        public override void InflictStatus(Unit unit)
        {
            // Do nothing
        }
        
        public override void OnAdded(Unit target)
        {
            // foreach (var stat in buffedStats) AffectThisStat(stat, target, true, false);
            // foreach (var stat in debuffedStats) AffectThisStat(stat, target, false, false);
            AffectThisStat(buffedStat, target, true, false);
            AffectThisStat(debuffedStat, target, false, false);
            
            if (target.statusBox == null) return;
            
            var alreadyHasIcon = target.id == 0 ? target.statusBox.GetChild(0).Find(name) : target.statusBox.Find(name);
            if (icon != null && alreadyHasIcon == null)
            {
                var iconGO = Instantiate(icon, target.id == 0 ?
                    target.statusBox.GetChild(0).transform : target.statusBox.transform);
                
                iconGO.name = name;
                iconGO.GetComponent<StatusEffectTimer>().SetTimer(this, target);
            }
            
            else if (icon != null && alreadyHasIcon != null) {
                alreadyHasIcon.gameObject.SetActive(true);
                alreadyHasIcon.GetComponent<StatusEffectTimer>().SetTimer(this, target);
            }
        }
        
        public override void OnRemoval(Unit unit)
        {
            Debug.Log("Stat Change has been removed from " + unit);
            // foreach (var stat in buffedStats) AffectThisStat(stat, unit, true, true);
            // foreach (var stat in debuffedStats) AffectThisStat(stat, unit, false, true);
            AffectThisStat(buffedStat, unit, true, true);
            AffectThisStat(debuffedStat, unit, false, true);
            
            if (unit.statusBox == null) return;
            
            var iconGO = unit.id == 0 ? unit.statusBox.GetChild(0).Find(name) : unit.statusBox.Find(name);
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
                case AffectedStat.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }
        }
    }
}