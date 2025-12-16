using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;


public class GenericPlayerInputHandler : NetworkBehaviour
{
    private InputSystem_Actions _inputActions;
    
    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }
    
    public override void OnStartLocalPlayer()
    {
        _inputActions.Player.Enable();
        
        _inputActions.Player.Escape.performed += OnEscape;
    }

    private void OnEscape(InputAction.CallbackContext obj)
    {
        if(Cursor.lockState == CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.None;
        else if(Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
        
        // open or close menu
    }
}
