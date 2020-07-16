using DamagePrefab;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(fileName = "DOT Effect", menuName = "Status Effect/Damage Over Time Effect")]
    public class DamageOverTime : StatusEffect
    {
        [Space] [Range(0, 1)] [VerticalGroup("Icon/Info"), LabelWidth(120)] 
        public float damagePercentage;
        
        private void Awake() => effectType = EffectType.DamageOverTime;
        
        public override void InflictStatus(UnitBase unitBase)
        {
            DamagePrefabManager.Instance.DamageTextColor = color;
            var modifier = StatusEffectModifier(unitBase);
            var dmg = (int) (damagePercentage * modifier * unitBase.Unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unitBase.TakeDamage(dmg, null);
        }
    }
}