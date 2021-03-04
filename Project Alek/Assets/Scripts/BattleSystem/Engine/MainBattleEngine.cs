﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Kryz.CharacterStats;
using MoreMountains.InventoryEngine;
using ScriptableObjectArchitecture;
using SceneManagement;
using Characters;
using Audio;
using MEC;

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "Main Battle Engine", menuName = "Battle Engine/Main Battle Engine")]
    public class MainBattleEngine : BattleEngine, IGameEventListener<BattleEvent>, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        public override void OnStart()
        {
            TimeManager.ResumeTime();
            
            ResetFields();

            inventoryInputManager = FindObjectOfType<InventoryInputManager>();
            generator = FindObjectOfType<BattleGenerator>();
            _sortingCalculator = FindObjectOfType<SortingCalculator>();
            
            BattleEvents.NormalEvent.AddListener(this);
            BattleEvents.EndOfTurnEvent.AddListener(this);
            BattleEvents.SkipTurnEvent.AddListener(this);

            canGiveCommand = true;
            roundCount = 0;

            AudioController.PlayAudio(CommonAudioTypes.MainBattleTheme, true, 2);
            PartyManager.Order();
            Timing.RunCoroutine(SetupBattle());
        }

        public override void OnDisabled()
        {
            membersForThisBattle.ForEach(m => m.onDeath -= RemoveFromBattle);
            enemiesForThisBattle.ForEach(e => e.onDeath -= RemoveFromBattle);
            
            PartyManager.Members.ForEach(m => m.onRevival -= AddToBattle);

            BattleEvents.NormalEvent.RemoveListener(this);
            BattleEvents.EndOfTurnEvent.RemoveListener(this);
            BattleEvents.SkipTurnEvent.RemoveListener(this);
            
            ResetFields();
        }

        protected override IEnumerator<float> SetupBattle()
        {
            generator.SetupBattle();

            SelectableObjectManager.SetEnemySelectables();
            SelectableObjectManager.SetPartySelectables();
      
            membersForThisBattle.ForEach(m => m.Unit.GetComponent<ChooseTarget>().Setup());
            enemiesForThisBattle.ForEach(e => e.Unit.GetComponent<ChooseTarget>().Setup());

            membersForThisBattle.ForEach(m => { m.onDeath += RemoveFromBattle; m.onRevival += AddToBattle; });
            enemiesForThisBattle.ForEach(e => e.onDeath += RemoveFromBattle);

            BattleEvents.SetupCompleteEvent.Raise();
            membersForThisBattle.ForEach(m => m.LevelUpEvent += 
                battleResults.GetComponent<BattleResultsUI>().Enqueue);
            
            yield return Timing.WaitForSeconds(1);
            Timing.RunCoroutine(GetNextTurn());
        }

        protected override IEnumerator<float> GetNextTurn()
        {
            if (PartyOrEnemyTeamIsDead) { EndOfBattle(); yield break; }

            yield return Timing.WaitForSeconds(0.25f);
            
            if (NewRoundCondition) { BattleEvents.NormalEvent.Raise(BattleEvent.NewRound);
                yield return Timing.WaitUntilTrue(SortingCalculator.SortByInitiative); }

            while (NextUnitInTurnIsDead) membersAndEnemiesThisTurn.Remove(membersAndEnemiesThisTurn[0]);

            var character = membersAndEnemiesThisTurn[0];
            
            yield return Timing.WaitUntilDone(character.InflictStatus(Rate.EveryTurn, 1, true));
                
            if (PartyOrEnemyTeamIsDead) EndOfBattle();
            else if (!character.GetStatus()) BattleEvents.SkipTurnEvent.Raise(character, BattleEvents.SkipTurnEvent);
            else Timing.RunCoroutine((CharacterType)character.id == CharacterType.PartyMember
                ? ThisPlayerTurn((PartyMember) character)
                : ThisEnemyTurn((Enemy) character));
        }

        protected override IEnumerator<float> ThisPlayerTurn(PartyMember character)
        {
            activeUnit = character;
            inventoryInputManager.TargetInventoryContainer = character.Container;
            inventoryInputManager.TargetInventoryDisplay = character.InventoryDisplay;
            character.inventoryDisplay.SetActive(true);
            character.InventoryDisplay.RedrawInventoryDisplay();
            
            character.ReplenishAP();
            var battlePanel = (BattleOptionsPanel) character.battleOptionsPanel;

            #region Main Menu
            
            main_menu:
            EventSystem.current.sendNavigationEvents = true;
            BattleEvents.CharacterTurnEvent.Raise(character, BattleEvents.CharacterTurnEvent);
            BattleInput._canPressBack = false;
            usingItem = false;
            battlePanel.ShowBattlePanel();
            
            #endregion

            #region Choosing Option

            yield return Timing.WaitUntilFalse(() => choosingOption);
            
            yield return Timing.WaitForOneFrame;
            while (choosingAbility) 
            {
                BattleInput._canPressBack = true;
                abilityMenuLast = true;
                if (BattleInput.CancelCondition)
                {
                    abilityMenuLast = false;
                    goto main_menu;
                }
                yield return Timing.WaitForOneFrame;
            }
            
            while (choosingSpell) 
            {
                BattleInput._canPressBack = true;
                spellMenuLast = true;
                if (BattleInput.CancelCondition)
                {
                    spellMenuLast = false;
                    goto main_menu;
                }
                yield return Timing.WaitForOneFrame;
            }
            
            #endregion

            if (endThisMembersTurn) goto end_of_turn;

            #region Choosing Target
            
            yield return Timing.WaitForOneFrame;
            if (skipChooseTarget) skipChooseTarget = false;
            else BattleEvents.ChooseTargetEvent.Raise(character, BattleEvents.ChooseTargetEvent);

            while (choosingTarget)
            {
                yield return Timing.WaitForOneFrame;
                BattleInput._canPressBack = true;
                if (BattleInput.CancelCondition) goto main_menu;
            }
            
            abilityMenuLast = false;
            spellMenuLast = false;
            
            #endregion

            #region Performing Action

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
            else
            {
                var attacker = character.Unit.hasSummon ? (UnitBase) character.Unit.currentSummon : character;
                BattleEvents.CharacterAttackEvent.Raise(attacker, BattleEvents.CharacterAttackEvent);
                BattleEvents.CommandEvent.Raise(attacker, BattleEvents.CommandEvent);
            }

            yield return Timing.WaitUntilFalse(() => performingAction);

            if (endTurnAfterCommand) goto end_of_turn;
            yield return Timing.WaitUntilDone(character.InflictStatus
                (Rate.AfterEveryAction, 0.5f, true));
            
            #endregion

            skip_command_execution:
            if (PartyOrEnemyTeamIsDead) goto end_of_turn;
            if (character.GetStatus() && character.CurrentAP > 0) goto main_menu;
            
            end_of_turn:
            endThisMembersTurn = false;
            endTurnAfterCommand = false;
            BattleEvents.EndOfTurnEvent.Raise(character, BattleEvents.EndOfTurnEvent);
            character.inventoryDisplay.SetActive(false);
        }

        protected override IEnumerator<float> ThisEnemyTurn(Enemy enemy)
        {
            activeUnit = enemy;

            BattleEvents.EnemyTurnEvent.Raise(enemy, BattleEvents.EnemyTurnEvent);
            enemy.ReplenishAP();

            while (enemy.GetStatus() && enemy.CurrentAP > 0)
            {
                var shouldAttack = enemy.SetAI(membersForThisBattle);
                if (!shouldAttack) break;

                enemy.CurrentAP -= enemy.Unit.actionCost;

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.BeforeEveryAction, 0.5f, true));

                if (!canGiveCommand) canGiveCommand = true;
                else { BattleEvents.CharacterAttackEvent.Raise(enemy, BattleEvents.CharacterAttackEvent);
                    BattleEvents.CommandEvent.Raise(enemy, BattleEvents.CommandEvent); }

                yield return Timing.WaitUntilFalse(() => performingAction);

                yield return Timing.WaitUntilDone(enemy.InflictStatus
                    (Rate.AfterEveryAction, 0.5f, true));

                if (PartyOrEnemyTeamIsDead) break;
                if (endThisMembersTurn) break;
                
                yield return Timing.WaitForSeconds(0.25f);
            }

            endThisMembersTurn = false;
            BattleEvents.EndOfTurnEvent.Raise(enemy, BattleEvents.EndOfTurnEvent);
        }

        protected override IEnumerator<float> WonBattleSequence()
        {
            AudioController.StopAudio(CommonAudioTypes.MainBattleTheme, true, 2);

            yield return Timing.WaitForSeconds(2);
            
            AudioController.PlayAudio(CommonAudioTypes.Victory);
            AudioController.PlayAudio(CommonAudioTypes.VictoryThemeBattle, true);
            
            membersForThisBattle.ForEach(member => member.Unit.anim.SetTrigger(AnimationHandler.VictoryTrigger));
            
            yield return Timing.WaitForSeconds(2f);
            battleResults.SetActive(true);
            var battleResultsUI = battleResults.GetComponent<BattleResultsUI>();
            battleResultsUI.DisableUI();
            
            foreach (var member in membersForThisBattle)
            {
                member.ResetLevelUpAmount();
                
                var totalXp = expGivers.Sum(giver =>
                    giver.CalculateExperience(member.level, member));
                
                var totalClassXp = expGivers.Sum(giver =>
                    giver.CalculateExperience(member.currentClass.level, member.currentClass));

                member.BattleExpReceived = totalXp;
                
                member.AdvanceTowardsNextLevel(totalXp);
                member.currentClass.AdvanceTowardsNextLevel(totalClassXp);
            }
            
            yield return Timing.WaitUntilTrue(() => BattleInput._controls.Battle.Confirm.triggered);
            yield return Timing.WaitForSeconds(0.75f);
            yield return Timing.WaitUntilTrue(() => BattleInput._controls.Battle.Confirm.triggered);
            
            Timing.RunCoroutine(battleResultsUI.ShowLevelUps());
            yield return Timing.WaitForSeconds(0.3f);
            yield return Timing.WaitUntilFalse(() => battleResultsUI.showingLevelUps);
            
            SceneLoadManager.LoadOverworld();
            
            membersForThisBattle.ForEach(m =>
            {
                m.currentClass.statsToIncrease = new List<CharacterStat>();
            });
        }

        protected override IEnumerator<float> LostBattleSequence()
        {
            AudioController.StopAudio(CommonAudioTypes.MainBattleTheme, true, 1);
            yield return Timing.WaitForSeconds(1);

            gameOverCanvas.SetActive(true);
        }

        public override IEnumerator<float> FleeBattleSequence()
        {
            //StopAllCoroutines();
            BattleEvents.FleeBattleEvent.Raise();

            yield return Timing.WaitForSeconds(0.5f);
            
            foreach (var member in membersForThisBattle.Where(member => !member.IsDead))
            {
                var position = member.Unit.transform.position;
                var newPosition = new Vector3(position.x, position.y, position.z - 15);
                
                member.Unit.EmptyAnimations();
                
                member.Unit.anim.SetInteger(AnimationHandler.AnimState, 0);
                member.Unit.anim.SetFloat(AnimationHandler.HorizontalHash, -1);
                member.Unit.anim.SetBool(AnimationHandler.IsWalkingHash, true);
                member.Unit.anim.SetBool(AnimationHandler.IsRunningHash, true);

                member.Unit.transform.DOMove(newPosition, 2);
            }
            yield return Timing.WaitForSeconds(1.5f);
            SceneLoadManager.LoadOverworld();
        }

        protected override void EndOfBattle()
        {
            if (AllEnemiesDead) BattleEvents.NormalEvent.Raise(BattleEvent.WonBattle);
            else if (AllMembersDead) BattleEvents.NormalEvent.Raise(BattleEvent.LostBattle);
        }

        protected override void RemoveFromTurn(UnitBase unit) => membersAndEnemiesThisTurn.Remove(unit);

        protected override void RemoveFromBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) enemiesForThisBattle.Remove((Enemy) unit);
            else membersForThisBattle.Remove((PartyMember) unit);
            
            membersAndEnemiesThisTurn.Remove(unit);
            membersAndEnemiesNextTurn.Remove(unit);
     
            BattleEvents.DeathEvent.Raise(unit, BattleEvents.DeathEvent);
            
            _sortingCalculator.ResortThisTurnOrder();
            _sortingCalculator.ResortNextTurnOrder();
            
            unit.onDeath -= RemoveFromBattle;
        }

        protected override void AddToBattle(UnitBase unit)
        {
            if (unit.id == CharacterType.Enemy) enemiesForThisBattle.Add((Enemy) unit);
            else membersForThisBattle.Add((PartyMember) unit);
            
            if (!unit.Unit.hasPerformedTurn) membersAndEnemiesThisTurn.Add(unit);
            membersAndEnemiesNextTurn.Add(unit);
            
            _sortingCalculator.ResortThisTurnOrder();
            _sortingCalculator.ResortNextTurnOrder();

            unit.onDeath += RemoveFromBattle;
        }

        public void OnEventRaised(BattleEvent value)
        {
            switch (value)
            {
                case BattleEvent.WonBattle: Timing.RunCoroutine(WonBattleSequence());
                    break;
                case BattleEvent.LostBattle: Timing.RunCoroutine(LostBattleSequence());
                    break;
            }
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 != BattleEvents.EndOfTurnEvent && value2 != BattleEvents.SkipTurnEvent) return;
            RemoveFromTurn(value1);
            Timing.RunCoroutine(GetNextTurn());
        }
    }
}