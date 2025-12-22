using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameScoreHandler : NetworkBehaviour
{
    public static GameScoreHandler Instance;
    
    public string ScoreText = "";

    public override void OnStartServer()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of GameRoundHandler detected! There should only be one instance.");
            NetworkServer.Destroy(gameObject);
        }
        base.OnStartServer();
    }
    
    public Dictionary<uint, int> playerScores = new Dictionary<uint, int>();
    
    public void AddScore(uint target, int scoreToAdd)
    {
        if (!isServer) return;
        
        if (playerScores.ContainsKey(target))
        {
            playerScores[target] += scoreToAdd;
        }
        else
        {
            playerScores[target] = scoreToAdd;
        }

        
        foreach (var kvp in playerScores)
        {
            ScoreText += $"{kvp.Key}: {kvp.Value}\n";
        }
    }
    
    [Server]
    public void RegisterPlayer(uint playerId)
    {
        if (!playerScores.ContainsKey(playerId))
        {
            playerScores[playerId] = 0;
        }
    }
    [Server]
    public void UnregisterPlayer(uint playerId)
    {
        if (playerScores.ContainsKey(playerId))
        {
            playerScores.Remove(playerId);
        }
    }
    
}
