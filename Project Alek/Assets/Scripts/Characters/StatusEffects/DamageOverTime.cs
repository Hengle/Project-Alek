using BattleSystem.DamagePrefab;
using Characters;
using Characters.StatusEffects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusEffects.DOT
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
            var dmg = (int) (damagePercentage * unitBase.Unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unitBase.TakeDamage(dmg, null);
        }
    }
}