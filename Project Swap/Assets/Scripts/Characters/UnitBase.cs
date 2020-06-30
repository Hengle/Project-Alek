using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Abilities;
using Abilities.Ranged_Attacks;
using Animations;
using BattleSystem;
using BattleSystem.DamagePrefab;
using Calculator;
using StatusEffects;

namespace Characters
{
    public enum Type { PartyMember, Enemy }
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
        public int CurrentHP { set { unit.currentHP = value < 0 ? 0 : value; OnHpValueChanged(); } }

        public Color profileBoxColor;
        public Color normalHealthColor = Color.green;
        public Color midHealthColor = Color.yellow;
        public Color lowHealthColor = Color.red;
        
        [HideInInspector] public int maxAP = 6;
        private Unit unit;

        public Unit Unit {
            get => unit;
            protected set => unit = value;
        }

        public List<Ability> abilities = new List<Ability>();

        public bool IsDead => unit.status == Status.Dead;

        public void GiveCommand() {
            BattleManager.battleFuncs.GetCommand(this);
            BattleManager.performingAction = true;
        }

        public Quaternion LookAtTarget()
        {
            var rangeAbility = (RangedAttack) unit.currentAbility;

            var transform1 = unit.transform;
            var originalRotation = transform1.rotation;
            var lookAtPosition = rangeAbility.lookAtTarget ? unit.currentTarget.Unit.transform.position : transform1.position;

            if (!rangeAbility.lookAtTarget) return originalRotation;
            
            unit.transform.LookAt(lookAtPosition);
            unit.transform.rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.forward);

            return originalRotation;
        }

        public Ability GetAndSetAbility(int index) => abilities[index];

        public void GetDamageValues()
        {
            if (unit.isAbility && unit.currentAbility.isMultiTarget) {
                foreach (var target in unit.multiHitTargets) 
                    unit.damageValueList.Add(DamageCalculator.CalculateAttackDamage(this, target));
            }

            else {
                unit.currentTarget = CheckTargetStatus(unit.currentTarget);
                unit.currentDamage = DamageCalculator.CalculateAttackDamage(this, unit.currentTarget);
            }
        }
        
        public void TakeDamage(int dmg)
        {
            CurrentHP = unit.currentHP - (dmg < 0 ? 0 : dmg);

            var position = unit.gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 3, position.z);
            
            var damage = DamagePrefabManager.Instance.ShowDamage(dmg, Unit.targetHasCrit);
            damage.transform.position = newPosition;

            if (Unit.targetHasCrit) Unit.targetHasCrit = false;

            if (dmg == -1 && unit.currentHP > 0) return;
            if (unit.currentHP > 0) unit.anim.SetTrigger(AnimationHandler.HurtTrigger);
            else Die();
        }
        
        private void Die() // If i want to make special death sequences, just make it an event or cutscene
        {
            Unit.status = Status.Dead;
            Unit.statusEffects = new List<StatusEffect>();
            BattleManager.RemoveFromBattle(this, id);
            Unit.anim.SetBool(AnimationHandler.DeathTrigger, true);
        }
        
        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (!(from statEffect in Unit.statusEffects
                where effect.name == statEffect.name select effect).Any()) return;
            
            Unit.statusEffects.Remove(effect);
            effect.OnRemoval(this);
        }
        
        public void ResetCommandsAndAP() {
            unit.currentAP += 2;
            if (unit.currentAP > 6) unit.currentAP = 6;
        }

        public void ResetAnimationStates() {
            unit.anim.SetInteger(AnimationHandler.PhysAttackState, 0);
        }
        
        public bool CheckUnitStatus()
        {
            switch (unit.status) {
                case Status.Normal: return true;
                case Status.Dead: return false;
                default: return true;
            }
        }

        private UnitBase CheckTargetStatus(UnitBase target) {
            if (target != null) return unit.currentTarget.Unit.status != Status.Dead ? target : this;
            return this;
        }

        public void SetupUnit(UnitBase reference)
        {
            Unit.id = id;
            Unit.level = level;
            Unit.status = Status.Normal;
            Unit.maxHealthRef = health;
            CurrentHP = health;
            Unit.currentStrength = strength;
            Unit.currentMagic = magic;
            Unit.currentAccuracy = accuracy;
            Unit.currentInitiative = initiative;
            Unit.currentCrit = criticalChance;
            Unit.currentDefense = defense;
            Unit.currentResistance = resistance;
            Unit.currentAP = maxAP;
            Unit.unitRef = reference;
            var chooseTarget = Unit.gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = this;
            chooseTarget.enabled = true;

        }

        public abstract void OnHpValueChanged();
    }
}