using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using System.Linq;
using Calculator;
using Characters;
using Animations;
using BattleSystem.Generator;
using Characters.PartyMembers;
using StatusEffects;
using Type = Characters.Type;

namespace BattleSystem
{
    // Need to rework the enums. They are not good right now
    public enum BattleState { Start, PartyTurn, EnemyTurn, Won, Lost }
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class BattleManager : MonoBehaviour
    {
        public static BattleState state;
        public delegate void BattleSystemEvent();
        public static event BattleSystemEvent NewRound;
        public static GlobalBattleFuncs battleFuncs;

        public static InputSystemUIInputModule inputModule;
        public static Controls controls;

        public static List<Enemy> enemiesForThisBattle = new List<Enemy>();
        public static List<PartyMember> membersForThisBattle = new List<PartyMember>();
        public static List<UnitBase> membersAndEnemies = new List<UnitBase>();

        public static bool choosingOption;
        public static bool choosingTarget;
        public static bool performingAction;
        public static bool endThisMembersTurn;
        public static bool choosingAbility;
        public static bool shouldGiveCommand;

        public BattleOptionsPanel battlePanel;
        private BattleGenerator generator;
        private Camera cam;

        private static bool allMembersDead;
        private static bool allEnemiesDead;
        private static bool PartyOrEnemyTeamIsDead {
            get
            {
                if (allEnemiesDead) state = BattleState.Won;
                else if (allMembersDead) state = BattleState.Lost;
                return allMembersDead || allEnemiesDead;
            }
        }

        private bool CancelCondition => inputModule.cancel.action.triggered && canPressBack;
        private bool canPressBack;

        private int roundCount; // use this for bonuses based on how many rounds it took

        private void Start()
        {
            DOTween.Init();
            controls = new Controls();
            controls.Enable();

            inputModule = GameObject.FindGameObjectWithTag
                ("EventSystem").GetComponent<InputSystemUIInputModule>();

            generator = GetComponent<BattleGenerator>();
            battleFuncs = GetComponent<GlobalBattleFuncs>();

            ResetStaticVariables();
            StartCoroutine(SetupBattle());
            state = BattleState.Start;
        }

        private IEnumerator SetupBattle()
        {
            yield return new WaitWhile(generator.SetupBattle);

            foreach (var partyMember in membersForThisBattle) {
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetEnemySelectables);
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetPartySelectables);
            }

            StartCoroutine(PerformThisRound());
        }

        private IEnumerator PerformThisRound()
        {
            NewRound?.Invoke();
            
            StartCoroutine(SortingCalculator.SortAndCombine());
            while (!SortingCalculator.isFinished) yield return null;
            
            foreach (var character in from character in membersAndEnemies
                let checkMemberStatus = character.CheckUnitStatus() where checkMemberStatus select character)
            {
                if (PartyOrEnemyTeamIsDead) break;

                var inflictStatusEffects = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                    (character, RateOfInfliction.EveryTurn, 1,true));
                
                yield return inflictStatusEffects;
                
                if (PartyOrEnemyTeamIsDead || character.IsDead) break;
                
                var round = StartCoroutine(character.id == Type.PartyMember ?
                    ThisPlayerTurn((PartyMember) character) : ThisEnemyTurn((Enemy) character));
                
                yield return round;
            }

            switch (state)
            {
                // Could make the sequences events
                case BattleState.Won:
                    StartCoroutine(WonBattleSequence());
                    break;
                case BattleState.Lost:
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
            state = BattleState.PartyTurn;
            character.ResetCommandsAndAP();

            main_menu:
            canPressBack = false;
            battlePanel.ShowBattlePanel(character);

            while (choosingOption) yield return null;
            yield return new WaitForSeconds(0.5f);
                
            while (choosingAbility) {
                canPressBack = true;
                if (CancelCondition) goto main_menu;
                yield return null;
            }

            if (endThisMembersTurn) { endThisMembersTurn = false; yield break; }
                
            ChooseTarget.ForThisMember(character);
            yield return new WaitForSeconds(0.5f);

            while (choosingTarget) {
                canPressBack = true;
                if (CancelCondition) goto main_menu;
                yield return null;
            }
                
            character.Unit.currentAP -= character.Unit.actionCost;
            character.Unit.actionPointAnim.SetInteger(AnimationHandler.APVal, character.Unit.currentAP);

            var inflictStatusEffectsBefore = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                (character, RateOfInfliction.BeforeEveryAction, 1,true));

            yield return inflictStatusEffectsBefore;
            
            if (!shouldGiveCommand) shouldGiveCommand = true;
            else character.GiveCommand();
            while (performingAction) yield return null;

            character.ResetAnimationStates();

            var inflictStatusEffectsAfter = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                (character, RateOfInfliction.AfterEveryAction, 1,true));
            
            yield return inflictStatusEffectsAfter;

            if (PartyOrEnemyTeamIsDead || character.IsDead) yield break;
            if (character.Unit.currentAP > 0) goto main_menu;
        }

        private IEnumerator ThisEnemyTurn(Enemy enemy)
        {
            state = BattleState.EnemyTurn;
            enemy.ResetCommandsAndAP();

            while (enemy.Unit.currentAP > 0)
            {
                if (enemy.IsDead) break;

                var shouldAttack = enemy.SetAI();
                if (!shouldAttack) break;

                enemy.Unit.currentAP -= enemy.Unit.actionCost;
                
                var inflictStatusEffectsBefore = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                    (enemy, RateOfInfliction.BeforeEveryAction, 1,true));

                yield return inflictStatusEffectsBefore;

                if (!shouldGiveCommand) shouldGiveCommand = true;
                else enemy.GiveCommand();
                while (performingAction) yield return null;
                
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

        public static void RemoveFromBattle(UnitBase unit, Type id)
        {
            if (id == Type.Enemy) enemiesForThisBattle.Remove((Enemy) unit);
            else membersForThisBattle.Remove((PartyMember) unit);

            if (membersForThisBattle.Count == 0) allMembersDead = true;
            else if (enemiesForThisBattle.Count == 0) allEnemiesDead = true;
        }

        private void ResetStaticVariables()
        {
            choosingOption = false;
            choosingTarget = false;
            performingAction = false;
            endThisMembersTurn = false;
            choosingAbility = false;
            allMembersDead = false;
            allEnemiesDead = false;
            shouldGiveCommand = true;
            roundCount = 0;
        }
    }
}