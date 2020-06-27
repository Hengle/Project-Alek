using BattleSystem;
using UnityEngine;

namespace Characters
{
    // Update this later so that each enemy type is a different script obj, not just EnemyData
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase
    {
        public bool SetAI()
        {
            if (unit.currentAP <= 2) return false;
            
            var rand = Random.Range(0, BattleHandler.membersForThisBattle.Count); // Inclusive apparently
            unit.currentTarget = BattleHandler.membersForThisBattle[rand].unit;

            rand = Random.Range(0, abilities.Count);
            unit.commandActionName = rand < abilities.Count ? "AbilityAction" : "UniversalAction";
            unit.commandActionOption = rand < abilities.Count ? rand : 1;
            unit.actionCost = rand < abilities.Count ? abilities[rand].actionCost : 2;

            // Will need to update to try to find an attack that does cost less
            return unit.currentAP - unit.actionCost >= 0;
        }

        public void SetUnitGO(GameObject enemyGO) => unit = enemyGO.GetComponent<Unit>();
    }
}
