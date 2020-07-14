using System.Collections.Generic;
using UnityEngine;
using BattleSystem;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Characters
{
    // Update this later so that each enemy type is a different script obj, not just Enemy
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase
    {
        [HideInInspector] public UnitStateMachine stateMachine;

        [TabGroup("Tabs","Checkmate Requirements")]
        [FoldoutGroup("Tabs/Checkmate Requirements/Requirements")]
        [HorizontalGroup("Tabs/Checkmate Requirements/Requirements/Requirement Lists")]
        public List<ScriptableObject> checkmateRequirements = new List<ScriptableObject>();
        
        [TabGroup("Tabs","Checkmate Requirements")]
        [FoldoutGroup("Tabs/Checkmate Requirements/Requirements")]
        [HorizontalGroup("Tabs/Checkmate Requirements/Requirements/Requirement Lists")]
        public List<TransitionRequirements> transitionRequirements = new List<TransitionRequirements>();

        public int CurrentAP 
        {
            get => Unit.currentAP;
            set => Unit.currentAP = value;
        }

        private void Awake() => id = CharacterType.Enemy;

        [TabGroup("Tabs","Checkmate Requirements")]
        [FoldoutGroup("Tabs/Checkmate Requirements/Requirements")]
        //[HorizontalGroup("Tabs/Checkmate Requirements/Requirements/Requirement Lists")]
        [Button] public bool ValidateLists()
        {
            if (checkmateRequirements.Count != transitionRequirements.Count)
            {
                Debug.LogError("Requirement lists count do not match!");
                return false;
            }

            for (var i = 0; i < checkmateRequirements.Count; i++)
            {
                var tryGetStatus = checkmateRequirements[i] as StatusEffect;
                var tryGetElement = checkmateRequirements[i] as ElementalType;

                if (tryGetStatus != null && transitionRequirements[i] != TransitionRequirements.StatusEffect)
                {
                    Debug.LogError($"Requirement lists do not match! index: {i}");
                    return false;
                }
                
                if (tryGetElement != null && transitionRequirements[i] != TransitionRequirements.ElementalDamage)
                {
                    Debug.LogError($"Requirement lists do not match! index: {i}");
                    return false;
                }

                if (tryGetStatus == null && tryGetElement == null)
                {
                    Debug.LogError("List contains object that isn't of type StatusEffect or ElementalType");
                    return false;
                }
            }
            
            if (!Application.isPlaying) Debug.Log("No Errors in Requirements Lists");
            return true;
        }

        public bool SetAI()
        {
            if (Unit.currentAP <= 2) return false;
            
            var rand = Random.Range(0, BattleManager.MembersForThisBattle.Count); // Inclusive apparently
            Unit.currentTarget = BattleManager.MembersForThisBattle[rand];

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
