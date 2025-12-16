using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models.Projectiles;

public class ProjectileHitHandler : NetworkBehaviour
{
    
    private ProjectileType _projectileType;
    private PlayerStatHandler _owner;
    private float _damage;
    
    public void Initialize(float damage, PlayerStatHandler owner, ProjectileType projectileType)
    {
        _damage = damage;
        _owner = owner;
        _projectileType = projectileType;
    }
    

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == _owner.gameObject) return;
        
        Debug.Log("Hit!: " + other.gameObject.name);
        
        var healthHandler = other.GetComponent<EntityHealthHandler>();
        if (healthHandler != null)
        {
            healthHandler.TakeDamage(_damage); 
        }
        else
        {
            // TODO: add effects for hitting walls or other objects
            
        }
        NetworkServer.Destroy(gameObject);
    }
}
