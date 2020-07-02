using UnityEngine;

namespace Characters.Abilities
{
    [CreateAssetMenu(fileName = "Non-Attack", menuName = "Ability/Non-Attack")]
    public class NonAttack : Ability
    {
        private void Awake() => abilityType = AbilityType.NonAttack;
    }
}