using System;
using Mirror;
using Models.Stats;
using UnityEngine;

[Serializable]
public class Stat
{
    public StatType Type { get; set; }
    public float Value { get; set; }
    public float Multiplier { get; set; }

    public float GetStatValueFloat()
    {
        return Value * Multiplier;
    }
    public int GetStatValueInt()
    {
        return Mathf.RoundToInt((Value * Multiplier));
    }
    public bool GetStatValueBool()
    {
        if (Value < 1)
        {
            return false;
        }
        return true;
    }
    
    public void ModifyFlat(float amount)
    {
        Value += amount;
    }
    /// <summary>
    /// Parameter must be in decimal form. so 0.2 for 20% increase
    /// </summary>
    /// <param name="amount"></param>
    public void ModifyMultiplier(float amount)
    {
        Multiplier += amount;
    }
}