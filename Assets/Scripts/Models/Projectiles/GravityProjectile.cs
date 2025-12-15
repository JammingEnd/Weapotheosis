using Mirror;
using UnityEngine;

public class GravityProjectile : ProjectileMovementHandler
{
    [SyncVar] private float _gravityScale;
    public void Initialize(float projectileSpeed, float lifetime, float gravityScale)
    {
        base.Initialize(projectileSpeed, lifetime);
        this._gravityScale = gravityScale;
    }

    
    public override void Move()
    {
        transform.rotation *= Quaternion.Euler(_gravityScale * Time.fixedDeltaTime, 0f, 0f);
        base.Move();
    }
}
