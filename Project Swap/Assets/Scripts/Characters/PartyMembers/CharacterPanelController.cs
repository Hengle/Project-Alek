using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Characters.PartyMembers
{
    public class CharacterPanelController : MonoBehaviour
    {
        public UnitBase member;

        private Image fillRectImage;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameUGUI;
        [SerializeField] private TextMeshProUGUI healthUGUI;
        [SerializeField] private Slider slider;

        private void Awake()
        {
            fillRectImage = slider.fillRect.GetComponent<Image>();
            icon.sprite = member.icon;
            nameUGUI.text = member.characterName.ToUpper();
            healthUGUI.text = $"HP: {member.health2.BaseValue}";
            slider.maxValue = member.health2.BaseValue;
            slider.value = member.health2.BaseValue;
            member.onHpValueChanged += OnHpValueChanged;
        }

        private void OnHpValueChanged() 
        {
            fillRectImage.color = member.Color;
            slider.value = member.Unit.currentHP;
            healthUGUI.text = $"HP: {member.Unit.currentHP}";
        }
    }
}