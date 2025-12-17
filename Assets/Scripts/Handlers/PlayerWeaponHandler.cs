using System.Collections;
using UnityEngine;
using Mirror;
using Models.Projectiles;
using Models.Stats;
using UnityEngine.InputSystem;

public class PlayerWeaponHandler : NetworkBehaviour
{
    public InputSystem_Actions inputActions;
    public GameObject projectilePrefab;
    public Transform firePoint;
    [SerializeField] private Animator gunAnimator;

    private PlayerStatHandler _stats;
    private float _currentCooldown;
    private bool _isReloading;
    private bool _isFiring;
    private double _nextServerFireTime;

    public int _currentAmmo { get; private set; }

    private float FireRate => _stats.GetStatValue<float>(StatType.GunAttackSpeed);
    private int MaxAmmo => _stats.GetStatValue<int>(StatType.GunMagazineSize);
    private float ReloadTime => _stats.GetStatValue<float>(StatType.GunReloadSpeed);

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _stats = GetComponent<PlayerStatHandler>();
    }

    public override void OnStartLocalPlayer()
    {
        inputActions.Player.Enable();

        inputActions.Player.Attack.started += _ => _isFiring = true;
        inputActions.Player.Attack.canceled += _ => _isFiring = false;
        inputActions.Player.Reload.performed += OnReload;
        inputActions.Player.Aim.performed += ctx => { /* handle aim */ };

        if (_stats != null)
            CmdReload();
    }

    private void OnDisable()
    {
        if (!isLocalPlayer) return;
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (_isFiring && _currentCooldown <= 0f)
        {
            _currentCooldown = FireRate;
            CmdFire();
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (_currentCooldown > 0f)
            _currentCooldown -= Time.fixedDeltaTime;
    }

    [Command]
    private void CmdFire()
    {
        if (_isReloading || _currentAmmo <= 0 || _stats == null)
            return;

        _currentAmmo--;
        _stats.CurrentAmmo = _currentAmmo;

        double now = NetworkTime.time;
        if (now < _nextServerFireTime)
            return;

        _nextServerFireTime = now + FireRate;
        SpawnProjectile();
    }

    private void SpawnProjectile()
    {
        if (_stats == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.GetComponent<ProjectileHitHandler>().Initialize(_stats.GetStatValue<float>(StatType.GunDamage), _stats, ProjectileType.Bullet);
        // bullet type
        if (_stats.GetStat(StatType.HasBulletGravity, out bool hasGravity)) {
            if (!hasGravity)
            {
                // normal bullet
                BulletProjectile pmh = proj.AddComponent<BulletProjectile>(); 
                pmh.Initialize( _stats.GetStatValue<float>(StatType.GunProjectileSpeed), _stats.GetStatValue<float>(StatType.GunProjectileLifetime));
                
            }
            else
            {
                GravityProjectile gmh = proj.AddComponent<GravityProjectile>(); 
                gmh.Initialize( 
                    _stats.GetStatValue<float>(StatType.GunProjectileSpeed), 
                    _stats.GetStatValue<float>(StatType.GunProjectileLifetime),
                    _stats.GetStatValue<float>(StatType.GravityScale)); 
            }
            
        } 
        NetworkServer.Spawn(proj);
    }

    [Command]
    private void CmdReload()
    {
        if (_stats == null) return;
        _currentAmmo = MaxAmmo;
        _stats.CurrentAmmo = _currentAmmo;
    }

    [Command]
    private void StartReload(bool fullReload)
    {
        RpcPlayReloadAnimation(fullReload);
    }

    [ClientRpc]
    private void RpcPlayReloadAnimation(bool fullReload)
    {
        if (_stats == null) return;

        float reloadSpeedMultiplier = 1.5f / ReloadTime;
        gunAnimator.SetFloat("animSpeed", reloadSpeedMultiplier);

        if (fullReload)
            gunAnimator.SetTrigger("ReloadFull");
        else
            gunAnimator.SetTrigger("ReloadPartial");
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (_isReloading || _currentAmmo >= MaxAmmo) return;
        StartCoroutine(ReloadCoroutine());
    }
    
    

    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;
        _isFiring = false;
        
        

        float timer = 0f;
        float reloadTime = ReloadTime;
        if (_currentAmmo <= 0)
            reloadTime *= 1.5f;
            
        StartReload(_currentAmmo <= 0);
        
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            if (_stats != null)
            {
                _stats.CurrentReloadProgress = timer;
                _stats.CurrentReloadNormalized = timer / reloadTime;
            }
            yield return null;
        }

        if (_stats != null)
        {
            _stats.CurrentReloadProgress = reloadTime;
            _stats.CurrentReloadNormalized = 1f;
        }

        if (isLocalPlayer)
            CmdReload();

        _isReloading = false;
        _isFiring = false;
    }
}
