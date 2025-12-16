using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformReliable))]
public class ProjectileMovementHandler : NetworkBehaviour
{
    [SyncVar] protected float _projectileSpeed;
    [SyncVar] protected float _lifetime;
    
    public void Initialize(float projectileSpeed, float lifetime)
    {
        this._projectileSpeed = projectileSpeed;
        this._lifetime = lifetime;
    }
    
    [ServerCallback]
    private void FixedUpdate()
    {
        if (_lifetime <= 0f)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }

        Move();
        _lifetime -= Time.fixedDeltaTime;
    }

    public virtual void Move()
    {
        transform.position += transform.forward * Time.fixedDeltaTime * _projectileSpeed;
    }
}
