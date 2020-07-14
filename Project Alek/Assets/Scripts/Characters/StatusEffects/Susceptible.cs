using UnityEngine;

namespace Characters.StatusEffects
{
    [CreateAssetMenu(menuName = "Status Effect/Inhibiting Effect/Susceptible")]
    public class Susceptible : StatusEffect
    {
        private void Awake() => effectType = EffectType.Inhibiting;
    }
}