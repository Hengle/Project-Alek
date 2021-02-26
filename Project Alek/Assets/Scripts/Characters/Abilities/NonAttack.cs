using UnityEngine;

namespace Characters
{
    /*
     * I don't know if I want there to be abilities that purely buff/debuff. It may lead to a reliance on
     * applying these effects at the start of every major encounter. I may want to tie them to skills that
     * will give a buff based on a condition or something.
     */
    [CreateAssetMenu(fileName = "Non-Attack", menuName = "Ability/Non-Attack")]
    public class NonAttack : Ability
    {
        private void Awake() => abilityType = AbilityType.NonAttack;
    }
}