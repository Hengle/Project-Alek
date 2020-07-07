using System.Collections.Generic;
using UnityEngine;

namespace Characters.StatusEffects
{
    public enum EffectType { DamageOverTime, Inhibiting, AI, StatChange }
    public enum RateOfInfliction { EveryTurn, BeforeEveryAction, AfterEveryAction, AfterAttacked, Once }
    public abstract class StatusEffect : ScriptableObject
    {
        public GameObject icon;
        [ReadOnly] public EffectType effectType;
        
        [Tooltip("How often the effect is inflicted")] 
        public List<RateOfInfliction> rateOfInfliction = new List<RateOfInfliction>();
        
        [Tooltip("Set the color that the damage text will be")] 
        public Color color;
        
        [Tooltip("Number of turns that the effect lasts")] 
        public int duration;

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