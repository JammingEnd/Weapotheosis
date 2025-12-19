using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class EntityHealthHandler : NetworkBehaviour
{
    private PlayerStatHandler _stats;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _stats = GetComponent<PlayerStatHandler>();
    }
    
    [Server]
    public void TakeDamage(float damage)
    {
        if (_stats == null)
        {
            Debug.LogWarning("No stats found");
            return;
        }
        
        Debug.Log("Ouch!, damage received " + damage);
        
        _stats.CurrentHealth -= Mathf.RoundToInt(damage);
        if (_stats.CurrentHealth <= 0)
        {
            Die();
        }
    }

    [Server]
    private void Die()
    {
        // turn on ghost mode for the player, disabling combat components 
        
        //TODO: respawn player... use respawn method from PlayerStatHandler
    }
    

}
