﻿using System.Collections;
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
using UnityEngine.UI;

namespace Characters
{
    public enum Status { Normal, Dead }
    public class Unit : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [HideInInspector] public TextMeshProUGUI healthText;
        [HideInInspector] public TextMeshPro nameText;

        [HideInInspector] public GameObject spriteParentObject;
        [HideInInspector] public GameObject closeUpCam;
        [HideInInspector] public GameObject closeUpCamCrit;
        [HideInInspector] public GameObject battlePanelRef;

        [HideInInspector] public Slider slider;
        [HideInInspector] public Animator actionPointAnim;
        [HideInInspector] public Animator anim;
        [HideInInspector] public AnimationHandler animationHandler;
        [HideInInspector] public SpriteOutline outline;
        [HideInInspector] public Transform characterPanelRef;
        public Transform statusBox;
        [HideInInspector] public UnitBase unitRef;
        public Unit currentTarget; // Might have to make list again. Did not think about how multi-target attacks will work;
        public Ability currentAbility;
        [HideInInspector] public Button button;
        [HideInInspector] public Image fillRect;
        
        [HideInInspector] public bool isSwapping;
        [HideInInspector] public bool isCrit;
        [HideInInspector] public bool isAbility;
        [HideInInspector] public bool battlePanelIsSet;

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

        public int CurrentHP { set { currentHP = value < 0 ? 0 : value; OnHpValueChanged(); } }

        public List<StatusEffect> statusEffects = new List<StatusEffect>();

        private Color normalHealthColor;
        private Color midHealthColor;
        private Color lowHealthColor;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            outline = GetComponent<SpriteOutline>();
            button = GetComponent<Button>();
            animationHandler = GetComponent<AnimationHandler>();
            
            normalHealthColor = Color.green;
            midHealthColor = Color.yellow;
            lowHealthColor = Color.red;
            
            outline.enabled = false;
            //nameText.renderer.enabled = false;
        }

        private void Update()
        {
            //nameText.gameObject.SetActive(BattleHandler.choosingTarget);
            button.enabled = BattleHandler.choosingTarget;

            if (!battlePanelIsSet) return;
            closeUpCam.SetActive(battlePanelRef.activeSelf && battlePanelRef.transform.GetChild(0).gameObject.activeSelf);
        }

        public void TakeDamage(int dmg, Unit unit)
        {
            CurrentHP = currentHP - dmg;

            var position = gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 3, position.z);
            
            var damage = DamagePrefabManager.Instance.ShowDamage(dmg, unit.isCrit);
            damage.transform.position = newPosition;

            if (unit.isCrit) unit.isCrit = false;
            
            if (currentHP > 0) anim.SetTrigger(AnimationHandler.HurtTrigger);
            else Die();
        }

        private void InflictStatusEffectOnTarget(StatusEffect effect)
        {
            if ((from statusEffect in currentTarget.statusEffects
                where statusEffect.name == effect.name select statusEffect).Any()) return;
            
            var randomValue = Random.value;
            if (randomValue > currentAbility.chanceOfInfliction) return;
            
            Debug.Log(currentTarget.unitName + " is inflicted with " + effect.name);
            effect.OnAdded(currentTarget);
            currentTarget.statusEffects.Add(effect);
                
            //var timer = statusEffectGO.AddComponent<StatusEffectTimer>();
            //BattleHandler.newRound.AddListener(() => timer.DecrementTimer());
        }

        public void RemoveStatusEffect(StatusEffect effect)
        {
            if (!(from statEffect in statusEffects
                where effect.name == statEffect.name select effect).Any()) return;
            
            statusEffects.Remove(effect);
            effect.OnRemoval(this);
        }

        private void OnHpValueChanged()
        {
            if (currentHP <= 0.25f * maxHealthRef) outline.color = lowHealthColor;
            else if (currentHP <= 0.5f * maxHealthRef) outline.color = midHealthColor;
            else outline.color = normalHealthColor;
            
            if (id != 1) return;
            fillRect.color = outline.color;
            slider.value = currentHP;
            healthText.text = "HP: " + currentHP;
        }

        private void Die() // If i want to make special death sequences, just make it an event or cutscene
        {
            status = Status.Dead;
            statusEffects = new List<StatusEffect>();
            BattleHandler.RemoveFromBattle(unitRef, id);
            anim.SetBool(AnimationHandler.DeathTrigger, true);
        }

        // These functions are called from the animator
        [UsedImplicitly] public void TryToInflictStatusEffect() { if (isAbility && currentAbility.hasStatusEffect) 
            InflictStatusEffectOnTarget(currentAbility.statusEffect); }
        [UsedImplicitly] public void TargetTakeDamage() => currentTarget.TakeDamage(currentDamage, this);
        [UsedImplicitly] public void RecalculateDamage() => currentDamage = DamageCalculator.CalculateAttackDamage(unitRef);

        public void OnSelect(BaseEventData eventData)
        {
            outline.enabled = true;
            if (id != 1) statusBox.GetComponent<CanvasGroup>().alpha = 1;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            outline.enabled = false;
            if (id != 1) statusBox.GetComponent<CanvasGroup>().alpha = 0;
        }
    }
}