namespace Models.Stats
{
    public enum StatType
    {
        // floats
        HealthRegen,
        ShieldRegenRate,
        ShieldRegenDelay,
        HealthLeechFlat,
        HealthLeechPercent,
        HealthOnKill,
        ShieldOnKill,
        HealthOnBlock,
        ShieldOnBlock,
        StaminaOnKill,
        StaminaOnBlock,
        BlockCooldown,
        BlockDuration,
        MovementSpeed,
        JumpHeight,
        GravityScale,
        StaminaRegenRate,
        StaminaRegenDelay,
        CritChance,
        CritDamage,
        GunDamage,
        GunAttackSpeed,
        GunReloadSpeed,
        GunMagazineSize,
        GunAccuracy,
        GunProjectileSpeed,
        GunProjectileLifetime,
        
        //ints
        MaxHealth,
        MaxShield,
        MaxStamina,
        GunProjectileCount,
        
        //booleans
        CanDoubleJump,
        CanSuperSprint,
        CanRicochet,
        HasBulletGravity,
        
    }
}