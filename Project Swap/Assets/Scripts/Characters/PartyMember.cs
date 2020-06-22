using Abilities;
using BattleSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase, IUnitBase
    {
        [Range(0,4)] public int positionInParty;
        [HideInInspector] public GameObject battlePanel;
        private GameObject unitGO;

        public void GiveCommand(bool isSwapping)
        {
            BattleHandler.battleFuncs.GetCommand(this, isSwapping);
            BattleHandler.performingAction = true;
        }

        public void SetupUnit()
        {
            unit.id = 1;
            unit.level = level;
            unit.status = Status.Normal;
            unit.unitName = characterName;
            unit.nameText.text = characterName.ToUpper();
            unit.CurrentHP = health;
            unit.maxHealthRef = health;
            unit.currentStrength = strength;
            unit.currentMagic = magic;
            unit.currentAccuracy = accuracy;
            unit.currentInitiative = initiative;
            unit.currentCrit = criticalChance;
            unit.currentDefense = defense;
            unit.currentAP = maxAP;
            unit.iUnitRef = this;
        }

        public bool SetAI()
        {
            // Empty for now
            return true;
        }

        public Ability GetAndSetAbility(int index)
        {
            unit.currentAbility = abilities[index];
            return abilities[index];
        }

        public void SetAbilityMenuOptions(BattleOptionsPanel battleOptionsPanel)
        {
            var abilityMenu = battlePanel.transform.GetChild(0).transform.GetChild(1).transform;
            var abilityListIndex = 0;
            
            while (abilities.Count > 5) abilities.Remove(abilities[abilities.Count-1]);

            for (var buttonIndex = 2; buttonIndex < abilities.Count + 2; buttonIndex++)
            {
                var optionButton = abilityMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshPro>().text = abilities[abilityListIndex].name;
                optionButton.SetActive(true);
                
                var param = abilities[abilityListIndex].GetParameters(abilityListIndex);
                optionButton.GetComponent<Button>().onClick.AddListener(delegate { battleOptionsPanel.GetCommandInformation(param); });
                abilityListIndex++;
            }
        }

        public void SetUnitGO(GameObject memberGO, Slider slider, TextMeshProUGUI healthText, ShowDamageSO showDamageSO)
        {
            memberGO.name = characterName;
            unit = memberGO.GetComponent<Unit>();
            
            unit.slider = slider;
            unit.slider.maxValue = health;
            unit.slider.value = health;
            unit.fillRect = slider.fillRect.GetComponent<Image>();
            
            unit.healthText = healthText;
            unit.showDamageSO = showDamageSO;
        }

        public void SetCameras()
        {
            unit.closeUpCam = unit.spriteParentObject.transform.GetChild(0).gameObject;
            unit.closeUpCamCrit = unit.spriteParentObject.transform.GetChild(1).gameObject;
        }
    }
}
