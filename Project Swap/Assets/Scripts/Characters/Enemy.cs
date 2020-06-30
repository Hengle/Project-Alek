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
            if (Unit.currentAP <= 2) return false;
            
            var rand = Random.Range(0, BattleManager.membersForThisBattle.Count); // Inclusive apparently
            Unit.currentTarget = BattleManager.membersForThisBattle[rand];

            rand = Random.Range(0, abilities.Count);
            Unit.commandActionName = rand < abilities.Count ? "AbilityAction" : "UniversalAction";
            Unit.commandActionOption = rand < abilities.Count ? rand : 1;
            Unit.actionCost = rand < abilities.Count ? abilities[rand].actionCost : 2;

            // Will need to update to try to find an attack that does cost less
            return Unit.currentAP - Unit.actionCost >= 0;
        }

        // Probably unnecessary. should do this automatically i think
        public void SetUnitGO(GameObject enemyGO) => Unit = enemyGO.GetComponent<Unit>();
        
        public override void OnHpValueChanged()
        {
            if (Unit.currentHP <= 0.25f * Unit.maxHealthRef) Unit.outline.color = lowHealthColor;
            else if (Unit.currentHP <= 0.5f * Unit.maxHealthRef) Unit.outline.color = midHealthColor;
            else Unit.outline.color = normalHealthColor;
        }
    }
}
