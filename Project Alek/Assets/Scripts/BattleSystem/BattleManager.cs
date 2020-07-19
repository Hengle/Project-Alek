using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Characters;
using BattleSystem.Generator;
using Characters.PartyMembers;
using Characters.StatusEffects;
using Input;
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
        public static PartyMember _activePartyMember;
        
        [ShowInInspector] [ReadOnly]
        public static bool _choosingOption;
        [ShowInInspector] [ReadOnly]
        public static bool _choosingTarget;
        [ShowInInspector] [ReadOnly]
        public static bool _performingAction;
        [ShowInInspector] [ReadOnly]
        public static bool _endThisMembersTurn;
        [ShowInInspector] [ReadOnly]
        public static bool _choosingAbility;
        [ShowInInspector] [ReadOnly]
        public static bool _shouldGiveCommand;
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
            Timing.RunCoroutine(SetupBattle());
            state = BattleState.Start;
            GameEventsManager.AddListener(this);
        }

        private IEnumerator<float> SetupBattle()
        {
            yield return Timing.WaitUntilFalse(generator.SetupBattle);

            foreach (var partyMember in MembersForThisBattle)
            {
                yield return Timing.WaitUntilTrue(partyMember.battlePanel.GetComponent<MenuController>().SetEnemySelectables);
                yield return Timing.WaitUntilTrue(partyMember.battlePanel.GetComponent<MenuController>().SetPartySelectables);
            }

            SortByInitiative();

            foreach (var character in MembersForThisBattle) character.onDeath += RemoveFromBattle;
            
            Timing.RunCoroutine(PerformThisRound());
        }

        #endregion

        #region RoundsAndCharacterTurns
        
        private IEnumerator<float> PerformThisRound()
        {
            BattleEvents.Trigger(BattleEventType.NewRound);
            
            SortByInitiative();

            foreach (var character in from character in _membersAndEnemies
                let checkMemberStatus = character.GetStatus() where checkMemberStatus select character)
            {
                if (PartyOrEnemyTeamIsDead) break;

                yield return Timing.WaitUntilDone(InflictStatus.OnThisUnit
                    (character, RateOfInfliction.EveryTurn, 1, true));

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
            _activePartyMember = character;
            
            _inventoryInputManager.TargetInventoryContainer =
                character.inventoryDisplay.GetComponent<CanvasGroup>();
            
            _inventoryInputManager.TargetInventoryDisplay =
                character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();
            
            character.inventoryDisplay.SetActive(true);

            state = BattleState.PartyTurn;
            character.ResetAP();
            
            main_menu:
            CharacterEvents.Trigger(CEventType.CharacterTurn, character);
            BattleInput._canPressBack = false;
            ((BattleOptionsPanel) character.battleOptionsPanel).ShowBattlePanel();
            
            while (_choosingOption) yield return Timing.WaitForOneFrame;

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
            
            character.CurrentAP -= character.Unit.actionCost;

            yield return Timing.WaitUntilDone(InflictStatus.OnThisUnit
                (character, RateOfInfliction.BeforeEveryAction, 1, true));

            if (!_shouldGiveCommand) _shouldGiveCommand = true;
            
            else 
            {
                CharacterEvents.Trigger(CEventType.CharacterAttacking, character);
                CharacterEvents.Trigger(CEventType.NewCommand, character);
            }
            
            while (_performingAction) yield return Timing.WaitForOneFrame;

            yield return Timing.WaitUntilDone(InflictStatus.OnThisUnit
                (character, RateOfInfliction.AfterEveryAction, 1, true));

            if (PartyOrEnemyTeamIsDead || character.IsDead) goto end_of_turn;
            
            if (character.CurrentAP > 0) goto main_menu;
            
            end_of_turn:
            _endThisMembersTurn = false;
            CharacterEvents.Trigger(CEventType.EndOfTurn, character);
            character.inventoryDisplay.SetActive(false); // TODO: Make this a part of the EndOfTurn event
        }

        private static IEnumerator<float> ThisEnemyTurn(Enemy enemy)
        {
            state = BattleState.EnemyTurn;
            enemy.ResetAP();

            while (enemy.CurrentAP > 0)
            {
                if (enemy.IsDead) break;

                var shouldAttack = enemy.SetAI(MembersForThisBattle);
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;

                yield return Timing.WaitUntilDone(InflictStatus.OnThisUnit
                    (enemy, RateOfInfliction.BeforeEveryAction, 1, true));

                if (!_shouldGiveCommand) _shouldGiveCommand = true;
                else CharacterEvents.Trigger(CEventType.NewCommand, enemy);

                while (_performingAction) yield return Timing.WaitForOneFrame;

                yield return Timing.WaitUntilDone(InflictStatus.OnThisUnit
                    (enemy, RateOfInfliction.AfterEveryAction, 1, true));

                if (PartyOrEnemyTeamIsDead || enemy.IsDead) break;

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
        
        #endregion

        #region EndOfBattle

        private IEnumerator<float> WonBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            Logger.Log("yay, you won");
        }

        private IEnumerator<float> LostBattleSequence()
        {
            yield return Timing.WaitForSeconds(0.5f);
            Logger.Log("you lost idiot");
        }
        
        #endregion

        #region Misc

        private static void SortByInitiative()
        {
            _membersAndEnemies = new List<UnitBase>();
            
            foreach (var member in MembersForThisBattle) _membersAndEnemies.Add(member);
            foreach (var enemy in EnemiesForThisBattle) _membersAndEnemies.Add(enemy);
            
            _membersAndEnemies = _membersAndEnemies.OrderByDescending
                (e => e.initiative.Value).ToList();
        }

        private static void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) EnemiesForThisBattle.Remove((Enemy) unit);
            else MembersForThisBattle.Remove((PartyMember) unit);

            if (MembersForThisBattle.Count == 0) allMembersDead = true;
            if (EnemiesForThisBattle.Count == 0) allEnemiesDead = true;
        }

        private static void ResetStaticVariables()
        {
            _activePartyMember = null;
            _choosingOption = false;
            _choosingTarget = false;
            _performingAction = false;
            _endThisMembersTurn = false;
            _choosingAbility = false;
            allMembersDead = false;
            allEnemiesDead = false;
            _shouldGiveCommand = true;
        }
        
        #endregion

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType == CEventType.CantPerformAction)
            {
                _shouldGiveCommand = false;
            }
        }
    }
}