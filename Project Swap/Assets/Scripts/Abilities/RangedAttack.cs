﻿using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Ranged Attack", menuName = "Ability/Ranged Attack")]
    public class RangedAttack : Ability
    {
        public bool lookAtTarget;
        private void Awake() => abilityType = AbilityType.Ranged;
    }
}