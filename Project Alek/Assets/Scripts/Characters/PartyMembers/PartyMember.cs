using System;
using Characters.Abilities;
using UnityEngine;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase, ICanBeLeveled
    {
        [Range(0,4), VerticalGroup("Basic/Info")]
        public int positionInParty;

        [Range(0, 1), VerticalGroup("Basic/Info")]
        public float specialAttackBarVal;

        [TabGroup("Tabs","Inventory")]
        public Inventory weaponInventory;
        [TabGroup("Tabs","Inventory")]
        public Inventory armorInventory;
        [TabGroup("Tabs","Inventory")] [SerializeField]
        public WeaponItem equippedWeapon;

        [TabGroup("Tabs", "Abilities")] [InlineEditor]
        public Ability specialAttack;

        [HideInInspector] public ScriptableObject battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;
        
        public CanvasGroup Container => inventoryDisplay.GetComponent<CanvasGroup>();
        public InventoryDisplay InventoryDisplay => inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

        public override void Heal(float amount)
        {
            CurrentHP += (int) amount;
        }
        
        public bool SetAI() => true;

        private int currentExperience;
        
        [ShowInInspector, ProgressBar(0, nameof(ExperienceToNextLevel)), VerticalGroup("Basic/Info"), LabelWidth(120)]
        public int CurrentExperience
        {
            get => currentExperience;
            set
            {
                if (value > ExperienceToNextLevel)
                {
                    currentExperience = value - ExperienceToNextLevel;
                    ExperienceToNextLevel = GetNextExperienceThreshold(ExperienceToNextLevel);
                    LevelUp();
                }
                else currentExperience = value;
            }
        }
        
        [ShowInInspector, VerticalGroup("Basic/Info")]
        public int ExperienceToNextLevel { get; set; }
        
        [VerticalGroup("Basic/Info")]
        [Button("Give EXP",ButtonSizes.Medium, ButtonStyle.Box)]
        public void AdvanceTowardsNextLevel(int xp)
        {
            CurrentExperience += xp;
        }
        
        public int GetNextExperienceThreshold(int prev)
        {
            return (int)(prev * 1.5f);
        }

        public Action LevelUpEvent { get; set; }

        private int Level
        {
            get => level;
            set => level = value;
        }
        
        public void LevelUp()
        {
            Debug.Log($"Yay! {characterName} leveled up!");
            Level += 1;
            LevelUpEvent?.Invoke();
        }
    }
}
