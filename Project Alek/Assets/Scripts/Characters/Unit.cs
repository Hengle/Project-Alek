using System;
using System.Collections.Generic;
using BattleSystem;
using BattleSystem.Generator;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Characters.Abilities;
using Characters.Animations;
using Characters.StatusEffects;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Characters
{
    // TODO: Use these with status effects (like inhibiting)
    public enum Status { Normal, Dead, UnableToPerformAction, Overexerted }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler, IGameEventListener<CharacterEvents>, IGameEventListener<BattleEvents>
    {
        #region HideInInspector

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
        
        [ReadOnly] public UnitBase currentTarget;
        [ReadOnly] public Status status = Status.Normal;
        [ReadOnly] public UnitStates currentState;
        [ReadOnly] public int level;
        [ReadOnly] public int currentHP;
        [FormerlySerializedAs("conversionAmount")]
        [ReadOnly] public int conversionLevel;
        [ReadOnly] public float conversionFactor = 1;

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

        [ReadOnly] public int finalInitVal;
        [ReadOnly] public float initModifier;
        
        [ReadOnly] public List<StatusEffect> statusEffects = new List<StatusEffect>();
        [ReadOnly] public List<UnitBase> multiHitTargets = new List<UnitBase>();
        [ReadOnly] public List<int> damageValueList = new List<int>();
        
        [ReadOnly] public bool targetHasCrit;
        [ReadOnly] public bool isCrit;
        [ReadOnly] public bool isAbility;
        [ReadOnly] public bool attackerHasMissed;
        [ReadOnly] public bool parry;
        [ReadOnly] public bool timedAttack;
        [ReadOnly] public bool isCountered;
        [ReadOnly] public bool hasPerformedTurn;

        #endregion

        #region OtherFieldsAndProperies
        
        public Action<bool> onTimedAttack;
        public Action<bool> onTimedDefense;
        public Action<int, bool> onDmgValueChanged;
        public Action<int, bool> onDefValueChanged;
        public Action<int> borrowAP;
        public Action<UnitBase> recoveredFromOverexertion;
        public Action<UnitBase> recoveredFromMaxOverexertion;
        
        public Action onSelect;
        public Action onDeselect;

        [SerializeField] private Material material;
        private SpriteRenderer spriteRenderer;
        private BattleGeneratorDatabase database;

        public float ShieldFactor =>
            currentState == UnitStates.Weakened || currentState == UnitStates.Checkmate ? 1.0f : 0.80f;

        public bool HasMissedAllTargets {
            get { if (currentTarget != null) return currentTarget.Unit.attackerHasMissed;

                return multiHitTargets.Count > 0 && multiHitTargets.TrueForAll
                           (t => t.Unit.attackerHasMissed);
            }
        }

        public UnitBase parent;
        
        #endregion

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            database = FindObjectOfType<BattleGeneratorDatabase>();
            material = GetComponent<SpriteRenderer>().material;
            spriteRenderer = GetComponent<SpriteRenderer>();

            //spriteRenderer.material = database.shadowMaterial;
            
            outline.enabled = false;
            status = Status.Normal;
            GameEventsManager.AddListener<CharacterEvents>(this);
            GameEventsManager.AddListener<BattleEvents>(this);
        }

        private void OnDisable()
        {
            GameEventsManager.RemoveListener<CharacterEvents>(this);
            GameEventsManager.RemoveListener<BattleEvents>(this);
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

        public static bool CanBorrow(int amount) => amount <= BattleManager.Instance.globalVariables.maxLoanAmount;
        
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

        private void ResetTargets()
        {
            currentTarget = null;
            multiHitTargets.Clear();
            damageValueList.Clear();
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            switch (eventType._eventType)
            {
                case CEventType.CharacterTurn:
                    outline.enabled = false;
                    button.enabled = false;
                    if (eventType._character == parent) ResetTargets();
                    break;
                
                case CEventType.ChoosingTarget: button.enabled = true;
                    break;
                
                case CEventType.CharacterAttacking: outline.enabled = false;
                    break;
                
                case CEventType.StatChange when eventType._character == parent: SetStats();
                    break;
                
                case CEventType.EndOfTurn when eventType._character == parent: hasPerformedTurn = true;
                    break;
                
                case CEventType.SkipTurn when eventType._character == parent: hasPerformedTurn = true;
                    break;
            }
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            if (eventType._battleEventType == BattleEventType.NewRound)
            {
                hasPerformedTurn = false;
            }
        }
    }
}