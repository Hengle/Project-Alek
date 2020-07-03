using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using BattleSystem;
using BattleSystem.Calculator;
using Characters.Abilities;
using Characters.Animations;
using Characters.StatusEffects;
using Input;
using Random = UnityEngine.Random;

namespace Characters
{
    public enum Status { Normal, Dead }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [HideInInspector] public GameObject battlePanelRef; // Rework camera system so i can get rid of this
        [HideInInspector] public Type id; // Redundant. base class has type already. remove this
        
        [HideInInspector] public GameObject parent;

        //[HideInInspector] public UnitBase unitRef; // Get rid of this
        [HideInInspector] public UnitBase currentTarget;
        [HideInInspector] public Ability currentAbility;
        [HideInInspector] public Animator anim; // could maybe keep, but remove direct access
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Button button;

        [HideInInspector] public bool targetHasCrit;
        [HideInInspector] public bool isCrit;
        [HideInInspector] public bool isAbility;
        public bool attackerHasMissed;

        // Could convert all of this to events
        [HideInInspector] public int commandActionOption;
        [HideInInspector] public int maxHealthRef;
        [HideInInspector] public int currentAP;
        [HideInInspector] public int weaponMT = 20; // temporary, just for testing
        [HideInInspector] public int actionCost;
        [HideInInspector] public int currentDamage;
        [HideInInspector] public string commandActionName;

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

        // public Unit(UnitBase unitBase)
        // {
        //     id = unitBase.id;
        //     name = unitBase.characterName;
        //     level = unitBase.level;
        //     status = Status.Normal;
        //     maxHealthRef = unitBase.health;
        //     currentHP = unitBase.health;
        //     currentStrength = unitBase.strength;
        //     currentMagic = unitBase.magic;
        //     currentAccuracy = unitBase.accuracy;
        //     currentInitiative = unitBase.initiative;
        //     currentCrit = unitBase.criticalChance;
        //     currentDefense = unitBase.defense;
        //     currentResistance = unitBase.resistance;
        //     currentAP = unitBase.maxAP;
        //     outline.color = unitBase.Color;
        //     
        //     var chooseTarget = gameObject.GetComponent<ChooseTarget>();
        //     chooseTarget.thisUnitBase = unitBase;
        //     chooseTarget.enabled = true;
        // }

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            outline.enabled = false;
        }

        private void Update() 
        {
            button.enabled = BattleManager._choosingTarget;

            if (!BattleManager._choosingTarget) outline.enabled = false;
            //if (BattleInputManager._controls.Menu.TopButton.triggered && outline.enabled) ProfileBoxManager.ShowProfileBox(unitRef);
            //if (BattleInputManager._controls.Menu.Back.triggered && ProfileBoxManager._isOpen) ProfileBoxManager.CloseProfileBox();
        }

        public void OnSelect(BaseEventData eventData) {
            outline.enabled = true;
            onSelect?.Invoke();
        }

        public void OnDeselect(BaseEventData eventData) {
            outline.enabled = false;
            onDeselect?.Invoke();
        }
    }
}