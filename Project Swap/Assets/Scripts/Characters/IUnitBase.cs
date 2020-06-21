using Abilities;
using Animations;
using UnityEngine;

namespace Characters
{
    public interface IUnitBase
    {
        void GiveCommand(bool isSwapping);
        void ResetCommandsAndAP();
        void SetupUnit();
        int GetCriticalChance();
        Unit CheckTargetStatus(bool isSwap);
        Ability GetAndSetAbility(int index);
        bool CheckUnitStatus();
        bool SetAI();
        Animator GetAnimator();
        AnimationHandler GetAnimationHandler();
        Unit GetUnit();
    }
}