using System;
using System.Linq;
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
            unit.overrideElemental = true;
            unit.overrideAbility = unit.currentAbility;
        }

        [UsedImplicitly] public void SetStatusEffect()
        {
            var effect = unit.currentAbility.statusEffects[0];
            foreach (var e in unit.statusEffects.Where(e => e.GetType() == effect.GetType()))
            {
                unit.statusEffects.Remove(e);
                e.OnRemoval(unit.parent);
            }
            
            effect.OnAdded(unit.parent);
            unit.statusEffects.Add(effect);
        }
    }
}