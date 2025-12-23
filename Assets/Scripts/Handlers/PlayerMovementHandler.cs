using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Models.Stats;
using Unity.VisualScripting;

namespace NetworkHandlers
{
    public class PlayerMovementHandler : NetworkBehaviour, ICharacterController
    {
        public InputSystem_Actions inputActions;
        private PlayerStatHandler _stats;
        public KinematicCharacterMotor _motor;

        private float _yaw;
        private float _pendingYaw;
        public float RotationSharpness = 15f;

        public void AddYaw(float amount)
        {
            if(!isLocalPlayer) return;
            _pendingYaw += amount;
            CmdRotate(amount);
        }
        
        [Header("Ground Movement")]
        private float _maxMoveSpeed => _stats.GetStatValue<float>(StatType.MovementSpeed);
        public float AccelerationSharpness = 15f;
        public float Drag = 0.1f;
        Vector3 _moveInputVector;
        Vector3 _previousMoveInputVector;

        [Header("Air Movement")] 
        public float AirSpeedModifier = 0.8f;
        public float AirControlSharpness = 2f;
        
        [Header("Jumping")]
        private float _jumpForce => _stats.GetStatValue<float>(StatType.JumpHeight);
        private bool _canDoubleJump => _stats.GetStatValue<bool>(StatType.CanDoubleJump);
        private bool _hasDoubleJumped;
        private bool _hasJumped;
        private bool _jumpRequested;
        private bool _jumpedThisFrame;
        private float _timeSinceLastJump;
        
        [Header("External forces")]
        public float KnockbackRecoverySpeed = 5f;
        private float _gravity => _stats.GetStatValue<float>(StatType.GravityScale);
        Vector3 _knockbackVelocity;
        

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _motor.CharacterController = this;
            _stats = GetComponent<PlayerStatHandler>();
        }
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;

            if (isServer)
            {
                _motor.enabled = false;
            }
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();
            CmdMove(inputVector);
            
        }
        [Command]
        void CmdMove(Vector2 moveInput)
        {
            _moveInputVector = moveInput;
            _moveInputVector = new Vector3(moveInput.x, 0f, moveInput.y);
            _moveInputVector = Vector3.ClampMagnitude(_moveInputVector, 1f);
        }
        [Command]
        void CmdRotate(float yaw)
        {
            _pendingYaw += yaw;
        }
        
        private void OnDisable()
        {
            if (isLocalPlayer)
            {
                inputActions.Player.Disable();
            }
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            CmdJump();
        }
        
        [Command]
        void CmdJump()
        {
            Jump();
        }

        [Server]
        public void Jump()
        {
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                _jumpRequested = true;
            }
            if (_hasJumped && (_canDoubleJump && !_hasDoubleJumped))
            {
                _jumpRequested = true;
            }
        }
        
        [Server]
        public void ApplyKnockback(Vector3 direction, float force)
        {
            _knockbackVelocity += direction.normalized * force;
        }

        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_pendingYaw != 0f)
            {
                _yaw += _pendingYaw;
                _pendingYaw = 0f;
            }

            currentRotation = Quaternion.Euler(0f, _yaw, 0f);
        }
        

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetMovementVelocity = Vector3.zero;
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                Vector3 moveDir =
                    (_motor.CharacterForward * _moveInputVector.z) +
                    (_motor.CharacterRight * _moveInputVector.x);

                moveDir = Vector3.ClampMagnitude(moveDir, 1f);

                Vector3 targetVelocity = moveDir * _maxMoveSpeed;

                currentVelocity = Vector3.Lerp(
                    currentVelocity,
                    targetVelocity,
                    1f - Mathf.Exp(-AccelerationSharpness * deltaTime)
                );

                currentVelocity.y = 0f;
            }
            else
            {
                Vector3 verticalVel = Vector3.Project(currentVelocity, _motor.CharacterUp);
                Vector3 horizontalVel = currentVelocity - verticalVel;
                
                Vector3 inputDir =
                    (_motor.CharacterForward * _moveInputVector.z) +
                    (_motor.CharacterRight * _moveInputVector.x);
                inputDir = Vector3.ClampMagnitude(inputDir, 1f);

                float airControl = 1f; // how effective you can steer (tweak)
                Vector3 desiredVelChange = inputDir * _maxMoveSpeed * AirSpeedModifier - horizontalVel;

                // Only apply a fraction of the difference
                horizontalVel += desiredVelChange * airControl * deltaTime;
                
                currentVelocity = horizontalVel + verticalVel;
                currentVelocity += Vector3.down * _gravity * deltaTime;
            }
            
            _jumpedThisFrame = false;
            _timeSinceLastJump += deltaTime;
            // jumping
            if (_jumpRequested)
            {
                _motor.ForceUnground();

                Vector3 jumpDirection = _motor.CharacterUp;
                if (_hasJumped) 
                {
                    // Double jump
                    Vector3 verticalVel = Vector3.Project(currentVelocity, _motor.CharacterUp);
                    Vector3 inputDir =
                        (_motor.CharacterForward * _moveInputVector.z) +
                        (_motor.CharacterRight * _moveInputVector.x);
                    
                    inputDir = Vector3.ClampMagnitude(inputDir, 1f);
                    inputDir *= _maxMoveSpeed;
                    
                    currentVelocity = verticalVel + inputDir; // reset velocity for double jump
                    _hasDoubleJumped = true;
                }
                else
                {
                    // First jump
                    _hasJumped = true;
                }

                currentVelocity += (jumpDirection * _jumpForce) - Vector3.Project(currentVelocity, jumpDirection);
                _jumpedThisFrame = true;
                _jumpRequested = false;
            }
            
            // gravity 
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                if (currentVelocity.y < 0f)
                    currentVelocity.y = 0f;
            }
            else
            {
                currentVelocity += Vector3.down * _gravity * deltaTime;
            }

            _previousMoveInputVector = _moveInputVector;
            // drag 
            currentVelocity *= (1f / (1f + ( Drag * deltaTime)));
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            _hasJumped = false;
            _hasDoubleJumped = false;
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }
    }
}

