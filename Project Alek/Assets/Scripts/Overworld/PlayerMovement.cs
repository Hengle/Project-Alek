﻿using System;
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

        private void FixedUpdate() => HandleMovement();

        private void HandleMovement()
        {
            var isRunning = anim.GetBool(IsRunningHash);
            var isWalking = anim.GetBool(IsWalkingHash);

            anim.SetFloat(HorizontalHash, currentMovement.x);
            
            if (movementPressed && !isWalking) { anim.SetBool(IsWalkingHash, true); speed = walkSpeed; }
            if (!movementPressed && isWalking) { anim.SetBool(IsWalkingHash, false); speed = walkSpeed; }
            
            if (movementPressed && runPressed && !isRunning) { anim.SetBool(IsRunningHash, true); speed = runSpeed; }
            if (!movementPressed || !runPressed && isRunning) { anim.SetBool(IsRunningHash, false); speed = walkSpeed; }

            var playerTransform = transform;
            var forwardPos = playerTransform.forward * currentMovement.y;
            var rightPos = playerTransform.right * currentMovement.x;
            var finalPos = forwardPos + rightPos;
 
            rb.MovePosition(playerTransform.position + finalPos * (speed * Time.fixedDeltaTime));
        }

        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();
    }
}