using System.Collections.Generic;
using BattleSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Characters.PartyMembers
{
    public class MenuController : MonoBehaviour
    {
        [HideInInspector] public List<GameObject> enemySelectable = new List<GameObject>();
        [HideInInspector] public List<GameObject> memberSelectable = new List<GameObject>();

        public Button swapButton;
        private static bool Null => EventSystem.current.currentSelectedGameObject == null;
        private static bool CanSwap => !BattleHandler.partyHasChosenSwap;

        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject abilityMenu;
        [SerializeField] private GameObject mainMenuFirstSelected;
        [SerializeField] private GameObject abilityMenuFirstSelected;

        private GameObject currentlySelected;
        private Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();

            mainMenu = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
            mainMenuFirstSelected = mainMenu.transform.GetChild(0).gameObject;
            abilityMenu = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
            abilityMenuFirstSelected = abilityMenu.transform.GetChild(0).gameObject;
            swapButton = abilityMenu.transform.GetChild(0).GetComponent<Button>();

            BattleHandler.newRound.AddListener(EnableSwap);
            
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
        }

        private void Update() {
            if (!BattleHandler.inputModule.move && !BattleHandler.inputModule.submit) return;
            if (Null) EventSystem.current.SetSelectedGameObject(currentlySelected);
        }

        private void OnEnable()
        {
            swapButton.interactable = CanSwap;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            currentlySelected = mainMenuFirstSelected;
        }
        
        private void EnableSwap() => swapButton.interactable = true;
        [UsedImplicitly] public void DisableInput() => BattleHandler.inputModule.enabled = false;
    
        [UsedImplicitly] public void SetMainMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            currentlySelected = mainMenuFirstSelected;
            BattleHandler.inputModule.enabled = true;
        }

        [UsedImplicitly] public void SetAbilityMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(abilityMenuFirstSelected);
            currentlySelected = abilityMenuFirstSelected;
            BattleHandler.inputModule.enabled = true;
        }

        public void SetTargetFirstSelected()
        {
            switch (ChooseTarget.targetOptions)
            {
                case 0:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(enemySelectable[0]);
                    currentlySelected = enemySelectable[0];
                    break;
                case 1:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(memberSelectable[0]);
                    currentlySelected = memberSelectable[0];
                    break;
                case 2:
                    break;
            }
            BattleHandler.inputModule.enabled = true;
        }

        public bool SetSelectables()
        {
            for (var i = 0; i < enemySelectable.Count; i++)
            {
                var unit = enemySelectable[i].GetComponent<Unit>();
                var nav = unit.GetComponent<Selectable>().navigation;

                nav.selectOnDown = i + 1 < enemySelectable.Count ?
                    enemySelectable[i + 1].gameObject.GetComponent<Button>() : enemySelectable[0].gameObject.GetComponent<Button>();
            
                if (i - 1 >= 0) nav.selectOnUp = enemySelectable[i - 1].gameObject.GetComponent<Button>();
                else if (i == 0) nav.selectOnUp = enemySelectable[enemySelectable.Count-1].gameObject.GetComponent<Button>();

                unit.button.navigation = nav;
            }
        
            for (var i = 0; i < memberSelectable.Count; i++)
            {
                var unit = memberSelectable[i].GetComponent<Unit>();
                var nav = unit.GetComponent<Selectable>().navigation;

                nav.selectOnDown = i + 1 < memberSelectable.Count ?
                    memberSelectable[i + 1].gameObject.GetComponent<Button>() : memberSelectable[0].gameObject.GetComponent<Button>();
            
                if (i - 1 >= 0) nav.selectOnUp = memberSelectable[i - 1].gameObject.GetComponent<Button>();
                else if (i == 0) nav.selectOnUp = memberSelectable[memberSelectable.Count-1].gameObject.GetComponent<Button>();

                unit.button.navigation = nav;
            }
        
            return true;
        }
    }
}
