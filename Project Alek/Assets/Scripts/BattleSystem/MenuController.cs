using JetBrains.Annotations;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters.Animations;
using Characters.PartyMembers;

namespace BattleSystem
{
    public class MenuController : MonoBehaviour, MMEventListener<MMInventoryEvent>, IGameEventListener<CharacterEvents>
    {
        #region FieldsAndProperties

        private GameObject battleMenu;
        private GameObject mainMenu;
        private GameObject abilityMenu;
        private GameObject mainMenuFirstSelected;
        private GameObject abilityMenuFirstSelected;

        private Animator animator;
        
        private bool isEnabled;

        #endregion
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            
            battleMenu = transform.Find("Mask").transform.Find("Battle Menu").gameObject;
            
            mainMenu = battleMenu.gameObject.transform.Find("Main Options").gameObject;
            mainMenuFirstSelected = mainMenu.transform.GetChild(0).gameObject;
            
            abilityMenu = transform.Find("Mask").transform.Find("Battle Menu").gameObject.transform.Find("Ability Menu").gameObject;
            abilityMenuFirstSelected = abilityMenu.transform.GetChild(0).gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
        }

        private void OnEnable() 
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);

            MMEventManager.AddListener(this);
            GameEventsManager.AddListener(this);

            isEnabled = true;
        }

        [UsedImplicitly] public void DisableInput() => BattleInput._controls.Disable();

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

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterTurn && eventType._eventType != CEventType.EndOfTurn) return;
            if (eventType._character.GetType() != typeof(PartyMember)) return;

            var character = (PartyMember) eventType._character;
            if (character.MenuController == this)
            {
                isEnabled = eventType._eventType == CEventType.CharacterTurn;
            }
        }
    }
}
