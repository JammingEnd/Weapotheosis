using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NetworkHandlers;


public class PlayerCameraController : NetworkBehaviour
{
    public Camera _playerCamera;
    public Transform playercamRoot;
    public PlayerMovementHandler _movement;
    private PlayerStatHandler _stats;
    
    private InputSystem_Actions inputActions;

    private Vector2 lookInput;
    public float MouseSensitivity = 1f;
    private float xRotation = 0f;
    private float mouseX, mouseY;
    public Transform cameraPivot;

    private void Update()
    {
        if (!isLocalPlayer) return;
        if(_stats.DisableControls) return;

        float mouseX = lookInput.x * MouseSensitivity;
        float mouseY = lookInput.y * MouseSensitivity;

        // Rotate player body (yaw)
        _movement.AddYaw(mouseX);

        // Rotate camera pivot (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        if (playercamRoot != null)
            playercamRoot.gameObject.SetActive(false);
    }
    
   

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        inputActions.Player.Enable();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        if (playercamRoot != null)
            playercamRoot.gameObject.SetActive(true);

        _stats = GetComponent<PlayerStatHandler>();
        
        
    }
    private void OnDisable()
    {
        if (!isLocalPlayer) return;

        inputActions.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => lookInput = Vector2.zero;
        inputActions.Player.Disable();
    }
}
