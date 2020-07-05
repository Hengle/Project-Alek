using UnityEngine;
using BattleSystem;

namespace Characters
{
    // Update this later so that each enemy type is a different script obj, not just Enemy
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase
    {
        public int CurrentAP {
            get => Unit.currentAP;
            set => Unit.currentAP = value;
        }
        
        public bool SetAI()
        {
            if (Unit.currentAP <= 2) return false;
            
            var rand = Random.Range(0, BattleManager._membersForThisBattle.Count); // Inclusive apparently
            Unit.currentTarget = BattleManager._membersForThisBattle[rand];

            rand = Random.Range(0, abilities.Count);
            Unit.commandActionName = rand < abilities.Count ? "AbilityAction" : "UniversalAction";
            Unit.commandActionOption = rand < abilities.Count ? rand : 1;
            Unit.actionCost = rand < abilities.Count ? abilities[rand].actionCost : 2;

            // Will need to update to try to find an attack that does cost less
            return Unit.currentAP - Unit.actionCost >= 0;
        }

        public override void Heal(float amount)
        {
            CurrentHP += (int) amount;
            CurrentAP -= 1;
        }
    }
}
