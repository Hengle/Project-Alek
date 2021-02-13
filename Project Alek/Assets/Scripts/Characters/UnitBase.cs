﻿using System;
using System.Collections.Generic;
using BattleSystem;
using BattleSystem.Calculators;
using BattleSystem.Mechanics;
using UnityEngine;
using Characters.Abilities;
using Characters.Animations;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using DamagePrefab;
using JetBrains.Annotations;
using Kryz.CharacterStats;
using SingletonScriptableObject;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Characters
{
    public enum CharacterType { PartyMember, Enemy, Both }
    public abstract class UnitBase : SerializedScriptableObject
    {
        #region FieldsAndProperties

        [HorizontalGroup("Basic", 175), PreviewField(175), HideLabel]
        public Sprite icon;
        
        [VerticalGroup("Basic/Info"), LabelWidth(120)] 
        public Vector3 scale = Vector3.one;

        [VerticalGroup("Basic/Info"), HideInInspector, LabelWidth(120)] 
        public CharacterType id;

        [HorizontalGroup("Basic/Info/Prefab"), LabelText("Name"), LabelWidth(50)] 
        public string characterName;
        
        [HorizontalGroup("Basic/Info/Prefab"), LabelText("Prefab"), LabelWidth(50)] 
        public GameObject characterPrefab;

        [PropertySpace(30)]
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
        
        [Range(1,99), VerticalGroup("Basic/Info"), LabelWidth(75)]
        public int level;
        
        [Space(20)]
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty] [Title("Stats")]
        public CharacterStat health;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty] 
        public CharacterStat strength;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty] 
        public CharacterStat magic;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty] 
        public CharacterStat accuracy;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty]
        public CharacterStat initiative;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty] 
        public CharacterStat defense;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty] 
        public CharacterStat resistance;
        [SerializeField] [VerticalGroup("Stat Data/Stats")] [LabelWidth(100)] [InlineProperty]
        public CharacterStat criticalChance;

        [PropertySpace(SpaceBefore = 20, SpaceAfter = 20), TextArea(5,15), Title("Description"), HideLabel]
        public string  description;

        [InlineProperty(LabelWidth = 115)]
        public struct ElementalResStruct
        {
            [UsedImplicitly] public ElementalType _type;
            [UsedImplicitly] public ElementalResistanceScalar _scalar;
        }
        
        [InlineProperty(LabelWidth = 115)]
        public struct ElementalWeaknessStruct
        {
            [UsedImplicitly] public ElementalType _type;
            [UsedImplicitly] public ElementalWeaknessScalar _scalar;
        }
        
        [InlineProperty(LabelWidth = 115)]
        public struct StatusEffectStruct
        {
            [UsedImplicitly] public StatusEffect _effect;
            [UsedImplicitly] public InflictionChanceModifier _modifier;
        }

        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Elements")]
        [DictionaryDrawerSettings(KeyLabel = "Revealed?", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public readonly Dictionary<ElementalResStruct, bool> _elementalResistances = new Dictionary<ElementalResStruct, bool>();
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Elements")]
        [DictionaryDrawerSettings(KeyLabel = "Revealed?", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public readonly Dictionary<ElementalWeaknessStruct, bool> _elementalWeaknesses = new Dictionary<ElementalWeaknessStruct, bool>();
  
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Status Effects")]
        [DictionaryDrawerSettings(KeyLabel = "Revealed?", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public readonly Dictionary<StatusEffectStruct, bool> _statusEffectResistances = new Dictionary<StatusEffectStruct, bool>();
        [ShowInInspector] [TabGroup("Tabs/Resistances & Weaknesses", "Status Effects")]
        [DictionaryDrawerSettings(KeyLabel = "Revealed?", DisplayMode = DictionaryDisplayOptions.Foldout)]
        public readonly Dictionary<StatusEffectStruct, bool> _statusEffectWeaknesses = new Dictionary<StatusEffectStruct, bool>();
        
        [TabGroup("Tabs/Resistances & Weaknesses", "Damage Types")]
        [DictionaryDrawerSettings(KeyLabel = "Damage Type", ValueLabel = "Revealed?", DisplayMode = DictionaryDisplayOptions.Foldout)]
        [HideIf(nameof(id), CharacterType.PartyMember)]
        public readonly Dictionary<WeaponDamageType, bool> _damageTypeWeaknesses = new Dictionary<WeaponDamageType, bool>();

        [Range(1,99), TabGroup("Tabs","Weapon Stats")] public int weaponMight;
        [Range(1,99), TabGroup("Tabs","Weapon Stats")] public int magicMight;
        [Range(1,150), TabGroup("Tabs","Weapon Stats")] public int weaponAccuracy;
        [Range(1,99), TabGroup("Tabs","Weapon Stats")] public int weaponCriticalChance;

        [TabGroup("Tabs","Spells & Abilities")] [InlineEditor]
        [ValidateInput(nameof(CheckAbilityCount), "Cannot have more than 5 abilities equipped!")] 
        public List<Ability> abilities = new List<Ability>();
        
        [TabGroup("Tabs","Spells & Abilities")] [InlineEditor]
        [ValidateInput(nameof(CheckSpellCount), "Cannot have more than 5 spells equipped!")] 
        public List<Spell> spells = new List<Spell>();

        private bool CheckAbilityCount => abilities.Count <= 5;
        private bool CheckSpellCount => spells.Count <= 5;

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
        
        public MenuController MenuController { get; set; }
        
        public Selectable Selectable { get; set; }
        
        public Animator BattlePanelAnim { get; set; }
        
        public UnitStates CurrentState
        {
            get => Unit.currentState;
            set => Unit.currentState = value;
        }
        
        public List<StatusEffect> StatusEffects => Unit.statusEffects;
        
        public UnitBase CurrentTarget => Unit.currentTarget;

        public Ability CurrentAbility
        {
            get => Unit. currentAbility;
            set => Unit.currentAbility = value;
        }
        
        public AnimationHandler AnimationHandler => Unit.animationHandler;

        public const int MaxAP = 6;

        [HideInInspector] public Action<UnitBase> onDeath;
        [HideInInspector] public Action<UnitBase> onRevival;
        [HideInInspector] public Action onHpValueChanged;
        [HideInInspector] public Action<int> onShieldValueChanged;
        [HideInInspector] public Action onShieldBroken;
        [HideInInspector] public Action onShieldRestored;
        [HideInInspector] public Action<ElementalType> onElementalDamageReceived;
        [HideInInspector] public Action<WeaponDamageType> onWeaponDamageTypeReceived;
        [HideInInspector] public Action<StatusEffect> onStatusEffectReceived;
        [HideInInspector] public Action<StatusEffect> onStatusEffectRemoved;
        [HideInInspector] public Action<int> onApValChanged;

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

        public int CurrentAP 
        {
            get => Unit.currentAP;
            set
            {
                if (value < 0) Unit.currentAP = 0;
                else if (value > 6) Unit.currentAP = 6;
                else Unit.currentAP = value;
                
                onApValChanged?.Invoke(Unit.currentAP);
            }
        }

        #endregion

        public void OnNewState(UnitStates state)
        {
            switch (state)
            {
                case UnitStates.Checkmate:
                    break;
                case UnitStates.Susceptible:
                    // Do something
                    break;
                case UnitStates.Weakened:
                    // Do something
                    break;
            }
        }

        public virtual void ReplenishAP() => CurrentAP += 2;

        #region GetFunctions

        public Ability GetAndSetAbility(int index) => abilities[index];

        public void GetDamageValues(bool isSpecial)
        {
            if (isSpecial)
            {
                Unit.currentDamage = Calculator.CalculateSpecialAttackDamage(this, Unit.currentTarget);
            }
            else if (Unit.isAbility && Unit.currentAbility.isMultiTarget)
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
                case Status.Overexerted: return false;
                default: return true;
            }
        }

        #endregion

        public virtual void Heal(float amount) {}

        public virtual void TakeDamage(int dmg, ElementalType elementalDmg = null, WeaponDamageType weaponDamageType = null)
        {
            if (dmg != -1)
            {
                if (Unit.parry)
                {
                    dmg = (int) (dmg * GlobalVariables.Instance.timedDefenseBonus);
                    CurrentHP -= dmg;
                    Unit.parry = false;
                    Unit.onTimedDefense?.Invoke(true);
                }
                else if (Unit.timedAttack)
                {
                    dmg = (int) (dmg * GlobalVariables.Instance.timedAttackBonus);
                    CurrentHP -= dmg;
                    Unit.timedAttack = false;
                }
                else CurrentHP -= dmg;
                
                if (elementalDmg) onElementalDamageReceived?.Invoke(elementalDmg);
                if (weaponDamageType) onWeaponDamageTypeReceived?.Invoke(weaponDamageType);
            }

            else
            {
                if (Unit.parry)
                {
                    Unit.parry = false;
                    Unit.onTimedDefense?.Invoke(true);
                }
            }

            var position = Unit.gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 3, position.z);
            
            var damage = DamagePrefabManager.Instance.ShowDamage(dmg, Unit.targetHasCrit);
            damage.transform.position = newPosition;

            if (Unit.targetHasCrit) Unit.targetHasCrit = false;
            
            if (dmg == -1 && Unit.currentHP > 0) return;
            if (Unit.currentHP > 0) Unit.anim.SetTrigger(AnimationHandler.HurtTrigger);
            else Die();
        }

        public void TakeDamageSpecial(int dmg)
        {
            CurrentHP -= dmg;
            
            var position = Unit.gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 3, position.z);
            
            var damage = DamagePrefabManager.Instance.ShowDamage(dmg, false);
            damage.transform.position = newPosition;
            
            if (Unit.currentHP > 0) Unit.anim.SetTrigger(AnimationHandler.HurtTrigger);
            else Die();
        }

        protected virtual void Die()
        {
            Unit.status = Status.Dead;
            CurrentAP = 0;
            onDeath?.Invoke(this);
            Unit.anim.SetBool(AnimationHandler.DeathTrigger, true);
        }

        public virtual void Revive(float percentage, int apAmount)
        {
            Unit.status = Status.Normal;
            CurrentHP = (int)(health.BaseValue * percentage);
            CurrentAP = apAmount;
            onRevival?.Invoke(this);
            Unit.anim.SetTrigger(AnimationHandler.RecoverTrigger);
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

        private void RemoveMods()
        {
            if (!Application.isPlaying) return;
            health = new CharacterStat(health.BaseValue, health.maxValue);
            strength = new CharacterStat(strength.BaseValue, strength.maxValue);
            magic = new CharacterStat(magic.BaseValue, magic.maxValue);
            accuracy = new CharacterStat(accuracy.BaseValue, accuracy.maxValue);
            initiative = new CharacterStat(initiative.BaseValue, initiative.maxValue);
            defense = new CharacterStat(defense.BaseValue, defense.maxValue);
            resistance = new CharacterStat(resistance.BaseValue, resistance.maxValue);
            criticalChance = new CharacterStat(criticalChance.BaseValue, criticalChance.maxValue);
        }

        private void OnEnable() => RemoveMods();
    }
}