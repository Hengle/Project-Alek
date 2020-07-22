using System;
using System.Collections.Generic;
using UnityEngine;
using Characters.Abilities;
using Characters.Animations;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using DamagePrefab;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace Characters
{
    public enum CharacterType { PartyMember, Enemy }
    public abstract class UnitBase : SerializedScriptableObject
    {
        #region FieldsAndProperties

        [HorizontalGroup("Basic", 125), PreviewField(125), HideLabel] 
        public Sprite icon;
        
        [VerticalGroup("Basic/Info"), LabelWidth(120)] 
        public Vector3 scale = Vector3.one;

        [VerticalGroup("Basic/Info"), ReadOnly, LabelWidth(120)] 
        public CharacterType id;

        [VerticalGroup("Basic/Info"), LabelWidth(120)] 
        public string characterName;
        
        [VerticalGroup("Basic/Info"), LabelWidth(120)] 
        public GameObject characterPrefab;

        [HideLabel, ShowInInspector, HorizontalGroup("Stat Data", 175), PreviewField(175), ShowIf(nameof(characterPrefab))] 
        public Sprite CharacterPrefab 
        {
            get 
            {
                if (characterPrefab == null) return null;
                return characterPrefab.TryGetComponent(out SpriteRenderer renderer)? renderer.sprite : null;
            }
            set => characterPrefab.GetComponent<SpriteRenderer>().sprite = value;
        }

        [ProgressBar(1,99), VerticalGroup("Basic/Info"), LabelWidth(75)]
        public int level;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty, Title("Stats")]
        public CharacterStat health;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty] 
        public CharacterStat strength;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty] 
        public CharacterStat magic;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty] 
        public CharacterStat accuracy;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty]
        public CharacterStat initiative;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty] 
        public CharacterStat defense;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty] 
        public CharacterStat resistance;
        
        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(100), InlineProperty] 
        public CharacterStat criticalChance;

        [PropertySpace(SpaceBefore = 20, SpaceAfter = 20), TextArea(5,15), Title("Description"), HideLabel]
        public string  description;
        
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Elements")]
        [DictionaryDrawerSettings(KeyLabel = "Element", ValueLabel = "Resistance Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<ElementalType, ElementalScalar> _elementalResistances = new Dictionary<ElementalType, ElementalScalar>();
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Elements")]
        [DictionaryDrawerSettings(KeyLabel = "Element", ValueLabel = "Weakness Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<ElementalType, ElementalWeaknessScalar> _elementalWeaknesses = new Dictionary<ElementalType, ElementalWeaknessScalar>();
  
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Status Effects")]
        [DictionaryDrawerSettings(KeyLabel = "Status Effect", ValueLabel = "Resistance Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<StatusEffect, InflictionChanceModifier> _statusEffectResistances = new Dictionary<StatusEffect, InflictionChanceModifier>();
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Status Effects")]
        [DictionaryDrawerSettings(KeyLabel = "Status Effect", ValueLabel = "Weakness Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<StatusEffect, InflictionChanceModifier> _statusEffectWeaknesses = new Dictionary<StatusEffect, InflictionChanceModifier>();
        
        [Range(1,99), TabGroup("Tabs","Weapon Stats")] public int weaponMight;
        [Range(1,99), TabGroup("Tabs","Weapon Stats")] public int magicMight;
        [Range(1,150), TabGroup("Tabs","Weapon Stats")] public int weaponAccuracy;
        [Range(1,99), TabGroup("Tabs","Weapon Stats")] public int weaponCriticalChance;

        [TabGroup("Tabs","Abilities")] [InlineEditor]
        [OnValueChanged(nameof(CheckAbilityCount))] 
        public List<Ability> abilities = new List<Ability>();

        public void CheckAbilityCount() 
        {
            if (abilities.Count <= 5) return;
            Debug.LogError("Cannot have more than 5 abilities at a time!");
        }
        
        public Color Color 
        {
            get
            {
                if (Unit.currentHP <= 0.3f * (int) health.BaseValue)
                {
                    Unit.outline.color = Color.red;
                    return Color.red;
                }
                if (Unit.currentHP <= 0.6f * (int) health.BaseValue)
                {
                    Unit.outline.color = Color.yellow;
                    return Color.yellow;
                }
                Unit.outline.color = Color.green;
                return Color.green;
            }
        }

        public Unit Unit { get; set; }
        
        [HideInInspector] public int maxAP = 6;

        [HideInInspector] public Action<UnitBase> onDeath;
        [HideInInspector] public Action onHpValueChanged;
        [HideInInspector] public Action<UnitStates> onNewState;
        [HideInInspector] public Action<ElementalType> onElementalDamageReceived;
        [HideInInspector] public Action<StatusEffect> onStatusEffectReceived;
        [HideInInspector] public Action<StatusEffect> onStatusEffectRemoved;

        public bool IsDead => Unit.status == Status.Dead;

        protected int CurrentHP 
        {
            get => Unit.currentHP;
            set 
            {
                Unit.currentHP = value < 0 ? 0 : value;
                if (Unit.currentHP > health.BaseValue) Unit.currentHP = (int) health.BaseValue;
                onHpValueChanged?.Invoke();
                Unit.outline.color = Color;
            } 
        }
        
        #endregion

        private void OnValidate()
        {
            onHpValueChanged = null;
            onStatusEffectReceived = null;
            onStatusEffectRemoved = null;
            onDeath = null;
            onNewState = null;
        }

        public virtual void ResetAP() 
        {
            Unit.currentAP += 2;
            if (Unit.currentAP > 6) Unit.currentAP = 6;
        }

        #region GetFunctions

        public Ability GetAndSetAbility(int index) => abilities[index];

        public void GetDamageValues()
        {
            if (Unit.isAbility && Unit.currentAbility.isMultiTarget)
            {
                Unit.multiHitTargets.ForEach(t => Unit.damageValueList.Add
                    (Calculator.CalculateAttackDamage(this, t)));
            }

            else
            {
                Unit.currentTarget = NullCheck(Unit.currentTarget);
                Unit.currentDamage = Calculator.CalculateAttackDamage(this, Unit.currentTarget);
            }
        }

        // TODO: This seems unnecessary, it does not seem like target can be null when called
        private UnitBase NullCheck(UnitBase target) => target != null && target.Unit.status != Status.Dead? target : this;
        
        public bool GetStatus()
        {
            switch (Unit.status)
            {
                case Status.Normal: return true;
                case Status.Dead: return false;
                case Status.UnableToPerformAction: return false;
                default: return true;
            }
        }

        #endregion

        public virtual void Heal(float amount) {}

        public virtual void TakeDamage(int dmg, ElementalType elementalDmg)
        {
            if (dmg != -1)
            {
                if (Unit.parry) { dmg = (int) (dmg * 0.80f); Unit.parry = false; }
                CurrentHP -= dmg;
                if (elementalDmg != null) onElementalDamageReceived?.Invoke(elementalDmg);
            }

            var position = Unit.gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 3, position.z);
            
            var damage = DamagePrefabManager.Instance.ShowDamage(dmg, Unit.targetHasCrit);
            damage.transform.position = newPosition;

            if (Unit.targetHasCrit) Unit.targetHasCrit = false;

            // Damage is set to -1 when it misses
            // TODO: Add condition for parry to trigger parry animation, challenge event (which will stop attacker's animation)
            if (dmg == -1 && Unit.currentHP > 0) return;
            if (Unit.currentHP > 0) Unit.anim.SetTrigger(AnimationHandler.HurtTrigger);
            else Die();
        }

        protected virtual void Die()
        {
            Unit.status = Status.Dead;
            onDeath?.Invoke(this);
            Unit.anim.SetBool(AnimationHandler.DeathTrigger, true);
        }

        public Quaternion LookAtTarget()
        {
            var rangeAbility = (RangedAttack) Unit.currentAbility;

            var transform1 = Unit.transform;
            var originalRotation = transform1.rotation;
            var lookAtPosition = rangeAbility.lookAtTarget?
                Unit.currentTarget.Unit.transform.position : transform1.position;

            if (!rangeAbility.lookAtTarget) return originalRotation;
            
            Unit.transform.LookAt(lookAtPosition);
            Unit.transform.rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.forward);

            return originalRotation;
        }
    }
}