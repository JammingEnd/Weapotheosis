using System;

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
        HasBulletGravity,
    }

    [Serializable]
    public class StatModifier
    {
        public StatType targetStat;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        
        public bool timed;
        public float duration; // in seconds
        
        public StatModifier(StatType stat, float value, float duration = 0f)
        {
            targetStat = stat;
            floatValue = value;
            this.duration = duration;
        }

        public StatModifier(StatType stat, int value, float duration = 0f)
        {
            targetStat = stat;
            intValue = value;
            this.duration = duration;
        }

        public StatModifier(StatType stat, bool value, float duration = 0f)
        {
            targetStat = stat;
            boolValue = value;
            this.duration = duration;
        }
    }
}