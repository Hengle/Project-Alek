using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Ranged Attack", menuName = "Ability/Ranged Attack")]
    public class RangedAttack : Ability
    {
        private void Awake() => type = AbilityType.Ranged;
    }
}