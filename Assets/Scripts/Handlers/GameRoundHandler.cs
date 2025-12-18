using System;
using System.Collections;
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

   #region Round Timer

   [SyncVar(hook = nameof(OnBoonTimerChanged))]
   private float cardSelectTimer;

   [SerializeField] private float chooseTime = 10f;
   
   // using the hook, this is called Client-side
   private void OnBoonTimerChanged(float oldValue, float newValue)
   {
      // Update UI Timer
      PlayerBoonUIHandler.Instance.UpdateTimer(newValue);
   }

   [Server]
   private IEnumerator BoonTimer()
   {
      cardSelectTimer = chooseTime;
      while (cardSelectTimer > 0)
      {
         yield return new WaitForSeconds(1f);
         cardSelectTimer -= 1f;
      }
      // Time's up, proceed with default selection or random selection
      cardSelectTimer = 0f;
      ConcludeBoonPhase();
   }

   #endregion
   

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
      StopAllCoroutines();
      StartCoroutine(BoonTimer());
   }
   
   [Server]
   public void ConcludeBoonPhase()
   {
      foreach (var conn in NetworkServer.connections.Values)
      {
         if(conn.identity == null) continue;
         
         RpcActivateBoons();
      }
   }

   [ClientRpc]
   private void RpcActivateBoons()
   {
         PlayerBoonUIHandler.Instance.ActivateBoon();
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
      HashSet<int> picked = new();
      List<int> result = new();

      int safety = 50; // prevent infinite loops

      while (result.Count < count && safety-- > 0)
      {
         BoonCardSC boon = GetBoon(player, picked);
         if (boon == null)
            break;

         picked.Add(boon.BoonId);
         result.Add(boon.BoonId);
      }

      return result;
   }



   private BoonCardSC GetBoon(PlayerStatHandler player, HashSet<int> alreadySelected)
   {
      BoonRarity rarity = RollRarity();

      while (rarity >= BoonRarity.Common)
      {
         List<BoonCardSC> pool = availableBoons.AvailableBoons
            .Where(b =>
               b.Rarity == rarity &&
               player.IsBoonValid(b.BoonId) &&
               !alreadySelected.Contains(b.BoonId)
            )
            .ToList();

         if (pool.Count > 0)
         {
            return pool[UnityEngine.Random.Range(0, pool.Count)];
         }

         rarity--; // fallback
      }

      Debug.LogWarning("No valid boons found!");
      return null;
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
