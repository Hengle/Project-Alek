using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSystem
{
    public class BattleInputManager : MonoBehaviour
    {
        public static InventoryInputManager _inventoryInputManager;
        public static Controls _controls;
        
        private static GameObject currentlySelected;

        public static bool _canPressBack;
        public static bool CancelCondition => _controls.Menu.Back.triggered && _canPressBack;
        private static bool ProfileBoxCondition => _controls.Menu.TopButton.triggered && currentlySelected;
        
        private void Awake()
        {
            _controls = new Controls();
            _controls.Enable();
            //_controls.Menu.Confirm.
            
            _inventoryInputManager = FindObjectOfType<InventoryInputManager>();
        }
        
        private void Update()
        {
            currentlySelected = EventSystem.current.currentSelectedGameObject;
            if (ProfileBoxCondition) UIEvents.Trigger(UIEventType.ToggleProfileBox, currentlySelected);
        }
    }
}
