using Abilities;
using BattleSystem;
using UnityEngine;

namespace Characters
{
    // Update this later so that each enemy type is a different script obj, not just EnemyData
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase
    {
        public override void GiveCommand(bool isSwapping)
        {
            BattleHandler.battleFuncs.GetCommand(this,false);
            BattleHandler.performingAction = true;
        }
        
        public bool SetAI()
        {
            var rand = Random.Range(0, BattleHandler.membersForThisBattle.Count);
            unit.currentTarget = BattleHandler.membersForThisBattle[rand].unit;
            unit.commandActionName = "UniversalAction";
            unit.commandActionOption = 1;
            // When I add abilities / more options to enemies, set AI to check cost and if it can use ability
            unit.actionCost = 2;

            return true;
        }

        public void SetUnitGO(GameObject enemyGO) => unit = enemyGO.GetComponent<Unit>();
    }
}
