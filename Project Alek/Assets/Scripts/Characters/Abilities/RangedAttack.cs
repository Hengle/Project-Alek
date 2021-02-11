using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Abilities
{
    [CreateAssetMenu(fileName = "Ranged Attack", menuName = "Ability/Ranged Attack")]
    public class RangedAttack : Ability
    {
        [Space] [Title("Ranged Ability Specific Fields"), LabelWidth(120)]
        public bool lookAtTarget;
        
        protected virtual void Awake() => abilityType = AbilityType.Ranged;
    }
}