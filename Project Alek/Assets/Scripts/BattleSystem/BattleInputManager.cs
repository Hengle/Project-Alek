using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem
{
    public class BattleInputManager : MonoBehaviour
    {
        public InputAction parryButton;

        private void OnEnable()
        {
            parryButton.Enable();
        }

        private void OnDisable()
        {
            parryButton.Disable();
        }
    }
}
