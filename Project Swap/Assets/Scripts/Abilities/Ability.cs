using System;
using StatusEffects;
using UnityEngine;

namespace Abilities
{
    public enum AbilityType { Physical, Ranged, NonAttack }
    public enum DamageType { Str, Mag }
    public abstract class Ability : ScriptableObject
    {
        public AbilityType abilityType;
        public DamageType damageType;
        [TextArea(5,15)] public string  description = "Insert description for this ability";
        [Range(0, 6)] public int actionCost;
        [Tooltip("0 = Enemy, 1 = Party Member, 2 = All")]    
        [Range(0, 2)] public int targetOptions;
        public bool hasStatusEffect;
        public StatusEffect statusEffect;
        [Range(0, 1)] public float chanceOfInfliction;
        [Range(0,3)] public float damageMultiplier = 1f;
        public int attackState = 2;

        public string GetParameters(int actionOption) { return $"AbilityAction,{actionOption},{targetOptions},{actionCost}"; }
    }
}