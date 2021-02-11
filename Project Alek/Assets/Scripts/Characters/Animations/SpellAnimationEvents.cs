using Characters.ElementalTypes;
using JetBrains.Annotations;
using UnityEngine;

namespace Characters.Animations
{
    public class SpellAnimationEvents : MonoBehaviour
    {
        private Unit unit;
        private ElementalType type;

        public void Setup(Unit unitParam, ElementalType elementalType)
        {
            unit = unitParam;
            type = elementalType;
        }

        [UsedImplicitly] private void TargetTakeDamage()
        {
            if (!unit.currentAbility.isMultiTarget)
            {
                if (unit.currentTarget == null) return;
                unit.currentTarget.TakeDamage(unit.currentDamage, type);
                return;
            }
            
            unit.multiHitTargets.ForEach(t => t.TakeDamage
                (unit.damageValueList[unit.multiHitTargets.IndexOf(t)], type));
        }

        [UsedImplicitly] private void SetAttackBoolFalse()
        {
            unit.animationHandler.isAttacking = false;
            Destroy(gameObject);
        }
    }
}