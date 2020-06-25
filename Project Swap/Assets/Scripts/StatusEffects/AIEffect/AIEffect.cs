using Characters;
using UnityEngine;

namespace StatusEffects.AIEffect
{
    [CreateAssetMenu(fileName = "AI Effect", menuName = "Status Effect/AI Effect")]
    public class AIEffect : StatusEffect
    {
        private void Awake() => effectType = EffectType.AI;
        
        public override void InflictStatus(Unit unit)
        {
            
        }

        public override void OnAdded(Unit target)
        {
            
        }

        public override void OnRemoval(Unit unit)
        {
            
        }
    }
}