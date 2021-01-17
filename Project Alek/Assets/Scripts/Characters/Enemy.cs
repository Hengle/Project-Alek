using System.Collections.Generic;
using BattleSystem.Mechanics;
using UnityEngine;
using Characters.PartyMembers;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Characters
{
    // Update this later so that each enemy type is a different script obj, not just Enemy
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase
    {
        public BreakSystem BreakSystem { get; set; }

        [SerializeField, VerticalGroup("Stat Data/Stats"), LabelWidth(120), Range(1,10)]
        public int maxShieldCount = 1;

        private void Awake() => id = CharacterType.Enemy;

        public bool SetAI(List<PartyMember> targets)
        {
            if (Unit.currentAP <= 2) return false;
            
            var rand = Random.Range(0, targets.Count); // Inclusive apparently
            Unit.currentTarget = targets[rand];

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
