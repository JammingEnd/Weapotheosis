using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models.Stats;
using UnityEngine.InputSystem;

public class PlayerWeaponHandler : NetworkBehaviour
{
    public InputSystem_Actions inputActions;
    public GameObject projectilePrefab;
    public Transform firePoint;

    float FireRate => (float)_stats.GetStat(StatType.GunAttackSpeed);
    
    private float _currentCooldown;
    private PlayerStatHandler _stats;

    private int MaxAmmo => (int)_stats.GetStat(StatType.GunMagazineSize);
    public int _currentAmmo { get; private set; }
    
    private float ReloadTime => (float)_stats.GetStat(StatType.GunReloadSpeed);
    private bool _isReloading = false;

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
        
        if(_isReloading) return;
        if(_currentAmmo <= 0)
            return;
        
        _currentAmmo--;
        _stats.CurrentAmmo = _currentAmmo;
        
        double now = NetworkTime.time;
        float fireInterval = FireRate;

        if (now < _nextServerFireTime)
            return;

        _nextServerFireTime = now + fireInterval;

        SpawnProjectile();
    }
    
    void SpawnProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        // projectil type
        if (!(bool)_stats.GetStat(StatType.HasBulletGravity))
        {
            // normal bullet
            BulletProjectile pmh = projectile.AddComponent<BulletProjectile>();
            pmh.Initialize(
                (float)_stats.GetStat(StatType.GunProjectileSpeed),
                (float)_stats.GetStat(StatType.GunProjectileLifetime)
            );
        }
        else if ((bool)_stats.GetStat(StatType.HasBulletGravity))
        {
            GravityProjectile gmh = projectile.AddComponent<GravityProjectile>();
            gmh.Initialize(
                (float)_stats.GetStat(StatType.GunProjectileSpeed),
                (float)_stats.GetStat(StatType.GunProjectileLifetime),
                (float)_stats.GetStat(StatType.GravityScale));
        }
        NetworkServer.Spawn(projectile);
    }
    
    private void OnAim(InputAction.CallbackContext ctx)
    {
        Debug.Log("Aim");
    }
    private void OnReload(InputAction.CallbackContext obj)
    {
        if(_isReloading) return;
        if(_currentCooldown == MaxAmmo) return;

        StartCoroutine(Reload());

    }

    private IEnumerator Reload()
    {
        _isReloading = true;
        _isFiring = false;

        float timer = 0f;

        while (timer < ReloadTime)
        {
            timer += Time.deltaTime;

            // Update reload progress (0 → ReloadTime)
            _stats.CurrentReloadProgress = timer;

            // Optional: for UI bar you can also store normalized value 0 → 1
            _stats.CurrentReloadNormalized = timer / ReloadTime;

            yield return null; // wait for next frame
        }

        // Make sure it's fully reloaded
        _stats.CurrentReloadProgress = ReloadTime;
        _stats.CurrentReloadNormalized = 1f;

        if (isLocalPlayer)
            CmdReload();

        _isReloading = false;
        _isFiring = false;
    }

    
    [Command]
    private void CmdReload()
    {
        _currentAmmo = MaxAmmo;
    }

    public override void OnStartLocalPlayer()
    {
        inputActions.Player.Enable();
        
        inputActions.Player.Attack.started += _ => _isFiring = true;
        inputActions.Player.Attack.canceled += _ => _isFiring = false;
        inputActions.Player.Aim.performed += OnAim;
        inputActions.Player.Reload.performed += OnReload;
        
        _stats = GetComponent<PlayerStatHandler>();
        
        CmdReload();
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
