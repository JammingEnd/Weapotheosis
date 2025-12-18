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
        public float maxSpeed => _stats.GetStatValue<float>(StatType.MovementSpeed);
        public float airControlMultiplier = 0.4f;
        
        [SyncVar] private Vector2 movementInput;
        [SyncVar] private bool isGrounded;
        [SyncVar] private bool _hasDoubleJumped = false;
        [SerializeField] private LayerMask groundMask;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }
        
        /// <summary>
        /// updating on server
        /// </summary>
        private void FixedUpdate()
        {
            if (!isServer) return;
            
            CheckGrounded();
            Move();
        }
        
        /// <summary>
        /// send movementinput from client to server
        /// </summary>
        /// <param name="input"></param>
        [Command]
        private void CmdSetMoveInput(Vector2 input)
        {
            movementInput = input;
        }

        /// <summary>
        /// server checks if player is grounded
        /// </summary>
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

        /// <summary>
        /// server should move the player
        /// </summary>
        [Server]
        void Move()
        {
            if (_stats == null || !_stats.Initialized) return;
    
            Vector3 wishDir = transform.TransformDirection(
                new Vector3(movementInput.x, 0, movementInput.y)
            );

            float control = isGrounded ? 1f : airControlMultiplier;

            rb.AddForce(wishDir * moveAcceleration * control, ForceMode.Acceleration);

            // Clamp speed
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
            }
        }
        
        /// <summary>
        /// local player jump input
        /// </summary>
        /// <param name="ctx"></param>
        void OnJump(InputAction.CallbackContext ctx)
        {
            if (!isLocalPlayer) return;
            
            Debug.Log("Jump input received");
            CmdJump();
        }
        
        /// <summary>
        /// send jump command to server
        /// </summary>
        [Command]
        void CmdJump()
        {
            if (isGrounded || _stats.GetStatValue<bool>(StatType.CanDoubleJump) && !_hasDoubleJumped)
            {
                if (!isGrounded) _hasDoubleJumped = true;

                float jumpForce = _stats.GetStatValue<float>(StatType.JumpHeight);

                // Reset vertical velocity to make jumps consistent
                Vector3 vel = rb.linearVelocity;
                vel.y = 0;
                rb.linearVelocity = vel;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }

        }
        
        /// <summary>
        /// get local stats
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            _stats = GetComponent<PlayerStatHandler>();
        }
        
        /// <summary>
        /// enable input actions for local player
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;
            
            _stats = GetComponent<PlayerStatHandler>();
        }

        /// <summary>
        /// disable input
        /// </summary>
        private void OnDisable()
        {
            if (isLocalPlayer)
            {
                inputActions.Player.Disable();
            }
        }

        /// <summary>
        /// local player move input
        /// </summary>
        /// <param name="ctx"></param>
        private void OnMove(InputAction.CallbackContext ctx)
        {
            if (!isLocalPlayer) return;
            if(_stats.DisableControls) return;
            
            Debug.Log("Move input received");

            movementInput = ctx.ReadValue<Vector2>();
            CmdSetMoveInput(movementInput);
        }
        
    }
}

