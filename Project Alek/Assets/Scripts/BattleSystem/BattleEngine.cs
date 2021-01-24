using System.Collections.Generic;
using System.Linq;
using BattleSystem.Calculators;
using UnityEngine;
using Characters;
using BattleSystem.Generator;
using Characters.Animations;
using Characters.CharacterExtensions;
using Characters.Enemies;
using Characters.PartyMembers;
using Characters.StatusEffects;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using MEC;

namespace BattleSystem
{
    public class BattleEngine : MonoBehaviour, IGameEventListener<CharacterEvents>, IGameEventListener<BattleEvents>
    {
        #region FieldsAndProperties
        
        public GlobalVariables globalVariables;

        private static BattleEngine instance;
        
        public static BattleEngine Instance {
            get { if (!instance) Debug.LogError("BattleManager is null");
                return instance; }
        }

        [HideInInspector] public InventoryInputManager inventoryInputManager;
        [HideInInspector] public BattleGenerator generator;

        //TODO: Make new list for enemies that is not updated for exp calculations
        [ReadOnly] public readonly List<IGiveExperience> _expGivers = new List<IGiveExperience>();
        [ReadOnly] public readonly List<Enemy> _enemiesForThisBattle = new List<Enemy>();
        [ReadOnly] public readonly List<PartyMember> _membersForThisBattle = new List<PartyMember>();

        [ReadOnly] public List<UnitBase> membersAndEnemiesThisTurn = new List<UnitBase>();
        [ReadOnly] public List<UnitBase> membersAndEnemiesNextTurn = new List<UnitBase>();

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

        private bool AllMembersDead => _membersForThisBattle.Count == 0;
        private bool AllEnemiesDead => _enemiesForThisBattle.Count == 0;
        private bool PartyOrEnemyTeamIsDead => AllMembersDead || AllEnemiesDead;

        #endregion

        #region SettingUpBattle
        
        private void Awake() => instance = this;
        
        private void Start()
        {
            inventoryInputManager = FindObjectOfType<InventoryInputManager>();
            generator = GetComponent<BattleGenerator>();
            
            canGiveCommand = true;
            roundCount = 0;

            SetupBattle();
            
            GameEventsManager.AddListener<CharacterEvents>(this);
            GameEventsManager.AddListener<BattleEvents>(this);
        }

        private void SetupBattle()
        {
            generator.SetupBattle();

            SelectableObjectManager.SetEnemySelectables();
            SelectableObjectManager.SetPartySelectables();
      
            _membersForThisBattle.ForEach(m => m.Unit.GetComponent<ChooseTarget>().Setup());
            _enemiesForThisBattle.ForEach(e => e.Unit.GetComponent<ChooseTarget>().Setup());

            _membersForThisBattle.ForEach(m => { m.onDeath += RemoveFromBattle; m.onRevival += AddToBattle; });
            _enemiesForThisBattle.ForEach(e => { e.onDeath += RemoveFromBattle; e.onRevival += AddToBattle; });
            
            Timing.RunCoroutine(GetNextTurn());
        }

        #endregion

        #region RoundsAndCharacterTurns

        private IEnumerator<float> GetNextTurn()
        {
            if (PartyOrEnemyTeamIsDead) { EndOfBattle(); yield break; }

            yield return Timing.WaitForSeconds(0.25f);
            
            if (membersAndEnemiesThisTurn.Count == 0) { BattleEvents.Trigger(BattleEventType.NewRound);
                yield return Timing.WaitUntilTrue(SortingCalculator.SortByInitiative); }

            var character = membersAndEnemiesThisTurn[0];
                
            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.EveryTurn, 1, true));
                
            if (PartyOrEnemyTeamIsDead) EndOfBattle();
            
            else if (!character.GetStatus()) CharacterEvents.Trigger(CEventType.SkipTurn, character);
            
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
            
            main_menu:
            CharacterEvents.Trigger(CEventType.CharacterTurn, character);
            BattleInput._canPressBack = false;
            usingItem = false;
            ((BattleOptionsPanel) character.battleOptionsPanel).ShowBattlePanel();

            yield return Timing.WaitUntilFalse(() => choosingOption);
            
            while (choosingAbility) 
            {
                BattleInput._canPressBack = true;
                if (BattleInput.CancelCondition) goto main_menu;
                yield return Timing.WaitForOneFrame;
            }

            if (endThisMembersTurn) goto end_of_turn;
            
            yield return Timing.WaitForOneFrame;
            CharacterEvents.Trigger(CEventType.ChoosingTarget, character);

            while (choosingTarget)
            {
                yield return Timing.WaitForOneFrame;
                BattleInput._canPressBack = true;
                if (BattleInput.CancelCondition) goto main_menu;
            }

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
            else { CharacterEvents.Trigger(CEventType.CharacterAttacking, character);
                CharacterEvents.Trigger(CEventType.NewCommand, character); }

            yield return Timing.WaitUntilFalse(() => performingAction);

            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.AfterEveryAction, 0.5f, true));

            skip_command_execution:
            if (PartyOrEnemyTeamIsDead) goto end_of_turn;

            if (character.GetStatus() && character.CurrentAP > 0) goto main_menu;
            
            end_of_turn:
            endThisMembersTurn = false;
            CharacterEvents.Trigger(CEventType.EndOfTurn, character);
            character.inventoryDisplay.SetActive(false); // TODO: Make this a part of the EndOfTurn event
        }

        private IEnumerator<float> ThisEnemyTurn(Enemy enemy)
        {
            activeUnit = enemy;
            
            CharacterEvents.Trigger(CEventType.EnemyTurn, enemy);
            enemy.ReplenishAP();

            while (enemy.GetStatus() && enemy.CurrentAP > 0)
            {
                var shouldAttack = enemy.SetAI(_membersForThisBattle);
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.BeforeEveryAction, 0.5f, true));

                if (!canGiveCommand) canGiveCommand = true;
                else { CharacterEvents.Trigger(CEventType.CharacterAttacking, enemy);
                    CharacterEvents.Trigger(CEventType.NewCommand, enemy); }

                yield return Timing.WaitUntilFalse(() => performingAction);

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.AfterEveryAction, 0.5f, true));

                if (PartyOrEnemyTeamIsDead) break;
                
                yield return Timing.WaitForSeconds(0.25f);
            }
            
            CharacterEvents.Trigger(CEventType.EndOfTurn, enemy);
        }
        
        #endregion

        #region EndOfBattle

        private void EndOfBattle()
        {
            if (AllEnemiesDead) BattleEvents.Trigger(BattleEventType.WonBattle);
            else if (AllMembersDead) BattleEvents.Trigger(BattleEventType.LostBattle);
        }

        private IEnumerator<float> WonBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            _membersForThisBattle.ForEach(member => member.Unit.anim.SetTrigger(AnimationHandler.VictoryTrigger));
      
            foreach (var member in _membersForThisBattle)
            {
                var totalXp = _expGivers.Sum(giver => giver.CalculateExperience(member.level));
                member.AdvanceTowardsNextLevel(totalXp);
            }
            
            SceneLoader.Instance.LoadOverworld();
        }

        private IEnumerator<float> LostBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            Logger.Log("you lost idiot");
            
            SceneLoader.Instance.LoadOverworld();
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
            
            CharacterEvents.Trigger(CEventType.CharacterDeath, unit);
            
            SortingCalculator.ResortThisTurnOrder();
            SortingCalculator.ResortNextTurnOrder();

            unit.onDeath -= RemoveFromBattle;
        }
        
        #endregion

        private void AddToBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) _enemiesForThisBattle.Add((Enemy) unit);
            else _membersForThisBattle.Add((PartyMember) unit);
            
            membersAndEnemiesThisTurn.Add(unit);
            membersAndEnemiesNextTurn.Add(unit);
            
            SortingCalculator.ResortThisTurnOrder();
            SortingCalculator.ResortNextTurnOrder();
            
            unit.onDeath += RemoveFromBattle;
        }
        
        private void OnDisable()
        {
            _membersForThisBattle.ForEach(m => m.onDeath -= RemoveFromBattle);
            _enemiesForThisBattle.ForEach(e => e.onDeath -= RemoveFromBattle);
            
            GameEventsManager.RemoveListener<CharacterEvents>(this);
            GameEventsManager.RemoveListener<BattleEvents>(this);
        }
        
        public void OnGameEvent(CharacterEvents eventType)
        {
            switch (eventType._eventType)
            {
                case CEventType.CantPerformAction: canGiveCommand = false;
                    break;
                case CEventType.StatChange:
                    SortingCalculator.ResortThisTurnOrder();
                    SortingCalculator.ResortNextTurnOrder();
                    break;
                case CEventType.EndOfTurn:
                case CEventType.SkipTurn:
                    RemoveFromTurn((UnitBase) eventType._character);
                    Timing.RunCoroutine(GetNextTurn());
                    break;
            }
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            switch (eventType._battleEventType)
            {
                case BattleEventType.WonBattle: Timing.RunCoroutine(WonBattleSequence());
                    break;
                case BattleEventType.LostBattle: Timing.RunCoroutine(LostBattleSequence());
                    break;
            }
        }
    }
}