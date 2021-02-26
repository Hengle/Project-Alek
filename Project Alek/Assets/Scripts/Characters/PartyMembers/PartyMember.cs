using System;
using System.Collections.Generic;
using BattleSystem;
using MEC;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase, ICanBeLeveled
    {
        private const int MaxLevel = 99;
        private const int BaseExperience = 500;

        [VerticalGroup("Basic/Info")]
        public Class currentClass;

        [VerticalGroup("Basic/Info")]
        public AnimatorOverrideController overworldController;

        [Range(1,4), VerticalGroup("Basic/Info")]
        public int positionInParty;

        [Range(0, 1), VerticalGroup("Basic/Info")]
        public float specialAttackBarVal;

        [TabGroup("Tabs","Inventory")]
        public Inventory weaponInventory;
        [TabGroup("Tabs","Inventory")]
        public Inventory armorInventory;
        [TabGroup("Tabs","Inventory")] [SerializeField]
        public WeaponItem equippedWeapon;

        [TabGroup("Tabs", "Spells & Abilities")] [InlineEditor]
        public Ability specialAttack;

        [HideInInspector] public ScriptableObject battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;
        
        public CanvasGroup Container => inventoryDisplay.GetComponent<CanvasGroup>();
        public InventoryDisplay InventoryDisplay => inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

        #region BaseStats
        
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseLevel;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseHealth;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseStrength;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseMagic;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseAccuracy;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseInitiative;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseDefense;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseResistance;
        [TabGroup("Tabs", "Base Stats")]
        [SerializeField] private float baseCriticalChance;

        [VerticalGroup("Basic/Info")] 
        [Button("Reset to Base")]
        private void ResetToBase()
        {
            level = (int) baseLevel;
            currentExperience = 0;
            health.BaseValue = baseHealth;
            strength.BaseValue = baseStrength;
            magic.BaseValue = baseMagic;
            accuracy.BaseValue = baseAccuracy;
            initiative.BaseValue = baseInitiative;
            defense.BaseValue = baseDefense;
            resistance.BaseValue = baseResistance;
            criticalChance.BaseValue = baseCriticalChance;
        }
        
        #endregion

        public override void Heal(float amount) => CurrentHP += (int) amount;

        private int currentExperience;
        
        [ShowInInspector, ProgressBar(0, nameof(ExperienceToNextLevel)), VerticalGroup("Basic/Info"), LabelWidth(120)]
        public int CurrentExperience
        {
            get => currentExperience;
            set
            {
                if (level == MaxLevel) return;
                if (value >= ExperienceToNextLevel)
                {
                    currentExperience = value - ExperienceToNextLevel;
                    LevelUp();
                    
                    while (currentExperience >= ExperienceToNextLevel)
                    {
                        currentExperience -= ExperienceToNextLevel;
                        LevelUp();
                        if (level == MaxLevel) break;
                    }
                }
                else currentExperience = value;
            }
        }
        
        public int BattleExpReceived { get; set; }

        private IEnumerator<float> ExperienceCoroutine(int xp)
        {
            var e = 0;
            while (e != xp)
            {
                if (BattleInput._controls.Battle.Confirm.triggered)
                {
                    var leftover = xp - e;
                    CurrentExperience += leftover;
                    yield break;
                }
                
                CurrentExperience++;
                e++;
                yield return Timing.WaitForSeconds(0.01f);
            }
        }

        [ShowInInspector, VerticalGroup("Basic/Info")]
        public int ExperienceToNextLevel => GetNextExperienceThreshold();

        [VerticalGroup("Basic/Info")]
        [Button("Give EXP", ButtonSizes.Medium, ButtonStyle.Box)]
        public void AdvanceTowardsNextLevel(int xp) => Timing.RunCoroutine(ExperienceCoroutine(xp));
        
        public int GetNextExperienceThreshold() => (int) (BaseExperience * Math.Pow(1.1f, level - 1));
        
        public Action<object> LevelUpEvent { get; set; }

        public void LevelUp()
        {
            level += 1;
            currentClass.IncreaseStats();
            LevelUpEvent?.Invoke(this);
        }

        public void ResetLevelUpAmount()
        {
            health.amountIncreasedBy = 0;
            strength.amountIncreasedBy = 0;
            magic.amountIncreasedBy = 0;
            accuracy.amountIncreasedBy = 0;
            initiative.amountIncreasedBy = 0;
            defense.amountIncreasedBy = 0;
            resistance.amountIncreasedBy = 0;
            criticalChance.amountIncreasedBy = 0;
        }
    }
}
