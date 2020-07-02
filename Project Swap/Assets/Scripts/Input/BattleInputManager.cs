using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public class BattleInputManager : MonoBehaviour
    {
        public static InputSystemUIInputModule _inputModule;
        public static InventoryInputManager _inventoryInputManager;
        public static Controls _controls;

        public static bool _canPressBack;
        public static bool CancelCondition => _inputModule.cancel.action.triggered && _canPressBack;

        private void Awake()
        {
            _controls = new Controls();
            _controls.Enable();
            
            _inputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
            _inventoryInputManager = FindObjectOfType<InventoryInputManager>();
        }
    }
}