using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models;
using Models.Boons;
using Models.Stats;
using Unity.VisualScripting;



public class PlayerStatHandler : NetworkBehaviour
{
    [SyncVar] public bool Initialized;
    
    public PlayerStats BaseStats;

    [SyncVar] public bool DisableControls = false;
    
    #region Stats
    
    public readonly SyncDictionary<StatType, Stat> Stats = new SyncDictionary<StatType, Stat>();

    [SyncVar] public int CurrentHealth;
    [SyncVar] public int CurrentStamina;
    [SyncVar] public int CurrentShield;
    [SyncVar] public int CurrentAmmo;
    [SyncVar] public float CurrentReloadProgress;
    [SyncVar] public float CurrentReloadNormalized;
    
    public bool GetStat(StatType stat, out float value)
    {
        if (Stats.TryGetValue(stat, out Stat statValue))
        {
            value = statValue.GetStatValueFloat();
            return true;
        }
        value = 0;
        return false;
    }
    public bool GetStat(StatType stat, out int value)
    {
        if (Stats.TryGetValue(stat, out Stat statValue))
        {
            value = statValue.GetStatValueInt();
            return true;
        }
        value = 0;
        return false;
    }
    public bool GetStat(StatType stat, out bool value)
    {
        if (Stats.TryGetValue(stat, out Stat statValue))
        {
            value = statValue.GetStatValueBool();
            return true;
        }
        value = false;
        return false;
    }

    public T GetStatValue<T>(StatType stat)
    {
        if (Stats.TryGetValue(stat, out Stat statValue))
        {
            if (typeof(T) == typeof(int))
            {
                return (T)(object)statValue.GetStatValueInt();
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)statValue.GetStatValueFloat();
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)statValue.GetStatValueBool();
            }
        }
        return default;
    }

    public bool HasStat(StatType stat)
    {
        if (Stats.TryGetValue(stat, out Stat statValue))
        {
            return true;
        }
        return false;
    }

    [Server]
    public void ModifyStat(StatType stat, float amount, IncrementTypes type)
    {
        if (Stats.TryGetValue(stat, out Stat statValue))
        {
            switch (type)
            {
                case IncrementTypes.Flat:
                    statValue.ModifyFlat(amount);
                    break;
                case IncrementTypes.Percentage:
                    statValue.ModifyMultiplier(amount);
                    break;
                case IncrementTypes.Override:
                    statValue.Value = amount;
                    break;
            }
        }
    }

    [Server]
    public void InitializeStats()
    {
        Stats.Clear();
        foreach (var pair in BaseStats.Stats)
        {
            Stats[pair.Key] = new Stat
            {
                Type = pair.Key,
                Value = pair.Value,
                Multiplier = 1f
            };
        }
        CurrentHealth = Stats[StatType.MaxHealth].GetStatValueInt();
        CurrentStamina = Stats[StatType.MaxStamina].GetStatValueInt();
        CurrentShield = Stats[StatType.MaxShield].GetStatValueInt();
        CurrentAmmo = Stats[StatType.GunMagazineSize].GetStatValueInt();
        
        Initialized = true;
    }
    #endregion

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializeStats();
        
        GameRoundHandler.Instance.RegisterPlayer(this);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        PlayerUIHandler ui = FindObjectOfType<PlayerUIHandler>();
        PlayerBoonUIHandler playerBoonUIHandler = FindObjectOfType<PlayerBoonUIHandler>();
        if (ui != null)
        {            
            ui.Initialize(this);
            playerBoonUIHandler.Initialize(this);
        }
    }

    #region Modifiers
    
    #endregion
   

    #region Boons

    // Card and stacks
    private Dictionary<int, int> activeBoons = new();
    
    [Server]
    public void AddBoon(int id)
    {
        if (activeBoons.TryGetValue(id, out int value))
        {
            activeBoons[id] = value + 1;
        }
        else
        {
            activeBoons[id] = 1;
        }
    }
    
    [Server]
    public  void RemoveBoon(int id) 
    {
        if (!activeBoons.TryGetValue(id, out int stacks))
            return;

        stacks--;

        if (stacks <= 0)
            activeBoons.Remove(id);
        else
            activeBoons[id] = stacks;
    }

    [Server]
    public bool IsBoonValid(int id)
    {
        BoonCardSC boon = BoonDatabase.GetBoonById(id);

        if (activeBoons.TryGetValue(id, out int stacks))
            return stacks < boon.MaxStacks;

        return true;
    }

    [Server]
    public void RecalculateStatsRoundStart()
    {
        InitializeStats();
        
        foreach (var pair in activeBoons)
        {
            int boonId = pair.Key;
            int stacks = pair.Value;

            BoonCardSC boon = BoonDatabase.GetBoonById(boonId);

            for (int i = 0; i < stacks; i++)
            {
                foreach (var effect in boon.effects)
                {
                    ModifyStat(effect.statType, effect.value, effect.incrementType);
                }
            }
        }
    }
    
    [Command]
    public void CmdSelectBoon(int boonId)
    {
        BoonCardSC boon = BoonDatabase.GetBoonById(boonId);
        AddBoon(boon.BoonId);
    }
    
    #endregion
    
    
    
} 
