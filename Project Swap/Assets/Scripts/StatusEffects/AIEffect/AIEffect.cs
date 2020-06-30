using Characters;
using UnityEngine;

namespace StatusEffects.AIEffect
{
    [CreateAssetMenu(fileName = "AI Effect", menuName = "Status Effect/AI Effect")]
    public class AIEffect : StatusEffect
    {
        private void Awake() => effectType = EffectType.AI;
        
        public override void InflictStatus(UnitBase unit)
        {
            
        }

        public override void OnAdded(UnitBase target)
        {
            
        }

        public override void OnRemoval(UnitBase unit)
        {
            
        }
    }
}