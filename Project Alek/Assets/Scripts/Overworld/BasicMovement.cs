using System;
using UnityEngine;

namespace Overworld
{
    public class BasicMovement : MonoBehaviour
    {
        private Animator anim;
        private Controls controls;
        private Rigidbody rb;

        private Vector2 currentMovement;
        
        private int speed;
        public int walkSpeed = 5;
        public int runSpeed = 10;
        
        private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
        private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
        private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        private static readonly int AnimState = Animator.StringToHash("AnimState");

        private bool movementPressed;
        private bool runPressed;

        private void Awake()
        {
            controls = new Controls();
            controls.Overworld.Move.performed += ctx =>
            {
                currentMovement = ctx.ReadValue<Vector2>();
                movementPressed = Math.Abs(currentMovement.x) > 0.01f || Math.Abs(currentMovement.y) > 0.01f;
            };
            controls.Overworld.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
        }

        private void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            anim.SetInteger(AnimState, 0);
        }

        private void Update() => HandleMovement();

        private void HandleMovement()
        {
            var isRunning = anim.GetBool(IsRunningHash);
            var isWalking = anim.GetBool(IsWalkingHash);

            anim.SetFloat(HorizontalHash, currentMovement.x);
            
            if (movementPressed && !isWalking) { anim.SetBool(IsWalkingHash, true); speed = walkSpeed; }
            if (!movementPressed && isWalking) { anim.SetBool(IsWalkingHash, false); speed = walkSpeed; }
            
            if (movementPressed && runPressed && !isRunning) { anim.SetBool(IsRunningHash, true); speed = runSpeed; }
            if (!movementPressed || !runPressed && isRunning) { anim.SetBool(IsRunningHash, false); speed = walkSpeed; }
            
            var newPosition = new Vector3(currentMovement.x, 0, currentMovement.y);
            rb.MovePosition(transform.position + newPosition * (speed * Time.fixedDeltaTime));
        }

        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();
    }
}