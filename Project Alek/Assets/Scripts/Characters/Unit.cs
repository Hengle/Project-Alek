using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BattleSystem;
using BattleSystem.Mechanics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Characters.Abilities;
using Characters.Animations;
using Characters.Enemies;
using Characters.StatusEffects;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using Sirenix.OdinInspector;

namespace Characters
{
    // TODO: Use these with status effects (like inhibiting)
    public enum Status { Normal, Dead, UnableToPerformAction, Overexerted }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler, IGameEventListener<BattleEvent>, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        #region HideInInspector

        [HideInInspector] public Animator anim;
        public AnimatorOverrideController animOverride;

        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Button button;

        [HideInInspector] public int commandActionOption;
        [HideInInspector] public int maxHealthRef;
        public int currentAP;
        [HideInInspector] public int actionCost;
        [HideInInspector] public int specialAttackAP;
        [HideInInspector] public int currentDamage;
        [HideInInspector] public string commandActionName;

        #endregion

        #region ReadOnly

        [ReadOnly] public Ability currentAbility;
        [ReadOnly] public UnitBase currentTarget;
        [ReadOnly] public Status status = Status.Normal;
        [ReadOnly] public UnitStates currentState;
        
        [ReadOnly] public int currentHP;
        [ReadOnly] public int conversionLevel;
        [ReadOnly] public float conversionFactor = 1;

        [ReadOnly] public int finalInitVal;
        [ReadOnly] public float initModifier;
        
        [ReadOnly] public List<StatusEffect> statusEffects = new List<StatusEffect>();
        [ReadOnly] public List<UnitBase> multiHitTargets = new List<UnitBase>();
        [ReadOnly] public List<int> damageValueList = new List<int>();

        [ReadOnly] public bool hasPerformedTurn;
        [ReadOnly] public bool targetHasCrit;
        [ReadOnly] public bool isAbility;
        [ReadOnly] public bool attackerHasMissed;
        [ReadOnly] public bool parry;
        [ReadOnly] public bool timedAttack;
        [ReadOnly] public bool isCountered;
        [ReadOnly] public bool isBorrowing;
        [ReadOnly] public bool overrideElemental;
        [ReadOnly] public bool hasSummon;

        public Enemy currentSummon;
        public Ability overrideAbility;

        #endregion

        #region OtherFieldsAndProperies

        public Action onSpecialAttack;
        public Action<float> onSpecialBarValChanged;
        public Action<bool> onTimedAttack;
        public Action<bool> onTimedDefense;
        public Action<int, bool> onDmgValueChanged;
        public Action<int, bool> onDefValueChanged;
        public Action<int> borrowAP;
        public Action<UnitBase> recoveredFromOverexertion;
        public Action<UnitBase> recoveredFromMaxOverexertion;
        public Action onSelect;
        public Action onDeselect;

        public float ShieldFactor => currentState == UnitStates.Weakened ||
                                     currentState == UnitStates.Checkmate ? 1.0f : 0.80f;

        public bool HasMissedAllTargets {
            get { if (currentTarget) return currentTarget.Unit.attackerHasMissed;

                return multiHitTargets.Count > 0 && multiHitTargets.TrueForAll
                           (t => t.Unit.attackerHasMissed);
            }
        }

        public UnitBase parent;
        
        #endregion

        private void Awake()
        {
            anim = GetComponent<Animator>();
            animOverride = (AnimatorOverrideController) anim.runtimeAnimatorController;
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();

            anim.SetInteger("AnimState", 1);
            outline.enabled = false;
            status = Status.Normal;
            
            BattleEvents.Instance.characterTurnEvent.AddListener(this);
            BattleEvents.Instance.characterAttackEvent.AddListener(this);
            BattleEvents.Instance.chooseTargetEvent.AddListener(this);
            BattleEvents.Instance.battleEvent.AddListener(this);
            BattleEvents.Instance.endOfTurnEvent.AddListener(this);
            BattleEvents.Instance.skipTurnEvent.AddListener(this);
        }

        private void OnDisable()
        {
            BattleEvents.Instance.characterTurnEvent.RemoveListener(this);
            BattleEvents.Instance.characterAttackEvent.RemoveListener(this);
            BattleEvents.Instance.chooseTargetEvent.RemoveListener(this);
            BattleEvents.Instance.battleEvent.RemoveListener(this);
            BattleEvents.Instance.endOfTurnEvent.RemoveListener(this);
            BattleEvents.Instance.skipTurnEvent.RemoveListener(this);
        }

        private void OnDestroy()
        {
            EmptyAnimations();
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

        public static bool CanBorrow(int amount) => amount <= GlobalVariables.Instance.maxLoanAmount;

        private void ResetTargets()
        {
            currentTarget = null;
            multiHitTargets.Clear();
            damageValueList.Clear();
        }

        public void EmptyAnimations()
        {
            if (!parent || parent.id != CharacterType.PartyMember) return;
            for (var i = 0; i < parent.abilities.Count; i++)
            {
                animOverride[$"Ability {i + 1}"] = null;
            }
            
            anim.runtimeAnimatorController = animOverride;
        }

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public void DestroyGO()
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
        

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == BattleEvents.Instance.characterTurnEvent)
            {
                outline.enabled = false;
                button.enabled = false;
                isAbility = false;
                currentAbility = null;
                if (value1 == parent) ResetTargets();
            }
            else if (value2 == BattleEvents.Instance.chooseTargetEvent) button.enabled = true;
            else if (value2 == BattleEvents.Instance.characterAttackEvent) outline.enabled = false;
            else if (value1 == parent && value2 == BattleEvents.Instance.endOfTurnEvent ||
                     value2 == BattleEvents.Instance.skipTurnEvent)
            {
                hasPerformedTurn = true;
            }
        }
        
        public void OnEventRaised(BattleEvent value)
        {
            if (value == BattleEvent.NewRound)
            {
                hasPerformedTurn = false;
            }
        }
    }
}