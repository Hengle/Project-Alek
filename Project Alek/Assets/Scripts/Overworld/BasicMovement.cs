using System;
using UnityEngine;

namespace Overworld
{
    public class BasicMovement : MonoBehaviour
    {
        private Animator anim;
        private SpriteRenderer spriteRenderer;
        private Controls controls;
        private Rigidbody rb;

        private Vector2 currentMovement;
        
        public int speed;
        public int walkSpeed = 5;
        public int runSpeed = 10;
        private int isWalkingHash;
        private int isRunningHash;
        private int horizontalHash;
        
        private bool movementPressed;
        private bool runPressed;

        private void Awake()
        {
            controls = new Controls();
            controls.Overworld.Move.performed += ctx =>
            {
                currentMovement = ctx.ReadValue<Vector2>();
                movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
            };
            controls.Overworld.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
        }

        private void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            isWalkingHash = Animator.StringToHash("isWalking");
            isRunningHash = Animator.StringToHash("isRunning");
            horizontalHash = Animator.StringToHash("Horizontal");
            anim.SetInteger("AnimState", 0);
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            var isRunning = anim.GetBool(isRunningHash);
            var isWalking = anim.GetBool(isWalkingHash);

            anim.SetFloat(horizontalHash, currentMovement.x);
            
            if (movementPressed && !isWalking) { anim.SetBool(isWalkingHash, true); speed = walkSpeed; }
            if (!movementPressed && isWalking) { anim.SetBool(isWalkingHash, false); speed = walkSpeed; }
            
            if (movementPressed && runPressed && !isRunning) { anim.SetBool(isRunningHash, true); speed = runSpeed; }
            if (!movementPressed || !runPressed && isRunning) { anim.SetBool(isRunningHash, false); speed = walkSpeed; }
            
            var newPosition = new Vector3(currentMovement.x, 0, currentMovement.y);
            rb.MovePosition(transform.position + newPosition * (speed * Time.fixedDeltaTime));
        }

        private void OnEnable() => controls.Enable();

        private void OnDisable() => controls.Disable();
        
    }
}
