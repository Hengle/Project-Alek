﻿using System;
using BattleSystem;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.StatusEffects
{
    public enum AffectedStat { None, MaxHP, Strength, Magic, Accuracy, Initiative, CriticalChance, Defense, Resistance }
    public enum Multiplier { None, Slight, Moderate, Significant}
    [CreateAssetMenu(fileName = "Stat Effect", menuName = "Status Effect/Stat Change")]
    public class StatChange : StatusEffect
    {
        #region FieldsAndProperties

        public string shortStatName;
        
        [Space] [SerializeField] [Title("Buffs"), VerticalGroup("Icon/Info"), LabelWidth(120)]
        [HideIf(nameof(name), "Checkmate")]
        private bool buffs;
        
        [ShowIf(nameof(buffs))] [HideIf(nameof(name), "Checkmate")]
        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public AffectedStat buffedStat;
        
        [ShowIf(nameof(buffs))] [HideIf(nameof(name), "Checkmate")]
        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public Multiplier buffMultiplier;
        
        [Space] [HideIf(nameof(name), "Checkmate")]
        [SerializeField] [Title("Debuffs"), VerticalGroup("Icon/Info"), LabelWidth(120)]
        private bool debuffs;
        
        [ShowIf(nameof(debuffs))] [HideIf(nameof(name), "Checkmate")]
        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public AffectedStat debuffedStat;
        
        [ShowIf(nameof(debuffs))] [HideIf(nameof(name), "Checkmate")]
        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public Multiplier debuffMultiplier;

        private StatModifier modifier;
        
        private float BuffMultiplier {
            get
            {
                switch (buffMultiplier)
                {
                    case Multiplier.Slight: return BattleEngine.Instance.globalVariables.slightBuff;
                    case Multiplier.Moderate: return BattleEngine.Instance.globalVariables.moderateBuff;
                    case Multiplier.Significant: return BattleEngine.Instance.globalVariables.significantBuff;
                    case Multiplier.None: return 1;
                    default: return 1;
                }
            }
        }

        private float DebuffMultiplier {
            get
            {
                switch (debuffMultiplier)
                {
                    case Multiplier.Slight: return BattleEngine.Instance.globalVariables.slightDebuff;
                    case Multiplier.Moderate: return BattleEngine.Instance.globalVariables.moderateDebuff;
                    case Multiplier.Significant: return BattleEngine.Instance.globalVariables.significantDebuff;
                    case Multiplier.None: return 1;
                    default: return 1;
                }
            }
        }
        
        #endregion

        private void Awake() => effectType = EffectType.StatChange;

        public override void OnAdded(UnitBase target)
        {
            base.OnAdded(target);
            AddModifiers(buffedStat, target, true);
            AddModifiers(debuffedStat, target, false);
            CharacterEvents.Trigger(CEventType.StatChange, target);
        }
        
        public override void OnRemoval(UnitBase unitBase)
        {
            base.OnRemoval(unitBase);
            RemoveModifier(buffedStat, unitBase);
            RemoveModifier(debuffedStat, unitBase);
            CharacterEvents.Trigger(CEventType.StatChange, unitBase);
        }
        
        private void AddModifiers(AffectedStat stat, UnitBase unit, bool isBuff)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP: break;
                
                case AffectedStat.Strength: modifier = isBuff?
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) : 
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.strength.AddModifier(modifier); break;
                
                case AffectedStat.Magic: modifier = isBuff? 
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) :
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.magic.AddModifier(modifier); break;
                
                case AffectedStat.Accuracy: modifier = isBuff? 
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) : 
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.accuracy.AddModifier(modifier); break;
                
                case AffectedStat.Initiative: modifier = isBuff? 
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) :
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.initiative.AddModifier(modifier); break;
                
                case AffectedStat.CriticalChance: modifier = isBuff? 
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) : 
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.criticalChance.AddModifier(modifier); break;
                
                case AffectedStat.Defense: modifier = isBuff?
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) : 
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.defense.AddModifier(modifier); break;
                
                case AffectedStat.Resistance: modifier = isBuff? 
                        new StatModifier(BuffMultiplier, StatModType.PercentAdd, this) : 
                        new StatModifier(DebuffMultiplier, StatModType.PercentAdd, this);
                    unit.resistance.AddModifier(modifier); break;
                
                case AffectedStat.None: break;
                default: Logger.Log("Could not find the stat to modify"); break;
            }
        }

        private void RemoveModifier(AffectedStat stat, UnitBase unit)
        {
            switch (stat)
            {
                case AffectedStat.MaxHP: break;
                case AffectedStat.Strength: unit.strength.RemoveModifier(modifier); break;
                case AffectedStat.Magic: unit.magic.RemoveModifier(modifier); break;
                case AffectedStat.Accuracy: unit.accuracy.RemoveModifier(modifier); break;
                case AffectedStat.Initiative: unit.initiative.RemoveModifier(modifier); break;
                case AffectedStat.CriticalChance: unit.criticalChance.RemoveModifier(modifier); break;
                case AffectedStat.Defense: unit.defense.RemoveModifier(modifier); break;
                case AffectedStat.Resistance: unit.resistance.RemoveModifier(modifier); break;
                case AffectedStat.None: break;
            }
        }
    }
}