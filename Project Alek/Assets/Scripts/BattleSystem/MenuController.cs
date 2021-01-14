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
using Sirenix.OdinInspector;

namespace BattleSystem
{
    public class MenuController : MonoBehaviour, MMEventListener<MMInventoryEvent>, IGameEventListener<CharacterEvents>
    {
        #region FieldsAndProperties
        
        // [ShowInInspector] public static List<GameObject> _enemySelectable = new List<GameObject>();
        // [ShowInInspector] public static List<GameObject> _memberSelectable = new List<GameObject>();

        private GameObject battleMenu;
        private GameObject mainMenu;
        private GameObject abilityMenu;
        private GameObject mainMenuFirstSelected;
        private GameObject abilityMenuFirstSelected;
        //private GameObject memberFirstSelected;

        private Animator animator;
        [SerializeField] private bool isEnabled;

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

        public void SetTargetFirstSelected()
        {
            switch (ChooseTarget._targetOptions)
            {
                case 0:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(SelectableObjectManager._enemySelectable[0].gameObject);
                    break;
                case 1:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(SelectableObjectManager._memberFirstSelected);
                    break;
                case 2:
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(SelectableObjectManager._memberFirstSelected);
                    break;
            }
            
            BattleInput._controls.Enable();
        }

        // public void SetPartySelectables()
        // {
        //     for (var i = 0; i < _memberSelectable.Count; i++)
        //     {
        //         var unit = _memberSelectable[i].GetComponent<Unit>();
        //         var nav = unit.GetComponent<Selectable>().navigation;
        //
        //         nav.selectOnDown = i + 1 < _memberSelectable.Count?
        //             _memberSelectable[i + 1].gameObject.GetComponent<Button>() :
        //             _memberSelectable[0].gameObject.GetComponent<Button>();
        //     
        //         if (i - 1 >= 0) nav.selectOnUp = _memberSelectable[i - 1].gameObject.GetComponent<Button>();
        //         else if (i == 0) nav.selectOnUp = _memberSelectable[_memberSelectable.Count-1].gameObject.GetComponent<Button>();
        //
        //         nav.selectOnRight = _enemySelectable[i] != null ?
        //             _enemySelectable[i].gameObject.GetComponent<Button>() :
        //             _enemySelectable[_enemySelectable.Count-1].gameObject.GetComponent<Button>();
        //
        //         unit.button.navigation = nav;
        //         if (i == 0) memberFirstSelected = _memberSelectable[0].gameObject;
        //     }
        // }
        //
        // public void SetEnemySelectables()
        // {
        //     for (var i = 0; i < _enemySelectable.Count; i++)
        //     {
        //         var unit = _enemySelectable[i].GetComponent<Unit>();
        //         var nav = unit.GetComponent<Selectable>().navigation;
        //
        //         nav.selectOnDown = i + 1 < _enemySelectable.Count?
        //             _enemySelectable[i + 1].gameObject.GetComponent<Button>() :
        //             _enemySelectable[0].gameObject.GetComponent<Button>();
        //     
        //         if (i - 1 >= 0) nav.selectOnUp = _enemySelectable[i - 1].gameObject.GetComponent<Button>();
        //         else if (i == 0) nav.selectOnUp = _enemySelectable[_enemySelectable.Count-1].gameObject.GetComponent<Button>();
        //         
        //         nav.selectOnLeft = i < _memberSelectable.Count ?
        //             _memberSelectable[i].gameObject.GetComponent<Button>() :
        //             _memberSelectable[_memberSelectable.Count-1].gameObject.GetComponent<Button>();
        //
        //         unit.button.navigation = nav;
        //     }
        // }

        // private static void UpdateEnemySelectables(UnitBase enemy)
        // {
        //     var found = false;
        //     for (var i = 0; i < _enemySelectable.Count; i++)
        //     {
        //         if (_enemySelectable[i] != enemy.Unit.gameObject) continue;
        //             
        //         found = true;
        //         var prevElement = i == 0 ? _enemySelectable.Count - 1 : i - 1;
        //         var nextElement = i == _enemySelectable.Count - 1 ? 0 : i + 1;
        //
        //         if (prevElement == nextElement) return;
        //
        //         var button1 = _enemySelectable[prevElement].GetComponent<Selectable>();
        //         var nav1 = button1.navigation;
        //                 
        //         var button2 = _enemySelectable[nextElement].GetComponent<Selectable>();
        //         var nav2 = button2.navigation;
        //
        //         nav1.selectOnDown = _enemySelectable[nextElement].GetComponent<Selectable>();
        //         nav2.selectOnUp = _enemySelectable[prevElement].GetComponent<Selectable>();
        //
        //         button1.navigation = nav1;
        //         button2.navigation = nav2;
        //     }
        //         
        //     if (found) _enemySelectable.Remove(enemy.Unit.gameObject);
        //     UpdatePartySelectables();
        // }
        //
        // private static void UpdatePartySelectables()
        // {
        //     for (var i = 0; i < _memberSelectable.Count; i++)
        //     {
        //         var button = _memberSelectable[i].GetComponent<Selectable>();
        //         var nav = button.navigation;
        //
        //         if (_enemySelectable.Contains(nav.selectOnRight.gameObject)) continue;
        //         
        //         Logger.Log($"Found yah {_memberSelectable[i].gameObject.name}");
        //         
        //         nav.selectOnRight = i < _enemySelectable.Count ?
        //             _enemySelectable[i].GetComponent<Selectable>() :
        //             _enemySelectable[_enemySelectable.Count - 1].GetComponent<Selectable>();
        //
        //         button.navigation = nav;
        //     }
        // }

        #endregion

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (BattleManager.Instance.usingItem) return;
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
            // if (eventType._eventType == CEventType.CharacterDeath && eventType._character as Enemy)
            // {
            //     UpdateEnemySelectables((UnitBase)eventType._character);
            // }
            if (eventType._eventType != CEventType.CharacterTurn && eventType._eventType != CEventType.EndOfTurn) return;
            if (eventType._character.GetType() != typeof(PartyMember)) return;

            // May be able to delete this
            var character = (PartyMember) eventType._character;
            if (character.battlePanel.GetComponent<MenuController>() == this)
            {
                isEnabled = eventType._eventType == CEventType.CharacterTurn;
            }
        }
    }
}
