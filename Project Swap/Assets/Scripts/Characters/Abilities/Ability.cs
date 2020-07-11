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

        [ReadOnly, VerticalGroup("Icon/Info"), Title("Main Info"), LabelWidth(120)] 
        public AbilityType abilityType;

        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public DamageType damageType;

        [VerticalGroup("Icon/Info"), LabelWidth(120)] [EnumPaging]
        public TargetOptions targetOptions;

        [HorizontalGroup("Icon", 175)] [PreviewField(175)] [HideLabel] 
        public Sprite icon;

        [Range(1,6)] [VerticalGroup("Icon/Info"), LabelWidth(120)]
        public int actionCost;

        [Range(1,3)] [VerticalGroup("Icon/Info"), LabelWidth(120)]
        public float damageMultiplier = 1f;

        [VerticalGroup("Icon/Info"), LabelWidth(120)]
        public int attackState = 2;

        [VerticalGroup("Icon/Info"), LabelWidth(120)]
        public bool isMultiTarget;

        [Space(15)] [TextArea(5,15)] [Title("Description"), HideLabel]
        public string  description = "Insert description for this ability";

        [Space(15)] [Title("Elements"), LabelWidth(120)] public bool hasElemental;
        
        [ShowIf(nameof(hasElemental)), InlineEditor, LabelWidth(120)]
        public ElementalType elementalType;
        
        [ShowIf(nameof(hasElemental)), SerializeField, EnumPaging, HorizontalGroup("Scaler"), LabelWidth(120)] 
        private ElementalScaler elementalScaler;
        
        [ShowInInspector, ShowIf(nameof(hasElemental)), HideLabel, VerticalGroup("Scaler/Value"), LabelWidth(5)]
        public float ElementalScaler => (float) elementalScaler / 100;

        [Space(15)] [Title("Status Effects"), LabelWidth(120)] public bool hasStatusEffect;
        
        [ShowIf(nameof(hasStatusEffect)), InlineEditor]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        [ShowIf(nameof(hasStatusEffect))]
        [Range(0, 1)] public float chanceOfInfliction;
        
        #endregion

        public string GetParameters(int actionOption) => $"AbilityAction,{actionOption},{(int)targetOptions},{actionCost}";
    }
}