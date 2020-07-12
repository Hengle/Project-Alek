using System;
using System.Collections.Generic;
using UnityEngine;
using BattleSystem;
using BattleSystem.Calculator;
using BattleSystem.DamagePrefab;
using Characters.Abilities;
using Characters.Animations;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;

namespace Characters
{
    public enum CharacterType { PartyMember, Enemy }
    public abstract class UnitBase : SerializedScriptableObject
    {
        #region FieldsAndProperties

        [HorizontalGroup("Basic", 125), PreviewField(125), HideLabel] public Sprite icon;
        
        [VerticalGroup("Basic/Info"), LabelWidth(120)] public Vector3 scale = Vector3.one;

        [VerticalGroup("Basic/Info"), ReadOnly, LabelWidth(120)] public CharacterType id;

        [VerticalGroup("Basic/Info"), LabelWidth(120)] public string characterName;
        
        [VerticalGroup("Basic/Info"), LabelWidth(120)] public GameObject characterPrefab;

        [HideLabel, ShowInInspector, HorizontalGroup("Stat Data", 175), PreviewField(175), ShowIf(nameof(characterPrefab))] 
        public Sprite CharacterPrefab {
            get {
                if (characterPrefab == null) return null;
                return characterPrefab.TryGetComponent(out SpriteRenderer renderer) ? renderer.sprite : null;
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

        [Space(20), TextArea(5,15), Title("Description"), HideLabel] public string  description;
        
        // For cases where enemies lose their resistance when susceptible, just add a function/variable to 
        // The Elemental Type/Status Effects classes that disables them
        [Title("Elements"), ShowInInspector, FoldoutGroup("Resistances and Weaknesses")]
        [DictionaryDrawerSettings(KeyLabel = "Element", ValueLabel = "Resistance Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<ElementalType, ElementalScaler> _elementalResistances = new Dictionary<ElementalType, ElementalScaler>();
        [ShowInInspector, FoldoutGroup("Resistances and Weaknesses")]
        [DictionaryDrawerSettings(KeyLabel = "Element", ValueLabel = "Weakness Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<ElementalType, ElementalWeaknessScaler> _elementalWeaknesses = new Dictionary<ElementalType, ElementalWeaknessScaler>();
  
        [Title("Status Effects"), ShowInInspector, FoldoutGroup("Resistances and Weaknesses")]
        [DictionaryDrawerSettings(KeyLabel = "Status Effect", ValueLabel = "Resistance Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<StatusEffect, InflictionChanceModifier> _statusEffectResistances = new Dictionary<StatusEffect, InflictionChanceModifier>();
        [ShowInInspector, FoldoutGroup("Resistances and Weaknesses")]
        [DictionaryDrawerSettings(KeyLabel = "Status Effect", ValueLabel = "Weakness Level", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public readonly Dictionary<StatusEffect, InflictionChanceModifier> _statusEffectWeaknesses = new Dictionary<StatusEffect, InflictionChanceModifier>();
        
        [Range(1,99), FoldoutGroup("Weapon Stats")] public int weaponMight;
        [Range(1,99), FoldoutGroup("Weapon Stats")] public int magicMight;
        [Range(1,99), FoldoutGroup("Weapon Stats")] public int weaponAccuracy;
        [Range(1,99), FoldoutGroup("Weapon Stats")] public int weaponCriticalChance;

        [InlineEditor] [OnValueChanged(nameof(Abilities))] 
        public List<Ability> abilities = new List<Ability>();

        public void Abilities() {
            if (abilities.Count <= 5) return;
            Debug.LogError("Cannot have more than 5 abilities at a time!");
        }
        
        private readonly Color normalHealthColor = Color.green;
        private readonly Color midHealthColor = Color.yellow;
        private readonly Color lowHealthColor = Color.red;
        public Color Color {
            get
            {
                if (Unit.currentHP <= 0.25f * (int) health.BaseValue) {
                    Unit.outline.color = lowHealthColor;
                    return lowHealthColor;
                }
                if (Unit.currentHP <= 0.5f * (int) health.BaseValue) {
                    Unit.outline.color = midHealthColor;
                    return midHealthColor;
                } 
                Unit.outline.color = normalHealthColor;
                return normalHealthColor;
            }
        }

        public Unit Unit { get; set; }
        
        [HideInInspector] public int maxAP = 6;

        [HideInInspector] public Action<UnitBase> onDeath;
        [HideInInspector] public Action onHpValueChanged;
        [HideInInspector] public Action<StatusEffect> onStatusEffectReceived;
        [HideInInspector] public Action<StatusEffect> onStatusEffectRemoved;

        public bool IsDead => Unit.status == Status.Dead;

        protected int CurrentHP {
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
        }

        public virtual void Heal(float amount) {}
     
        public void GiveCommand()
        {
            BattleManager._battleFunctions.GetCommand(this);
            BattleManager._performingAction = true;
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

        public Ability GetAndSetAbility(int index) => abilities[index];
        
        public void GetDamageValues()
        {
            if (Unit.isAbility && Unit.currentAbility.isMultiTarget)
            {
                foreach (var target in Unit.multiHitTargets) 
                    Unit.damageValueList.Add(Calculator.CalculateAttackDamage(this, target));
            }

            else
            {
                Unit.currentTarget = CheckTargetStatus(Unit.currentTarget);
                Unit.currentDamage = Calculator.CalculateAttackDamage(this, Unit.currentTarget);
            }
        }

        public virtual void TakeDamage(int dmg)
        {
            CurrentHP -= dmg < 0 ? 0 : dmg;

            var position = Unit.gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 3, position.z);
            
            var damage = DamagePrefabManager.Instance.ShowDamage(dmg, Unit.targetHasCrit);
            damage.transform.position = newPosition;

            if (Unit.targetHasCrit) Unit.targetHasCrit = false;

            // Damage is set to -1 when it misses
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

        public virtual void ResetAP() 
        {
            Unit.currentAP += 2;
            if (Unit.currentAP > 6) Unit.currentAP = 6;
        }
        
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

        private UnitBase CheckTargetStatus(UnitBase target) 
        {
            if (target != null) return Unit.currentTarget.Unit.status != Status.Dead? target : this;
            return this;
        }
    }
}