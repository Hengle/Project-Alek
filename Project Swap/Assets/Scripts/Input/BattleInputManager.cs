using Characters.PartyMembers;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public class BattleInputManager : MonoBehaviour, IGameEventListener<UIEvents>
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

        private void UpdateInventoryDisplay(PartyMember character)
        {
            _inventoryInputManager.TargetInventoryContainer = character.inventoryDisplay.GetComponent<CanvasGroup>();
            _inventoryInputManager.TargetInventoryDisplay = character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();
            _inventoryInputManager.TargetInventoryDisplay.SetupInventoryDisplay();
        }

        public void OnGameEvent(UIEvents eventType)
        {
            // if (eventType._eventType != UIEventType.UpdateInventoryDisplay) return;
            // if (eventType._character.GetType() != typeof(PartyMember)) return;
            //
            // var character = (PartyMember) eventType._character;
            // UpdateInventoryDisplay(character);
        }
    }
}