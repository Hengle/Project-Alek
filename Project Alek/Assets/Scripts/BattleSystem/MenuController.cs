using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Characters;
using Characters.Animations;
using Characters.PartyMembers;
using Input;

namespace BattleSystem
{
    public class MenuController : MonoBehaviour, MMEventListener<MMInventoryEvent>, IGameEventListener<CharacterEvents>
    {
        #region FieldsAndProperties
        
        [HideInInspector] public List<GameObject> enemySelectable = new List<GameObject>();
        [HideInInspector] public List<GameObject> memberSelectable = new List<GameObject>();

        private GameObject battleMenu;
        private GameObject mainMenu;
        private GameObject abilityMenu;
        private GameObject mainMenuFirstSelected;
        private GameObject abilityMenuFirstSelected;

        private GameObject previousFirstSelected;
        private GameObject memberFirstSelected;

        private bool isEnabled;
        
        #endregion
        
        private void Awake()
        {
            battleMenu = transform.Find("Battle Menu").gameObject;
            
            mainMenu = battleMenu.gameObject.transform.Find("Main Options").gameObject;
            mainMenuFirstSelected = mainMenu.transform.GetChild(0).gameObject;
            
            abilityMenu = transform.Find("Battle Menu").gameObject.transform.Find("Ability Menu").gameObject;
            abilityMenuFirstSelected = abilityMenu.transform.GetChild(0).gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
        }

        private void OnEnable() 
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            previousFirstSelected = memberFirstSelected;
            MMEventManager.AddListener(this);
            GameEventsManager.AddListener(this);
        }
        
        private void UpdateSelectables() // If I use this later, remember to make an event for it
        {
            memberSelectable = memberSelectable.OrderBy
                (go => go.transform.parent.GetSiblingIndex()).ToList();

            SetPartySelectables();
        }

        [UsedImplicitly]
        public void DisableInput() => BattleInputManager._controls.Disable();
    
        [UsedImplicitly] public void SetMainMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            previousFirstSelected = mainMenuFirstSelected;
            BattleInputManager._controls.Enable();
        }

        [UsedImplicitly] public void SetAbilityMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(abilityMenuFirstSelected);
            previousFirstSelected = abilityMenuFirstSelected;
            BattleInputManager._controls.Enable();
        }

        public void SetTargetFirstSelected()
        {
            switch (ChooseTarget._targetOptions)
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
            
            BattleInputManager._controls.Enable();
        }

        public bool SetPartySelectables()
        {
            for (var i = 0; i < memberSelectable.Count; i++)
            {
                var unit = memberSelectable[i].GetComponent<Unit>();
                var nav = unit.GetComponent<Selectable>().navigation;

                nav.selectOnDown = i + 1 < memberSelectable.Count?
                    memberSelectable[i + 1].gameObject.GetComponent<Button>() :
                    memberSelectable[0].gameObject.GetComponent<Button>();
            
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

                nav.selectOnDown = i + 1 < enemySelectable.Count?
                    enemySelectable[i + 1].gameObject.GetComponent<Button>() :
                    enemySelectable[0].gameObject.GetComponent<Button>();
            
                if (i - 1 >= 0) nav.selectOnUp = enemySelectable[i - 1].gameObject.GetComponent<Button>();
                else if (i == 0) nav.selectOnUp = enemySelectable[enemySelectable.Count-1].gameObject.GetComponent<Button>();

                unit.button.navigation = nav;
            }
            
            return true;
        }

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (isEnabled && isActiveAndEnabled && eventType.InventoryEventType == MMInventoryEventType.InventoryCloses)
            {
                GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
                EventSystem.current.SetSelectedGameObject(previousFirstSelected);
                EventSystem.current.sendNavigationEvents = true;
                return;
            }

            if (isActiveAndEnabled && battleMenu.activeSelf && eventType.InventoryEventType == MMInventoryEventType.InventoryOpens) 
            {
                GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            }
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterTurn && eventType._eventType != CEventType.EndOfTurn) return;
            if (eventType._character.GetType() != typeof(PartyMember)) return;

            var character = (PartyMember) eventType._character;
            if (character.battlePanel.GetComponent<MenuController>() == this)
            {
                isEnabled = eventType._eventType == CEventType.CharacterTurn;
            }
        }
    }
}
