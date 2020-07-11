using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.StatusEffects
{
    public enum EffectType { DamageOverTime, Inhibiting, AI, StatChange }
    public enum RateOfInfliction { EveryTurn, BeforeEveryAction, AfterEveryAction, AfterAttacked, Once }
    public enum InflictionChanceModifier { Normal = 125, Moderate = 150, Significant = 75, Major = 100 }
    public abstract class StatusEffect : ScriptableObject
    {
        #region FieldsAndProperties
        
        [Space] [ReadOnly] public EffectType effectType;
        
        [HideLabel] public GameObject icon;

        [HideLabel] [ShowInInspector] [HorizontalGroup("Icon", 175)] [PreviewField(175)]
        public Sprite Icon
        {
            get => icon.GetComponent<Image>().sprite;
            set => icon.GetComponent<Image>().sprite = value;
        }

        [Space] [Tooltip("How often the effect is inflicted")] [VerticalGroup("Icon/Info")] [EnumPaging]
        public List<RateOfInfliction> rateOfInfliction = new List<RateOfInfliction>();
        
        [Space] [Tooltip("Set the color that the damage text will be")] [VerticalGroup("Icon/Info")]
        public Color color;
        
        [Space] [Tooltip("Number of turns that the effect lasts")] [VerticalGroup("Icon/Info")]
        public int duration;
        
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
    }
}