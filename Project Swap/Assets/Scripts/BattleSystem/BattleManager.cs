﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using System.Linq;
using Calculator;
using Characters;
using BattleSystem.Generator;
using Characters.PartyMembers;
using MoreMountains.InventoryEngine;
using StatusEffects;
using Type = Characters.Type;

namespace BattleSystem
{
    // Need to rework the enums. They are not good right now
    public enum BattleState { Start, PartyTurn, EnemyTurn, Won, Lost }
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class BattleManager : MonoBehaviour
    {
        private static BattleState state;
        public delegate void BattleSystemEvent();
        public static event BattleSystemEvent NewRound;
        public static event Action<BattleState> EndOfBattle;

        public static GlobalBattleFuncs _battleFuncs;

        public static InputSystemUIInputModule _inputModule;
        public static InventoryInputManager _inventoryInputManager;
        public static Controls _controls;

        public static List<Enemy> _enemiesForThisBattle = new List<Enemy>();
        public static List<PartyMember> _membersForThisBattle = new List<PartyMember>();
        public static List<UnitBase> _membersAndEnemies = new List<UnitBase>();

        public static bool _choosingOption;
        public static bool _choosingTarget;
        public static bool _performingAction;
        public static bool _endThisMembersTurn;
        public static bool _choosingAbility;
        public static bool _shouldGiveCommand;

        private BattleGenerator generator;

        //private Camera cam;

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

        private bool CancelCondition => _inputModule.cancel.action.triggered && canPressBack;
        private bool canPressBack;

        private int roundCount;


        private void Start()
        {
            DOTween.Init();
            _controls = new Controls();
            _controls.Enable();

            _inputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
            _inventoryInputManager = FindObjectOfType<InventoryInputManager>();

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
            while (!SortingCalculator.isFinished) yield return null;

            foreach (var character in _membersForThisBattle) 
                character.onDeath += RemoveFromBattle;
            
            StartCoroutine(PerformThisRound());
        }

        private IEnumerator PerformThisRound()
        {
            BattleEvent.Trigger(BattleEventType.NewRound);
            NewRound?.Invoke();

            // could be added to new round event
            StartCoroutine(SortingCalculator.SortAndCombine());
            while (!SortingCalculator.isFinished) yield return null;
            
            foreach (var character in from character in _membersAndEnemies
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
            _inventoryInputManager.TargetInventoryContainer = character.inventoryDisplay.GetComponent<CanvasGroup>();
            _inventoryInputManager.TargetInventoryDisplay = character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

            yield return new WaitForSeconds(1);
            character.inventoryDisplay.SetActive(true);

            state = BattleState.PartyTurn;
            character.ResetCommandsAndAP();


            main_menu:
            canPressBack = false;
            character.battleOptionsPanel.ShowBattlePanel();

            while (_choosingOption) yield return null;
            yield return new WaitForSeconds(0.5f);
                
            while (_choosingAbility) {
                canPressBack = true;
                if (CancelCondition) goto main_menu;
                yield return null;
            }

            if (_endThisMembersTurn)
            {
                _endThisMembersTurn = false;
                character.inventoryDisplay.SetActive(false);
                yield break;
            }
                
            ChooseTarget.ForThisMember(character);
            yield return new WaitForSeconds(0.5f);

            while (_choosingTarget)
            {
                canPressBack = true;
                if (CancelCondition) goto main_menu;
                yield return null;
            }
            
            character.CurrentAP -= character.Unit.actionCost;

            var inflictStatusEffectsBefore = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                (character, RateOfInfliction.BeforeEveryAction, 1,true));
            
            yield return inflictStatusEffectsBefore;

            if (!_shouldGiveCommand) _shouldGiveCommand = true;
            else character.GiveCommand();
            while (_performingAction) yield return null;
            

            var inflictStatusEffectsAfter = StartCoroutine(StatusEffectManager.TriggerOnThisUnit
                (character, RateOfInfliction.AfterEveryAction, 1,true));
            
            yield return inflictStatusEffectsAfter;

            if (PartyOrEnemyTeamIsDead || character.IsDead) {
                character.inventoryDisplay.SetActive(false);
                yield break;
            }
            
            if (character.CurrentAP > 0) goto main_menu;
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
            EndOfBattle?.Invoke(BattleState.Won);
            Logger.Log("yay, you won");
        }

        private IEnumerator LostBattleSequence()
        {
            yield return new WaitForSeconds(0.5f);
            EndOfBattle?.Invoke(BattleState.Lost);
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