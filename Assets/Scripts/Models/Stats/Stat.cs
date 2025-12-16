using System;
using Mirror;
using Models.Stats;

[Serializable]
public class Stats
{
    public int MaxHealth;
    public int MaxShield;
    public int MaxStamina;
    public int GunProjectileCount;
    public int GunMagazineSize;

    public float HealthRegenRate;
    public float HealthRegenDelay;
    public float StaminaRegenRate;
    public float StaminaRegenDelay;
    public float ShieldRegenRate;
    public float ShieldRegenDelay;
    public float HealthLeechFlat;
    public float HealthLeechPercent;
    public float HealthOnKill;
    public float ShieldOnKill;
    public float StaminaOnKill;
    public float HealthOnBlock;
    public float ShieldOnBlock;
    public float StaminaOnBlock;
    public float BlockCooldown;
    public float BlockDuration;
    public float MovementSpeed;
    public float JumpHeight;
    public float GravityScale;
    public float CritChance;
    public float CritDamage;
    public float GunDamage;
    public float GunAttackSpeed;
    public float GunReloadSpeed;
    public float GunAccuracy;
    public float GunProjectileSpeed;
    public float GunProjectileLifetime;

    public bool CanDoubleJump;
    public bool CanSuperSprint;
    public bool CanRicochet;
    public bool HasBulletGravity;
    

}