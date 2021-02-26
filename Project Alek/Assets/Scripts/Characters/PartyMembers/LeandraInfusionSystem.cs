using System.Collections.Generic;
using Characters.PartyMembers;
using JetBrains.Annotations;
using UnityEngine;

namespace Characters
{
    public class LeandraInfusionSystem : MonoBehaviour
    {
        [SerializeField] private PartyMember leandra;
        [SerializeField] private Material auraMaterial;

        private Material originalMaterial;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalMaterial = spriteRenderer.material;
        }

        [UsedImplicitly] public void SetOverrideAbility() => leandra.OverrideAbility = leandra.CurrentAbility;

        [UsedImplicitly] public void SetStatusEffect()
        {
            var effect = leandra.CurrentAbility.statusEffects[0];
            var effectsToRemove = new List<StatusEffect>();
            
            for (var i = leandra.StatusEffects.Count - 1; i >= 0; i--)
            {
                if (leandra.StatusEffects[i].GetType() != typeof(Infusion)) continue;
                effectsToRemove.Add(leandra.StatusEffects[i]);
            }

            leandra.CureAilments(effectsToRemove);
            leandra.StatusEffects.Add(effect);
            effect.OnAdded(leandra);
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