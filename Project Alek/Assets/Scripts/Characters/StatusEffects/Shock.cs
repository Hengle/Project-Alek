using BattleSystem;
using Characters;
using Characters.StatusEffects;
using DamagePrefab;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StatusEffects.InhibitingEffect
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
            var dmg = (int) (damagePercentage * unitBase.Unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unitBase.TakeDamage(dmg, null);
            Logger.Log($"{unitBase.characterName} is unable to attack due to {name}");
            // show shocked visual effect
            BattleManager._shouldGiveCommand = false;
        }
    }
}