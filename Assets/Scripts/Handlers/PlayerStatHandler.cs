using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models.Stats;
using Unity.VisualScripting;



public class PlayerStatHandler : NetworkBehaviour
{
    public PlayerStats BaseStats;
    public PlayerUIHandler UIHandler;

    private Dictionary<StatType, FloatStatEntry> FloatStats = new Dictionary<StatType, FloatStatEntry>();
    private Dictionary<StatType, IntStatEntry> IntStats = new Dictionary<StatType, IntStatEntry>();
    private Dictionary<StatType, BoolStatEntry> BoolStats = new Dictionary<StatType, BoolStatEntry>();
    
    public object GetStat(StatType statType)
    {
        if (FloatStats.ContainsKey(statType)) return FloatStats[statType].value;
        if (IntStats.ContainsKey(statType)) return IntStats[statType].value;
        if (BoolStats.ContainsKey(statType)) return BoolStats[statType].value;
        
        return null;
    }
    
    private List<StatModifier> statModifiers = new();
    
    [SyncVar] public int CurrentHealth;
    [SyncVar] public int CurrentShield;
    [SyncVar] public int CurrentStamina;
    [SyncVar] public int CurrentAmmo;
    [SyncVar] public float CurrentReloadProgress;
    [SyncVar] public float CurrentReloadNormalized;

    public override void OnStartServer()
    {
        InitializeStats();
    }

    [Server]
    public void InitializeStats()
    {
        this.FloatStats.Clear();
        this.IntStats.Clear();
        this.BoolStats.Clear();

        foreach (var f in BaseStats.FloatStatList) FloatStats[f.key] = f;
        foreach (var i in BaseStats.IntStatList) IntStats[i.key] = i;
        foreach (var b in BaseStats.BoolStatList) BoolStats[b.key] = b;
        
        CurrentHealth = (int)(IntStats[StatType.MaxHealth].value);
        CurrentShield = (int)(IntStats[StatType.MaxShield].value);
    }
  
} 
