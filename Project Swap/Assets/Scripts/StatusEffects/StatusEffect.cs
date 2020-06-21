using Characters;
using UnityEngine;

namespace StatusEffects
{
    public enum EffectType { DamageOverTime, Inhibiting, AI, StatChange }
    public enum RateOfInfliction { EveryTurn, EveryAction, AfterAttacked, Once }
    public abstract class StatusEffect : ScriptableObject
    {
        public GameObject icon;
        public EffectType effectType;
        [Tooltip("How often the effect is inflicted")] public RateOfInfliction rateOfInfliction;
        [Tooltip("Set the color that the damage text will be")] public Color color;
        [Tooltip("Number of turns that the effect lasts")] public int duration;
        public abstract void InflictStatus(Unit unit);
        public abstract void OnAdded(Unit unit);
        public abstract void OnRemoval(Unit unit);
    }
}