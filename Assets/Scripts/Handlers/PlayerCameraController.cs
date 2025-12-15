using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;



public class PlayerCameraController : NetworkBehaviour
{
    public Camera _playerCamera;
    
    private InputSystem_Actions inputActions;

    public float MouseSensitivity = 2f;
    private float xRotation = 0f;
    private float mouseX, mouseY;
    public Transform cameraPivot;

    private void Update()
    {
        if (!isLocalPlayer) return;

        float mouseX = lookInput.x * MouseSensitivity;
        float mouseY = lookInput.y * MouseSensitivity;

        // Rotate player body (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera pivot (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    
    private Vector2 lookInput;

    public override void OnStartLocalPlayer()
    {
        inputActions.Player.Enable();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        _playerCamera.gameObject.SetActive(true);

        if (isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }
}
