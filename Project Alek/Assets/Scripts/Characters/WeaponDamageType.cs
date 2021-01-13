using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    [CreateAssetMenu(fileName = "Damage Type")]
    public class WeaponDamageType : ScriptableObject
    {
        [HideLabel, ShowInInspector, HorizontalGroup("Icon", 100), PreviewField(100), ShowIf(nameof(icon))]
        public Sprite Icon
        {
            get => icon == null? null : icon.GetComponent<Image>().sprite;
            set => icon.GetComponent<Image>().sprite = value;
        }

        [VerticalGroup("Icon/Info")] public GameObject icon;
    }
}