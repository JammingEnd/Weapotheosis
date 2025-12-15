namespace Models.Stats
{
    public enum StatType
    {
        MaxHealth,
        HealthRegen,
        MaxShield,
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
        MaxStamina,
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
        GunProjectileCount,
        
        // abilities
        CanDoubleJump,
        CanSuperSprint,
        CanRicochet,
    }

    public class StatModifier
    {
        
    }
}