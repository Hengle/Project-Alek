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
using Characters.StatusEffects;
using Sirenix.OdinInspector;

namespace Characters
{
    // TODO: Use these with status effects (like inhibiting)
    public enum Status { Normal, Dead, UnableToPerformAction, Overexerted }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler, IGameEventListener<CharacterEvents>
    {
        #region HideInInspector

        [HideInInspector] public Ability currentAbility;
        [HideInInspector] public Animator anim;
        public AnimatorOverrideController animOverride;
        
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Button button;

        [HideInInspector] public int commandActionOption;
        [HideInInspector] public int maxHealthRef;
        [HideInInspector] public int currentAP;
        [HideInInspector] public int actionCost;
        [HideInInspector] public int specialAttackAP;
        [HideInInspector] public int currentDamage;
        [HideInInspector] public string commandActionName;

        #endregion

        #region ReadOnly
        
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
        
        [ReadOnly] public bool targetHasCrit;
        [ReadOnly] public bool isAbility;
        [ReadOnly] public bool attackerHasMissed;
        [ReadOnly] public bool parry;
        [ReadOnly] public bool timedAttack;
        [ReadOnly] public bool isCountered;

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
            GameEventsManager.AddListener(this);
        }

        private void OnDisable()
        {
            GameEventsManager.RemoveListener(this);
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

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public void DestroyGO()
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            switch (eventType._eventType)
            {
                case CEventType.CharacterTurn:
                    outline.enabled = false;
                    button.enabled = false;
                    isAbility = false;
                    currentAbility = null;
                    if (eventType._character == parent) ResetTargets();
                    break;
                
                case CEventType.ChoosingTarget: button.enabled = true;
                    break;
                
                case CEventType.CharacterAttacking: outline.enabled = false;
                    break;
            }
        }
    }
}