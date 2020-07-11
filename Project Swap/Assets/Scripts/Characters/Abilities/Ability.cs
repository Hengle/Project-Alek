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
        #region FieldsAndProperties
        
        [HorizontalGroup("Icon", 175)] [PreviewField(175)] [HideLabel] 
        public Sprite icon;
        
        [ReadOnly, VerticalGroup("Icon/Info")] 
        public AbilityType abilityType;
        
        [VerticalGroup("Icon/Info")] [EnumPaging]
        public DamageType damageType;
        
        [VerticalGroup("Icon/Info")] [EnumPaging]
        public TargetOptions targetOptions;

        [Space] [Range(1,6)] [VerticalGroup("Icon/Info")]
        public int actionCost;

        [Space] [Range(1,3)] [VerticalGroup("Icon/Info")]
        public float damageMultiplier = 1f;

        [Space] [VerticalGroup("Icon/Info")]
        public int attackState = 2;

        [Space] [VerticalGroup("Icon/Info")]
        public bool isMultiTarget;

        [Space] [TextArea(5,15)]
        public string  description = "Insert description for this ability";

        [Space] [BoxGroup("Elements")] public bool hasElemental;
        
        [BoxGroup("Elements")] [ShowIf(nameof(hasElemental)), InlineEditor]
        public ElementalType elementalType;
        
        [BoxGroup("Elements")] [ShowIf(nameof(hasElemental)), SerializeField] 
        private ElementalScaler elementalScaler;
        
        [BoxGroup("Elements")] [ShowInInspector, ShowIf(nameof(hasElemental)), HideLabel]
        public float ElementalScaler => (float) elementalScaler / 100;

        [Space] [BoxGroup("Status Effects")] public bool hasStatusEffect;
        
        [BoxGroup("Status Effects")] [ShowIf(nameof(hasStatusEffect)), InlineEditor]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        [BoxGroup("Status Effects")] [ShowIf(nameof(hasStatusEffect))]
        [Range(0, 1)] public float chanceOfInfliction;
        
        #endregion
        
        public string GetParameters(int actionOption) => $"AbilityAction,{actionOption},{(int)targetOptions},{actionCost}";
    }
}