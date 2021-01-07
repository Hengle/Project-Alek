using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Characters;
using BattleSystem.Generator;
using Characters.Animations;
using Characters.CharacterExtensions;
using Characters.PartyMembers;
using Characters.StatusEffects;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using MEC;

namespace BattleSystem
{
    public enum BattleState { Start, PartyTurn, EnemyTurn, Won, Lost }
    public class BattleManager : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        #region FieldsAndProperties
        
        public GlobalVariables globalVariables;

        private static BattleManager instance;
        
        public static BattleManager Instance {
            get { if (instance == null) 
                    Debug.LogError("BattleManager is null");
                return instance; }
        }

        [ShowInInspector] [ReadOnly]
        private  BattleState state;
        
        [ReadOnly] public InventoryInputManager inventoryInputManager;
        
        [ShowInInspector] [ReadOnly]
        public readonly List<Enemy> _enemiesForThisBattle = new List<Enemy>();
        [ShowInInspector] [ReadOnly]
        public readonly List<PartyMember> _membersForThisBattle = new List<PartyMember>();
        [ShowInInspector] [ReadOnly]
        public List<UnitBase> membersAndEnemies = new List<UnitBase>();
        [ShowInInspector] [ReadOnly]
        public List<UnitBase> membersAndEnemiesThisTurn = new List<UnitBase>();
        [ShowInInspector] [ReadOnly]
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
        [ReadOnly] private bool allMembersDead;
        [ReadOnly] private bool allEnemiesDead;
        [ShowInInspector] [ReadOnly]
        private bool PartyOrEnemyTeamIsDead
        {
            get
            {
                if (allEnemiesDead) state = BattleState.Won;
                else if (allMembersDead) state = BattleState.Lost;
                return allMembersDead || allEnemiesDead;
            }
        }
        
        private BattleGenerator generator;

        #endregion

        #region SettingUpBattle
        
        private void Awake() => instance = this;
        
        private void Start()
        {
            inventoryInputManager = FindObjectOfType<InventoryInputManager>();
            roundCount = 0;
            generator = GetComponent<BattleGenerator>();
            canGiveCommand = true;

            SetupBattle();
            state = BattleState.Start;
            GameEventsManager.AddListener(this);
        }

        private void SetupBattle()
        {
            generator.SetupBattle();

            foreach (var partyMember in _membersForThisBattle)
            {
                partyMember.battlePanel.GetComponent<MenuController>().SetEnemySelectables();
                partyMember.battlePanel.GetComponent<MenuController>().SetPartySelectables();
            }

            _membersForThisBattle.ForEach(m => m.onDeath += RemoveFromBattle);
            _enemiesForThisBattle.ForEach(e => e.onDeath += RemoveFromBattle);

            Timing.RunCoroutine(PerformThisRound());
        }

        #endregion

        #region RoundsAndCharacterTurns
        
        private IEnumerator<float> PerformThisRound()
        {
            BattleEvents.Trigger(BattleEventType.NewRound);
            
            SortByInitiative();
            yield return Timing.WaitForSeconds(1);

            foreach (var character in from character in membersAndEnemies
                let checkMemberStatus = character.GetStatus() where checkMemberStatus select character)
            {
                if (PartyOrEnemyTeamIsDead) break;

                yield return Timing.WaitUntilDone(character.InflictStatus
                    (Rate.EveryTurn, 1, true));
                
                if (PartyOrEnemyTeamIsDead || character.IsDead) break;

                yield return Timing.WaitUntilDone(character.id == CharacterType.PartyMember
                    ? ThisPlayerTurn((PartyMember) character)
                    : ThisEnemyTurn((Enemy) character));
            }

            switch (state)
            {
                case BattleState.Won:
                    BattleEvents.Trigger(BattleEventType.WonBattle);
                    Timing.RunCoroutine(WonBattleSequence());
                    break;
                
                case BattleState.Lost:
                    BattleEvents.Trigger(BattleEventType.LostBattle);
                    Timing.RunCoroutine(LostBattleSequence());
                    break;
                
                default:
                    roundCount++;
                    Timing.RunCoroutine(PerformThisRound());
                    break;
            }
        }

        private IEnumerator<float> ThisPlayerTurn(PartyMember character)
        {
            activeUnit = character;
            
            inventoryInputManager.TargetInventoryContainer =
                character.inventoryDisplay.GetComponent<CanvasGroup>();
            
            inventoryInputManager.TargetInventoryDisplay =
                character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();
            
            character.inventoryDisplay.SetActive(true);

            state = BattleState.PartyTurn;
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
                (Rate.BeforeEveryAction, 1, true));

            if (!canGiveCommand) canGiveCommand = true;
            else { CharacterEvents.Trigger(CEventType.CharacterAttacking, character);
                CharacterEvents.Trigger(CEventType.NewCommand, character); }

            yield return Timing.WaitUntilFalse(() => performingAction);

            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.AfterEveryAction, 1, true));

            skip_command_execution:
            if (PartyOrEnemyTeamIsDead || character.IsDead) goto end_of_turn;
            
            if (character.CurrentAP > 0) goto main_menu;
            
            end_of_turn:
            endThisMembersTurn = false;
            CharacterEvents.Trigger(CEventType.EndOfTurn, character);
            character.inventoryDisplay.SetActive(false); // TODO: Make this a part of the EndOfTurn event
        }

        private IEnumerator<float> ThisEnemyTurn(Enemy enemy)
        {
            activeUnit = enemy;

            state = BattleState.EnemyTurn;
            CharacterEvents.Trigger(CEventType.EnemyTurn, enemy);
            enemy.ReplenishAP();

            while (enemy.CurrentAP > 0)
            {
                if (enemy.IsDead) break;

                var shouldAttack = enemy.SetAI(_membersForThisBattle);
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.BeforeEveryAction, 1, true));

                if (!canGiveCommand) canGiveCommand = true;
                else { CharacterEvents.Trigger(CEventType.CharacterAttacking, enemy);
                    CharacterEvents.Trigger(CEventType.NewCommand, enemy); }

                yield return Timing.WaitUntilFalse(() => performingAction);

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.AfterEveryAction, 1, true));

                if (PartyOrEnemyTeamIsDead || enemy.IsDead) break;
                
                yield return Timing.WaitForSeconds(0.5f);
            }
            
            CharacterEvents.Trigger(CEventType.EndOfTurn, enemy);
        }
        
        #endregion

        #region EndOfBattle

        private IEnumerator<float> WonBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            _membersForThisBattle.ForEach(member => member.Unit.anim.SetTrigger(AnimationHandler.VictoryTrigger));
            Logger.Log("yay, you won");
        }

        private IEnumerator<float> LostBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            Logger.Log("you lost idiot");
        }
        
        #endregion

        #region Sorting

        private void SortByInitiative()
        {
            if (membersAndEnemiesNextTurn.Count > 0)
            {
                membersAndEnemies = membersAndEnemiesNextTurn;
                membersAndEnemiesThisTurn = new List<UnitBase>(membersAndEnemies);
                GetNewListNextTurn();
            }
            else
            {
                GetNewList();
                GetNewListNextTurn();
            }
            
            BattleEvents.Trigger(BattleEventType.ThisTurnListCreated);
            BattleEvents.Trigger(BattleEventType.NextTurnListCreated);
        }

        private void GetNewList()
        {
            membersAndEnemies = new List<UnitBase>();
            membersAndEnemiesThisTurn = new List<UnitBase>();
            
            foreach (var member in _membersForThisBattle) membersAndEnemies.Add(member);
            foreach (var enemy in _enemiesForThisBattle) membersAndEnemies.Add(enemy);
                
            membersAndEnemies = membersAndEnemies.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(membersAndEnemies);
                
            membersAndEnemies = membersAndEnemies.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();

            membersAndEnemiesThisTurn = new List<UnitBase>(membersAndEnemies);
        }

        private void GetNewListNextTurn()
        {
            membersAndEnemiesNextTurn = new List<UnitBase>();
            
            foreach (var member in _membersForThisBattle) membersAndEnemiesNextTurn.Add(member);
            foreach (var enemy in _enemiesForThisBattle) membersAndEnemiesNextTurn.Add(enemy);
                
            membersAndEnemiesNextTurn = membersAndEnemiesNextTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(membersAndEnemiesNextTurn);
                
            membersAndEnemiesNextTurn = membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
        }
        
        private static void GetFinalInitValues(List<UnitBase> list)
        {
            var minModifier = 1.8f;
            foreach (var t in list)
            {
                t.Unit.initModifier = Random.Range(minModifier, 2.0f);
                t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier);
                minModifier -= 0.1f;
            }
        }

        private void ResortNextTurnOrder()
        {
            membersAndEnemiesNextTurn.ForEach(t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            membersAndEnemiesNextTurn = membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
            
            BattleEvents.Trigger(BattleEventType.NextTurnListCreated);
        }
        
        #endregion

        #region Misc

        private void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy)
            {
                _enemiesForThisBattle.Remove((Enemy) unit);
                membersAndEnemiesThisTurn.Remove((Enemy) unit);
                membersAndEnemiesNextTurn.Remove((Enemy) unit);
            }
            else
            {
                _membersForThisBattle.Remove((PartyMember) unit);
                membersAndEnemiesThisTurn.Remove((PartyMember) unit);
                membersAndEnemiesNextTurn.Remove((PartyMember) unit);
            }
            
            ResortNextTurnOrder();
            CharacterEvents.Trigger(CEventType.CharacterDeath, unit);

            if (_membersForThisBattle.Count == 0) allMembersDead = true;
            if (_enemiesForThisBattle.Count == 0) allEnemiesDead = true;
        }
        
        
        #endregion

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.CantPerformAction)
            {
                canGiveCommand = false;
            }
            
            else if (eventType._eventType == CEventType.StatChange)
            {
                ResortNextTurnOrder();
            }
        }
    }
}