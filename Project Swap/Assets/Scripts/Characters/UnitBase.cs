using System.Collections.Generic;
using Abilities;
using Animations;
using UnityEngine;

namespace Characters
{
    public abstract class UnitBase : ScriptableObject
    {
        public Vector3 scale = Vector3.one;
        public GameObject characterPrefab;
        public Sprite icon;

        public int id; // change this to enum later
        public string characterName;
        [TextArea(5,15)] public string  description;
        [Range(0,99)] public int level;
        [Range(0,9999)] public int health;
        [Range(0,99)] public int strength;
        [Range(0,99)] public int magic;
        [Range(0,99)] public int accuracy;
        [Range(0,99)] public int initiative;
        [Range(0,99)] public int defense;
        [Range(0,99)] public int resistance;
        [Range(0,99)] public int criticalChance;
        
        [HideInInspector] public int maxAP = 6;
        [HideInInspector] public Unit unit;
        
        public List<Ability> abilities = new List<Ability>();
        
        public abstract void GiveCommand(bool isSwapping);

        public Ability GetAndSetAbility(int index)
        {
            //unit.currentAbility = abilities[index]; // redundant??
            return abilities[index];
        }
        
        public void ResetCommandsAndAP()
        {
            unit.currentAP += 2;
            if (unit.currentAP > 6) unit.currentAP = 6;
        }

        public void ResetAnimationStates()
        {
            unit.anim.SetInteger(AnimationHandler.PhysAttackState, 0);
        }
        
        public bool CheckUnitStatus()
        {
            switch (unit.status)
            {
                case Status.Normal: return true;
                case Status.Dead: return false;
                default: return true;
            }
        }
        
        public Unit CheckTargetStatus(bool isSwap)
        {
            return unit.currentTarget.status != Status.Dead ? unit.currentTarget : unit;
        }
        
        public void SetupUnit(UnitBase reference)
        {
            unit.id = id;
            unit.level = level;
            unit.status = Status.Normal;
            unit.unitName = characterName;
            //unit.nameText.text = characterName.ToUpper();
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