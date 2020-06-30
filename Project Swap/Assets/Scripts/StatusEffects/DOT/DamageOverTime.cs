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
        
        public override void InflictStatus(UnitBase unitBase)
        {
            DamagePrefabManager.Instance.DamageTextColor = color;
            var dmg = (int) (damagePercentage * unitBase.Unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unitBase.TakeDamage(dmg);
        }
        
        public override void OnAdded(UnitBase target) {
            Logger.Log($"{target.characterName} is inflicted with {name}.");
            SetIconAndTimer(target);
        }
        
        public override void OnRemoval(UnitBase unitBase)
        {
            Logger.Log($"{unitBase.characterName} is no longer inflicted with {name}.");
            if (unitBase.Unit.statusBox == null) return;
            
            var iconGO = unitBase.id == Type.Enemy?
                unitBase.Unit.statusBox.GetChild(0).Find(name) : unitBase.Unit.statusBox.Find(name);
            
            if (iconGO != null) iconGO.gameObject.SetActive(false);
        }
    }
}