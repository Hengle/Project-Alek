using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Non-Attack", menuName = "Ability/Non-Attack")]
    public class NonAttack : Ability
    {
        private void Awake() => type = AbilityType.NonAttack;
    }
}