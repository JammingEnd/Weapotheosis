using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
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

   #region Spawning

   [SerializeField] private List<Transform> spawnPoints;
   private List<Transform> _availableSpawnPoints = new List<Transform>();
   
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
      
      ResetSpawnPoints();
        
   }
   
   void ResetSpawnPoints()
   {
      _availableSpawnPoints = new List<Transform>(spawnPoints);
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
      ResetSpawnPoints();
   }

   [Server]
   public void StartRound()
   {
      //TODO: Restting players
      _roundActive = true;
      AssignSpawnPoints();
      InitiateBoonPhase();
   }
   [Server]
   void AssignSpawnPoints()
   {
      foreach (var player in _players)
      {
         if(_availableSpawnPoints.Count == 0)
            ResetSpawnPoints();
         
         int index = UnityEngine.Random.Range(0, _availableSpawnPoints.Count);
         Transform spawnPoint = _availableSpawnPoints[index];
         RespawnPlayer(player, spawnPoint);
         _availableSpawnPoints.RemoveAt(index);
      }
   }

   [Server]
   void RespawnPlayer(PlayerStatHandler player, Transform spawnPoint)
   {
      KinematicCharacterMotor kcc = player.GetComponent<KinematicCharacterMotor>();
      kcc.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
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
      RpcActivateBoons();
      StopAllCoroutines();
   }

   [ClientRpc]
   private void RpcActivateBoons()
   {
         Debug.Log("Activating selected boon");
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
      HashSet<int> unavailble = player.GetUnavailbleBoons();
      HashSet<int> availableBoonIds = new HashSet<int>(
         availableBoons.AvailableBoons.Select(b => b.BoonId).Where(id => !unavailble.Contains(id))
      );
      List<int> result = new();
      
      while (result.Count < count)
      {
         BoonRarity rarity = RollRarity();
         List<int> possibleBoons = availableBoonIds
            .Where(id => BoonDatabase.GetBoonById(id).Rarity == rarity)
            .ToList();
         
         
         if(possibleBoons.Count == 0) continue;

         int pick = possibleBoons[UnityEngine.Random.Range(0, possibleBoons.Count)];
         result.Add(pick);
         availableBoonIds.Remove(pick);
      }
      return result;
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
