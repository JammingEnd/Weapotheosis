using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Models.Boons;

public class GameRoundHandler : NetworkBehaviour
{
   public static GameRoundHandler Instance;
   
   [SyncVar] public int _currentRound = 0;
   private bool _roundActive = false;
   
   [Header("Boon Settings")]
   [SerializeField] private BoonContainer availableBoons;
   [SerializeField] private int boonsPerRound = 3;

   List<PlayerStatHandler> _players = new List<PlayerStatHandler>();

   public void RegisterPlayer(PlayerStatHandler player)
   {
      if(!_players.Contains(player))
         _players.Add(player);
      
      if(_players.Count == NetworkServer.connections.Count)
      {
         StartRound();
      }
   }
   
   
   public override void OnStartServer()
   {
         base.OnStartServer();
         if (Instance == null)
         {
            Instance = this;
         }
         else
         {
            Debug.LogError("Multiple instances of GameRoundHandler detected! There should only be one instance.");
            NetworkServer.Destroy(gameObject);
         }
         BoonDatabase.Initialize(availableBoons);
         
         
        
   }

   public override void OnStartClient()
   {
      BoonDatabase.Initialize(availableBoons);
   }

   [Server]
   public void EndRound()
   {
      _currentRound++;
      _roundActive = false;
   }

   [Server]
   public void StartRound()
   {
      //TODO: Restting players
      _roundActive = true;
      InitiateBoonPhase();
   }
   
   [Server]
   public void InitiateBoonPhase()
   {
      GrantBoons();
   }

   #region Boons

   [Server]
   public void GrantBoons()
   {
      foreach (var conn in NetworkServer.connections.Values)
      {
         if(conn.identity == null) continue;
         
         Debug.unityLogger.Log("Granted boon: " + conn.connectionId);
         
         PlayerStatHandler player = conn.identity.GetComponent<PlayerStatHandler>();
         player.DisableControls = true;
         
         if (player != null)
         {
            int[] boons = SelectBoons(boonsPerRound, player).ToArray(); 
            TargetReceiveBoons(conn, boons);
         }
      }
   }

   private List<int> SelectBoons(int count, PlayerStatHandler player)
   {
      List<int> result = new();

      for (int i = 0; i < count; i++)
      {
         BoonCardSC boon = GetBoon(player);
         if (boon == null) break;

         result.Add(boon.BoonId);
      }

      return result;
   }


   private BoonCardSC GetBoon(PlayerStatHandler player)
   {
      BoonRarity rarity = RollRarity();

      // Copy list so we can remove safely
      List<BoonCardSC> pool = availableBoons.AvailableBoons
         .Where(b => b.Rarity == rarity)
         .ToList();

      // Fallback if no boons of this rarity
      while (pool.Count == 0 && rarity > BoonRarity.Common)
      {
         rarity--;
         pool = availableBoons.AvailableBoons
            .Where(b => b.Rarity == rarity && !pool.Contains(b))
            .ToList();
      }

      // Remove invalid boons
      pool.RemoveAll(b => !player.IsBoonValid(b.BoonId));

      if (pool.Count == 0)
      {
         Debug.LogWarning("No valid boons found!");
         return null;
      }

      return pool[UnityEngine.Random.Range(0, pool.Count)];
   }

   private BoonRarity RollRarity()
   {
      // Weights (can be tuned easily)
      const int commonWeight = 60;
      const int rareWeight = 25;
      const int epicWeight = 10;
      const int legendaryWeight = 5;
      
      int roll = UnityEngine.Random.Range(0,
         commonWeight + rareWeight + epicWeight + legendaryWeight);

      if (roll < commonWeight)
         return BoonRarity.Common;

      roll -= commonWeight;
      if (roll < rareWeight)
         return BoonRarity.Rare;

      roll -= rareWeight;
      if (roll < epicWeight)
         return BoonRarity.Epic;

      return BoonRarity.Legendary;
   }
   
   [TargetRpc]
   public void TargetReceiveBoons(NetworkConnection target, int[] boonIds)
   {
      PlayerBoonUIHandler.Instance.ShowBoons(boonIds);
   }
   #endregion
}
