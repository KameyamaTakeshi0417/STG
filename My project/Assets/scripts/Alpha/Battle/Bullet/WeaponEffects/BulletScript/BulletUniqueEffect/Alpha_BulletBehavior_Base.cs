using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

public abstract class Alpha_BulletBehavior_Base : MonoBehaviour
{
    [Tooltip("If true, this behavior will be copied to child bullets (e.g., scatter, reflect).")]
    public bool canInherit = true;

    protected Bullet_Base bullet;
    protected int rarity;

    public virtual void Initialize(Bullet_Base b, int r)
    {
        bullet = b;
        rarity = r;
    }

    public virtual void OnSpawn() { }
    
    public virtual void OnFlight(float deltaTime) { }
    
    public virtual void OnHit(Collider2D collision) { }
    
    public virtual void OnReturnToPool() { }
}
