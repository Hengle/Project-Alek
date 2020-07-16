﻿using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.StatusEffects
{
    public enum EffectType { DamageOverTime, Inhibiting, AI, StatChange }
    public enum RateOfInfliction { EveryTurn, BeforeEveryAction, AfterEveryAction, AfterAttacked, Once }
    public enum InflictionChanceModifier { Normal = 25, Moderate = 50, Significant = 75, Major = 100 }
    public abstract class StatusEffect : ScriptableObject
    {
        #region FieldsAndProperties
        
        [Space, ReadOnly, VerticalGroup("Icon/Info")]
        public EffectType effectType;
        
        [VerticalGroup("Icon/Info")]
        public GameObject icon;

        [HideLabel, ShowInInspector, HorizontalGroup("Icon", 200), PreviewField(200), ShowIf(nameof(icon))]
        public Sprite Icon
        {
            get => icon == null ? null : icon.GetComponent<Image>().sprite;
            set => icon.GetComponent<Image>().sprite = value;
        }

        [HideIf(nameof(effectType), EffectType.StatChange)] [HideIf(nameof(name), "Susceptible")]
        [Space, Tooltip("How often the effect is inflicted"), VerticalGroup("Icon/Info"), EnumPaging]
        public List<RateOfInfliction> rateOfInfliction = new List<RateOfInfliction>();
        
        [HideIf(nameof(effectType), EffectType.StatChange)] [HideIf(nameof(name), "Susceptible")] 
        [Space, VerticalGroup("Icon/Info"), ColorPalette, HideLabel] 
        public Color color;
        
        [Space, VerticalGroup("Icon/Info"), Range(1,5), LabelWidth(120)] 
        public int turnDuration;
        
        #endregion
        
        public virtual void InflictStatus(UnitBase unitBase) { }

        public virtual void OnAdded(UnitBase target)
        {
            Logger.Log($"{target.characterName} is inflicted with {name}.");
            target.onStatusEffectReceived?.Invoke(this);
        }

        public virtual void OnRemoval(UnitBase unitBase)
        {
            Logger.Log($"{unitBase.characterName} is no longer inflicted with {name}.");
            unitBase.onStatusEffectRemoved?.Invoke(this);
        }
        
        public float StatusEffectModifier( UnitBase target)
        {
            if (target._statusEffectResistances.TryGetValue(this, out var resistanceModifier))
            {
                Logger.Log($"{target.characterName} resists {name}!");
                return 1.0f - (float) resistanceModifier / 100;
            }

            if (target._statusEffectWeaknesses.TryGetValue(this, out var weaknessModifier))
            {
                Logger.Log($"{target.characterName} is weak to {name}!");
                return 1.0f + (float) weaknessModifier / 100;
            }

            return 1;
        }
    }
}