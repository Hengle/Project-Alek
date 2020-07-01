using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BattleSystem;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;

namespace Characters.PartyMembers
{
    public class MenuController : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public InventoryInputManager inventoryInputManager;
        [HideInInspector] public List<GameObject> enemySelectable = new List<GameObject>();
        [HideInInspector] public List<GameObject> memberSelectable = new List<GameObject>();

        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject abilityMenu;
        [SerializeField] private GameObject mainMenuFirstSelected;
        [SerializeField] private GameObject abilityMenuFirstSelected;

        private GameObject previousFirstSelected;
        private GameObject memberFirstSelected;

        private void Awake()
        {
            mainMenu = transform.Find("Battle Menu").gameObject.transform.Find("Main Options").gameObject;
            mainMenuFirstSelected = mainMenu.transform.GetChild(0).gameObject;
            
            abilityMenu = transform.Find("Battle Menu").gameObject.transform.Find("Ability Menu").gameObject;
            abilityMenuFirstSelected = abilityMenu.transform.GetChild(0).gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);

            inventoryInputManager = FindObjectOfType<InventoryInputManager>();
            //inventoryInputManager.op
            //MMGameEvent.
            //MMEventManager.AddListener(Void Test);
        }

        private void Update()
        {
            // if (MMGameEvent.Trigger())
            // {
            //     
            // }
        }

        private void Test()
        {
            Logger.Log("Menu Controller knows what's up");
        }

        private void UpdateSelectables() // If I use this later, remember to make an event for it
        {
            memberSelectable = memberSelectable.OrderBy
                (go => go.transform.parent.GetSiblingIndex()).ToList();

            SetPartySelectables();
        }
        
        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            previousFirstSelected = memberFirstSelected;
            MMEventManager.AddListener(this);
            //MMEventStartListening<MMGameEvent>();
        }

        [UsedImplicitly] public void DisableInput() => BattleManager.inputModule.enabled = false;
    
        [UsedImplicitly] public void SetMainMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            previousFirstSelected = mainMenuFirstSelected;
            BattleManager.inputModule.enabled = true;
        }

        [UsedImplicitly] public void SetAbilityMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(abilityMenuFirstSelected);
            previousFirstSelected = abilityMenuFirstSelected;
            BattleManager.inputModule.enabled = true;
        }

        public void SetTargetFirstSelected()
        {
            switch (ChooseTarget.targetOptions)
            {
                case 0:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(enemySelectable[0].gameObject);
                    previousFirstSelected = enemySelectable[0].gameObject;
                    break;
                case 1:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(memberFirstSelected);
                    previousFirstSelected = memberFirstSelected;
                    break;
                case 2:
                    break;
            }
            
            BattleManager.inputModule.enabled = true;
        }

        public bool SetPartySelectables()
        {
            for (var i = 0; i < memberSelectable.Count; i++)
            {
                var unit = memberSelectable[i].GetComponent<Unit>();
                var nav = unit.GetComponent<Selectable>().navigation;

                nav.selectOnDown = i + 1 < memberSelectable.Count ?
                    memberSelectable[i + 1].gameObject.GetComponent<Button>() : memberSelectable[0].gameObject.GetComponent<Button>();
            
                if (i - 1 >= 0) nav.selectOnUp = memberSelectable[i - 1].gameObject.GetComponent<Button>();
                else if (i == 0) nav.selectOnUp = memberSelectable[memberSelectable.Count-1].gameObject.GetComponent<Button>();

                unit.button.navigation = nav;
                if (i == 0) memberFirstSelected = memberSelectable[0].gameObject;
            }

            return true;
        }

        public bool SetEnemySelectables()
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

            return true;
        }

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (!isActiveAndEnabled || eventType.InventoryEventType != MMInventoryEventType.InventoryCloses) return;

            EventSystem.current.SetSelectedGameObject(previousFirstSelected);
            EventSystem.current.sendNavigationEvents = true;
        }
    }
}
