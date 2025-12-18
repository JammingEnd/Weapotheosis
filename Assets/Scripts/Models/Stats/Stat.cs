using System;
using UnityEngine;
using Models.Stats;

[Serializable]
public struct Stat
{
    public StatType Type;
    public float Value;
    public float Multiplier;

    public float GetStatValueFloat()
    {
        return Value * Multiplier;
    }

    public int GetStatValueInt()
    {
        return Mathf.RoundToInt(Value * Multiplier);
    }

    public bool GetStatValueBool()
    {
        return Value >= 1f;
    }

    public void ModifyFlat(float amount)
    {
        Value += amount;
    }

    /// <summary>
    /// amount must be decimal (0.2 = +20%)
    /// </summary>
    public void ModifyMultiplier(float amount)
    {
        Multiplier += amount;
    }
}