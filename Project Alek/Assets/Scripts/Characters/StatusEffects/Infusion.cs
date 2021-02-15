using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(menuName = "Status Effect/Unique/Infusion")]
    public class Infusion : StatusEffect
    {
        [SerializeField] private GameObject auraEffect;

        public override void OnAdded(UnitBase target)
        {
            base.OnAdded(target);
            //TODO: Instantiate effect
        }

        public override void OnRemoval(UnitBase unitBase)
        {
            base.OnRemoval(unitBase);
            unitBase.Unit.overrideElemental = false;
        }
    }
}