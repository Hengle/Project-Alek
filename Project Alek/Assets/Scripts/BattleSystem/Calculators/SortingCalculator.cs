using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace BattleSystem.Calculators
{
    public class SortingCalculator : MonoBehaviour
    {
        public static bool SortByInitiative()
        {
            if (BattleEngine.Instance.membersAndEnemiesNextTurn.Count > 0)
            {
                BattleEngine.Instance.membersAndEnemiesThisTurn = new List<UnitBase>(BattleEngine.Instance.membersAndEnemiesNextTurn);
                GetNewListNextTurn();
            }
            
            else
            {
                GetNewList();
                GetNewListNextTurn();
            }
            
            BattleEvents.Instance.thisTurnListCreatedEvent.Raise();
            BattleEvents.Instance.nextTurnListCreatedEvent.Raise();

            return true;
        }

        private static void GetNewList()
        {
            BattleEngine.Instance.membersAndEnemiesThisTurn = new List<UnitBase>();

            foreach (var member in BattleEngine.Instance._membersForThisBattle) BattleEngine.Instance.membersAndEnemiesThisTurn.Add(member);
            foreach (var enemy in BattleEngine.Instance._enemiesForThisBattle) BattleEngine.Instance.membersAndEnemiesThisTurn.Add(enemy);
                
            BattleEngine.Instance.membersAndEnemiesThisTurn = BattleEngine.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(BattleEngine.Instance.membersAndEnemiesThisTurn);
                
            BattleEngine.Instance.membersAndEnemiesThisTurn = BattleEngine.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
        }

        private static void GetNewListNextTurn()
        {
            BattleEngine.Instance.membersAndEnemiesNextTurn = new List<UnitBase>();
            
            foreach (var member in BattleEngine.Instance._membersForThisBattle) BattleEngine.Instance.membersAndEnemiesNextTurn.Add(member);
            foreach (var enemy in BattleEngine.Instance._enemiesForThisBattle) BattleEngine.Instance.membersAndEnemiesNextTurn.Add(enemy);
                
            BattleEngine.Instance.membersAndEnemiesNextTurn = BattleEngine.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(BattleEngine.Instance.membersAndEnemiesNextTurn);
                
            BattleEngine.Instance.membersAndEnemiesNextTurn = BattleEngine.Instance.membersAndEnemiesNextTurn.OrderByDescending
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

        public void ResortThisTurnOrder()
        {
            BattleEngine.Instance.membersAndEnemiesThisTurn.ForEach
                (t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            
            BattleEngine.Instance.membersAndEnemiesThisTurn = BattleEngine.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();

            BattleEngine.Instance.membersAndEnemiesThisTurn.Remove(BattleEngine.Instance.activeUnit);
            BattleEngine.Instance.membersAndEnemiesThisTurn.Insert(0, BattleEngine.Instance.activeUnit);
            
            BattleEvents.Instance.thisTurnListCreatedEvent.Raise();
        }

        public void ResortNextTurnOrder()
        {
            BattleEngine.Instance.membersAndEnemiesNextTurn.ForEach
                (t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            
            BattleEngine.Instance.membersAndEnemiesNextTurn = BattleEngine.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
            
            BattleEvents.Instance.nextTurnListCreatedEvent.Raise();
        }
    }
}