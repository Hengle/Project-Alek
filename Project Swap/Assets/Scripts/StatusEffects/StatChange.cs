using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    public enum AffectedStat { MaxHP, Strength, Magic, Accuracy, Initiative, CriticalChance, Defense, Resistance }
    [CreateAssetMenu(fileName = "Stat Effect", menuName = "Status Effect/Stat Change")]
    public class StatChange : StatusEffect
    {
        public List<AffectedStat> buffedStats = new List<AffectedStat>();
        public List<AffectedStat> debuffedStats = new List<AffectedStat>();

        private readonly float buffMultiplier = 1.10f;
        private readonly float debuffMultiplier = 0.90f;
        
        private void Awake() => effectType = EffectType.StatChange;

        public override void InflictStatus(Unit unit)
        {
            // Do nothing
        }
        
        public override void OnAdded(Unit unit)
        {
            foreach (var stat in buffedStats) AffectThisStat(stat, unit, true, false);
            foreach (var stat in debuffedStats) AffectThisStat(stat, unit, false, false);
        }
        
        public override void OnRemoval(Unit unit)
        {
            foreach (var stat in buffedStats) AffectThisStat(stat, unit, true, true);
            foreach (var stat in debuffedStats) AffectThisStat(stat, unit, false, true);
        }

        private void AffectThisStat(AffectedStat stat, Unit unit, bool isBuff, bool removing)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP:
                    float maxHealth = unit.maxHealthRef;
                    if (removing) maxHealth /= isBuff ? buffMultiplier : debuffMultiplier;
                    else maxHealth *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.maxHealthRef = (int) maxHealth;
                    break;
                
                case AffectedStat.Strength:
                    float strength = unit.currentStrength;
                    if (removing) { strength /= isBuff ? buffMultiplier : debuffMultiplier; strength++; }
                    else strength *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentStrength = (int) strength;
                    break;
                
                case AffectedStat.Magic:
                    float magic = unit.currentMagic;
                    if (removing) { magic /= isBuff ? buffMultiplier : debuffMultiplier; magic++; }
                    else magic *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentMagic = (int) magic;
                    break;
                
                case AffectedStat.Accuracy:
                    float accuracy = unit.currentAccuracy;
                    if (removing) { accuracy /= isBuff ? buffMultiplier : debuffMultiplier; accuracy++; }
                    else accuracy *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentAccuracy = (int) accuracy;
                    break;
                
                case AffectedStat.Initiative:
                    float init = unit.currentInitiative;
                    if (removing) { init /= isBuff ? buffMultiplier : debuffMultiplier; init++; }
                    else init *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentInitiative = (int) init;
                    break;
                
                case AffectedStat.CriticalChance:
                    float crit = unit.currentCrit;
                    if (removing) { crit /= isBuff ? buffMultiplier : debuffMultiplier; crit++; }
                    else crit *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentCrit = (int) crit;
                    break;
                
                case AffectedStat.Defense:
                    float defense = unit.currentDefense;
                    if (removing) { defense /= isBuff ? buffMultiplier : debuffMultiplier; defense++; }
                    else defense *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentDefense = (int) defense;
                    break;
                
                case AffectedStat.Resistance:
                    float resistance = unit.currentResistance;
                    if (removing) { resistance /= isBuff ? buffMultiplier : debuffMultiplier; resistance++; }
                    else resistance *= isBuff ? buffMultiplier : debuffMultiplier;
                    unit.currentResistance = (int) resistance;
                    break;
            }
        }
    }
}