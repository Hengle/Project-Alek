using Kryz.CharacterStats;
using SingletonScriptableObject;
using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(fileName = "Checkmate", menuName = "Status Effect/Checkmate")]
    public class Checkmate : StatChange
    {
        private void Awake() => effectType = EffectType.StatChange;
        
        public override void OnAdded(UnitBase target)
        {
            base.OnAdded(target);
            
            AddModifiers(target);
            statChangeEvent.Raise(target, statChangeEvent);
        }
        
        public override void OnRemoval(UnitBase unitBase)
        {
            base.OnRemoval(unitBase);
            
            unitBase.Unit.status = Status.Normal;
            RemoveModifiers(unitBase);
            statChangeEvent.Raise(unitBase, statChangeEvent);
        }

        private void AddModifiers(UnitBase target)
        {
            target.strength.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
            target.magic.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
            target.accuracy.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
            target.initiative.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
            target.criticalChance.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
            target.defense.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
            target.resistance.AddModifier(new StatModifier
                (GlobalVariables.Instance.moderateDebuff, StatModType.PercentAdd, this));
        }

        private void RemoveModifiers(UnitBase target)
        {
            target.strength.RemoveAllModifiersFromSource(this);
            target.magic.RemoveAllModifiersFromSource(this);
            target.accuracy.RemoveAllModifiersFromSource(this);
            target.initiative.RemoveAllModifiersFromSource(this);
            target.criticalChance.RemoveAllModifiersFromSource(this);
            target.defense.RemoveAllModifiersFromSource(this);
            target.resistance.RemoveAllModifiersFromSource(this);
        }
    }
}