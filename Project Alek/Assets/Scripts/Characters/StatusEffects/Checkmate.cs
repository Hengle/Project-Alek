using BattleSystem;
using Kryz.CharacterStats;
using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(fileName = "Checkmate", menuName = "Status Effect/Checkmate")]
    public class Checkmate : StatChange
    {
        private void Awake() => effectType = EffectType.StatChange;
        
        public override void OnAdded(UnitBase target)
        {
            Logger.Log($"{target.characterName} is in {name}!");
            target.onStatusEffectReceived?.Invoke(this);
            
            AddModifiers(target);
            CharacterEvents.Trigger(CEventType.StatChange, target);
        }
        
        public override void OnRemoval(UnitBase unitBase)
        {
            Logger.Log($"{unitBase.characterName} is no longer in {name}.");
            unitBase.onStatusEffectRemoved?.Invoke(this);
            
            RemoveModifiers(unitBase);
            CharacterEvents.Trigger(CEventType.StatChange, unitBase);
        }

        private void AddModifiers(UnitBase target)
        {
            target.strength.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
            target.magic.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
            target.accuracy.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
            target.initiative.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
            target.criticalChance.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
            target.defense.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
            target.resistance.AddModifier(new StatModifier
                (BattleManager.Instance.globalVariables.moderateDebuff, StatModType.PercentAdd, this));
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