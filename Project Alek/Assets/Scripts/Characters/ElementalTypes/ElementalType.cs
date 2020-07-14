﻿using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.ElementalTypes
{
    public enum ElementalScaler { Normal = 25, Moderate = 50, Significant = 75, Major = 100 }
    public enum ElementalWeaknessScaler { Normal = 125, Moderate = 150, Significant = 75, Major = 100 }
    [CreateAssetMenu(menuName = "Elemental Type")]
    public class ElementalType : ScriptableObject
    {
        [HideLabel, ShowInInspector, HorizontalGroup("Icon", 100), PreviewField(100), ShowIf(nameof(icon))]
        public Sprite Icon
        {
            get => icon == null? null : icon.GetComponent<Image>().sprite;
            set => icon.GetComponent<Image>().sprite = value;
        }

        [VerticalGroup("Icon/Info")] public GameObject icon;

        [VerticalGroup("Icon/Info")] public GameObject visualEffect;
    }
}