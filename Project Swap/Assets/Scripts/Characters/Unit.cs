using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattleSystem;
using Characters.Abilities;
using Characters.Animations;
using Characters.StatusEffects;

namespace Characters
{
    // Use these with status effects (like inhibiting)
    public enum Status { Normal, Dead, UnableToPerformAction }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        #region HideInInspector

        [HideInInspector] public UnitBase currentTarget;
        [HideInInspector] public Ability currentAbility;
        [HideInInspector] public Animator anim;
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Button button;

        [HideInInspector] public bool targetHasCrit;
        [HideInInspector] public bool isCrit;
        [HideInInspector] public bool isAbility;
        [HideInInspector] public bool attackerHasMissed;
        
        [HideInInspector] public int commandActionOption;
        [HideInInspector] public int maxHealthRef;
        [HideInInspector] public int currentAP;
        [HideInInspector] public int actionCost;
        [HideInInspector] public int currentDamage;
        [HideInInspector] public string commandActionName;

        #endregion

        public Status status = Status.Normal;

        public int level;
        public int currentHP;
        public int currentStrength;
        public int currentMagic;
        public int currentAccuracy;
        public int currentInitiative;
        public int currentCrit;
        public int currentDefense;
        public int currentResistance;

        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        public List<UnitBase> multiHitTargets = new List<UnitBase>();
        public List<int> damageValueList = new List<int>();
        
        public Action onSelect;
        public Action onDeselect;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            outline.enabled = false;
            status = Status.Normal;
        }

        private void Update() 
        {
            button.enabled = BattleManager._choosingTarget;
            if (!BattleManager._choosingTarget) outline.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            outline.enabled = true;
            onSelect?.Invoke();
        }

        public void OnDeselect(BaseEventData eventData) 
        {
            outline.enabled = false;
            onDeselect?.Invoke();
        }

        public void Setup(UnitBase unitBase)
        {
            unitBase.Unit = this;
            name = unitBase.characterName;
            level = unitBase.level;
            status = Status.Normal;
            maxHealthRef = (int) unitBase.health2.Value;
            currentHP = (int) unitBase.health2.Value;
            currentStrength = (int) unitBase.strength2.Value;
            currentMagic = (int) unitBase.magic2.Value;
            currentAccuracy = (int) unitBase.accuracy2.Value;
            currentInitiative = (int) unitBase.initiative2.Value;
            currentCrit = (int) unitBase.criticalChance2.Value;
            currentDefense = (int) unitBase.defense2.Value;
            currentResistance = (int) unitBase.resistance2.Value;
            
            // maxHealthRef = unitBase.health;
            // currentHP = unitBase.health;
            // currentStrength = unitBase.strength;
            // currentMagic = unitBase.magic;
            // currentAccuracy = unitBase.accuracy;
            // currentInitiative = unitBase.initiative;
            // currentCrit = unitBase.criticalChance;
            // currentDefense = unitBase.defense;
            // currentResistance = unitBase.resistance;
            
            
            currentAP = unitBase.maxAP;
            outline.color = unitBase.Color;
            
            var chooseTarget = gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = unitBase;
            chooseTarget.enabled = true;
        }
    }
}