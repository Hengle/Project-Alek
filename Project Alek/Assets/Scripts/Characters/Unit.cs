using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattleSystem;
using Characters.Abilities;
using Characters.Animations;
using Characters.StatusEffects;
using Sirenix.OdinInspector;

namespace Characters
{
    // Use these with status effects (like inhibiting)
    public enum Status { Normal, Dead, UnableToPerformAction }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler, IGameEventListener<CharacterEvents>
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

        #region ReadOnly
        
        [ReadOnly] public Status status = Status.Normal;
        [ReadOnly] public UnitStates currentState;
        [ReadOnly] public bool newState;
        [ReadOnly] public int level;
        [ReadOnly] public int currentHP;
        
        [SerializeField] [ReadOnly] 
        private protected int currentStrength;
        [SerializeField] [ReadOnly]
        private protected int currentMagic;
        [SerializeField] [ReadOnly]
        private protected int currentAccuracy;
        [SerializeField] [ReadOnly]
        private protected int currentInitiative;
        [SerializeField] [ReadOnly]
        private protected int currentCrit;
        [SerializeField] [ReadOnly]
        private protected int currentDefense;
        [SerializeField] [ReadOnly]
        private protected int currentResistance;
        
        [ReadOnly] public List<StatusEffect> statusEffects = new List<StatusEffect>();
        [ReadOnly] public List<UnitBase> multiHitTargets = new List<UnitBase>();
        [ReadOnly] public List<int> damageValueList = new List<int>();

        #endregion

        #region OtherFieldsAndProperies
        
        public Action onSelect;
        public Action onDeselect;

        private UnitBase parent;
        
        #endregion

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            
            outline.enabled = false;
            status = Status.Normal;
            GameEventsManager.AddListener(this);
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
            parent = unitBase;
            name = unitBase.characterName;
            level = unitBase.level;
            status = Status.Normal;
            
            // Would need to update UI slider if i want to be able to modify max health
            maxHealthRef = (int) parent.health.Value;
            currentHP = (int) parent.health.Value;
            
            SetStats();
            
            currentAP = unitBase.maxAP;
            outline.color = unitBase.Color;
            
            var chooseTarget = gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = unitBase;
            chooseTarget.enabled = true;
        }

        private void SetStats()
        {
            currentStrength = (int) parent.strength.Value;
            currentMagic = (int) parent.magic.Value;
            currentAccuracy = (int) parent.accuracy.Value;
            currentInitiative = (int) parent.initiative.Value;
            currentCrit = (int) parent.criticalChance.Value;
            currentDefense = (int) parent.defense.Value;
            currentResistance = (int) parent.resistance.Value;
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.StatChange && eventType._character == parent)
            {
                SetStats();
            }
        }
    }
}