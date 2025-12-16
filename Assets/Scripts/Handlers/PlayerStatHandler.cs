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
    
    private object GetStatRef(StatType statType)
    {
        if (FloatStats.ContainsKey(statType)) return FloatStats[statType];
        if (IntStats.ContainsKey(statType)) return IntStats[statType];
        if (BoolStats.ContainsKey(statType)) return BoolStats[statType];
        
        return null;
    }
    
    private List<BoonEffectSC> activeBoons = new();
    
    private List<StatModifier> statModifiers = new();
    private List<StatModifier> tempModifiers = new();
    
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

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        PlayerUIHandler.instance.Initialize(this);
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
        CurrentAmmo = (int)(IntStats[StatType.GunMagazineSize].value);
        CurrentStamina = (int)(IntStats[StatType.MaxStamina].value);
    }
    
    [Server]
    public void AddBoonEffect(BoonEffectSC boon)
    {
        activeBoons.Add(boon);
        
    }

    [Server]
    public void RecalculateStats()
    {
        // Reset to base stats
        InitializeStats();
        
        //handle boons
        foreach (var boon in activeBoons)
        {
            foreach (var modifier in boon.StatModifiers)
            {
                statModifiers.Add(modifier);
            }
        }

        // Apply each modifier
        foreach (var modifier in statModifiers)
        {
            // all are adjective for simplicity
            if (FloatStats.ContainsKey(modifier.targetStat))
            {
                FloatStats[modifier.targetStat] = new FloatStatEntry
                {
                    key = modifier.targetStat,
                    value = FloatStats[modifier.targetStat].value + modifier.floatValue
                };
            }
            if (IntStats.ContainsKey(modifier.targetStat))
            {
                IntStats[modifier.targetStat] = new IntStatEntry
                {
                    key = modifier.targetStat,
                    value = IntStats[modifier.targetStat].value + modifier.intValue
                };
            }

            if (BoolStats.ContainsKey(modifier.targetStat))
            {
                BoolStats[modifier.targetStat] = new BoolStatEntry
                {
                    key = modifier.targetStat,
                    value = modifier.boolValue
                };
            }

        }
    }
    
    [Server]
    public void RemoveStatModifier(StatModifier modifier)
    {
        statModifiers.Remove(modifier);
        RecalculateStats();
    }

    [Server]
    public void ApplyTimedModifier(StatModifier modifier)
    {
        tempModifiers.Add(modifier);
        HandleTimedStat(modifier);
    }

    private void HandleTimedStat(StatModifier modifier, bool remove = false)
    {
        var targetedStat = GetStatRef(modifier.targetStat);
        
        if (targetedStat is FloatStatEntry floatStat)
        {
            if(remove)
                floatStat.value -= modifier.floatValue;
            else
                floatStat.value += modifier.floatValue;
        }
        else if (targetedStat is IntStatEntry intStat)
        {
            if(remove)
                intStat.value -= modifier.intValue;
            else
                intStat.value += modifier.intValue;
        }
        else if (targetedStat is BoolStatEntry boolStat)
        {
            if(remove)
                boolStat.value = !modifier.boolValue;
            else
                boolStat.value = modifier.boolValue;
        }
    }

    private void FixedUpdate()
    {
        for (int i = tempModifiers.Count - 1; i >= 0; i--)
        {
            var modifier = tempModifiers[i];
            modifier.duration -= Time.fixedDeltaTime;

            if (modifier.duration <= 0f)
            {
                HandleTimedStat(modifier, true); // revert
                tempModifiers.RemoveAt(i);        // remove from active list
            }
        }
    }
} 
