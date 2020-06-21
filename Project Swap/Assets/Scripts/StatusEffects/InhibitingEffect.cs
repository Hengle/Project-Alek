using Characters;
using UnityEngine;

namespace StatusEffects
{
    [CreateAssetMenu(fileName = "Inhibiting Effect", menuName = "Status Effect/Inhibiting Effect")]
    public class InhibitingEffect : StatusEffect
    {
        private void Awake() => effectType = EffectType.Inhibiting;
        
        public override void InflictStatus(Unit unit)
        {
            
        }
        
        public override void OnAdded(Unit unit)
        {
            
        }
        
        public override void OnRemoval(Unit unit)
        {
            
        }
    }
}