using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Characters.Abilities;
using Characters.Animations;
using Characters.StatusEffects;
using Sirenix.OdinInspector;

namespace Characters
{
    // TODO: Use these with status effects (like inhibiting)
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
        
        [ReadOnly] public bool targetHasCrit;
        [ReadOnly] public bool isCrit;
        [ReadOnly] public bool isAbility;
        [ReadOnly] public bool attackerHasMissed;
        [ReadOnly] public bool parry;
        [ReadOnly] public bool timedAttack;

        #endregion

        #region OtherFieldsAndProperies
        
        [HideInInspector] public Action<bool> onTimedAttack;
        [HideInInspector] public Action<bool> onTimedDefense;
        [HideInInspector] public Action<int, bool> onDmgValueChanged;
        [HideInInspector] public Action<int, bool> onDefValueChanged;
        
        public Action onSelect;
        public Action onDeselect;

        public UnitBase parent;
        
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
            
            // TODO: Would need to update UI slider if i want to be able to modify max health
            maxHealthRef = (int) parent.health.Value;
            currentHP = (int) parent.health.Value;
            
            SetStats();
            
            currentAP = unitBase.maxAP;
            outline.color = unitBase.Color;
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
            switch (eventType._eventType)
            {
                case CEventType.CharacterTurn:
                    outline.enabled = false;
                    button.enabled = false;
                    break;
                
                case CEventType.ChoosingTarget: button.enabled = true;
                    break;
                
                case CEventType.CharacterAttacking: outline.enabled = false;
                    break;
                
                case CEventType.StatChange when eventType._character == parent: SetStats();
                    break;
            }
        }
    }
}