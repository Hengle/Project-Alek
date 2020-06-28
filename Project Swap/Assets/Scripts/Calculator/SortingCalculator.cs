using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using Characters;

namespace Calculator
{
    public static class SortingCalculator
    {
        private static bool isCalculating;
        public static  bool isFinished;
        
        public static IEnumerator SortAndCombine()
        {
            isFinished = false;
            
            SortByInitiative();
            while (isCalculating) yield return null;

            CombinePlayerAndEnemyList();
            while (isCalculating) yield return null;

            isFinished = true;
        }

        private static void SortByInitiative()
        {
            isCalculating = true;
            
            BattleHandler.membersForThisBattle = BattleHandler.membersForThisBattle.OrderByDescending(e => e.unit.currentInitiative).ToList();
            BattleHandler.enemiesForThisBattle = BattleHandler.enemiesForThisBattle.OrderByDescending(f => f.unit.currentInitiative).ToList();

            isCalculating = false;
        }
        
        private static void CombinePlayerAndEnemyList()
        {
            isCalculating = true;
            
            var i = 0; // party member
            var e = 0; // enemy

            // Keep track of the combined count for both lists
            var total = BattleHandler.membersForThisBattle.Count + BattleHandler.enemiesForThisBattle.Count;

            BattleHandler.membersAndEnemies = new List<UnitBase>();

            while ((i + e) < total)
            {
                // Store the initiative values into variables
                var memberI = 0;
                var enemyI = 0;

                // Checks if the index is out of range, then assigns 0 or the initiative value appropriately
                memberI = i >= BattleHandler.membersForThisBattle.Count ? 0 : BattleHandler.membersForThisBattle[i].unit.currentInitiative;
                enemyI = e >= BattleHandler.enemiesForThisBattle.Count ? 0 : BattleHandler.enemiesForThisBattle[e].unit.currentInitiative;

                var compare = memberI >= enemyI;

                // If the party member's initiative is >= the enemy's, that member is added to the list. If not, then the enemy is added
                if (compare) { BattleHandler.membersAndEnemies.Add(BattleHandler.membersForThisBattle[i]); i++; }
                else { BattleHandler.membersAndEnemies.Add(BattleHandler.enemiesForThisBattle[e]); e++; }
            }

            isCalculating = false;
        }
    }
}