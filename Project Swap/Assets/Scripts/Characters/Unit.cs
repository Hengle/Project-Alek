using System.Collections.Generic;
using System.Linq;
using Abilities;
using Animations;
using BattleSystem;
using BattleSystem.DamagePrefab;
using Calculator;
using JetBrains.Annotations;
using StatusEffects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Characters
{
    public enum Status { Normal, Dead }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        //  EVERYTHING THAT NEEDS TO BE REMOVED ------------------------------------------------------------------

        [HideInInspector] public GameObject battlePanelRef; // Rework camera system so i can get rid of this
        [HideInInspector] public Animator actionPointAnim; // Move to PartyMember
        [HideInInspector] public Image fillRect; // Remove when onhpchanged is moved
        [HideInInspector] public bool battlePanelIsSet; // can rid of this after battlePanelRef
        [HideInInspector] public Type id; // Redundant. base class has type already. remove this

        //  EVERYTHING THAT CAN STAY IN THIS SCRIPT ----------------------------------------------------------

        [HideInInspector] public TextMeshProUGUI healthText;
        [HideInInspector] public Slider slider;

        [FormerlySerializedAs("spriteParentObject")]
        [HideInInspector] public GameObject parent;
        [HideInInspector] public GameObject closeUpCam;
        [HideInInspector] public GameObject closeUpCamCrit;

        [HideInInspector] public UnitBase unitRef; // Get rid of this
        [HideInInspector] public UnitBase currentTarget; // change to unitbase
        [HideInInspector] public Ability currentAbility;
        [HideInInspector] public Animator anim; // could maybe keep, but remove direct access
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Transform statusBox;
        [HideInInspector] public Button button;

        [HideInInspector] public bool targetHasCrit;
        [HideInInspector] public bool isCrit;
        [HideInInspector] public bool isAbility;
        [HideInInspector] public bool targetHasMissed;

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

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            outline.enabled = false;
        }

        private void Update() {
            button.enabled = BattleManager.choosingTarget;

            if (!BattleManager.choosingTarget) outline.enabled = false;
            if (BattleManager.controls.Menu.TopButton.triggered && outline.enabled) ProfileBoxManager.ShowProfileBox(unitRef);
            if (BattleManager.controls.Menu.Back.triggered && ProfileBoxManager.isOpen) ProfileBoxManager.CloseProfileBox();

            if (!battlePanelIsSet) return;
            closeUpCam.SetActive(battlePanelRef.activeSelf && battlePanelRef.transform.GetChild(1).gameObject.activeSelf);
        }
        

        private void InflictStatusEffectOnTarget(StatusEffect effect)
        {
            if (!isAbility) return;
            
            if (!currentAbility.isMultiTarget)
            {
                if (currentTarget.Unit.targetHasMissed) return;
                
                if ((from statusEffect in currentTarget.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any()) return;

                var randomValue = Random.value;
                if (randomValue > currentAbility.chanceOfInfliction) return;

                effect.OnAdded(currentTarget);
                currentTarget.Unit.statusEffects.Add(effect);
                return;
            }

            foreach (var target in multiHitTargets.Where(target => !target.Unit.targetHasMissed))
            {
                if ((from statusEffect in target.Unit.statusEffects
                    where statusEffect.name == effect.name select statusEffect).Any()) return;

                var randomValue = Random.value;
                if (randomValue > currentAbility.chanceOfInfliction) return;

                effect.OnAdded(target);
                target.Unit.statusEffects.Add(effect);
            }
        }
        
        // These functions are called from the animator. I could just fire off an event from the animation that the UnitBase listens to
        // And remove these functions from this class. Each unit could have its own event (So it doesn't trigger other units).
        // That way i can have different logic for these functions (if needed). A boss could have special logic for these functions
        [UsedImplicitly] public void TryToInflictStatusEffect()
        {
            if (!isAbility || !currentAbility.hasStatusEffect) return;
            
            foreach (var statusEffect in currentAbility.statusEffects)
                InflictStatusEffectOnTarget(statusEffect);
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
            closeUpCamCrit.SetActive(true);
            TimeManager.slowTimeCrit = true;
        }

        public void OnSelect(BaseEventData eventData) {
            outline.enabled = true;
            if (id != Type.PartyMember && status != Status.Dead) statusBox.GetComponent<CanvasGroup>().alpha = 1;
        }

        public void OnDeselect(BaseEventData eventData) {
            outline.enabled = false;
            if (id != Type.PartyMember) statusBox.GetComponent<CanvasGroup>().alpha = 0;
        }
    }
}