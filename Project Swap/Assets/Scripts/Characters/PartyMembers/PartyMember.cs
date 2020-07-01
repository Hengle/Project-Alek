using Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BattleSystem;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase
    {
        [HideInInspector] public Animator actionPointAnim;
        [Range(0,4)] public int positionInParty;
        [HideInInspector] public GameObject battlePanel; // Could set this in inspector and just instantiate it and remove it from database
        private GameObject unitGO;
        [HideInInspector] public BattleOptionsPanel battleOptionsPanel;
        
        public int CurrentAP {
            get => Unit.currentAP;
            set {
                Unit.currentAP = value;
                actionPointAnim.SetInteger(AnimationHandler.APVal, Unit.currentAP);
            }
    }
        
        public bool SetAI() => true;

        public void SetAbilityMenuOptions(BattleOptionsPanel panel)
        {
            battleOptionsPanel = Instantiate(panel);
            battleOptionsPanel.character = this;
            
            var abilityMenu = battlePanel.transform.Find("Battle Menu").transform.Find("Ability Menu").transform;
            var abilityListIndex = 0;
            
            while (abilities.Count > 5) abilities.Remove(abilities[abilities.Count-1]);

            for (var buttonIndex = 0; buttonIndex < abilities.Count; buttonIndex++)
            {
                var optionButton = abilityMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshPro>().text = abilities[abilityListIndex].name;
                optionButton.SetActive(true);
                
                var param = abilities[abilityListIndex].GetParameters(abilityListIndex);
                optionButton.GetComponent<Button>().onClick.AddListener(delegate { battleOptionsPanel.GetCommandInformation(param); });

                if (abilities[abilityListIndex].isMultiTarget)
                    optionButton.GetComponent<Button>().onClick.AddListener(delegate { ChooseTarget.isMultiTarget = true; });
                    
                optionButton.GetComponent<InfoBoxScript>().information = 
                    $"{abilities[abilityListIndex].description} ({abilities[abilityListIndex].actionCost} AP)";
                abilityListIndex++;

                if (buttonIndex != abilities.Count - 1) continue;

                var firstOption = abilityMenu.GetChild(0).gameObject;
                var firstOpNav = firstOption.GetComponent<Selectable>().navigation;
                var nav = optionButton.GetComponent<Selectable>().navigation;

                nav.selectOnDown = firstOption.GetComponent<Button>();
                optionButton.GetComponent<Selectable>().navigation = nav;

                firstOpNav.selectOnUp = optionButton.GetComponent<Button>();
                firstOption.GetComponent<Selectable>().navigation = firstOpNav;
            }
        }

        public void SetUnitGO(GameObject memberGO)
        {
            memberGO.name = characterName;
            Unit = memberGO.GetComponent<Unit>();
            
            // Unit.slider = slider;
            // Unit.slider.maxValue = health;
            // Unit.slider.value = health;
            // Unit.fillRect = slider.fillRect.GetComponent<Image>();
            
            //Unit.healthText = healthText;
        }
    }
}
