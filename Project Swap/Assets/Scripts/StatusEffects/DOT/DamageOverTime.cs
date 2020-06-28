using BattleSystem.DamagePrefab;
using Characters;
using UnityEngine;

namespace StatusEffects.DOT
{
    [CreateAssetMenu(fileName = "DOT Effect", menuName = "Status Effect/Damage Over Time Effect")]
    public class DamageOverTime : StatusEffect
    {
        [Range(0, 1)] public float damagePercentage;
        private void Awake() => effectType = EffectType.DamageOverTime;
        
        public override void InflictStatus(Unit unit)
        {
            DamagePrefabManager.Instance.DamageTextColor = color;
            var dmg = (int) ((damagePercentage) * unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unit.TakeDamage(dmg, unit);
        }
        
        public override void OnAdded(Unit target) {
            Logger.Log($"{target.unitName} is inflicted with {name}.");
            SetIconAndTimer(target);
        }
        
        public override void OnRemoval(Unit unit)
        {
            Logger.Log($"{unit.unitName} is no longer inflicted with {name}.");
            if (unit.statusBox == null) return;
            
            var iconGO = unit.id == Type.Enemy ? unit.statusBox.GetChild(0).Find(name) : unit.statusBox.Find(name);
            if (iconGO != null) iconGO.gameObject.SetActive(false);
        }
    }
}