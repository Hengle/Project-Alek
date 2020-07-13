using System.Collections.Generic;
using System.Linq;
using Characters;

namespace BattleSystem.Calculator
{
    public static class SortingCalculator
    {
        public static void SortByInitiative()
        {
            BattleManager._membersAndEnemies = new List<UnitBase>();
            
            foreach (var member in BattleManager.MembersForThisBattle) BattleManager._membersAndEnemies.Add(member);
            foreach (var enemy in BattleManager.EnemiesForThisBattle) BattleManager._membersAndEnemies.Add(enemy);
            
            BattleManager._membersAndEnemies = BattleManager._membersAndEnemies.
                OrderByDescending(e => e.initiative.Value).ToList();
        }
    }
}