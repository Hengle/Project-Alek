using System.Collections.Generic;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Abilities
{
    public enum AbilityType { CloseRange, Ranged, NonAttack }
    public enum DamageType { Physical, Magic }
    public enum TargetOptions { Enemies = 0, PartyMembers = 1, Both = 1 }
    public abstract class Ability : ScriptableObject
    {
        [TabGroup("Main Information")]
        public Sprite icon;
        
        [ReadOnly, TabGroup("Main Information")]
        public AbilityType abilityType;
        
        [TabGroup("Main Information")]
        public DamageType damageType;
        
        [TabGroup("Main Information")]
        public TargetOptions targetOptions;
        
        [TabGroup("Main Information")]
        public bool isMultiTarget;
        
        [Space] [TextArea(5,15), TabGroup("Main Information")]
        public string  description = "Insert description for this ability";
        
        [Range(1,6), TabGroup("Main Information")]
        public int actionCost;
        
        [Range(1,3), TabGroup("Main Information")] 
        public float damageMultiplier = 1f;
        
        [Space] [TabGroup("Main Information")]
        public int attackState = 2;

        [Space] [TabGroup("Elemental Damage")]
        public bool hasElemental;
        
        [ShowIf(nameof(hasElemental)), TabGroup("Elemental Damage"),  InlineEditor]
        public ElementalType elementalType;
        
        [ShowIf(nameof(hasElemental)), TabGroup("Elemental Damage"), SerializeField] 
        private ElementalScaler elementalScaler;
        
        [ShowInInspector, ShowIf(nameof(hasElemental)), TabGroup("Elemental Damage")]
        public float ElementalScaler => (float) elementalScaler / 100;

        [Space] [TabGroup("Status Effects")]
        public bool hasStatusEffect;
        
        [ShowIf(nameof(hasStatusEffect)), TabGroup("Status Effects"), InlineEditor]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        [ShowIf(nameof(hasStatusEffect)), TabGroup("Status Effects")]
        [Range(0, 1)] public float chanceOfInfliction;
        
        public string GetParameters(int actionOption)
        {
            return $"AbilityAction,{actionOption},{(int)targetOptions},{actionCost}";
        }
    }
}