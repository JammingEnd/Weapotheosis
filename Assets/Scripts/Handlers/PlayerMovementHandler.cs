using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Models.Stats;

namespace NetworkHandlers
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerStatHandler))]
    public class PlayerMovementHandler : NetworkBehaviour
    {
        [Header("References")]
        public Rigidbody rb;
        private PlayerStatHandler _stats;

        [Header("Movement Settings")]
        public float moveAcceleration = 30f;
        public float airControlMultiplier = 0.4f;
        private Vector2 movementInput;

        [Header("Grounding")]
        [SerializeField] private LayerMask groundMask;
        private bool isGrounded;
        private bool hasDoubleJumped = false;

        private InputSystem_Actions inputActions;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _stats = GetComponent<PlayerStatHandler>();
            inputActions = new InputSystem_Actions();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += ctx => OnMove(ctx);
            inputActions.Player.Move.canceled += ctx => OnMove(ctx);
            inputActions.Player.Jump.performed += ctx => OnJump();
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
            if (!isLocalPlayer || _stats.DisableControls) return;

            Vector2 input = ctx.ReadValue<Vector2>();
            movementInput = input;
            CmdSetMoveInput(input);
        }

        private void OnJump()
        {
            if (!isLocalPlayer || _stats.DisableControls) return;
            CmdJump();
        }

        #region Commands

        [Command]
        private void CmdSetMoveInput(Vector2 input)
        {
            movementInput = input; // update server-side
        }

        [Command]
        private void CmdJump()
        {
            if (!_stats.Initialized) return;

            if (isGrounded || (_stats.GetStatValue<bool>(StatType.CanDoubleJump) && !hasDoubleJumped))
            {
                if (!isGrounded) hasDoubleJumped = true;

                float jumpForce = _stats.GetStatValue<float>(StatType.JumpHeight);

                Vector3 vel = rb.linearVelocity;
                vel.y = 0; // reset vertical velocity for consistent jumps
                rb.linearVelocity = vel;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }

        #endregion

        #region Server Movement

        [ServerCallback]
        private void FixedUpdate()
        {
            if (!_stats.Initialized) return;

            CheckGrounded();
            Move();
        }

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

            if (isGrounded)
            {
                hasDoubleJumped = false;
            }
        }

        [Server]
        private void Move()
        {
            if (_stats == null) return;

            Vector3 wishDir = transform.TransformDirection(new Vector3(movementInput.x, 0, movementInput.y));
            float control = isGrounded ? 1f : airControlMultiplier;

            Vector3 velocity = rb.linearVelocity;
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);

            // Apply acceleration
            rb.AddForce(wishDir * moveAcceleration * control, ForceMode.Acceleration);

            // Clamp horizontal speed
            float maxSpeed = _stats.GetStatValue<float>(StatType.MovementSpeed);
            if (horizontalVel.magnitude > maxSpeed)
            {
                Vector3 clamped = horizontalVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(clamped.x, velocity.y, clamped.z);
            }
        }

        #endregion
    }
}
