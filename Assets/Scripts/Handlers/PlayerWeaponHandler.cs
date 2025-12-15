using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models.Stats;
using UnityEngine.InputSystem;

public class PlayerWeaponHandler : NetworkBehaviour
{
    public InputSystem_Actions inputActions;

    float FireRate => (float)_stats.GetStat(StatType.GunAttackSpeed);
    
    private float _currentCooldown;
    private PlayerStatHandler _stats;

    private bool _isFiring;
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        if (_currentCooldown > 0f)
            _currentCooldown -= Time.fixedDeltaTime;
    }

    [Client]
    private void Update()
    {
        if (!isLocalPlayer) return;

        if (_isFiring && _currentCooldown <= 0f)
        {
            _currentCooldown = FireRate;
            CmdFire();
        }
    }
    private double _nextServerFireTime;

    [Command]
    private void CmdFire()
    {
        double now = NetworkTime.time;
        float fireInterval = FireRate;

        if (now < _nextServerFireTime)
            return;

        _nextServerFireTime = now + fireInterval;

        // Spawn projectile here
    }
    
    private void OnAim(InputAction.CallbackContext ctx)
    {
        Debug.Log("Aim");
    }

    public override void OnStartLocalPlayer()
    {
        inputActions.Player.Enable();
        
        inputActions.Player.Attack.started += _ => _isFiring = true;
        inputActions.Player.Attack.canceled += _ => _isFiring = false;
        inputActions.Player.Aim.performed += OnAim;
        
        _stats = GetComponent<PlayerStatHandler>();
    }

    private void OnDisable()
    {
        if (!isLocalPlayer) return;

        inputActions.Player.Attack.started -= _ => _isFiring = true;
        inputActions.Player.Attack.canceled -= _ => _isFiring = false;
        inputActions.Player.Aim.performed -= OnAim;

        inputActions.Player.Disable();
    }
}
