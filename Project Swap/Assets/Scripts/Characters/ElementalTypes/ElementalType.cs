using UnityEngine;

namespace Characters.ElementalTypes
{
    // Can use this for damage scaling and resistance scaling
    public enum ElementalScaler { Normal = 25, Moderate = 50, Significant = 75, Major = 100 }
    public enum ElementalWeaknessScaler { Normal = 125, Moderate = 150, Significant = 75, Major = 100 }
    [CreateAssetMenu(menuName = "Elemental Type")]
    public class ElementalType : ScriptableObject
    {
        public GameObject icon;
        // maybe field for color
        // Field for visual effect
    }
}