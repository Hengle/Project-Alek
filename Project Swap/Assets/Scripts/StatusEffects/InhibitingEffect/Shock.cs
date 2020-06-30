using BattleSystem;
using BattleSystem.DamagePrefab;
using Characters;
using UnityEngine;

namespace StatusEffects.InhibitingEffect
{
    [CreateAssetMenu(menuName = "Status Effect/Inhibiting Effect/Shock")]
    public class Shock : StatusEffect
    {
        [Range(0, 1)] public float damagePercentage;
        [Range(0, 1)] public float chanceOfInfliction;

        private void Awake() {
            effectType = EffectType.Inhibiting;
            rateOfInfliction.Add(RateOfInfliction.BeforeEveryAction);
        }

        public override void InflictStatus(UnitBase unitBase)
        {
            var random = Random.value;
            if (random > chanceOfInfliction) return;
            
            DamagePrefabManager.Instance.DamageTextColor = color;
            var dmg = (int) (damagePercentage * unitBase.Unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unitBase.TakeDamage(dmg);
            // show shocked visual effect
            BattleManager.shouldGiveCommand = false;
        }

        public override void OnAdded(UnitBase target)
        {
            // show shocked visual effect
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