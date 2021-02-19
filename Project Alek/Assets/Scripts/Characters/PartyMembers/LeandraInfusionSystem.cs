using System.Collections.Generic;
using Characters.StatusEffects;
using JetBrains.Annotations;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class LeandraInfusionSystem : MonoBehaviour
    {
        private Unit unit;
        private Material originalMaterial;
        [SerializeField] private Material auraMaterial;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalMaterial = spriteRenderer.material;
        }

        [UsedImplicitly] public void SetOverrideAbility() => unit.overrideAbility = unit.currentAbility;

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

        [UsedImplicitly] public void SetAuraMaterial()
        {
            spriteRenderer.material = auraMaterial;
        }

        [UsedImplicitly] public void SetMaterialBackToOriginal()
        {
            spriteRenderer.material = originalMaterial;
        }
    }
}