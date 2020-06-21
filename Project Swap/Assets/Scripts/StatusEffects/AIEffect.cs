using System;
using Characters;
using UnityEngine;

namespace StatusEffects
{
    [CreateAssetMenu(fileName = "AI Effect", menuName = "Status Effect/AI Effect")]
    public class AIEffect : StatusEffect
    {
        private void Awake() => effectType = EffectType.AI;
        
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