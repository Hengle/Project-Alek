using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BattleSystem;
using Characters.Animations;
using MoreMountains.InventoryEngine;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase
    {
        [HideInInspector] public Animator actionPointAnim;
        [Range(0,4)] public int positionInParty;
        public int weaponMight;
        public int magicMight;
        public int weaponAccuracy;
        public int weaponCriticalChance;
        
        public Inventory weaponInventory;
        public Inventory armorInventory;
        [HideInInspector] public BattleOptionsPanel battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;
        private GameObject unitGO;

        public int CurrentAP 
        {
            get => Unit.currentAP;
            set
            {
                Unit.currentAP = value;
                actionPointAnim.SetInteger(AnimationHandler.APVal, Unit.currentAP);
            }
        }
        
        public bool SetAI() => true;

        public void SetAbilityMenuOptions()
        {
            var abilityMenu = battlePanel.transform.Find("Battle Menu").transform.Find("Ability Menu").transform;
            var abilityListIndex = 0;
            
            while (abilities.Count > 5) abilities.Remove(abilities[abilities.Count-1]);

            for (var buttonIndex = 0; buttonIndex < abilities.Count; buttonIndex++)
            {
                var optionButton = abilityMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshPro>().text = abilities[abilityListIndex].name;
                optionButton.transform.Find("Icon").GetComponent<Image>().sprite = abilities[abilityListIndex].icon;
                optionButton.SetActive(true);
                
                var param = abilities[abilityListIndex].GetParameters(abilityListIndex);
                optionButton.GetComponent<Button>().onClick.AddListener(delegate { battleOptionsPanel.GetCommandInformation(param); });

                if (abilities[abilityListIndex].isMultiTarget)
                    optionButton.GetComponent<Button>().onClick.AddListener(delegate { ChooseTarget._isMultiTarget = true; });
                    
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
    }
}
