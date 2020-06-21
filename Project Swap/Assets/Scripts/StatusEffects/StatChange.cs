using Characters;
using UnityEngine;

namespace StatusEffects
{
    [CreateAssetMenu(fileName = "Stat Effect", menuName = "Status Effect/Stat Change")]
    public class StatChange : StatusEffect
    {
        private void Awake() => effectType = EffectType.StatChange;

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