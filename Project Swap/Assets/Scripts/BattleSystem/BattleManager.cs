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
using Type = Characters.Type;

namespace BattleSystem
{
    // Need to rework the enums. They are not good right now
    public enum BattleState { Start, PartyTurn, EnemyTurn, Won, Lost }
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class BattleManager : MonoBehaviour
    {
        private static BattleState state;
        public static GlobalBattleFuncs _battleFuncs;

        public static List<Enemy> _enemiesForThisBattle = new List<Enemy>();
        public static List<PartyMember> _membersForThisBattle = new List<PartyMember>();
        public static List<UnitBase> _membersAndEnemies = new List<UnitBase>();

        public static bool _choosingOption;
        public static bool _choosingTarget;
        public static bool _performingAction;
        public static bool _endThisMembersTurn;
        public static bool _choosingAbility;
        public static bool _shouldGiveCommand;
        
        private static bool allMembersDead;
        private static bool allEnemiesDead;
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

        private int roundCount;
        
        private void Start()
        {
            generator = GetComponent<BattleGenerator>();
            _battleFuncs = GetComponent<GlobalBattleFuncs>();

            ResetStaticVariables();
            StartCoroutine(SetupBattle());
            state = BattleState.Start;
        }

        private IEnumerator SetupBattle()
        {
            yield return new WaitWhile(generator.SetupBattle);

            foreach (var partyMember in _membersForThisBattle) {
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetEnemySelectables);
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetPartySelectables);
            }

            StartCoroutine(SortingCalculator.SortAndCombine());
            while (!SortingCalculator._isFinished) yield return null;

            foreach (var character in _membersForThisBattle) 
                character.onDeath += RemoveFromBattle;
            
            StartCoroutine(PerformThisRound());
        }

        private IEnumerator PerformThisRound()
        {
            BattleEvents.Trigger(BattleEventType.NewRound);

            // could be added to new round event
            StartCoroutine(SortingCalculator.SortAndCombine());
            while (!SortingCalculator._isFinished) yield return null;
            
            foreach (var character in from character in _membersAndEnemies
                let checkMemberStatus = character.CheckUnitStatus() where checkMemberStatus select character)
            {
                if (PartyOrEnemyTeamIsDead) break;
                
                var inflictStatusEffects = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                    (character, RateOfInfliction.EveryTurn, 1,true));
                
                yield return inflictStatusEffects;

                if (PartyOrEnemyTeamIsDead || character.IsDead) break;
                
                var round = StartCoroutine(character.id == Type.PartyMember?
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
            BattleInputManager._inventoryInputManager.TargetInventoryContainer = character.inventoryDisplay.GetComponent<CanvasGroup>();
            BattleInputManager._inventoryInputManager.TargetInventoryDisplay = character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

            yield return new WaitForSeconds(0.5f);
            character.inventoryDisplay.SetActive(true);

            state = BattleState.PartyTurn;
            character.ResetCommandsAndAP();
            
            main_menu:
            CharacterEvents.Trigger(CEventType.CharacterTurn, character);
            BattleInputManager._canPressBack = false;
            character.battleOptionsPanel.ShowBattlePanel();

            while (_choosingOption) yield return null;
            yield return new WaitForSeconds(0.5f);
                
            while (_choosingAbility) {
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

            var inflictStatusEffectsBefore = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                (character, RateOfInfliction.BeforeEveryAction, 1,true));
            
            yield return inflictStatusEffectsBefore;

            if (!_shouldGiveCommand) _shouldGiveCommand = true;
            else {
                CharacterEvents.Trigger(CEventType.CharacterAttacking, character);
                yield return new WaitForSeconds(0.5f);
                character.GiveCommand();
            }
            while (_performingAction) yield return null;
            
            var inflictStatusEffectsAfter = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                (character, RateOfInfliction.AfterEveryAction, 1,true));
            
            yield return inflictStatusEffectsAfter;

            if (PartyOrEnemyTeamIsDead || character.IsDead) {
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
            enemy.ResetCommandsAndAP();

            while (enemy.CurrentAP > 0)
            {
                if (enemy.IsDead) break;

                var shouldAttack = enemy.SetAI();
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;
                
                var inflictStatusEffectsBefore = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                    (enemy, RateOfInfliction.BeforeEveryAction, 1,true));
                
                yield return inflictStatusEffectsBefore;

                if (!_shouldGiveCommand) _shouldGiveCommand = true;
                else enemy.GiveCommand();
                while (_performingAction) yield return null;
                
                var inflictStatusEffectsAfter = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                    (enemy, RateOfInfliction.AfterEveryAction, 1,true));
                
                yield return inflictStatusEffectsAfter;

                if (PartyOrEnemyTeamIsDead || enemy.IsDead) break;
            }
        }

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

        private static void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == Type.Enemy) _enemiesForThisBattle.Remove((Enemy) unit);
            Logger.Log($"{unit.characterName} is being removed from battle");
            if (unit.id == Type.Enemy) _enemiesForThisBattle.Remove((Enemy) unit);
            else _membersForThisBattle.Remove((PartyMember) unit);

            if (_membersForThisBattle.Count == 0) allMembersDead = true;
        }

        private void ResetStaticVariables()
        {
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
    }
}