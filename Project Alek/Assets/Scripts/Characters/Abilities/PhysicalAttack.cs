using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "Physical Attack", menuName = "Ability/Physical Attack")]
    public class PhysicalAttack : Ability
    {
        private void Awake() => abilityType = AbilityType.CloseRange;
    }
}