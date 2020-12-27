using System.Collections.Generic;
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
        [SerializeField] private List<GameObject> dmgBoostBars;
        [SerializeField] private List<GameObject> defBoostBars;

        private int dmgBoostLvl;
        private int defBoostLvl;
        
        #endregion

        private void Awake()
        {
            fillRectImage = slider.fillRect.GetComponent<Image>();
            icon.sprite = member.icon;
            nameUGUI.text = member.characterName.ToUpper();
            healthUGUI.text = $"HP: {member.health.BaseValue}";
            slider.maxValue = member.health.BaseValue;
            slider.value = member.health.BaseValue;

            dmgBoostLvl = 0;
            defBoostLvl = 0;
            
            dmgBoostBars.ForEach(o => o.gameObject.SetActive(false));
            defBoostBars.ForEach(o => o.gameObject.SetActive(false));
            
            member.onHpValueChanged += OnHpValueChanged;
            member.Unit.onDmgValueChanged += OnDmgBoostValueChanged;
            member.Unit.onDefValueChanged += OnDefBoostValueChanged;
        }

        private void OnHpValueChanged() 
        {
            fillRectImage.color = member.Color;
            slider.value = member.Unit.currentHP;
            healthUGUI.text = $"HP: {member.Unit.currentHP}";
        }

        private void OnDmgBoostValueChanged(int val, bool condition)
        {
            if (condition)
            {
                if (dmgBoostLvl < 5) dmgBoostBars[val-1].gameObject.SetActive(true);
            }
            else
            {
                dmgBoostBars.ForEach(o => o.gameObject.SetActive(false));
            }

            dmgBoostLvl = val;
        }
        
        private void OnDefBoostValueChanged(int val, bool condition)
        {
            if (condition)
            {
                if (defBoostLvl < 5) defBoostBars[val-1].gameObject.SetActive(true);
            }
            else
            {
                defBoostBars.ForEach(o => o.gameObject.SetActive(false));
            }

            defBoostLvl = val;
        }

        private void OnDisable()
        {
            member.onHpValueChanged -= OnHpValueChanged;
            member.Unit.onDmgValueChanged -= OnDmgBoostValueChanged;
            member.Unit.onDefValueChanged -= OnDefBoostValueChanged;
        }
    }
}