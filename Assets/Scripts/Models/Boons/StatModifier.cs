using System.Collections.Generic;
using Models;
using Models.Stats;
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public IncrementTypes incrementType;
    
    public float value;
}

