using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BattleSystem;
using BattleSystem.Calculator;
using BattleSystem.DamagePrefab;
using Characters.Abilities;
using Characters.Animations;
using Characters.StatusEffects;

namespace Characters
{
    public enum Type { PartyMember, Enemy, All }
    public abstract class UnitBase : ScriptableObject
    {
        public Vector3 scale = Vector3.one;
        public GameObject characterPrefab;
        public Sprite icon;

        public Type id;
        public string characterName;
        [TextArea(5,15)] public string  description;
        [Range(0,99)] public int level;
        [Range(0,99999)] public int health;
        [Range(0,99)] public int strength;
        [Range(0,99)] public int magic;
        [Range(0,99)] public int accuracy;
        [Range(0,99)] public int initiative;
        [Range(0,99)] public int defense;
        [Range(0,99)] public int resistance;
        [Range(0,99)] public int criticalChance;

        public Color profileBoxColor;
        private readonly Color normalHealthColor = Color.green;
        private readonly Color midHealthColor = Color.yellow;
        private readonly Color lowHealthColor = Color.red;

        public Color Color
        {
            get
            {
                if (Unit.currentHP <= 0.25f * Unit.maxHealthRef) {
                    Unit.outline.color = lowHealthColor;
                    return lowHealthColor;
                }
                if (Unit.currentHP <= 0.5f * Unit.maxHealthRef) {
                    Unit.outline.color = midHealthColor;
                    return midHealthColor;
                } 
                Unit.outline.color = normalHealthColor;
                return normalHealthColor;
            }
        }
        
        [HideInInspector] public int maxAP = 6;
        public Unit Unit { get; private set; }
        public List<Ability> abilities = new List<Ability>();

        public Action<UnitBase> onDeath;
        public Action<int> onHpValueChanged;
        public Action<StatusEffect> onStatusEffectReceived;
        public Action<StatusEffect> onStatusEffectRemoved;

        public bool IsDead => Unit.status == Status.Dead;
        
        private int CurrentHP 
        {
            set 
            { 
                Unit.currentHP = value < 0 ? 0 : value;
                onHpValueChanged?.Invoke(Unit.currentHP);
                Unit.outline.color = Color;
            } 
        }

        public void GiveCommand() {
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
            if (Unit.isAbility && Unit.currentAbility.isMultiTarget) {
                foreach (var target in Unit.multiHitTargets) 
                    Unit.damageValueList.Add(DamageCalculator.CalculateAttackDamage(this, target));
            }

            else {
                Unit.currentTarget = CheckTargetStatus(Unit.currentTarget);
                Unit.currentDamage = DamageCalculator.CalculateAttackDamage(this, Unit.currentTarget);
            }
        }

        public void TakeDamage(int dmg)
        {
            CurrentHP = Unit.currentHP - (dmg < 0 ? 0 : dmg);

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

        private void Die() // If i want to make special death sequences, just make it an event or cutscene
        {
            Unit.status = Status.Dead;
            onDeath?.Invoke(this);
            Unit.anim.SetBool(AnimationHandler.DeathTrigger, true);
        }

        public void ResetAP() {
            Unit.currentAP += 2;
            if (Unit.currentAP > 6) Unit.currentAP = 6;
        }
        

        public bool GetStatus()
        {
            switch (Unit.status)
            {
                case Status.Normal: return true;
                case Status.Dead: return false;
                default: return true;
            }
        }

        private UnitBase CheckTargetStatus(UnitBase target) {
            if (target != null) return Unit.currentTarget.Unit.status != Status.Dead? target : this;
            return this;
        }

        public void SetupUnit(GameObject unitGO)
        {
            unitGO.name = characterName;
            Unit = unitGO.GetComponent<Unit>();
            
            Unit.id = id;
            Unit.level = level;
            Unit.status = Status.Normal;
            Unit.maxHealthRef = health;
            Unit.currentHP = health;
            Unit.currentStrength = strength;
            Unit.currentMagic = magic;
            Unit.currentAccuracy = accuracy;
            Unit.currentInitiative = initiative;
            Unit.currentCrit = criticalChance;
            Unit.currentDefense = defense;
            Unit.currentResistance = resistance;
            Unit.currentAP = maxAP;
            Unit.outline.color = Color;

            var chooseTarget = Unit.gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = this;
            chooseTarget.enabled = true;
        }
    }
}