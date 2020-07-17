using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public class BattleInputManager : MonoBehaviour
    {
        public static Controls _controls;

        public static InputSystemUIInputModule _inputModule;
        
        private static GameObject currentlySelected;

        public static bool _canPressBack;
        public static bool _canOpenBox;
        public static bool CancelCondition => _controls.Menu.Back.triggered && _canPressBack;
        private static bool ProfileBoxCondition =>
            _canOpenBox && _controls.Menu.TopButton.triggered && currentlySelected != null;
        
        private void Awake()
        {
            _controls = new Controls();
            _controls.Enable();

            _inputModule = FindObjectOfType<InputSystemUIInputModule>();

            _canOpenBox = true;
        }
        
        private void Update()
        {
            currentlySelected = EventSystem.current.currentSelectedGameObject;
            if (ProfileBoxCondition) UIEvents.Trigger(UIEventType.ToggleProfileBox, currentlySelected);
        }
    }
}
