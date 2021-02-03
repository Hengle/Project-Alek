using System;
using System.Collections.Generic;
using BattleSystem;
using Characters.Abilities;
using MEC;
using UnityEngine;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using UnityEditor.Animations;

namespace Characters.PartyMembers
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

        [TabGroup("Tabs", "Abilities")] [InlineEditor]
        public Ability specialAttack;

        [HideInInspector] public ScriptableObject battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;
        
        public CanvasGroup Container => inventoryDisplay.GetComponent<CanvasGroup>();
        public InventoryDisplay InventoryDisplay => inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

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
                    currentExperience += leftover;
                    yield break;
                }
                
                CurrentExperience++;
                e++;
                yield return Timing.WaitForSeconds(0.015f);
            }
        }

        [ShowInInspector, VerticalGroup("Basic/Info")]
        public int ExperienceToNextLevel => GetNextExperienceThreshold();

        [VerticalGroup("Basic/Info")]
        [Button("Give EXP", ButtonSizes.Medium, ButtonStyle.Box)]
        public void AdvanceTowardsNextLevel(int xp) => Timing.RunCoroutine(ExperienceCoroutine(xp));
        
        public int GetNextExperienceThreshold() => (int) (BaseExperience * Math.Pow(1.1f, level - 1));
        
        public Action<int> LevelUpEvent { get; set; }

        public void LevelUp()
        {
            level += 1;
            currentClass.IncreaseStats();
            LevelUpEvent?.Invoke(level);
        }
    }
}
