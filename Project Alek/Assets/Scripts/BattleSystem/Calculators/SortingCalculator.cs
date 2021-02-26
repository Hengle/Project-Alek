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
            if (OldBattleEngine.Instance.membersAndEnemiesNextTurn.Count > 0)
            {
                OldBattleEngine.Instance.membersAndEnemiesThisTurn = new List<UnitBase>(OldBattleEngine.Instance.membersAndEnemiesNextTurn);
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
            OldBattleEngine.Instance.membersAndEnemiesThisTurn = new List<UnitBase>();

            foreach (var member in OldBattleEngine.Instance._membersForThisBattle) OldBattleEngine.Instance.membersAndEnemiesThisTurn.Add(member);
            foreach (var enemy in OldBattleEngine.Instance._enemiesForThisBattle) OldBattleEngine.Instance.membersAndEnemiesThisTurn.Add(enemy);
                
            OldBattleEngine.Instance.membersAndEnemiesThisTurn = OldBattleEngine.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(OldBattleEngine.Instance.membersAndEnemiesThisTurn);
                
            OldBattleEngine.Instance.membersAndEnemiesThisTurn = OldBattleEngine.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
        }

        private static void GetNewListNextTurn()
        {
            OldBattleEngine.Instance.membersAndEnemiesNextTurn = new List<UnitBase>();
            
            foreach (var member in OldBattleEngine.Instance._membersForThisBattle) OldBattleEngine.Instance.membersAndEnemiesNextTurn.Add(member);
            foreach (var enemy in OldBattleEngine.Instance._enemiesForThisBattle) OldBattleEngine.Instance.membersAndEnemiesNextTurn.Add(enemy);
                
            OldBattleEngine.Instance.membersAndEnemiesNextTurn = OldBattleEngine.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.initiative.Value).ToList();

            GetFinalInitValues(OldBattleEngine.Instance.membersAndEnemiesNextTurn);
                
            OldBattleEngine.Instance.membersAndEnemiesNextTurn = OldBattleEngine.Instance.membersAndEnemiesNextTurn.OrderByDescending
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
            OldBattleEngine.Instance.membersAndEnemiesThisTurn.ForEach
                (t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            
            OldBattleEngine.Instance.membersAndEnemiesThisTurn = OldBattleEngine.Instance.membersAndEnemiesThisTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();

            OldBattleEngine.Instance.membersAndEnemiesThisTurn.Remove(OldBattleEngine.Instance.activeUnit);
            OldBattleEngine.Instance.membersAndEnemiesThisTurn.Insert(0, OldBattleEngine.Instance.activeUnit);
            
            BattleEvents.Instance.thisTurnListCreatedEvent.Raise();
        }

        public void ResortNextTurnOrder()
        {
            OldBattleEngine.Instance.membersAndEnemiesNextTurn.ForEach
                (t => t.Unit.finalInitVal = (int) (t.initiative.Value * t.Unit.initModifier));
            
            OldBattleEngine.Instance.membersAndEnemiesNextTurn = OldBattleEngine.Instance.membersAndEnemiesNextTurn.OrderByDescending
                (e => e.Unit.finalInitVal).ToList();
            
            BattleEvents.Instance.nextTurnListCreatedEvent.Raise();
        }
    }
}