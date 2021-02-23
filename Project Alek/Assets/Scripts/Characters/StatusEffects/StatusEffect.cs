using System.Collections.Generic;
using System.Linq;
using DamagePrefab;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.StatusEffects
{
    [EnumPaging] public enum EffectType { DamageOverTime, Inhibiting, AI, StatChange, Unique }
    [EnumPaging] public enum Rate { EveryTurn, BeforeEveryAction, AfterEveryAction, AfterAttacked, Once }
    [EnumPaging] [LabelWidth(90)] public enum InflictionChanceModifier { Normal = 25, Moderate = 50, Significant = 75, Major = 100 }
    public abstract class StatusEffect : ScriptableObject
    {
        #region FieldsAndProperties
        
        [Space, VerticalGroup("Icon/Info")]
        public EffectType effectType;
        
        [VerticalGroup("Icon/Info")]
        public GameObject icon;

        [HideLabel, ShowInInspector, HorizontalGroup("Icon", 200), PreviewField(200), ShowIf(nameof(icon))]
        public Sprite Icon
        {
            get => icon == null ? null : icon.GetComponent<Image>().sprite;
            set => icon.GetComponent<Image>().sprite = value;
        }

        [HideIf(nameof(effectType), EffectType.StatChange)] 
        [HideIf(nameof(effectType), EffectType.Unique)]
        [HideIf(nameof(name), "Susceptible")]
        [Space, Tooltip("How often the effect is inflicted"), VerticalGroup("Icon/Info"), EnumPaging]
        public List<Rate> rateOfInfliction = new List<Rate>();
        
        [Space, VerticalGroup("Icon/Info"), ColorPalette, HideLabel] 
        public Color color;
        
        [Space, VerticalGroup("Icon/Info"), Range(1,5), LabelWidth(120)] 
        public int turnDuration;
        
        [Space, VerticalGroup("Icon/Info"), LabelWidth(120)]
        public string shortStatName = "";
        
        #endregion
        
        public virtual void InflictStatus(UnitBase unitBase) { }

        public virtual void OnAdded(UnitBase target)
        {
            target.onStatusEffectReceived?.Invoke(this);

            DamagePrefabManager.Instance.DamageTextColor = color;
            DamagePrefabManager.Instance.ShowText(shortStatName != "" ?
                $"+ {shortStatName}" : $"+ {name.ToUpper()}", target.Unit.transform, 8f);
        }

        public virtual void OnRemoval(UnitBase unitBase)
        {
            unitBase.onStatusEffectRemoved?.Invoke(this);
    
            DamagePrefabManager.Instance.DamageTextColor = color;
            DamagePrefabManager.Instance.ShowText(shortStatName != "" ?
                $"- {shortStatName}" : $"- {name.ToUpper()}", unitBase.Unit.transform, 8f);
        }
        
        public float StatusEffectModifier(UnitBase target)
        {
            foreach (var effect in target._statusEffectResistances.
                Where(effect => effect.Key._effect == this))
            {
                return 1.0f - (float) effect.Key._modifier / 100;
            }
            
            foreach (var effect in target._statusEffectWeaknesses.
                Where(effect => effect.Key._effect == this))
            {
                return 1.0f + (float) effect.Key._modifier / 100;
            }

            return 1;
        }
    }
}