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
            
            BattleManager.membersForThisBattle = BattleManager.membersForThisBattle.
                OrderByDescending(e => e.Unit.currentInitiative).ToList();
            
            BattleManager.enemiesForThisBattle = BattleManager.enemiesForThisBattle.
                OrderByDescending(f => f.Unit.currentInitiative).ToList();
            
            isCalculating = false;
        }
        
        private static void CombinePlayerAndEnemyList()
        {
            isCalculating = true;
            
            var partyMemberCount = 0; // party member
            var enemyCount = 0; // enemy

            // Keep track of the combined count for both lists
            var total = BattleManager.membersForThisBattle.Count + BattleManager.enemiesForThisBattle.Count;

            BattleManager.membersAndEnemies = new List<UnitBase>();

            while (partyMemberCount + enemyCount < total)
            {
                // Checks if the index is out of range, then assigns 0 or the initiative value appropriately
                var memberI = partyMemberCount >= BattleManager.membersForThisBattle.Count?
                    0 : BattleManager.membersForThisBattle[partyMemberCount].Unit.currentInitiative;
                
                var enemyI = enemyCount >= BattleManager.enemiesForThisBattle.Count?
                    0 : BattleManager.enemiesForThisBattle[enemyCount].Unit.currentInitiative;

                var compare = memberI >= enemyI;

                // If the party member's initiative is >= the enemy's, that member is added to the list. If not, then the enemy is added
                if (compare) {
                    BattleManager.membersAndEnemies.Add(BattleManager.membersForThisBattle[partyMemberCount]);
                    partyMemberCount++;
                }
                
                else {
                    BattleManager.membersAndEnemies.Add(BattleManager.enemiesForThisBattle[enemyCount]);
                    enemyCount++;
                }
            }

            isCalculating = false;
        }
    }
}