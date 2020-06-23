using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Calculator;
using Characters;
using Animations;
using StatusEffects;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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
            allMembersDead,
            allEnemiesDead,
            partyHasChosenSwap;

        public static BattleState state;
        public static UnityEvent newRound;
        public static InputSystemUIInputModule inputModule;
        
        public BattleOptionsPanel battlePanel;

        private BattleGenerator generator;
        private Camera cam;
        
        private bool canPressBack;
        private bool performingSwap;
        private bool performingRound;
        
        private int roundCount; // use this for bonuses based on how many rounds it took

        private void Start()
        {
            newRound = new UnityEvent();
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

            foreach (var partyMember in membersForThisBattle)
                yield return new WaitUntil(partyMember.battlePanel.GetComponent<MenuController>().SetSelectables);
            
            StartCoroutine(PerformThisRound());
        }

        private IEnumerator PerformThisRound()
        {
            newRound.Invoke();
            yield return new WaitForSeconds(1);

            StartCoroutine(SortingCalculator.SortAndCombine());
            while (!SortingCalculator.isFinished) yield return null;
            
            foreach (var character in from character in membersAndEnemies
                let checkMemberStatus = character.CheckUnitStatus() where checkMemberStatus select character)
            {
                if (allMembersDead || allEnemiesDead) break;
                
                foreach (var statusEffect in from statusEffect in character.unit.statusEffects
                    where statusEffect.rateOfInfliction == RateOfInfliction.EveryTurn 
                    select statusEffect) statusEffect.InflictStatus(character.unit);
                
                if (allMembersDead || allEnemiesDead) break;
                
                StartCoroutine(character.unit.id == 1 ? ThisPlayerTurn((PartyMember) character) : ThisEnemyTurn((Enemy) character));
                while (performingRound) yield return null;
            }
            
            if (allEnemiesDead) state = BattleState.Won;
            else if (allMembersDead) state = BattleState.Lost;

            StartCoroutine(ExecuteSwap());
            while (performingSwap) yield return null;

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
            performingRound = true;
            state = BattleState.PartyTurn;
            character.ResetCommandsAndAP();
            
            main_menu:
            canPressBack = false;
            battlePanel.ShowBattlePanel(character);

            while (choosingOption) yield return null;
            yield return new WaitForSeconds(0.5f);
                
            while (choosingAbility)
            {
                canPressBack = true;
                if (inputModule.cancel.action.triggered && canPressBack) goto main_menu;
                yield return null;
            }

            if (endThisMembersTurn)
            {
                endThisMembersTurn = false;
                performingRound = false;
                yield break;
            }
                
            ChooseTarget.GetPartyMember(character);
            yield return new WaitForSeconds(0.5f);

            while (choosingTarget)
            {
                canPressBack = true;
                if (inputModule.cancel.action.triggered && canPressBack) goto main_menu;
                yield return null;
            }
                
            character.unit.currentAP -= character.unit.actionCost;
            character.unit.actionPointAnim.SetInteger(AnimationHandler.APVal, character.unit.currentAP);

            if (partyHasChosenSwap) partyHasChosenSwap = false;
            else character.GiveCommand(false);
            while (performingAction) yield return null;

            character.ResetAnimationStates();
            
            foreach (var statusEffect in from statusEffect in character.unit.statusEffects
                where statusEffect.rateOfInfliction == RateOfInfliction.EveryAction
                select statusEffect) statusEffect.InflictStatus(character.unit);
            
            if (allMembersDead || allEnemiesDead || character.unit.status == Status.Dead) { performingRound = false; yield break; }
            if (character.unit.currentAP > 0) goto main_menu;
            performingRound = false;
        }

        private IEnumerator ThisEnemyTurn(Enemy enemy)
        {
            performingRound = true;
            state = BattleState.EnemyTurn;
            enemy.ResetCommandsAndAP();

            while (enemy.unit.currentAP > 0)
            {
                yield return new WaitUntil(enemy.SetAI);
                
                enemy.unit.currentAP -= enemy.unit.actionCost;
                
                enemy.GiveCommand(false);
                while (performingAction) yield return null;

                foreach (var statusEffect in from statusEffect in enemy.unit.statusEffects
                    where statusEffect.rateOfInfliction == RateOfInfliction.EveryAction select statusEffect)
                {
                    statusEffect.InflictStatus(enemy.unit);
                    yield return new WaitForSeconds(1.5f);
                    if (enemy.unit.status == Status.Dead) break;
                }
                
                if (enemy.unit.status == Status.Dead) break;

                if (!allMembersDead && !allEnemiesDead) continue;
                performingRound = false; break;
            }
            performingRound = false;
        }

        private IEnumerator ExecuteSwap()
        {
            if (partyMemberWhoChoseSwap != null && partyMemberWhoChoseSwap.unit.isSwapping)
            {
                performingSwap = true;
                partyMemberWhoChoseSwap.unit.currentTarget = partySwapTarget;
                partyMemberWhoChoseSwap.GiveCommand(true);
                while (performingAction) yield return null;
            }
            performingSwap = false;
        }

        private IEnumerator WonBattleSequence()
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("yay, you won");
        }

        private IEnumerator LostBattleSequence()
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("you lost idiot");
        }

        public static void RemoveFromBattle(UnitBase unit, int id)
        {
            if (id == 0) enemiesForThisBattle.Remove((Enemy) unit);
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
            roundCount = 0;
        }
    }
}