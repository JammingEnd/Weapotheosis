using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Models.Projectiles;

public class ProjectileHitHandler : NetworkBehaviour
{
    
    private ProjectileType _projectileType;
    
    private PlayerStatHandler _owner;
    private float _damage;
    private float _speed;
    private float _poldistance;
    
    Vector3 _previousPosition;
    
    public void Initialize(float damage, PlayerStatHandler owner, ProjectileType projectileType, float speed)
    {
        _damage = damage;
        _owner = owner;
        _projectileType = projectileType;
        _speed = speed;
    }
    
    public override void OnStartServer()
    {
        _previousPosition = transform.position;
    }


    private void FixedUpdate()
    {
        if (!isServer) return;
        
        Vector3 currentPosition = transform.position;

        Vector3 movement = transform.forward * _speed * Time.fixedDeltaTime;
        float distance = movement.magnitude;

        if (Physics.SphereCast(
                _previousPosition,
                0.07f,
                movement.normalized,
                out RaycastHit hit,
                distance))
        {
            Collide(hit.collider);
        }
       
    }


    
    private void Collide(Collider other)
    {
        if(other.gameObject == _owner.gameObject) return;
        
        if(other.TryGetComponent(out EntityHealthHandler healthHandler))
        {
            healthHandler.TakeDamage(_damage);
        }
        RpcPlayHitEffect(transform.position + (transform.forward * _poldistance), other.transform.rotation);
        StartCoroutine(DestroyProjectileNextFrame());
    }
    
    private IEnumerator DestroyProjectileNextFrame()
    {
        yield return null; // wait one frame
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void SendDamageToClient()
    {
        
    }


    [ClientRpc]
    private void RpcPlayHitEffect(Vector3 position, Quaternion direction)
    {
        PlayerEffectPoolHandler.Instance.SpawnEffect("ProjectileHitEffect", position, direction);
    }
}
