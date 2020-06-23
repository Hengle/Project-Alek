using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abilities;
using Animations;
using BattleSystem;
using Calculator;
using JetBrains.Annotations;
using StatusEffects;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Characters
{
    public enum Status { Normal, Dead }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [HideInInspector] public bool isSwapping;
        [HideInInspector] public bool isCrit;
        [HideInInspector] public bool isAbility;
        [HideInInspector] public bool battlePanelIsSet;

        [HideInInspector] public TextMeshProUGUI healthText;
        [HideInInspector] public TextMeshPro nameText;
        [HideInInspector] public TextMeshPro damageText;
        [HideInInspector] public TextMeshPro damageText2;
        [HideInInspector] public TextMeshPro critDamageText;
        [HideInInspector] public TextMeshPro critDamageText2;

        [HideInInspector] public GameObject spriteParentObject;
        [HideInInspector] public GameObject damagePrefab;
        [HideInInspector] public GameObject damagePrefab2;
        [HideInInspector] public GameObject critDamagePrefab;
        [HideInInspector] public GameObject critDamagePrefab2;
        [HideInInspector] public GameObject closeUpCam;
        [HideInInspector] public GameObject closeUpCamCrit;
        [HideInInspector] public GameObject battlePanelRef;
        [HideInInspector] public GameObject statusEffectGO;

        [HideInInspector] public Slider slider;
        [HideInInspector] public Animator actionPointAnim;
        [HideInInspector] public Animator anim;
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Transform characterPanelRef;
        [HideInInspector] public ShowDamageSO showDamageSO;
        [HideInInspector] public UnitBase unitRef;
        [HideInInspector] public Unit currentTarget; // Might have to make list again. Did not think about how multi-target attacks will work;
        [HideInInspector] public Ability currentAbility;
        [HideInInspector] public Button button;
        [HideInInspector] public Image fillRect;
        [HideInInspector] public Color currentColor;

        [HideInInspector] public int id;
        [HideInInspector] public int maxHealthRef;
        [HideInInspector] public int commandActionOption;
        [HideInInspector] public int currentAP;
        [HideInInspector] public int weaponMT = 20; // temporary, just for testing
        [HideInInspector] public int actionCost;
        [HideInInspector] public int currentDamage;

        [HideInInspector] public string commandActionName;
        public string unitName;

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
        public int CurrentHP { set { currentHP = value; OnHpValueChanged(); } }

        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        
        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (!(from statEffect in statusEffects where effect.name == statEffect.name select effect).Any()) return;
            
            statusEffects.Remove(effect);
            effect.OnRemoval(this);
        }

        // These functions are called from the animator
        [UsedImplicitly] public void TryToInflictStatusEffect() { if (currentAbility.hasStatusEffect) InflictStatusEffect(currentAbility.statusEffect); }
        [UsedImplicitly] public void TargetTakeDamage() => currentTarget.TakeDamage(currentDamage, this,false);
        [UsedImplicitly] public void RecalculateDamage() => currentDamage = DamageCalculator.CalculateAttackDamage(unitRef);

        public void SetColor(Color color) => damageText.color = color;

        public void TakeDamage(int dmg, Unit unit, bool colorHasChanged)
        {
            damagePrefab.SetActive(false);
            damagePrefab2.SetActive(false);
            critDamagePrefab.SetActive(false);
            currentHP -= dmg;

            if (currentHP < 0) currentHP = 0;
            else anim.SetTrigger(AnimationHandler.HurtTrigger);

            if (unit.isCrit && unit != this)
            {
                unit.isCrit = false;
                if (!critDamagePrefab.activeSelf) { critDamageText.text = dmg.ToString(); critDamagePrefab.SetActive(true); }
                else { critDamageText2.text = dmg.ToString(); critDamagePrefab2.SetActive(true); }
            }

            else
            {
                if (!damagePrefab.activeSelf) { damageText.text = dmg.ToString(); damagePrefab.SetActive(true);}
                else { damageText2.text = dmg.ToString(); damagePrefab2.SetActive(true); }
            }

            SetColor();
            CurrentHP = currentHP;

            if (colorHasChanged) StartCoroutine(ResetDamageColor());
            
            if (currentHP > 0) return;
            StartCoroutine(Die());
        }

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            
            currentColor = Color.green;
            outline.enabled = false;
            nameText.renderer.enabled = false;
        }

        private void Update()
        {
            // Shows the name text above the character's heads when choosing a target
            //nameText.gameObject.SetActive(BattleHandler.choosingTarget);
            button.enabled = BattleHandler.choosingTarget;
            
            if (!battlePanelIsSet) return;
            // Activates the camera during that member's turn
            closeUpCam.SetActive(battlePanelRef.activeSelf && battlePanelRef.transform.GetChild(0).gameObject.activeSelf);
        }

        private void InflictStatusEffect(StatusEffect effect)
        {
            if ((from statusEffect in currentTarget.statusEffects where statusEffect.name == effect.name select statusEffect).Any()) return;
            
            var randomValue = Random.value;
            if (randomValue > currentAbility.chanceOfInfliction) return;
            
            effect.OnAdded(currentTarget);
            currentTarget.statusEffects.Add(effect);
                
            var timer = new Timer(effect, currentTarget);
            BattleHandler.newRound.AddListener(() => timer.DecrementTimer());
        }

        private void SetColor()
        {
            if (currentHP <= 0.25f * maxHealthRef) currentColor = Color.red;
            else if (currentHP <= 0.5f * maxHealthRef) currentColor = Color.yellow;
        }

        private void OnHpValueChanged()
        {
            if (id != 1) return;
            slider.value = currentHP;
            healthText.text = "HP: " + currentHP;
            fillRect.color = currentColor;
        }

        private IEnumerator ResetDamageColor()
        {
            yield return new WaitForSeconds(1);
            damageText.color = Color.white;
        }
        
        private IEnumerator Die()
        {
            statusEffects = new List<StatusEffect>();
            BattleHandler.RemoveFromBattle(unitRef, id);
            anim.SetBool(AnimationHandler.DeathTrigger, true);
            status = Status.Dead;

            yield return new WaitForSeconds(2);
        }

        public void OnSelect(BaseEventData eventData) => outline.enabled = true;
        public void OnDeselect(BaseEventData eventData) => outline.enabled = false;
    }
}