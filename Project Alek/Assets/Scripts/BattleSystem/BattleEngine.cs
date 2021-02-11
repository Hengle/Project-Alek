using System.Collections.Generic;
using System.Linq;
using Audio;
using BattleSystem.Calculators;
using UnityEngine;
using Characters;
using BattleSystem.Generator;
using BattleSystem.UI;
using Characters.Animations;
using Characters.CharacterExtensions;
using Characters.Enemies;
using Characters.PartyMembers;
using Characters.StatusEffects;
using Kryz.CharacterStats;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using MEC;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using AudioType = Audio.AudioType;

namespace BattleSystem
{
    public class BattleEngine : MonoBehaviour, IGameEventListener<BattleEvent>, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        #region FieldsAndProperties

        private AudioController audioController;
        [SerializeField] private AudioType[] themes;
        [SerializeField] private GameObject battleResults;
        
        [FoldoutGroup("Events")] [SerializeField] 
        private GameEvent setupCompleteEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private BattleGameEvent battleEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent characterTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent enemyTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent chooseTargetEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent endOfTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent skipTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent characterAttackEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent commandEvent;
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent deathEvent;

        private SortingCalculator sortingCalculator;

        private static BattleEngine instance;
        
        public static BattleEngine Instance {
            get { if (!instance) Debug.LogError("BattleManager is null");
                return instance; }
        }

        [HideInInspector] public InventoryInputManager inventoryInputManager;
        [HideInInspector] public BattleGenerator generator;
        
        [ReadOnly] [FoldoutGroup("Lists")] 
        public readonly List<IGiveExperience> _expGivers = new List<IGiveExperience>();
        [ReadOnly] [FoldoutGroup("Lists")] 
        public readonly List<Enemy> _enemiesForThisBattle = new List<Enemy>();
        [ReadOnly] [FoldoutGroup("Lists")] 
        public readonly List<PartyMember> _membersForThisBattle = new List<PartyMember>();

        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<UnitBase> membersAndEnemiesThisTurn = new List<UnitBase>();
        [ReadOnly] [FoldoutGroup("Lists")] 
        public List<UnitBase> membersAndEnemiesNextTurn = new List<UnitBase>();

        [ReadOnly] public UnitBase activeUnit;

        [SerializeField] [ReadOnly]
        private int roundCount;

        [ReadOnly] public bool choosingOption;
        [ReadOnly] public bool choosingTarget;
        [ReadOnly] public bool usingItem;
        [ReadOnly] public bool performingAction;
        [ReadOnly] public bool endThisMembersTurn;
        [ReadOnly] public bool choosingAbility;
        [ReadOnly] public bool canGiveCommand = true;
        [ReadOnly] public bool abilityMenuLast = false;

        private bool AllMembersDead => _membersForThisBattle.Count == 0;
        private bool AllEnemiesDead => _enemiesForThisBattle.Count == 0;
        private bool PartyOrEnemyTeamIsDead => AllMembersDead || AllEnemiesDead;

        #endregion

        #region SettingUpBattle
        
        private void Awake() => instance = this;
        
        private void Start()
        {
            audioController = FindObjectOfType<AudioController>();
            inventoryInputManager = FindObjectOfType<InventoryInputManager>();
            generator = GetComponent<BattleGenerator>();
            sortingCalculator = GetComponent<SortingCalculator>();
            
            canGiveCommand = true;
            roundCount = 0;

            audioController.PlayAudio(themes[0], true, 2);
            PartyManager.Instance.Order();
            Timing.RunCoroutine(SetupBattle());
        }

        private void OnEnable()
        {
            battleEvent.AddListener(this);
            endOfTurnEvent.AddListener(this);
            skipTurnEvent.AddListener(this);
        }

        private IEnumerator<float> SetupBattle()
        {
            generator.SetupBattle();

            SelectableObjectManager.SetEnemySelectables();
            SelectableObjectManager.SetPartySelectables();
      
            _membersForThisBattle.ForEach(m => m.Unit.GetComponent<ChooseTarget>().Setup());
            _enemiesForThisBattle.ForEach(e => e.Unit.GetComponent<ChooseTarget>().Setup());

            _membersForThisBattle.ForEach(m => { m.onDeath += RemoveFromBattle; m.onRevival += AddToBattle; });
            _enemiesForThisBattle.ForEach(e => { e.onDeath += RemoveFromBattle; e.onRevival += AddToBattle; });

            setupCompleteEvent.Raise();
            _membersForThisBattle.ForEach(m => 
                m.LevelUpEvent += battleResults.GetComponent<BattleResultsUI>().Enqueue);
            
            yield return Timing.WaitForSeconds(1);
            Timing.RunCoroutine(GetNextTurn());
        }

        #endregion

        #region RoundsAndCharacterTurns
        
        private IEnumerator<float> GetNextTurn()
        {
            if (PartyOrEnemyTeamIsDead) { EndOfBattle(); yield break; }

            yield return Timing.WaitForSeconds(0.25f);
            
            if (membersAndEnemiesThisTurn.Count == 0) { battleEvent.Raise(BattleEvent.NewRound);
                yield return Timing.WaitUntilTrue(sortingCalculator.SortByInitiative); }

            var character = membersAndEnemiesThisTurn[0];
            
            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.EveryTurn, 1, true));
                
            if (PartyOrEnemyTeamIsDead) EndOfBattle();
            else if (!character.GetStatus()) skipTurnEvent.Raise(character, skipTurnEvent);
            else Timing.RunCoroutine(character.id == CharacterType.PartyMember
                ? ThisPlayerTurn((PartyMember) character)
                : ThisEnemyTurn((Enemy) character));
        }

        private IEnumerator<float> ThisPlayerTurn(PartyMember character)
        {
            activeUnit = character;
            inventoryInputManager.TargetInventoryContainer = character.Container;
            inventoryInputManager.TargetInventoryDisplay = character.InventoryDisplay;
            character.inventoryDisplay.SetActive(true);
            
            character.ReplenishAP();
            var battlePanel = (BattleOptionsPanel) character.battleOptionsPanel;
            
            main_menu:
            characterTurnEvent.Raise(character, characterTurnEvent);
            BattleInput._canPressBack = false;
            usingItem = false;
            battlePanel.ShowBattlePanel();

            yield return Timing.WaitUntilFalse(() => choosingOption);
            
            yield return Timing.WaitForOneFrame;
            while (choosingAbility) 
            {
                BattleInput._canPressBack = true;
                abilityMenuLast = true;
                if (BattleInput.CancelCondition)
                {
                    abilityMenuLast = false;
                    goto main_menu;
                }
                yield return Timing.WaitForOneFrame;
            }

            if (endThisMembersTurn) goto end_of_turn;
            
            yield return Timing.WaitForOneFrame;
            chooseTargetEvent.Raise(character, chooseTargetEvent);

            while (choosingTarget)
            {
                yield return Timing.WaitForOneFrame;
                BattleInput._canPressBack = true;
                if (BattleInput.CancelCondition) goto main_menu;
            }
            
            abilityMenuLast = false;
            
            if (usingItem)
            {
                character.CurrentAP -= 2;
                yield return Timing.WaitForOneFrame;
                
                while (character.Unit.animationHandler.usingItem) 
                    yield return Timing.WaitForOneFrame;
                
                yield return Timing.WaitForSeconds(0.5f);
                goto skip_command_execution;
            }
            
            character.CurrentAP -= character.Unit.actionCost;

            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.BeforeEveryAction, 0.5f, true));

            if (!canGiveCommand) canGiveCommand = true;
            else { characterAttackEvent.Raise(character, characterAttackEvent);
                commandEvent.Raise(character, commandEvent); }

            yield return Timing.WaitUntilFalse(() => performingAction);

            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.AfterEveryAction, 0.5f, true));

            skip_command_execution:
            if (PartyOrEnemyTeamIsDead) goto end_of_turn;
            if (character.GetStatus() && character.CurrentAP > 0) goto main_menu;
            
            end_of_turn:
            endThisMembersTurn = false;
            endOfTurnEvent.Raise(character, endOfTurnEvent);
            character.inventoryDisplay.SetActive(false); // TODO: Make this a part of the EndOfTurn event
        }

        private IEnumerator<float> ThisEnemyTurn(Enemy enemy)
        {
            activeUnit = enemy;

            enemyTurnEvent.Raise(enemy, enemyTurnEvent);
            enemy.ReplenishAP();

            while (enemy.GetStatus() && enemy.CurrentAP > 0)
            {
                var shouldAttack = enemy.SetAI(_membersForThisBattle);
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.BeforeEveryAction, 0.5f, true));

                if (!canGiveCommand) canGiveCommand = true;
                else { characterAttackEvent.Raise(enemy, characterAttackEvent);
                    commandEvent.Raise(enemy, commandEvent); }

                yield return Timing.WaitUntilFalse(() => performingAction);

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.AfterEveryAction, 0.5f, true));

                if (PartyOrEnemyTeamIsDead) break;
                
                yield return Timing.WaitForSeconds(0.25f);
            }
            
            endOfTurnEvent.Raise(enemy, endOfTurnEvent);
        }
        
        #endregion

        #region EndOfBattle

        private void EndOfBattle()
        {
            if (AllEnemiesDead) battleEvent.Raise(BattleEvent.WonBattle);
            else if (AllMembersDead) battleEvent.Raise(BattleEvent.LostBattle);
        }

        private IEnumerator<float> WonBattleSequence()
        {
            audioController.StopAudio(themes[0], true, 2);

            yield return Timing.WaitForSeconds(2);
            
            audioController.PlayAudio(themes[1], true, 2);
            
            _membersForThisBattle.ForEach(member => member.Unit.anim.SetTrigger(AnimationHandler.VictoryTrigger));
            
            yield return Timing.WaitForSeconds(2f);
            battleResults.SetActive(true);
            var battleResultsUI = battleResults.GetComponent<BattleResultsUI>();
            battleResultsUI.DisableUI();
            
            foreach (var member in _membersForThisBattle)
            {
                member.ResetLevelUpAmount();
                
                var totalXp = _expGivers.Sum(giver =>
                    giver.CalculateExperience(member.level, member));
                
                var totalClassXp = _expGivers.Sum(giver =>
                    giver.CalculateExperience(member.currentClass.level, member.currentClass));

                member.BattleExpReceived = totalXp;
                
                member.AdvanceTowardsNextLevel(totalXp);
                member.currentClass.AdvanceTowardsNextLevel(totalClassXp);
            }

            yield return Timing.WaitUntilTrue(() => BattleInput._controls.Battle.Confirm.triggered);
            
            Timing.RunCoroutine(battleResultsUI.ShowLevelUps());
            yield return Timing.WaitForSeconds(0.3f);
            yield return Timing.WaitUntilFalse(() => battleResultsUI.showingLevelUps);
            
            _membersForThisBattle.ForEach(m =>
            {
                m.currentClass.statsToIncrease = new List<CharacterStat>();
            });

            SceneLoadManager.Instance.LoadOverworld();
        }

        private IEnumerator<float> LostBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            Logging.Instance.Log("you lost idiot");
            
            //TODO: Show game over menu with options to load save or quit
        }
        
        #endregion
        
        #region Removal

        private void RemoveFromTurn(UnitBase unit) => membersAndEnemiesThisTurn.Remove(unit);
        
        private void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) _enemiesForThisBattle.Remove((Enemy) unit);
            else _membersForThisBattle.Remove((PartyMember) unit);
            
            membersAndEnemiesThisTurn.Remove(unit);
            membersAndEnemiesNextTurn.Remove(unit);
     
            deathEvent.Raise(unit, deathEvent);
            
            sortingCalculator.ResortThisTurnOrder();
            sortingCalculator.ResortNextTurnOrder();

            unit.onDeath -= RemoveFromBattle;
        }
        
        #endregion

        private void AddToBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) _enemiesForThisBattle.Add((Enemy) unit);
            else _membersForThisBattle.Add((PartyMember) unit);
            
            membersAndEnemiesThisTurn.Add(unit);
            membersAndEnemiesNextTurn.Add(unit);
            
            sortingCalculator.ResortThisTurnOrder();
            sortingCalculator.ResortNextTurnOrder();
            
            unit.onDeath += RemoveFromBattle;
        }
        
        private void OnDisable()
        {
            _membersForThisBattle.ForEach(m => m.onDeath -= RemoveFromBattle);
            _enemiesForThisBattle.ForEach(e => e.onDeath -= RemoveFromBattle);
            
            battleEvent.RemoveListener(this);
            endOfTurnEvent.RemoveListener(this);
            skipTurnEvent.RemoveListener(this);
        }

        public void OnEventRaised(BattleEvent value)
        {
            switch (value)
            {
                case BattleEvent.WonBattle: Timing.RunCoroutine(WonBattleSequence());
                    break;
                case BattleEvent.LostBattle: Timing.RunCoroutine(LostBattleSequence());
                    break;
            }
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 != endOfTurnEvent && value2 != skipTurnEvent) return;
            RemoveFromTurn(value1);
            Timing.RunCoroutine(GetNextTurn());
        }
    }
}