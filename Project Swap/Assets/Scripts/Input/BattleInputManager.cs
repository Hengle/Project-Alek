using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public class BattleInputManager : MonoBehaviour
    {
        public static InputSystemUIInputModule _inputModule;
        public static InventoryInputManager _inventoryInputManager;
        public static Controls _controls;
        
        private static GameObject currentlySelected;

        public static bool _canPressBack;
        public static bool CancelCondition => _inputModule.cancel.action.triggered && _canPressBack;
        private static bool ProfileBoxCondition => _controls.Menu.TopButton.triggered && currentlySelected;
        
        private void Awake()
        {
            _controls = new Controls();
            _controls.Enable();
            
            _inputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
            _inventoryInputManager = FindObjectOfType<InventoryInputManager>();
        }
        
        private void Update()
        {
            currentlySelected = EventSystem.current.currentSelectedGameObject;
            if (ProfileBoxCondition) UIEvents.Trigger(UIEventType.ToggleProfileBox, currentlySelected);
        }
    }
}