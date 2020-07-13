using Characters.StatusEffects;
using UnityEngine;

namespace StatusEffects.InhibitingEffect
{
    [CreateAssetMenu(menuName = "Status Effect/Inhibiting Effect/Susceptible")]
    public class Susceptible : StatusEffect
    {
        private void Awake() => effectType = EffectType.Inhibiting;
    }
}