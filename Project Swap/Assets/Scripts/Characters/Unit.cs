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
        //  EVERYTHING THAT NEEDS TO BE REMOVED ------------------------------------------------------------------

        [HideInInspector] public GameObject battlePanelRef; // Rework camera system so i can get rid of this
        //[HideInInspector] public Animator actionPointAnim; // Move to PartyMember
        [HideInInspector] public Type id; // Redundant. base class has type already. remove this

        //  EVERYTHING THAT CAN STAY IN THIS SCRIPT ----------------------------------------------------------
        
        [FormerlySerializedAs("spriteParentObject")]
        [HideInInspector] public GameObject parent;

        [HideInInspector] public UnitBase unitRef; // Get rid of this
        [HideInInspector] public UnitBase currentTarget;
        [HideInInspector] public Ability currentAbility;
        [HideInInspector] public Animator anim; // could maybe keep, but remove direct access
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline; // could get rid of and move logic to outline script
        [HideInInspector] public Button button;

        [HideInInspector] public bool targetHasCrit;
        [HideInInspector] public bool isCrit;
        [HideInInspector] public bool isAbility;
        [FormerlySerializedAs("targetHasMissed")]
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

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            outline.enabled = false;
        }

        private void Update() {
            button.enabled = BattleManager._choosingTarget;

            if (!BattleManager._choosingTarget) outline.enabled = false;
            if (BattleInputManager._controls.Menu.TopButton.triggered && outline.enabled) ProfileBoxManager.ShowProfileBox(unitRef);
            if (BattleInputManager._controls.Menu.Back.triggered && ProfileBoxManager._isOpen) ProfileBoxManager.CloseProfileBox();
        }
        
        #region functionsCalledFromAnimator

        [UsedImplicitly] public void TryToInflictStatusEffect()
        {
            if (!isAbility || !currentAbility.hasStatusEffect) return;
            
            if (!currentAbility.isMultiTarget)
            {
                if (currentTarget.Unit.attackerHasMissed || currentTarget.IsDead) return;

                foreach (var effect in from effect in currentAbility.statusEffects
                    where !(from statusEffect in currentTarget.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any()
                    
                    let randomValue = Random.value 
                    where !(randomValue > currentAbility.chanceOfInfliction) select effect)
                {
                    effect.OnAdded(currentTarget);
                    currentTarget.Unit.statusEffects.Add(effect);
                    return;
                }
            }
            
            foreach (var target in multiHitTargets.Where(target => !target.Unit.attackerHasMissed && !target.IsDead))
            {
                foreach (var effect in from effect in currentAbility.statusEffects
                    where !(from statusEffect in target.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any() 
                    
                    let randomValue = Random.value 
                    where !(randomValue > currentAbility.chanceOfInfliction) select effect)
                {
                    effect.OnAdded(target);
                    target.Unit.statusEffects.Add(effect);
                }
            }
        }

        [UsedImplicitly] public void TargetTakeDamage()
        {
            if (!isAbility || !currentAbility.isMultiTarget) {
                currentTarget.TakeDamage(currentDamage);
                isCrit = false;
                return;
            }

            for (var i = 0; i < multiHitTargets.Count; i++)
                multiHitTargets[i].TakeDamage(damageValueList[i]);

            isCrit = false;
        }

        [UsedImplicitly] public void RecalculateDamage() 
        {
            if (isAbility && currentAbility.isMultiTarget)
            {
                damageValueList = new List<int>();
                foreach (var target in multiHitTargets) 
                    damageValueList.Add(DamageCalculator.CalculateAttackDamage(unitRef, target));
                return;
            }
            
            currentDamage = DamageCalculator.CalculateAttackDamage(unitRef, currentTarget);
            if (id != Type.PartyMember || !isCrit) return;
            TimeManager._slowTimeCrit = true;
        }

        #endregion

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