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
        private Vector2 movementInput;
        
        private PlayerStatHandler _stats;
        
        public float MovementSpeed => (float)_stats.GetStat(StatType.MovementSpeed);

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            Vector3 inputDir = new Vector3(movementInput.x, 0, movementInput.y);
            Vector3 moveDir = transform.TransformDirection(inputDir); 
            transform.position += moveDir * MovementSpeed * Time.fixedDeltaTime;

        }

        public override void OnStartLocalPlayer()
        {
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
            
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

