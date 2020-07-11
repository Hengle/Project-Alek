using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Characters.PartyMembers
{
    public class CharacterPanelController : MonoBehaviour
    {
        #region FieldsAndProperties
        
        public UnitBase member;

        private Image fillRectImage;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameUGUI;
        [SerializeField] private TextMeshProUGUI healthUGUI;
        [SerializeField] private Slider slider;
        
        #endregion

        private void Awake()
        {
            fillRectImage = slider.fillRect.GetComponent<Image>();
            icon.sprite = member.icon;
            nameUGUI.text = member.characterName.ToUpper();
            healthUGUI.text = $"HP: {member.health.BaseValue}";
            slider.maxValue = member.health.BaseValue;
            slider.value = member.health.BaseValue;
            member.onHpValueChanged += OnHpValueChanged;
        }

        private void OnHpValueChanged() 
        {
            fillRectImage.color = member.Color;
            slider.value = member.Unit.currentHP;
            healthUGUI.text = $"HP: {member.Unit.currentHP}";
        }

        private void OnDisable() => member.onHpValueChanged -= OnHpValueChanged;
    }
}