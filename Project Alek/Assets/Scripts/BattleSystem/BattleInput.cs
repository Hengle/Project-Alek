using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Input
{
    public class BattleInput : MonoBehaviour
    {
        public static Controls _controls;
        public static InputSystemUIInputModule _inputModule;

        public static bool _canPressBack;
        public static bool _canOpenBox;
        public static bool CancelCondition => _controls.Menu.Back.triggered && _canPressBack;

        private void Awake()
        {
            _controls = new Controls();
            _controls.Enable();
            _inputModule = FindObjectOfType<InputSystemUIInputModule>();
            _canOpenBox = true;
        }
    }
}
