using System.Collections.Generic;
using Characters.PartyMembers;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters.Enemies
{
    // Update this later so that each enemy type is a different script obj, not just Enemy
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase, IGiveExperience
    {
        [HorizontalGroup("Basic/Info/Prefab"), LabelText("OW Prefab"), LabelWidth(100)]
        public GameObject overworldPrefab;

        [SerializeField, VerticalGroup("Basic/Info"), LabelWidth(120), Range(1,10)]
        public int maxShieldCount = 1;

        private void Awake() => id = CharacterType.Enemy;

        public bool SetAI(List<PartyMember> targets)
        {
            if (Unit.currentAP <= 2) return false;
            
            var rand = Random.Range(0, targets.Count); // Inclusive apparently
            Unit.currentTarget = targets[rand];

            rand = Random.Range(0, abilities.Count + 2);
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

        [VerticalGroup("Basic/Info")]
        [SerializeField] private int baseExperience;

        public int CalculateExperience(int lvl, object source)
        {
            var type = source.GetType();

            if (type == typeof(PartyMember))
            {
                var difference = level - lvl;
                double modifier = 1 + difference * 0.25f;
                var finalValue = baseExperience * modifier;

                return (int)finalValue;
            }
            
            if (type == typeof(Class)) return (int)(baseExperience / 5f);
            
            return 0;
        }
    }
}
