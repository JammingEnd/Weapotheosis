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
    
    private bool _initialized = false;
    
    Vector3 _previousPosition;
    
    public void Initialize(float damage, PlayerStatHandler owner, ProjectileType projectileType, float speed)
    {
        _damage = damage;
        _owner = owner;
        _projectileType = projectileType;
        _speed = speed;
        _previousPosition = transform.position;
    }


    private void FixedUpdate()
    {
        if (!isServer) return;

        if (!_initialized)
        {
            _previousPosition = transform.position;
            _initialized = true;
            return;
        }

        Vector3 movement = transform.position - _previousPosition;
        float distance = movement.magnitude;

        if (distance > 0f)
        {
            Vector3 direction = movement.normalized;
            if (Physics.SphereCast(_previousPosition, 0.06f, direction, out RaycastHit hit, distance))
            {
                _poldistance = hit.distance;
                if(hit.collider.gameObject != _owner.gameObject)
                    Collide(hit.collider.gameObject, hit.point);
            }
        }

        _previousPosition = transform.position;
    }
    
    [Server]
    private void Collide(GameObject other, Vector3 hitPos) 
    {
        if(!isServer) return;
        if(other == _owner.gameObject) return;
        
        if(other.TryGetComponent(out EntityHealthHandler healthHandler))
        {
            healthHandler.TakeDamage(_damage);
        }
        RpcPlayHitEffect(hitPos, other.transform.rotation);
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
