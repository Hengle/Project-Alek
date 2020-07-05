using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using UnityEngine;

namespace BattleSystem.Calculator
{
    public static class SortingCalculator
    {
        public static void SortAndCombine()
        {
            BattleManager._membersAndEnemies = new List<UnitBase>();
            
            foreach (var member in BattleManager._membersForThisBattle) BattleManager._membersAndEnemies.Add(member);
            foreach (var enemy in BattleManager._enemiesForThisBattle) BattleManager._membersAndEnemies.Add(enemy);
            
            BattleManager._membersAndEnemies = BattleManager._membersAndEnemies.
                OrderByDescending(e => e.initiative2.Value).ToList();
            
            foreach (var unitBase in BattleManager._membersAndEnemies) 
                Logger.Log($"{unitBase.characterName} initiative: {unitBase.initiative2.Value}");
        }
    }
}