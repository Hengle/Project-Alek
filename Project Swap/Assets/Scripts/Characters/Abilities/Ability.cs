using System.Collections.Generic;
using Characters.StatusEffects;
using StatusEffects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Abilities
{
    public enum AbilityType { CloseRange, Ranged, NonAttack }
    public enum DamageType { Physical, Magic }
    public enum TargetOptions { Enemies = 0, PartyMembers = 1, Both = 1 }
    public abstract class Ability : ScriptableObject
    {
        public Sprite icon;
        
        [Header("Main Information")]
        [ReadOnly] public AbilityType abilityType;
        public DamageType damageType;
        public TargetOptions targetOptions;
        public bool isMultiTarget;
        [Space] [TextArea(5,15)] public string  description = "Insert description for this ability";
        [Range(1,6)] public int actionCost;
        [Range(1,3)] public float damageMultiplier = 1f;
        [Space] public int attackState = 2;
        
        [Space] [Header("Status Effects")]
        public bool hasStatusEffect;
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        [Range(0, 1)] public float chanceOfInfliction;
        
        public string GetParameters(int actionOption)
        {
            return $"AbilityAction,{actionOption},{(int)targetOptions},{actionCost}";
        }
    }
}