using System;
using Characters.Animations;
using PixelCrushers;
using SingletonScriptableObject;
using UnityEngine;

namespace Overworld
{
    public class PlayerMovement : MonoBehaviour
    {
        private Animator anim;
        private Controls controls;
        private Rigidbody rb;

        private Vector2 currentMovement;

        private int speed;
        [SerializeField] private int walkSpeed = 5;
        [SerializeField] private int runSpeed = 10;

        protected static bool _isRegistered = false;
        private bool didIRegister = false;
        private bool movementPressed;
        private bool runPressed;

        private void Awake()
        {
            controls = new Controls();
            controls.Overworld.Move.performed += ctx =>
            {
                currentMovement = ctx.ReadValue<Vector2>();
                movementPressed = Math.Abs(currentMovement.x) > 0.01f ||
                                  Math.Abs(currentMovement.y) > 0.01f;
            };
            controls.Overworld.Run.performed += ctx =>
                runPressed = ctx.ReadValueAsButton();
        }

        private void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            anim.SetInteger(AnimationHandler.AnimState, 0);
        }

        private void FixedUpdate() => HandleMovement();

        private void HandleMovement()
        {
            var isRunning = anim.GetBool(AnimationHandler.IsRunningHash);
            var isWalking = anim.GetBool(AnimationHandler.IsWalkingHash);

            anim.SetFloat(AnimationHandler.HorizontalHash, currentMovement.x);
            
            if (movementPressed && !isWalking) { anim.SetBool(AnimationHandler.IsWalkingHash, true); speed = walkSpeed; }
            if (!movementPressed && isWalking) { anim.SetBool(AnimationHandler.IsWalkingHash, false); speed = walkSpeed; }
            
            if (movementPressed && runPressed && !isRunning) { anim.SetBool(AnimationHandler.IsRunningHash, true); speed = runSpeed; }
            if (!movementPressed || !runPressed && isRunning) { anim.SetBool(AnimationHandler.IsRunningHash, false); speed = walkSpeed; }

            var playerTransform = transform;
            var forwardPos = playerTransform.forward * currentMovement.y;
            var rightPos = playerTransform.right * currentMovement.x;
            var finalPos = forwardPos + rightPos;
 
            rb.MovePosition(playerTransform.position + finalPos * (speed * Time.fixedDeltaTime));
        }

        private void OnEnable()
        {
            if (!_isRegistered)
            {
                _isRegistered = true;
                didIRegister = true;
                controls.Enable();
                InputDeviceManager.RegisterInputAction("Confirm", controls.UI.Submit);
                InputDeviceManager.RegisterInputAction("Back", controls.UI.Cancel);
            }
        }

        private void OnDisable()
        {
            PlayerPositionManager.Position = transform.position;
            if (didIRegister)
            {
                _isRegistered = false;
                didIRegister = false;
                controls.Disable();
                InputDeviceManager.UnregisterInputAction("Confirm");
                InputDeviceManager.UnregisterInputAction("Back");
            }
        }
    }
}