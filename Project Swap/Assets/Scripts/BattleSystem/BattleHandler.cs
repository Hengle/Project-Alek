using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using System.Linq;
using Calculator;
using Characters;
using Animations;
using BattleSystem.Generator;
using Characters.PartyMembers;
using StatusEffects;
using UnityEngine.Events;
using DG.Tweening;
using Type = Characters.Type;

namespace BattleSystem
{
    // Need to rework the enums. They are not good right now
    public enum BattleState { Start, PartyTurn, EnemyTurn, Won, Lost }
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class BattleHandler : MonoBehaviour
    {
        public static GlobalBattleFuncs battleFuncs;

        public static List<Enemy> enemiesForThisBattle = new List<Enemy>();
        public static List<PartyMember> membersForThisBattle = new List<PartyMember>();
        public static List<UnitBase> membersAndEnemies = new List<UnitBase>();

        public static UnitBase partyMemberWhoChoseSwap;
        public static Unit partySwapTarget;

        public static bool
            choosingOption,
            choosingTarget,
            performingAction,
            endThisMembersTurn,
            choosingAbility,
            shouldGiveCommand,
            partyHasChosenSwap;
        
        public static BattleState state;
        public static UnityEvent newRound;
        public static InputSystemUIInputModule inputModule;
        public static Controls controls;

        public BattleOptionsPanel battlePanel;
        
        private BattleGenerator generator;
        private Camera cam;

        private static bool allMembersDead, allEnemiesDead;
        private bool canPressBack;
        private bool CancelCondition => inputModule.cancel.action.triggered && canPressBack;
        private static bool PartyOrEnemyTeamIsDead {
            get
            {
                if (allEnemiesDead) state = BattleState.Won;
                else if (allMembersDead) state = BattleState.Lost;
                return allMembersDead || allEnemiesDead;
            }
        }

        private int roundCount; // use this for bonuses based on how many rounds it took

        private void Start()
        {
            DOTween.Init();
            newRound = new UnityEvent();
            controls = new Controls();
            controls.Enable();

            inputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();

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
            Logger.Log("Round: " + roundCount);
            newRound.Invoke();
            partyHasChosenSwap = false;
            yield return new WaitForSeconds(1);

            StartCoroutine(SortingCalculator.SortAndCombine());
            while (!SortingCalculator.isFinished) yield return null;
            
            foreach (var character in from character in membersAndEnemies
                let checkMemberStatus = character.CheckUnitStatus() where checkMemberStatus select character)
            {
                if (PartyOrEnemyTeamIsDead) break;

                var inflictStatusEffects = StartCoroutine
                    (StatusEffectManager.Trigger
                    (character.unit, RateOfInfliction.EveryTurn, 1,true));
                
                yield return inflictStatusEffects;
                
                if (PartyOrEnemyTeamIsDead || character.unit.status == Status.Dead) break;
                
                var round = StartCoroutine(character.unit.id == Type.PartyMember ?
                    ThisPlayerTurn((PartyMember) character) : ThisEnemyTurn((Enemy) character));
                
                yield return round;
            }

            var swap = StartCoroutine(ExecuteSwap());
            yield return swap;

            switch (state)
            {
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
                
            character.unit.currentAP -= character.unit.actionCost;
            character.unit.actionPointAnim.SetInteger(AnimationHandler.APVal, character.unit.currentAP);

            if (!shouldGiveCommand) shouldGiveCommand = true;
            else character.GiveCommand();
            while (performingAction) yield return null;

            character.ResetAnimationStates();

            var inflictStatusEffects = StartCoroutine
                (StatusEffectManager.Trigger
                (character.unit, RateOfInfliction.EveryAction, 1,true));
            
            yield return inflictStatusEffects;

            if (PartyOrEnemyTeamIsDead || character.unit.status == Status.Dead) yield break;
            if (character.unit.currentAP > 0) goto main_menu;
        }

        private IEnumerator ThisEnemyTurn(Enemy enemy)
        {
            state = BattleState.EnemyTurn;
            enemy.ResetCommandsAndAP();

            while (enemy.unit.currentAP > 0)
            {
                if (enemy.unit.status == Status.Dead) break;

                var shouldAttack = enemy.SetAI();
                if (!shouldAttack) break;

                enemy.unit.currentAP -= enemy.unit.actionCost;

                enemy.GiveCommand();
                while (performingAction) yield return null;
                
                var inflictStatusEffects = StartCoroutine
                    (StatusEffectManager.Trigger
                    (enemy.unit, RateOfInfliction.EveryAction, 1,true));
                
                yield return inflictStatusEffects;
                
                if (PartyOrEnemyTeamIsDead || enemy.unit.status == Status.Dead) break;
            }
        }

        private static IEnumerator ExecuteSwap()
        {
            if (partyMemberWhoChoseSwap == null || !partyMemberWhoChoseSwap.unit.isSwapping) yield break;
            
            partyMemberWhoChoseSwap.unit.isSwapping = false;
            partyMemberWhoChoseSwap.unit.currentTarget = partySwapTarget;
            partyMemberWhoChoseSwap.unit.currentTarget = partyMemberWhoChoseSwap.CheckTargetStatus
                (partyMemberWhoChoseSwap.unit.currentTarget);

            var memberWhoChoseSwap = partyMemberWhoChoseSwap.unit.spriteParentObject;
            var target = partyMemberWhoChoseSwap.unit.currentTarget.spriteParentObject.transform;

            yield return memberWhoChoseSwap.transform.SwapPosition(target, 30);
            partyMemberWhoChoseSwap.unit.characterPanelRef.SwapSiblingIndex(partyMemberWhoChoseSwap.unit.currentTarget.characterPanelRef);
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
            partyHasChosenSwap = false;
            shouldGiveCommand = true;
            roundCount = 0;
        }
    }
}