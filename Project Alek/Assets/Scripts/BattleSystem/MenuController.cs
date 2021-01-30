using System;
using Characters;
using JetBrains.Annotations;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters.Animations;
using ScriptableObjectArchitecture;
using UnityEngine.UI;

namespace BattleSystem
{
    public class MenuController : MonoBehaviour, MMEventListener<MMInventoryEvent>, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        #region FieldsAndProperties

        [SerializeField] private CharacterGameEvent characterTurnEvent;
        [SerializeField] private CharacterGameEvent endOfTurnEvent;
        
        [SerializeField] private GameObject battleMenu;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject abilityMenu;

        [SerializeField] private RadialLayoutGroup mainMenuLayout;
        [SerializeField] private RadialLayoutGroup abilityMenuLayout;

        private GameObject mainMenuFirstSelected;
        private GameObject abilityMenuFirstSelected;

        private Animator animator;
        
        private bool isEnabled;

        #endregion
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            
            mainMenuFirstSelected = mainMenu.transform.Find("Attack Button").gameObject;

            var count = abilityMenu.transform.childCount;
            abilityMenuFirstSelected = abilityMenu.transform.GetChild(count-1).gameObject;

            mainMenuLayout.Arc = 145;
            mainMenuLayout.Offset = 30;
            mainMenuLayout.Radius = 200;
            mainMenuLayout.StartFrom = RadialLayoutGroup.RadialLayoutStart.bottom;
            
            abilityMenuLayout.Arc = 145;
            abilityMenuLayout.Offset = 30;
            abilityMenuLayout.Radius = 200;
            abilityMenuLayout.StartFrom = RadialLayoutGroup.RadialLayoutStart.bottom;
           
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
        }

        private void OnEnable() 
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);

            MMEventManager.AddListener(this);
            characterTurnEvent.AddListener(this);
            endOfTurnEvent.AddListener(this);

            isEnabled = true;
        }

        private void OnDisable()
        {
            characterTurnEvent.RemoveListener(this);
            endOfTurnEvent.RemoveListener(this);
        }

        [UsedImplicitly] public void DisableInput() => BattleInput._controls.Disable();

        #region MenuAnimation

        [UsedImplicitly] private void EvaluateMainMenuState()
        {
            Debug.Log($"Main Menu: {mainMenuLayout.Radius}");
            // if (Math.Abs(mainMenuLayout.Radius) < 0.01f) OpenMainMenu();
            // else CloseMainMenu();
        }

        [UsedImplicitly] private void EvaluateAbilityMenuState()
        {
            Debug.Log($"Ability Menu: {abilityMenuLayout.Radius}");
            // if (Math.Abs(abilityMenuLayout.Radius) < 0.01f) OpenAbilityMenu();
            // else CloseAbilityMenu();
        }
        
        private void OpenMainMenu()
        {
            Debug.Log($"Main Menu: {mainMenuLayout.Radius}");
            // while (Math.Abs(mainMenuLayout.Radius - 200) > 0.001f)
            // {
            //     mainMenuLayout.Radius++;
            // }
        }
        
        private void OpenAbilityMenu()
        {
            Debug.Log($"Ability Menu: {abilityMenuLayout.Radius}");
            // while (Math.Abs(abilityMenuLayout.Radius - 200) > 0.001f)
            // {
            //     abilityMenuLayout.Radius++;
            // }
        }

        private void CloseMainMenu()
        {
            Debug.Log($"Main Menu: {mainMenuLayout.Radius}");
            // while (Math.Abs(mainMenuLayout.Radius) > 0.001f)
            // {
            //     mainMenuLayout.Radius--;
            // }
        }
        
        private void CloseAbilityMenu()
        {
            Debug.Log($"Ability Menu: {abilityMenuLayout.Radius}");
            // while (Math.Abs(abilityMenuLayout.Radius) > 0.001f)
            // {
            //     abilityMenuLayout.Radius--;
            // }
        }

        #endregion
        
        #region SetSelected

        [UsedImplicitly] public void SetMainMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
            BattleInput._controls.Enable();
        }

        [UsedImplicitly] public void SetAbilityMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(abilityMenuFirstSelected);
            BattleInput._controls.Enable();
        }

        public static void SetTargetFirstSelected()
        {
            switch (ChooseTarget._targetOptions)
            {
                case 0: // Enemy
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(SelectableObjectManager._enemySelectable[0].gameObject);
                    break;
                case 1: // Party Member
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(SelectableObjectManager._memberFirstSelected);
                    break;
                case 2: // Both
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(SelectableObjectManager._memberFirstSelected);
                    break;
            }
            
            BattleInput._controls.Enable();
        }

        #endregion

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (BattleEngine.Instance.usingItem) return;
            if (isEnabled && isActiveAndEnabled && eventType.InventoryEventType == MMInventoryEventType.InventoryCloses)
            {
                animator.SetTrigger(AnimationHandler.Panel);
                EventSystem.current.sendNavigationEvents = true;
                return;
            }

            if (isActiveAndEnabled && battleMenu.activeSelf && eventType.InventoryEventType == MMInventoryEventType.InventoryOpens) 
            {
                animator.SetTrigger(AnimationHandler.Panel);
            }
        }

        public void OnEventRaised(UnitBase character, CharacterGameEvent charEvent)
        {
            if (charEvent != characterTurnEvent && charEvent != endOfTurnEvent) return;

            if (character.MenuController == this)
            {
                isEnabled = charEvent == characterTurnEvent;
            }
        }
    }
}
