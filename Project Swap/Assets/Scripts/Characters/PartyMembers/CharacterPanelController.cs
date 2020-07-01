using StatusEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private GameObject statusBox;

        private void Awake()
        {
            fillRectImage = slider.fillRect.GetComponent<Image>();
            icon.sprite = member.icon;
            nameUGUI.text = member.characterName.ToUpper();
            healthUGUI.text = $"HP: {member.health}";
            slider.maxValue = member.health;
            slider.value = member.health;
            member.onHpValueChanged += OnHpValueChanged;
            member.onStatusEffectReceived += AddStatusEffectIcon;
        }

        private void OnHpValueChanged(int amount)
        {
            fillRectImage.color = member.Color;
            slider.value = member.Unit.currentHP;
            healthUGUI.text = $"HP: {member.Unit.currentHP}";
        }

        private void AddStatusEffectIcon(StatusEffect effect)
        {
            var alreadyHasIcon = statusBox.transform.Find(effect.name);
            
            if (effect.icon != null && alreadyHasIcon == null) {
                var iconGO = Instantiate(effect.icon, statusBox.transform, false);
                
                iconGO.name = effect.name;
                iconGO.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
            
            else if (effect.icon != null && alreadyHasIcon != null) {
                alreadyHasIcon.gameObject.SetActive(true);
                alreadyHasIcon.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
        }
    }
}