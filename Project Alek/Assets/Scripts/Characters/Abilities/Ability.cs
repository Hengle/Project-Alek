using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Characters.ElementalTypes;
using Characters.StatusEffects;

namespace Characters.Abilities
{
    public enum AbilityType { CloseRange, Ranged, NonAttack }
    public enum DamageType { Physical, Magic, Special }
    public enum TargetOptions { Enemies = 0, PartyMembers = 1, Both = 2, Self = 3 }
    public abstract class Ability : ScriptableObject
    {
        #region FieldsAndProperties
        
        [VerticalGroup("Icon/Info"), LabelWidth(120)] 
        public AbilityType abilityType;

        [Title("Main Info"), VerticalGroup("Icon/Info"), LabelWidth(120)]
        [HideIf(nameof(abilityType), AbilityType.NonAttack)] [EnumPaging]
        public DamageType damageType;

        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public TargetOptions targetOptions;

        [HorizontalGroup("Icon", 175)] [PreviewField(175)] [HideLabel] 
        public Sprite icon;

        [Range(1,6)] [VerticalGroup("Icon/Info"), LabelWidth(120)]
        [HideIf(nameof(damageType), DamageType.Special)]
        public int actionCost;

        [Range(0,3)] [VerticalGroup("Icon/Info"), LabelWidth(120)]
        [HideIf(nameof(abilityType), AbilityType.NonAttack)]
        public float damageMultiplier = 1f;

        [VerticalGroup("Icon/Info"), LabelWidth(120)]
        [HideIf(nameof(damageType), DamageType.Special)]
        public int attackState = 2;

        [VerticalGroup("Icon/Info"), LabelWidth(120), InlineEditor(InlineEditorModes.LargePreview)]
        [HideIf(nameof(GetType), typeof(Spell))]
        public AnimationClip animation;

        [VerticalGroup("Icon/Info"), LabelWidth(120)]
        public bool hasOverride;

        [VerticalGroup("Icon/Info"), LabelWidth(120)]
        [HideIf(nameof(damageType), DamageType.Special)]
        public bool isMultiTarget;

        [Space(15)] [TextArea(5,15)] [Title("Description"), HideLabel]
        public string  description = "Insert description for this ability";

        [Space(15)] [Title("Elements"), LabelWidth(120)]
        [HideIf(nameof(abilityType), AbilityType.NonAttack)]
        [HideIf(nameof(damageType), DamageType.Special)]
        public bool hasElemental;
        
        [ShowIf(nameof(hasElemental)), InlineEditor, LabelWidth(120)]
        public ElementalType elementalType;
        
        [ShowIf(nameof(hasElemental)), SerializeField, EnumPaging, HorizontalGroup("Scalar"), LabelWidth(120)] 
        private ElementalResistanceScalar elementalScalar;
        
        [ShowInInspector, ShowIf(nameof(hasElemental)), HideLabel, VerticalGroup("Scalar/Value"), LabelWidth(5)]
        public float ElementalScalar => (float) elementalScalar / 100;

        [Space(15)] [Title("Status Effects"), LabelWidth(120)]
        public bool hasStatusEffect;
        
        [ShowIf(nameof(hasStatusEffect)), InlineEditor]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        [ShowIf(nameof(hasStatusEffect))]
        [Range(0, 1)] public float chanceOfInfliction;
        
        #endregion

        public virtual string GetParameters(int actionOption)
        {
            return $"AbilityAction,{actionOption},{(int) targetOptions},{actionCost}";
        }
    }
}