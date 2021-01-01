using UnityEngine;

namespace Characters.Animations
{
    public class AnimationHandler : MonoBehaviour
    {
        public static readonly int AttackTrigger = Animator.StringToHash("Attack");
        public static readonly int HurtTrigger = Animator.StringToHash("Hurt");
        public static readonly int DeathTrigger = Animator.StringToHash("Dead");
        public static readonly int VictoryTrigger = Animator.StringToHash("Victory");
        public static readonly int GuardTrigger = Animator.StringToHash("Guard");
        public static readonly int ItemTrigger = Animator.StringToHash("Item");
        public static readonly int PhysAttackState = Animator.StringToHash("PhysAttackState");
        public static readonly int APVal = Animator.StringToHash("AP State");
        public static readonly int Panel = Animator.StringToHash("Panel");
        public static readonly int AbilityMenu = Animator.StringToHash("Ability Menu");
        
        public bool isAttacking;
        public bool usingItem;
    }
}
