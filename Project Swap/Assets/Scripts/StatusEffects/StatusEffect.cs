using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    public enum EffectType { DamageOverTime, Inhibiting, AI, StatChange }
    public enum RateOfInfliction { EveryTurn, EveryAction, AfterAttacked, BeforeAttacked, Once }
    public abstract class StatusEffect : ScriptableObject
    {
        public GameObject icon;
        public EffectType effectType;
        [Tooltip("How often the effect is inflicted")] 
        public List<RateOfInfliction> rateOfInfliction = new List<RateOfInfliction>();
        [Tooltip("Set the color that the damage text will be")] 
        public Color color;
        [Tooltip("Number of turns that the effect lasts")] 
        public int duration;
        public abstract void InflictStatus(Unit unit);
        public abstract void OnAdded(Unit target);
        public abstract void OnRemoval(Unit unit);

        protected void SetIconAndTimer(Unit target)
        {
            if (target.statusBox == null) return;
            
            var alreadyHasIcon = target.id == Type.Enemy ? target.statusBox.GetChild(0).Find(name) : target.statusBox.Find(name);
            if (icon != null && alreadyHasIcon == null) {
                var iconGO = 
                    Instantiate(icon, target.id == Type.Enemy ? target.statusBox.transform.GetChild(0) : target.statusBox, false);
                
                iconGO.name = name;
                iconGO.GetComponent<StatusEffectTimer>().SetTimer(this, target);
            }
            
            else if (icon != null && alreadyHasIcon != null) {
                alreadyHasIcon.gameObject.SetActive(true);
                alreadyHasIcon.GetComponent<StatusEffectTimer>().SetTimer(this, target);
            }
        }
    }
}