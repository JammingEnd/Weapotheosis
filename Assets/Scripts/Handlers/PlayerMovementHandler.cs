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
        public float airControlMultiplier = 0.4f;
        public LayerMask groundMask;

        // Movement state
        [SyncVar] private Vector2 movementInput;
        private bool isGrounded;
        private bool _hasDoubleJumped = false;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!isServer) return; // Server-authoritative movement

            CheckGrounded();
            Move();
        }

        #region Input Commands

        [Command]
        private void CmdSetMoveInput(Vector2 input)
        {
            movementInput = input;
        }

        [Command]
        private void CmdJump()
        {
            if (_stats == null || !_stats.Initialized) return;

            bool canDoubleJump = _stats.GetStatValue<bool>(StatType.CanDoubleJump);
            if (isGrounded || (canDoubleJump && !_hasDoubleJumped))
            {
                if (!isGrounded) _hasDoubleJumped = true;

                float jumpForce = _stats.GetStatValue<float>(StatType.JumpHeight);

                // Reset vertical velocity for consistent jump
                Vector3 vel = rb.linearVelocity;
                vel.y = 0;
                rb.linearVelocity = vel;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }

        #endregion

        #region Movement

        [Server]
        private void CheckGrounded()
        {
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
        private void Move()
        {
            if (_stats == null || !_stats.Initialized) return;

            Vector3 wishDir = transform.TransformDirection(
                new Vector3(movementInput.x, 0, movementInput.y)
            );

            float control = isGrounded ? 1f : airControlMultiplier;

            // Apply acceleration
            rb.AddForce(wishDir * moveAcceleration * control, ForceMode.Acceleration);

            // Clamp horizontal speed
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float maxSpeed = _stats.GetStatValue<float>(StatType.MovementSpeed);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
            }
        }

        #endregion

        #region Unity Events

        public override void OnStartServer()
        {
            base.OnStartServer();
            _stats = GetComponent<PlayerStatHandler>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            _stats = GetComponent<PlayerStatHandler>();

            inputActions.Player.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            if (!isLocalPlayer) return;
            inputActions.Player.Disable();
        }

        #endregion

        #region Input Handlers

        private void OnMove(InputAction.CallbackContext ctx)
        {
            if (!isLocalPlayer || (_stats != null && _stats.DisableControls)) return;

            Vector2 input = ctx.ReadValue<Vector2>();
            CmdSetMoveInput(input);
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!isLocalPlayer) return;
            CmdJump();
        }

        #endregion
    }
}
