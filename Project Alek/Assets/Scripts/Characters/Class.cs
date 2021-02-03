using System;
using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using Characters.Abilities;
using Characters.PartyMembers;
using JetBrains.Annotations;
using MEC;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public enum ClassTier { Novice = 500, Intermediate = 1000, Expert = 2000, Master = 5000 }
    [CreateAssetMenu(fileName = "New Class", menuName = "Class")]
    public class Class : SerializedScriptableObject, ICanBeLeveled
    {
        [EnumToggleButtons] [HideLabel] public ClassTier classTier;
        [Tooltip("Who can access this class?")] [HorizontalGroup("Member Info"), LabelWidth(150)]
        [InlineEditor] public PartyMember partyMember;
        [HorizontalGroup("Member Info")] [LabelWidth(30)] [HideLabel]
        public Animator animator;

        private const int MaxLevel = 10;

        #region GrowthRates
        
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float strengthGrowthRate;
        private bool CanIncreaseStrength => Random.value <= strengthGrowthRate;
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float magicGrowthRate;
        private bool CanIncreaseMagic => Random.value <= magicGrowthRate;
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float accuracyGrowthRate;
        private bool CanIncreaseAccuracy => Random.value <= accuracyGrowthRate;
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float initiativeGrowthRate;
        private bool CanIncreaseInitiative => Random.value <= initiativeGrowthRate;
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float defenseGrowthRate;
        private bool CanIncreaseDefense => Random.value <= defenseGrowthRate;
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float resistanceGrowthRate;
        private bool CanIncreaseResistance => Random.value <= resistanceGrowthRate;
        [Range(0,1)] [TabGroup("Tabs","Growth Rates")]
        public float criticalGrowthRate;
        private bool CanIncreaseCritical => Random.value <= criticalGrowthRate;
        
        #endregion
        
        #region StatBonuses

        [Range(0, 1000)] [TabGroup("Tabs","Stat Bonuses")]
        public int healthBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int strengthBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int magicBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int accuracyBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int initiativeBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int defenseBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int resistanceBonus;
        [Range(0,5)] [TabGroup("Tabs","Stat Bonuses")]
        public int criticalBonus;

        #endregion

        public void PromotionBonus()
        {
            partyMember.health.BaseValue += healthBonus;
            partyMember.strength.BaseValue += strengthBonus;
            partyMember.magic.BaseValue += magicBonus;
            partyMember.accuracy.BaseValue += accuracyBonus;
            partyMember.initiative.BaseValue += initiativeBonus;
            partyMember.defense.BaseValue += defenseBonus;
            partyMember.resistance.BaseValue += resistanceBonus;
            partyMember.criticalChance.BaseValue += criticalBonus;
        }
        
        [TabGroup("Tabs","Spells and Abilities")]
        [ValidateInput(nameof(ValidateClassAbilities), "Class must be equal to this class!")]
        public readonly List<ClassAbility> _classAbilities = new List<ClassAbility>();

        private bool ValidateClassAbilities(List<ClassAbility> list) =>
            list.All(ability => ability._class == this);
        
        [Range(1,MaxLevel)] [Space(20)] [LabelWidth(50)]
        public int level;

        private int currentExperience;
        [ShowInInspector, ProgressBar(0, nameof(ExperienceToNextLevel)), LabelWidth(120)]
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

        [ShowInInspector] [HorizontalGroup("Experience")]
        public int BaseExperience => (int) classTier;
        
        [ShowInInspector] [HorizontalGroup("Experience")]
        public int ExperienceToNextLevel => GetNextExperienceThreshold();

        [Button("Give EXP",ButtonSizes.Medium, ButtonStyle.Box)]
        public void AdvanceTowardsNextLevel(int xp) => Timing.RunCoroutine(ExperienceCoroutine(xp));

        public int GetNextExperienceThreshold() => (int) (BaseExperience * Math.Pow(1.1f, level - 1));

        public Action<int> LevelUpEvent { get; set; }

        public void LevelUp()
        {
            level += 1;
            LevelUpEvent?.Invoke(level);
        }

        public void IncreaseStats()
        {
            while (true)
            {
                var count = 0;
                var results = new List<string>();

                if (CanIncreaseStrength)
                {
                    partyMember.strength.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s strength has been increased!\n");
                }

                if (CanIncreaseMagic)
                {
                    partyMember.magic.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s magic has been increased!\n");
                }

                if (CanIncreaseAccuracy)
                {
                    partyMember.accuracy.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s accuracy has been increased!\n");
                }

                if (CanIncreaseInitiative)
                {
                    partyMember.initiative.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s initiative has been increased!\n");
                }

                if (CanIncreaseDefense)
                {
                    partyMember.defense.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s defense has been increased!\n");
                }

                if (CanIncreaseResistance)
                {
                    partyMember.resistance.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s resistance has been increased!\n");
                }

                if (CanIncreaseCritical)
                {
                    partyMember.criticalChance.BaseValue += 1;
                    count++;
                    results.Add($"{partyMember.characterName}'s crit chance has been increased!\n");
                }

                if (count == 0) continue;
                results.ForEach(Debug.Log);
                break;
            }
        }
    }
    
    public struct ClassAbility
    {
        [InlineEditor] public Ability _ability;
        [UsedImplicitly] public Class _class;
        [Range(1,10)] [UsedImplicitly] public int _levelRequirement;
        [ShowInInspector] public bool IsUnlocked =>
            _class != null && _class.level >= _levelRequirement;
    }
}