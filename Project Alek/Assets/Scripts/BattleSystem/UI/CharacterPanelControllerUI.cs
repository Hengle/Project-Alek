using System.Collections.Generic;
using Characters;
using Characters.Animations;
using Characters.PartyMembers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class CharacterPanelControllerUI : MonoBehaviour
    {
        #region FieldsAndProperties
        
        public PartyMember member;

        private Image fillRectImage;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameUGUI;
        [SerializeField] private TextMeshProUGUI healthUGUI;
        [SerializeField] private Slider slider;
        [SerializeField] private List<GameObject> dmgBoostBars;
        [SerializeField] private List<GameObject> defBoostBars;
        [SerializeField] private List<GameObject> apBars;

        private Animator apBarAnim;
        private Animator dmgBoostAnim;
        private Animator defBoostAnim;

        private int dmgBoostLvl;
        private int defBoostLvl;
        private int currentAP;
        
        #endregion

        private void Awake()
        {
            apBarAnim = transform.Find("AP Bar").GetComponent<Animator>();
            dmgBoostAnim = transform.Find("DmgBoost Bar").GetComponent<Animator>();
            defBoostAnim = transform.Find("DefBoost Bar").GetComponent<Animator>();
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
            apBars.ForEach(o => o.gameObject.SetActive(true));
            
            member.onHpValueChanged += OnHpValueChanged;
            member.Unit.onDmgValueChanged += OnDmgBoostValueChanged;
            member.Unit.onDefValueChanged += OnDefBoostValueChanged;
            member.onApValChanged += OnAPValueChanged;
            
            currentAP = member.maxAP;
            apBarAnim.SetTrigger(AnimationHandler.maxAP);
        }

        private void Update() => apBarAnim.enabled = member.Unit.status != Status.Overexerted;
        
        private void OnHpValueChanged() 
        {
            fillRectImage.color = member.Color;
            slider.value = member.Unit.currentHP;
            healthUGUI.text = $"HP: {member.Unit.currentHP}";
        }

        private void OnAPValueChanged(int val)
        {
            if (val > currentAP)
            {
                for (var i = currentAP; i < val; i++)
                {
                    apBars[i].gameObject.SetActive(true);
                }
            }
            
            else if (val == 0) apBars.ForEach(o => o.gameObject.SetActive(false));
            
            else if (val < currentAP)
            {
                for (var i = currentAP-1; i >= val; i--)
                {
                    apBars[i].gameObject.SetActive(false);
                }
            }
            
            if (val == member.maxAP) apBarAnim.SetBool(AnimationHandler.maxAP, true);
            else if (currentAP == member.maxAP && val != currentAP) apBarAnim.SetBool(AnimationHandler.maxAP, false);

            currentAP = val;
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
                if (dmgBoostLvl == 5) dmgBoostAnim.SetTrigger(AnimationHandler.maxDmgBoost);
            }

            dmgBoostLvl = val;
            if (dmgBoostLvl == 5) dmgBoostAnim.SetTrigger(AnimationHandler.maxDmgBoost);
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
                if (defBoostLvl == 5) defBoostAnim.SetTrigger(AnimationHandler.maxDefBoost);
            }

            defBoostLvl = val;
            if (defBoostLvl == 5) defBoostAnim.SetTrigger(AnimationHandler.maxDefBoost);
        }

        private void OnDisable()
        {
            member.onHpValueChanged -= OnHpValueChanged;
            member.Unit.onDmgValueChanged -= OnDmgBoostValueChanged;
            member.Unit.onDefValueChanged -= OnDefBoostValueChanged;
            member.onApValChanged -= OnAPValueChanged;
        }
    }
}