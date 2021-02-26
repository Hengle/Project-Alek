using System.Collections.Generic;
using Characters;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public abstract class BattleEngineFieldsAndProperties : ScriptableObject
    {
        public GameObject battleResults;
        public GameObject gameOverCanvas;
   
        protected SortingCalculator _sortingCalculator;

        [HideInInspector] public InventoryInputManager inventoryInputManager;
        [HideInInspector] public BattleGenerator generator;
        
        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<IGiveExperience> expGivers = new List<IGiveExperience>();
        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<Enemy> enemiesForThisBattle = new List<Enemy>();
        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<PartyMember> membersForThisBattle = new List<PartyMember>();

        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<UnitBase> membersAndEnemiesThisTurn = new List<UnitBase>();
        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<UnitBase> membersAndEnemiesNextTurn = new List<UnitBase>();

        [ReadOnly] public UnitBase activeUnit;

        [SerializeField] [ReadOnly]
        protected int roundCount;

        [ReadOnly] public bool choosingOption;
        [ReadOnly] public bool choosingTarget;
        [ReadOnly] public bool skipChooseTarget;
        [ReadOnly] public bool usingItem;
        [ReadOnly] public bool performingAction;
        [ReadOnly] public bool endThisMembersTurn;
        [ReadOnly] public bool endTurnAfterCommand;
        [ReadOnly] public bool choosingAbility;
        [ReadOnly] public bool choosingSpell;
        [ReadOnly] public bool canGiveCommand = true;
        [ReadOnly] public bool abilityMenuLast = false;
        [ReadOnly] public bool spellMenuLast;

        protected bool AllMembersDead => membersForThisBattle.Count == 0;
        protected bool AllEnemiesDead => enemiesForThisBattle.Count == 0;
        protected bool PartyOrEnemyTeamIsDead => AllMembersDead || AllEnemiesDead;

        protected bool NewRoundCondition => membersAndEnemiesThisTurn.Count == 0 || membersAndEnemiesThisTurn.
            TrueForAll(u => u.IsDead);

        protected void ResetFields()
        {
            expGivers = new List<IGiveExperience>();
            enemiesForThisBattle = new List<Enemy>();
            membersForThisBattle = new List<PartyMember>();
            membersAndEnemiesThisTurn = new List<UnitBase>();
            membersAndEnemiesNextTurn = new List<UnitBase>();

            roundCount = 0;
            choosingOption = false;
            choosingTarget = false;
            skipChooseTarget = false;
            usingItem = false;
            performingAction = false;
            endThisMembersTurn = false;
            endTurnAfterCommand = false;
            choosingAbility = false;
            choosingSpell = false;
            canGiveCommand = true;
            abilityMenuLast = false;
            spellMenuLast = false;
        }
    }
}