using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Physical Attack", menuName = "Ability/Physical Attack")]
    public class PhysicalAttack : Ability
    {
        private void Awake() => abilityType = AbilityType.Physical;
    }
}