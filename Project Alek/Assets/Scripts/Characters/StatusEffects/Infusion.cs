using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(menuName = "Status Effect/Unique/Infusion")]
    public class Infusion : StatusEffect
    {
        public override void OnAdded(UnitBase target)
        {
            base.OnAdded(target);
            target.Unit.overrideElemental = true;
        }

        public override void OnRemoval(UnitBase unitBase)
        {
            base.OnRemoval(unitBase);
            unitBase.Unit.overrideElemental = false;
        }
    }
}