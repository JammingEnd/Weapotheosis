using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Models.Stats;
using Unity.VisualScripting;

namespace NetworkHandlers
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        public InputSystem_Actions inputActions;
        private PlayerStatHandler _stats;
        
        [Header("Physics")]
        public Rigidbody rb;
        public float moveAcceleration = 30f;
        public float maxSpeed => _stats.Stats.MovementSpeed;
        public float airControlMultiplier = 0.4f;
        
        private Vector2 movementInput;
        private bool isGrounded;
        [SerializeField] private LayerMask groundMask;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            CmdSetMoveInput(movementInput);
        }
        
        [ServerCallback]
        private void FixedUpdate()
        {
            
            if (!isServer) return;
            CheckGrounded();
            Move();
        }
        
        [Command]
        private void CmdSetMoveInput(Vector2 input)
        {
            movementInput = input;
        }

        [Server]
        void CheckGrounded()
        {
            // grounded check via spherecast
            float radius = 0.3f;
            float castDistance = 0.4f;

            Vector3 origin = transform.position + Vector3.up * (radius + 0.1f);
            isGrounded = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out RaycastHit hit,
                castDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (_hasDoubleJumped && isGrounded)
            {
                _hasDoubleJumped = false;
            }
        }

        [Server]
        void Move()
        {
            Vector3 wishDir = transform.TransformDirection(
                new Vector3(movementInput.x, 0, movementInput.y)
            );

            float control = isGrounded ? 1f : airControlMultiplier;

            Vector3 velocity = rb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

            // Apply acceleration
            rb.AddForce(wishDir * moveAcceleration * control, ForceMode.Acceleration);

            // Clamp max horizontal speed
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(clamped.x, velocity.y, clamped.z);
            }
        }
        
        void OnJump(InputAction.CallbackContext ctx)
        {
            if (!isLocalPlayer) return;

            CmdJump();
        }
        
        private bool _hasDoubleJumped = false;
        [Command]
        void CmdJump()
        {
            if (isGrounded || _stats.Stats.CanDoubleJump)
            {
                if (!isGrounded) _hasDoubleJumped = true;

                float jumpForce = _stats.Stats.JumpHeight;

                // Reset vertical velocity to make jumps consistent
                Vector3 vel = rb.linearVelocity;
                vel.y = 0;
                rb.linearVelocity = vel;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }

        }
        public override void OnStartServer()
        {
            base.OnStartServer();
            _stats = GetComponent<PlayerStatHandler>();
        }
        public override void OnStartLocalPlayer()
        {
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;
            
            _stats = GetComponent<PlayerStatHandler>();
        }

        private void OnDisable()
        {
            if (isLocalPlayer)
            {
                inputActions.Player.Disable();
            }
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            movementInput = ctx.ReadValue<Vector2>();
        }
        
    }
}

