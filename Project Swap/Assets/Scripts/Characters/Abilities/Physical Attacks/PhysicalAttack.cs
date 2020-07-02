using UnityEngine;

namespace Characters.Abilities.Physical_Attacks
{
    [CreateAssetMenu(fileName = "Physical Attack", menuName = "Ability/Physical Attack")]
    public class PhysicalAttack : Ability
    {
        private void Awake() => abilityType = AbilityType.Physical;
    }
}