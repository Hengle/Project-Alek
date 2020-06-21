using System;
using StatusEffects;
using UnityEngine;

namespace Abilities
{
    public enum AbilityType { Physical, Ranged, NonAttack }
    public abstract class Ability : ScriptableObject
    {
        public AbilityType type;
        [Range(0, 6)] public int actionCost;
        [Tooltip("0 = Enemy, 1 = Party Member, 2 = All")]
        [Range(0, 2)] public int targetOptions;
        public bool hasStatusEffect;
        public StatusEffect statusEffect;
        [Range(0, 1)] public float chanceOfInfliction;
        public int attackState;

        public string GetParameters(int actionOption) { return $"AbilityAction,{actionOption},{targetOptions},{actionCost}"; }
    }
}