using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using UnityEngine;

namespace BattleSystem.Calculators
{
    public class SortingCalculator : MonoBehaviour
    {
        public static bool SortByInitiative()
        {
            if (Battle.Engine.membersAndEnemiesNextTurn.Count > 0)
            {
                Battle.Engine.membersAndEnemiesThisTurn = new List<UnitBase>(Battle.Engine.membersAndEnemiesNextTurn);
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
            Battle.Engine.membersAndEnemiesThisTurn = new List<UnitBase>();

            foreach (var member in Battle.Engine.membersForThisBattle) Battle.Engine.membersAndEnemiesThisTurn.Add(member);
            foreach (var enemy in Battle.Engine.enemiesForThisBattle) Battle.Engine.membersAndEnemiesThisTurn.Add(enemy);
                
            Battle.Engine.membersAndEnemiesThisTurn = Battle.Engine.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(Battle.Engine.membersAndEnemiesThisTurn);
                
            Battle.Engine.membersAndEnemiesThisTurn = Battle.Engine.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
        }

        private static void GetNewListNextTurn()
        {
            Battle.Engine.membersAndEnemiesNextTurn = new List<UnitBase>();
            
            foreach (var member in Battle.Engine.membersForThisBattle) Battle.Engine.membersAndEnemiesNextTurn.Add(member);
            foreach (var enemy in Battle.Engine.enemiesForThisBattle) Battle.Engine.membersAndEnemiesNextTurn.Add(enemy);
                
            Battle.Engine.membersAndEnemiesNextTurn = Battle.Engine.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(Battle.Engine.membersAndEnemiesNextTurn);
                
            Battle.Engine.membersAndEnemiesNextTurn = Battle.Engine.membersAndEnemiesNextTurn.OrderByDescending
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
            Battle.Engine.membersAndEnemiesThisTurn.ForEach
                (t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            
            Battle.Engine.membersAndEnemiesThisTurn = Battle.Engine.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();

            Battle.Engine.membersAndEnemiesThisTurn.Remove(Battle.Engine.activeUnit);
            Battle.Engine.membersAndEnemiesThisTurn.Insert(0, Battle.Engine.activeUnit);
            
            BattleEvents.Instance.thisTurnListCreatedEvent.Raise();
        }

        public void ResortNextTurnOrder()
        {
            Battle.Engine.membersAndEnemiesNextTurn.ForEach
                (t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            
            Battle.Engine.membersAndEnemiesNextTurn = Battle.Engine.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
            
            BattleEvents.Instance.nextTurnListCreatedEvent.Raise();
        }
    }
}