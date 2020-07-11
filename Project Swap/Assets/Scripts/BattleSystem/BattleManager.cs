using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using System.Linq;
using BattleSystem.Calculator;
using Characters;
using BattleSystem.Generator;
using Characters.PartyMembers;
using Characters.StatusEffects;
using Input;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;

namespace BattleSystem
{
    // Need to rework the enums. They are not good right now
    public enum BattleState { Start, PartyTurn, EnemyTurn, Won, Lost }
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class BattleManager : MonoBehaviour
    {
        #region FieldsAndProperties
        
        [ShowInInspector]
        private static BattleState state;
        public static BattleFunctions _battleFunctions;

        [ShowInInspector] public static readonly List<Enemy> EnemiesForThisBattle = new List<Enemy>();
        [ShowInInspector] public static readonly List<PartyMember> MembersForThisBattle = new List<PartyMember>();
        [ShowInInspector] public static List<UnitBase> _membersAndEnemies = new List<UnitBase>();

        [ShowInInspector] public static PartyMember _activePartyMember;
        
        [ShowInInspector] public static bool _choosingOption;
        [ShowInInspector] public static bool _choosingTarget;
        [ShowInInspector] public static bool _performingAction;
        [ShowInInspector] public static bool _endThisMembersTurn;
        [ShowInInspector] public static bool _choosingAbility;
        [ShowInInspector] public static bool _shouldGiveCommand;
        
        [ShowInInspector] private static bool allMembersDead;
        [ShowInInspector] private static bool allEnemiesDead;
        [ShowInInspector] private static bool PartyOrEnemyTeamIsDead {
            get
            {
                if (allEnemiesDead) state = BattleState.Won;
                else if (allMembersDead) state = BattleState.Lost;
                return allMembersDead || allEnemiesDead;
            }
        }

        private BattleGenerator generator;

        [SerializeField] private int roundCount;
        
        #endregion

        #region SettingUpBattle
        
        private void Start()
        {
            roundCount = 0;
            generator = GetComponent<BattleGenerator>();
            _battleFunctions = GetComponent<BattleFunctions>();

            ResetStaticVariables();
            StartCoroutine(SetupBattle());
            state = BattleState.Start;
        }

        private IEnumerator SetupBattle()
        {
            yield return new WaitWhile(generator.SetupBattle);

            foreach (var partyMember in MembersForThisBattle) {
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetEnemySelectables);
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetPartySelectables);
            }

            SortingCalculator.SortByInitiative();

            foreach (var character in MembersForThisBattle) character.onDeath += RemoveFromBattle;
            
            StartCoroutine(PerformThisRound());
        }

        #endregion

        #region RoundsAndCharacterTurns
        
        private IEnumerator PerformThisRound()
        {
            BattleEvents.Trigger(BattleEventType.NewRound);
            
            SortingCalculator.SortByInitiative();

            foreach (var character in from character in _membersAndEnemies
                let checkMemberStatus = character.GetStatus() where checkMemberStatus select character)
            {
                if (PartyOrEnemyTeamIsDead) break;
                
                var inflictStatusEffects = StartCoroutine(InflictStatus.OnThisUnit
                    (character, RateOfInfliction.EveryTurn, 1,true));
                
                yield return inflictStatusEffects;

                if (PartyOrEnemyTeamIsDead || character.IsDead) break;
                
                var round = StartCoroutine(character.id == CharacterType.PartyMember?
                    ThisPlayerTurn((PartyMember) character) : ThisEnemyTurn((Enemy) character));

                yield return round;
            }

            switch (state)
            {
                // Could make the sequences events
                case BattleState.Won:
                    BattleEvents.Trigger(BattleEventType.WonBattle);
                    StartCoroutine(WonBattleSequence());
                    break;
                case BattleState.Lost:
                    BattleEvents.Trigger(BattleEventType.LostBattle);
                    StartCoroutine(LostBattleSequence());
                    break;
                default:
                    roundCount++;
                    StartCoroutine(PerformThisRound());
                    break;
            }
        }

        private IEnumerator ThisPlayerTurn(PartyMember character)
        {
            _activePartyMember = character;
            
            BattleInputManager._inventoryInputManager.TargetInventoryContainer = character.inventoryDisplay.GetComponent<CanvasGroup>();
            BattleInputManager._inventoryInputManager.TargetInventoryDisplay = character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

            yield return new WaitForSeconds(0.5f);
            character.inventoryDisplay.SetActive(true);

            state = BattleState.PartyTurn;
            character.ResetAP();
            
            main_menu:
            CharacterEvents.Trigger(CEventType.CharacterTurn, character);
            BattleInputManager._canPressBack = false;
            character.battleOptionsPanel.ShowBattlePanel();

            while (_choosingOption) yield return null;
            yield return new WaitForSeconds(0.5f);
                
            while (_choosingAbility) 
            {
                BattleInputManager._canPressBack = true;
                if (BattleInputManager.CancelCondition) goto main_menu;
                yield return null;
            }

            if (_endThisMembersTurn)
            {
                CharacterEvents.Trigger(CEventType.EndOfTurn, character);
                _endThisMembersTurn = false;
                character.inventoryDisplay.SetActive(false);
                yield break;
            }

            CharacterEvents.Trigger(CEventType.ChoosingTarget, character);
            yield return new WaitForSeconds(0.5f);

            while (_choosingTarget)
            {
                BattleInputManager._canPressBack = true;
                if (BattleInputManager.CancelCondition) goto main_menu;
                yield return null;
            }
            
            character.CurrentAP -= character.Unit.actionCost;

            var inflictStatusEffectsBefore = StartCoroutine(InflictStatus.OnThisUnit
                (character, RateOfInfliction.BeforeEveryAction, 1,true));
            
            yield return inflictStatusEffectsBefore;

            if (!_shouldGiveCommand) _shouldGiveCommand = true;
            
            else 
            {
                CharacterEvents.Trigger(CEventType.CharacterAttacking, character);
                character.GiveCommand();
            }
            
            while (_performingAction) yield return null;
            
            var inflictStatusEffectsAfter = StartCoroutine(InflictStatus.OnThisUnit
                (character, RateOfInfliction.AfterEveryAction, 1,true));
            
            yield return inflictStatusEffectsAfter;

            if (PartyOrEnemyTeamIsDead || character.IsDead)
            {
                CharacterEvents.Trigger(CEventType.EndOfTurn, character);
                character.inventoryDisplay.SetActive(false);
                yield break;
            }
            
            if (character.CurrentAP > 0) goto main_menu;
            
            CharacterEvents.Trigger(CEventType.EndOfTurn, character);
            character.inventoryDisplay.SetActive(false);
        }

        private IEnumerator ThisEnemyTurn(Enemy enemy)
        {
            state = BattleState.EnemyTurn;
            enemy.ResetAP();

            while (enemy.CurrentAP > 0)
            {
                if (enemy.IsDead) break;

                var shouldAttack = enemy.SetAI();
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;
                
                var inflictStatusEffectsBefore = StartCoroutine(InflictStatus.OnThisUnit
                    (enemy, RateOfInfliction.BeforeEveryAction, 1,true));
                
                yield return inflictStatusEffectsBefore;

                if (!_shouldGiveCommand) _shouldGiveCommand = true;
                else enemy.GiveCommand();
                while (_performingAction) yield return null;
                
                var inflictStatusEffectsAfter = StartCoroutine(InflictStatus.OnThisUnit
                    (enemy, RateOfInfliction.AfterEveryAction, 1,true));
                
                yield return inflictStatusEffectsAfter;

                if (PartyOrEnemyTeamIsDead || enemy.IsDead) break;
            }
        }
        
        #endregion

        #region EndOfBattle

        private IEnumerator WonBattleSequence()
        {
            yield return new WaitForSeconds(0.5f);
            Logger.Log("yay, you won");
        }

        private IEnumerator LostBattleSequence()
        {
            yield return new WaitForSeconds(0.5f);
            Logger.Log("you lost idiot");
        }
        
        #endregion

        #region Misc

        private static void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) EnemiesForThisBattle.Remove((Enemy) unit);
            Logger.Log($"{unit.characterName} is being removed from battle");
            if (unit.id == CharacterType.Enemy) EnemiesForThisBattle.Remove((Enemy) unit);
            else MembersForThisBattle.Remove((PartyMember) unit);

            if (MembersForThisBattle.Count == 0) allMembersDead = true;
        }

        private void ResetStaticVariables()
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
            roundCount = 0;
        }
        
        #endregion
    }
}