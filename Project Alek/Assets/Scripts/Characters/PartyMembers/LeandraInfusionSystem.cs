using System.Collections.Generic;
using Characters.StatusEffects;
using JetBrains.Annotations;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class LeandraInfusionSystem : MonoBehaviour
    {
        private Unit unit;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        [UsedImplicitly] public void SetOverrideAbility()
        {
            unit.overrideAbility = unit.currentAbility;
        }

        [UsedImplicitly] public void SetStatusEffect()
        {
            var effect = unit.currentAbility.statusEffects[0];
            var effectsToRemove = new List<StatusEffect>();
            
            for (var i = unit.statusEffects.Count - 1; i >= 0; i--)
            {
                if (unit.statusEffects[i].GetType() != typeof(Infusion)) continue;
                effectsToRemove.Add(unit.statusEffects[i]);
            }

            effectsToRemove.ForEach(e =>
            {
                unit.statusEffects.Remove(e);
                e.OnRemoval(unit.parent);
            });

            effect.OnAdded(unit.parent);
            unit.statusEffects.Add(effect);
        }
    }
}