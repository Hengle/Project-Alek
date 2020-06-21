using Characters;
using UnityEngine;

namespace StatusEffects
{
    [CreateAssetMenu(fileName = "DOT Effect", menuName = "Status Effect/Damage Over Time Effect")]
    public class DamageOverTime : StatusEffect
    {
        [Range(0, 1)] public float damagePercentage;
        private void Awake() => effectType = EffectType.DamageOverTime;
        
        public override void InflictStatus(Unit unit)
        {
            unit.SetColor(color);
            var dmg = (int) ((damagePercentage) * unit.maxHealthRef);
            dmg = Random.Range((int)(0.98f * dmg), (int)(1.02f * dmg));
            unit.TakeDamage(dmg, unit,true);
        }
        
        public override void OnAdded(Unit unit)
        {
            Debug.Log( $"{unit.unitName} is inflicted with {name.ToLower()}");
            // Add icon
        }
        
        public override void OnRemoval(Unit unit)
        {
            Debug.Log($"{unit.unitName} is no longer inflicted with {name}.");
        }
    }
}