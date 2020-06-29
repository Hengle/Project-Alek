using System.Collections.Generic;
using Abilities;
using Abilities.Ranged_Attacks;
using Animations;
using BattleSystem;
using Calculator;
using UnityEngine;

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

        public Color profileBoxColor;
        
        [HideInInspector] public int maxAP = 6;
        [HideInInspector] public Unit unit;
        
        public List<Ability> abilities = new List<Ability>();

        public void GiveCommand() {
            BattleHandler.battleFuncs.GetCommand(this);
            BattleHandler.performingAction = true;
        }

        public Quaternion LookAtTarget()
        {
            var rangeAbility = (RangedAttack) unit.currentAbility;

            var transform1 = unit.transform;
            var originalRotation = transform1.rotation;
            var lookAtPosition = rangeAbility.lookAtTarget ? unit.currentTarget.transform.position : transform1.position;

            if (!rangeAbility.lookAtTarget) return originalRotation;
            
            unit.transform.LookAt(lookAtPosition);
            unit.transform.rotation *= Quaternion.FromToRotation(Vector3.right, Vector3.forward);

            return originalRotation;
        }

        public Ability GetAndSetAbility(int index) => abilities[index];

        public void GetDamageValues()
        {
            if (unit.isAbility && unit.currentAbility.isMultiHit) {
                foreach (var target in unit.multiHitTargets) 
                    unit.damageValueList.Add(DamageCalculator.CalculateAttackDamage(this, target.unit));
            }

            else {
                unit.currentTarget = CheckTargetStatus(unit.currentTarget);
                unit.currentDamage = DamageCalculator.CalculateAttackDamage(this, unit.currentTarget);
            }
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
        
        public Unit CheckTargetStatus(Unit target) {
            if (target != null) return unit.currentTarget.status != Status.Dead ? target : unit;
            return unit;
        }

        public void SetupUnit(UnitBase reference)
        {
            unit.id = id;
            unit.level = level;
            unit.status = Status.Normal;
            unit.unitName = characterName;
            unit.maxHealthRef = health;
            unit.CurrentHP = health;
            unit.currentStrength = strength;
            unit.currentMagic = magic;
            unit.currentAccuracy = accuracy;
            unit.currentInitiative = initiative;
            unit.currentCrit = criticalChance;
            unit.currentDefense = defense;
            unit.currentResistance = resistance;
            unit.currentAP = maxAP;
            unit.unitRef = reference;
        }
    }
}