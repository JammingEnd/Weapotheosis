using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models;
using Models.Stats;
using Unity.VisualScripting;



public class PlayerStatHandler : NetworkBehaviour
{
    public PlayerStats BaseStats;

    #region Stats
    
    [SyncVar] public Stats Stats;
 
    [SyncVar] public int CurrentHealth;
    [SyncVar] public int CurrentStamina;
    [SyncVar] public int CurrentShield;
    [SyncVar] public int CurrentAmmo;
    [SyncVar] public float CurrentReloadProgress;
    [SyncVar] public float CurrentReloadNormalized;
    

    #endregion

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitialiseStats();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        PlayerUIHandler ui = FindObjectOfType<PlayerUIHandler>();
        if (ui != null)
        {            
            ui.Initialize(this);
        }
    }


    [Server]
    private void InitialiseStats()
    {
        Stats = new Stats();
        CopyStats(BaseStats.stats, Stats);
        
        CurrentHealth = Stats.MaxHealth;
        CurrentStamina = Stats.MaxStamina;
        CurrentShield = Stats.MaxShield;
        CurrentAmmo = Stats.GunMagazineSize;
    }

    private void CopyStats(Stats source, Stats target)
    {
        target.MaxHealth = source.MaxHealth;
        target.MaxShield = source.MaxShield;
        target.MaxStamina = source.MaxStamina;
        target.GunProjectileCount = source.GunProjectileCount;
        target.GunMagazineSize = source.GunMagazineSize;

        target.HealthRegenRate = source.HealthRegenRate;
        target.HealthRegenDelay = source.HealthRegenDelay;
        target.StaminaRegenRate = source.StaminaRegenRate;
        target.StaminaRegenDelay = source.StaminaRegenDelay;
        target.ShieldRegenRate = source.ShieldRegenRate;
        target.ShieldRegenDelay = source.ShieldRegenDelay;
        target.HealthLeechFlat = source.HealthLeechFlat;
        target.HealthLeechPercent = source.HealthLeechPercent;
        target.HealthOnKill = source.HealthOnKill;
        target.ShieldOnKill = source.ShieldOnKill;
        target.StaminaOnKill = source.StaminaOnKill;
        target.HealthOnBlock = source.HealthOnBlock;
        target.ShieldOnBlock = source.ShieldOnBlock;
        target.StaminaOnBlock = source.StaminaOnBlock;
        target.BlockCooldown = source.BlockCooldown;
        target.BlockDuration = source.BlockDuration;
        target.MovementSpeed = source.MovementSpeed;
        target.JumpHeight = source.JumpHeight;
        target.GravityScale = source.GravityScale;
        target.CritChance = source.CritChance;
        target.CritDamage = source.CritDamage;
        target.GunDamage = source.GunDamage;
        target.GunAttackSpeed = source.GunAttackSpeed;
        target.GunReloadSpeed = source.GunReloadSpeed;
        target.GunAccuracy = source.GunAccuracy;
        target.GunProjectileSpeed = source.GunProjectileSpeed;
        target.GunProjectileLifetime = source.GunProjectileLifetime;

        target.CanDoubleJump = source.CanDoubleJump;
        target.CanSuperSprint = source.CanSuperSprint;
        target.CanRicochet = source.CanRicochet;
        target.HasBulletGravity = source.HasBulletGravity;
    }
    
    private List<BoonEffectSC> activeBoons = new();
} 
