using System.Collections.Generic;
using System.Linq;
using Characters;
using UnityEngine;

namespace BattleSystem
{
    public static class SortingCalculator
    {
        public static bool SortByInitiative()
        {
            if (BattleManager.Instance.membersAndEnemiesNextTurn.Count > 0)
            {
                BattleManager.Instance.membersAndEnemiesThisTurn = new List<UnitBase>(BattleManager.Instance.membersAndEnemiesNextTurn);
                GetNewListNextTurn();
            }
            
            else
            {
                GetNewList();
                GetNewListNextTurn();
            }
            
            BattleEvents.Trigger(BattleEventType.ThisTurnListCreated);
            BattleEvents.Trigger(BattleEventType.NextTurnListCreated);

            return true;
        }

        private static void GetNewList()
        {
            BattleManager.Instance.membersAndEnemiesThisTurn = new List<UnitBase>();
            BattleManager.Instance.membersAndEnemiesThisTurn = new List<UnitBase>();
            
            foreach (var member in BattleManager.Instance._membersForThisBattle) BattleManager.Instance.membersAndEnemiesThisTurn.Add(member);
            foreach (var enemy in BattleManager.Instance._enemiesForThisBattle) BattleManager.Instance.membersAndEnemiesThisTurn.Add(enemy);
                
            BattleManager.Instance.membersAndEnemiesThisTurn = BattleManager.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(BattleManager.Instance.membersAndEnemiesThisTurn);
                
            BattleManager.Instance.membersAndEnemiesThisTurn = BattleManager.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
        }

        private static void GetNewListNextTurn()
        {
            BattleManager.Instance.membersAndEnemiesNextTurn = new List<UnitBase>();
            
            foreach (var member in BattleManager.Instance._membersForThisBattle) BattleManager.Instance.membersAndEnemiesNextTurn.Add(member);
            foreach (var enemy in BattleManager.Instance._enemiesForThisBattle) BattleManager.Instance.membersAndEnemiesNextTurn.Add(enemy);
                
            BattleManager.Instance.membersAndEnemiesNextTurn = BattleManager.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(BattleManager.Instance.membersAndEnemiesNextTurn);
                
            BattleManager.Instance.membersAndEnemiesNextTurn = BattleManager.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
        }
        
        private static void GetFinalInitValues(IEnumerable<UnitBase> list)
        {
            var minModifier = 1.8f;
            foreach (var t in list)
            {
                t.Unit.initModifier = Random.Range(minModifier, 2.0f);
                t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier);
                minModifier -= 0.1f;
            }
        }

        public static void ResortThisTurnOrder()
        {
            BattleManager.Instance.membersAndEnemiesThisTurn.ForEach(t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            BattleManager.Instance.membersAndEnemiesThisTurn = BattleManager.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();

            BattleManager.Instance.membersAndEnemiesThisTurn.Remove(BattleManager.Instance.activeUnit);
            BattleManager.Instance.membersAndEnemiesThisTurn.Insert(0, BattleManager.Instance.activeUnit);
            
            BattleEvents.Trigger(BattleEventType.ThisTurnListCreated);
        }

        public static void ResortNextTurnOrder()
        {
            BattleManager.Instance.membersAndEnemiesNextTurn.ForEach(t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            BattleManager.Instance.membersAndEnemiesNextTurn = BattleManager.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
            
            BattleEvents.Trigger(BattleEventType.NextTurnListCreated);
        }
    }
}