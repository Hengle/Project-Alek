using BattleSystem;
using DamagePrefab;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(menuName = "Status Effect/Inhibiting Effect/Shock")]
    public class Shock : StatusEffect
    {
        [Space] [Range(0, 1)] [VerticalGroup("Icon/Info")] public float damagePercentage;
        [Space] [Range(0, 1)] [VerticalGroup("Icon/Info")] public float chanceOfInfliction;

        private void Awake() => effectType = EffectType.Inhibiting;

        public override void InflictStatus(UnitBase unitBase)
        {
            var random = Random.value;
            if (random > chanceOfInfliction) return;
            
            DamagePrefabManager.Instance.DamageTextColor = color;
            var modifier = StatusEffectModifier(unitBase);
            var dmg = (int) (damagePercentage * modifier * unitBase.Unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unitBase.TakeDamage(dmg);
 
            // show shocked visual effect
            OldBattleEngine.Instance.canGiveCommand = false;
        }
    }
}