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
        
        [ShowInInspector] [ReadOnly]
        private static BattleState state;
        
        public static InventoryInputManager _inventoryInputManager;

        [ShowInInspector] [ReadOnly]
        public static readonly List<Enemy> EnemiesForThisBattle = new List<Enemy>();
        [ShowInInspector] [ReadOnly]
        public static readonly List<PartyMember> MembersForThisBattle = new List<PartyMember>();
        [ShowInInspector] [ReadOnly]
        public static List<UnitBase> _membersAndEnemies = new List<UnitBase>();
        [ShowInInspector] [ReadOnly]
        public static List<UnitBase> _membersAndEnemiesThisTurn = new List<UnitBase>();
        [ShowInInspector] [ReadOnly]
        public static List<UnitBase> _membersAndEnemiesNextTurn = new List<UnitBase>();

        [ShowInInspector] [ReadOnly]
        public static UnitBase _activeUnit;
        
        [ShowInInspector] [ReadOnly]
        public static bool _choosingOption;
        [ShowInInspector] [ReadOnly]
        public static bool _choosingTarget;
        [ShowInInspector] [ReadOnly]
        public static bool _usingItem;
        [ShowInInspector] [ReadOnly]
        public static bool _performingAction;
        [ShowInInspector] [ReadOnly]
        public static bool _endThisMembersTurn;
        [ShowInInspector] [ReadOnly]
        public static bool _choosingAbility;
        [ShowInInspector] [ReadOnly]
        public static bool _canGiveCommand;
        [ShowInInspector] [ReadOnly]
        private static bool allMembersDead;
        [ShowInInspector] [ReadOnly]
        private static bool allEnemiesDead;
        [ShowInInspector] [ReadOnly]
        private static bool PartyOrEnemyTeamIsDead
        {
            get
            {
                if (allEnemiesDead) state = BattleState.Won;
                else if (allMembersDead) state = BattleState.Lost;
                return allMembersDead || allEnemiesDead;
            }
        }

        private BattleGenerator generator;

        [SerializeField] [ReadOnly]
        private int roundCount;

        #endregion

        #region SettingUpBattle
        
        private void Start()
        {
            _inventoryInputManager = FindObjectOfType<InventoryInputManager>();
            roundCount = 0;
            generator = GetComponent<BattleGenerator>();

            ResetStaticVariables();
            SetupBattle();
            state = BattleState.Start;
            GameEventsManager.AddListener(this);
        }

        private void SetupBattle()
        {
            generator.SetupBattle();

            foreach (var partyMember in MembersForThisBattle)
            {
                partyMember.battlePanel.GetComponent<MenuController>().SetEnemySelectables();
                partyMember.battlePanel.GetComponent<MenuController>().SetPartySelectables();
            }

            MembersForThisBattle.ForEach(m => m.onDeath += RemoveFromBattle);
            EnemiesForThisBattle.ForEach(e => e.onDeath += RemoveFromBattle);

            Timing.RunCoroutine(PerformThisRound());
        }

        #endregion

        #region RoundsAndCharacterTurns
        
        private IEnumerator<float> PerformThisRound()
        {
            BattleEvents.Trigger(BattleEventType.NewRound);
            
            SortByInitiative();
            yield return Timing.WaitForSeconds(1);

            foreach (var character in from character in _membersAndEnemies
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

        private static IEnumerator<float> ThisPlayerTurn(PartyMember character)
        {
            _activeUnit = character;
            
            _inventoryInputManager.TargetInventoryContainer =
                character.inventoryDisplay.GetComponent<CanvasGroup>();
            
            _inventoryInputManager.TargetInventoryDisplay =
                character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();
            
            character.inventoryDisplay.SetActive(true);

            state = BattleState.PartyTurn;
            character.ReplenishAP();
            
            main_menu:
            CharacterEvents.Trigger(CEventType.CharacterTurn, character);
            BattleInput._canPressBack = false;
            _usingItem = false;
            ((BattleOptionsPanel) character.battleOptionsPanel).ShowBattlePanel();

            yield return Timing.WaitUntilFalse(() => _choosingOption);
            
            while (_choosingAbility) 
            {
                BattleInput._canPressBack = true;
                if (BattleInput.CancelCondition) goto main_menu;
                yield return Timing.WaitForOneFrame;
            }

            if (_endThisMembersTurn) goto end_of_turn;
            
            yield return Timing.WaitForOneFrame;
            CharacterEvents.Trigger(CEventType.ChoosingTarget, character);

            while (_choosingTarget)
            {
                yield return Timing.WaitForOneFrame;
                BattleInput._canPressBack = true;
                if (BattleInput.CancelCondition) goto main_menu;
            }

            if (_usingItem)
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

            if (!_canGiveCommand) _canGiveCommand = true;
            else { CharacterEvents.Trigger(CEventType.CharacterAttacking, character);
                CharacterEvents.Trigger(CEventType.NewCommand, character); }

            yield return Timing.WaitUntilFalse(() => _performingAction);

            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.AfterEveryAction, 1, true));

            skip_command_execution:
            if (PartyOrEnemyTeamIsDead || character.IsDead) goto end_of_turn;
            
            if (character.CurrentAP > 0) goto main_menu;
            
            end_of_turn:
            _endThisMembersTurn = false;
            CharacterEvents.Trigger(CEventType.EndOfTurn, character);
            character.inventoryDisplay.SetActive(false); // TODO: Make this a part of the EndOfTurn event
        }

        private static IEnumerator<float> ThisEnemyTurn(Enemy enemy)
        {
            _activeUnit = enemy;
            
            state = BattleState.EnemyTurn;
            CharacterEvents.Trigger(CEventType.EnemyTurn, enemy);
            enemy.ReplenishAP();

            while (enemy.CurrentAP > 0)
            {
                if (enemy.IsDead) break;

                var shouldAttack = enemy.SetAI(MembersForThisBattle);
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.BeforeEveryAction, 1, true));

                if (!_canGiveCommand) _canGiveCommand = true;
                else { CharacterEvents.Trigger(CEventType.CharacterAttacking, enemy);
                    CharacterEvents.Trigger(CEventType.NewCommand, enemy); }

                yield return Timing.WaitUntilFalse(() => _performingAction);

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
            MembersForThisBattle.ForEach(member => member.Unit.anim.SetTrigger(AnimationHandler.VictoryTrigger));
            Logger.Log("yay, you won");
        }

        private IEnumerator<float> LostBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            Logger.Log("you lost idiot");
        }
        
        #endregion

        #region Sorting

        private static void SortByInitiative()
        {
            if (_membersAndEnemiesNextTurn.Count > 0)
            {
                _membersAndEnemies = _membersAndEnemiesNextTurn;
                _membersAndEnemiesThisTurn = new List<UnitBase>(_membersAndEnemies);
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

        private static void GetNewList()
        {
            _membersAndEnemies = new List<UnitBase>();
            _membersAndEnemiesThisTurn = new List<UnitBase>();
            
            foreach (var member in MembersForThisBattle) _membersAndEnemies.Add(member);
            foreach (var enemy in EnemiesForThisBattle) _membersAndEnemies.Add(enemy);
                
            _membersAndEnemies = _membersAndEnemies.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(_membersAndEnemies);
                
            _membersAndEnemies = _membersAndEnemies.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();

            _membersAndEnemiesThisTurn = new List<UnitBase>(_membersAndEnemies);
        }
        
        private static void GetNewListNextTurn()
        {
            _membersAndEnemiesNextTurn = new List<UnitBase>();
            
            foreach (var member in MembersForThisBattle) _membersAndEnemiesNextTurn.Add(member);
            foreach (var enemy in EnemiesForThisBattle) _membersAndEnemiesNextTurn.Add(enemy);
                
            _membersAndEnemiesNextTurn = _membersAndEnemiesNextTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(_membersAndEnemiesNextTurn);
                
            _membersAndEnemiesNextTurn = _membersAndEnemiesNextTurn.OrderByDescending
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

        private static void ResortNextTurnOrder()
        {
            _membersAndEnemiesNextTurn.ForEach(t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            _membersAndEnemiesNextTurn = _membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
            
            BattleEvents.Trigger(BattleEventType.NextTurnListCreated);
        }
        
        #endregion

        #region Misc

        private static void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy)
            {
                EnemiesForThisBattle.Remove((Enemy) unit);
                _membersAndEnemiesThisTurn.Remove((Enemy) unit);
                _membersAndEnemiesNextTurn.Remove((Enemy) unit);
            }
            else
            {
                MembersForThisBattle.Remove((PartyMember) unit);
                _membersAndEnemiesThisTurn.Remove((PartyMember) unit);
                _membersAndEnemiesNextTurn.Remove((PartyMember) unit);
            }
            
            ResortNextTurnOrder();
            CharacterEvents.Trigger(CEventType.CharacterDeath, unit);

            if (MembersForThisBattle.Count == 0) allMembersDead = true;
            if (EnemiesForThisBattle.Count == 0) allEnemiesDead = true;
        }

        private static void ResetStaticVariables()
        {
            _membersAndEnemies = new List<UnitBase>();
            _membersAndEnemiesNextTurn = new List<UnitBase>();
            _membersAndEnemiesThisTurn = new List<UnitBase>();
            
            _activeUnit = null;
            _choosingOption = false;
            _choosingTarget = false;
            _performingAction = false;
            _endThisMembersTurn = false;
            _choosingAbility = false;
            allMembersDead = false;
            allEnemiesDead = false;
            _canGiveCommand = true;
            _usingItem = false;
        }
        
        #endregion

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.CantPerformAction)
            {
                _canGiveCommand = false;
            }
            
            else if (eventType._eventType == CEventType.StatChange)
            {
                ResortNextTurnOrder();
            }
        }
    }
}