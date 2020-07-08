﻿using System;
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
        public Vector3 scale = Vector3.one;
        public GameObject characterPrefab;
        public Sprite icon;

        public CharacterType id;
        public string characterName;
        [TextArea(5,15)] public string  description;
        
        [Header("Stats")]
        [Range(0,99)] public int level;
        [SerializeField] public CharacterStat health;
        [SerializeField] public CharacterStat strength;
        [SerializeField] public CharacterStat magic;
        [SerializeField] public CharacterStat accuracy;
        [SerializeField] public CharacterStat initiative;
        [SerializeField] public CharacterStat defense;
        [SerializeField] public CharacterStat resistance;
        [SerializeField] public CharacterStat criticalChance;

        // Need to account for potential mechanics where bosses change their resistances and weaknesses mid fight
        // For cases where enemies lose their resistance when susceptible, just add a function/variable to 
        // The Elemental Type/Status Effects classes that disables them
        [Header("Resistances and Weaknesses")]
        [ShowInInspector]
        public readonly Dictionary<ElementalType, ElementalScaler> _elementalResistances = new Dictionary<ElementalType, ElementalScaler>();
        [ShowInInspector]
        public readonly Dictionary<ElementalType, ElementalWeaknessScaler> _elementalWeaknesses = new Dictionary<ElementalType, ElementalWeaknessScaler>();
        [PropertySpace]
        [ShowInInspector]
        public readonly Dictionary<StatusEffect, InflictionChanceModifier> _statusEffectResistances = new Dictionary<StatusEffect, InflictionChanceModifier>();
        [ShowInInspector]
        public readonly Dictionary<StatusEffect, InflictionChanceModifier> _statusEffectWeaknesses = new Dictionary<StatusEffect, InflictionChanceModifier>();

        [Header("Weapon Stats")]
        [Range(1,99)] public int weaponMight;
        [Range(1,99)] public int magicMight;
        [Range(1,99)] public int weaponAccuracy;
        [Range(1,99)] public int weaponCriticalChance;

        public Color profileBoxColor;
        private readonly Color normalHealthColor = Color.green;
        private readonly Color midHealthColor = Color.yellow;
        private readonly Color lowHealthColor = Color.red;

        public Color Color
        {
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
        
        [HideInInspector] public int maxAP = 6;
        public Unit Unit { get; set; }
        [Header("Abilities")]
        public List<Ability> abilities = new List<Ability>();

        public Action<UnitBase> onDeath;
        public Action onHpValueChanged;
        public Action<StatusEffect> onStatusEffectReceived;
        public Action<StatusEffect> onStatusEffectRemoved;

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

        private void OnValidate()
        {
            foreach (var r in _elementalResistances)
            {
                if (_elementalWeaknesses.ContainsKey(r.Key))
                    Debug.LogError("Cannot have an Elemental Type in as both a weakness and a resistance");
                break;
            }
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