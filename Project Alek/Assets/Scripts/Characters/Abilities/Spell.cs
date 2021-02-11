using System;
using UnityEngine;

namespace Characters.Abilities
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Ability/Spell")]
    public class Spell : RangedAttack
    {
        public GameObject effectPrefab;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Magic;
            hasElemental = true;
        }
    }
}