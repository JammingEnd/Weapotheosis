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
        
        private Vector2 movementInput;
        [SyncVar] private bool isGrounded;
        private bool _hasDoubleJumped = false;
        [SerializeField] private LayerMask groundMask;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }
       
        private void FixedUpdate()
        {
            // PLAN: 
            // if i am the server, move normally. no calls just local input
            // if i am a client, send input to server and have server move me
            if(isServer)
                CheckGrounded(); 
            
            if (isServer && isLocalPlayer)
            {
                Move(movementInput);
                
            }
            else if(isClient && isLocalPlayer)
            {
                CmdMove();
            }
        }
        /// <summary>
        /// server should move the player
        /// </summary>
        void Move(Vector2 input)
        {
            
            if (_stats == null || !_stats.Initialized) return;
    
            Debug.Log("Speed: " + maxSpeed);
            
            Vector3 wishDir = transform.TransformDirection(
                new Vector3(input.x, 0, input.y)
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

        [Command]
        private void CmdMove()
        {
            MoveRpc(movementInput);
        }

        [ClientRpc]
        private void MoveRpc(Vector2 input)
        {
            Move(input);
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

        /// <summary>
        /// server checks if player is grounded
        /// </summary>
        [Server]
        private void CheckGrounded()
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

        private void Jump(bool isGrounded)
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

        [Command]
        private void CmdJump()
        {
            RpcJump(isGrounded);
        }
        
        [ClientRpc]
        private void RpcJump(bool isGrounded)
        {
            Jump(isGrounded);
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
        /// send movementinput from client to server
        /// </summary>
        /// <param name="input"></param>
        [Command]
        private void CmdSetMoveInput(Vector2 input)
        {
            movementInput = input;
        }
        
        /// <summary>
        /// local player jump input
        /// </summary>
        /// <param name="ctx"></param>
        void OnJump(InputAction.CallbackContext ctx)
        {
            if (isServer && isLocalPlayer)
            {
                Jump(isGrounded);
                
            }
            else if(isClient && isLocalPlayer)
            {
                CmdJump();
            }
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

       
        
    }
}

