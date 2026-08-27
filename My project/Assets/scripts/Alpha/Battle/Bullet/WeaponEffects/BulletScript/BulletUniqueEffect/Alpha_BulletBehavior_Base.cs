using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

[System.Serializable]
public class ActiveBulletBehavior_Alpha
{
    public Alpha_BulletBehavior_Base behaviorSO;
    public int rarity;
    public float stateTimer;

    public ActiveBulletBehavior_Alpha(Alpha_BulletBehavior_Base so, int rarity)
    {
        this.behaviorSO = so;
        this.rarity = rarity;
        this.stateTimer = 0f;
    }
}

public abstract class Alpha_BulletBehavior_Base : ScriptableObject
{
    [Tooltip("If true, this behavior will be copied to child bullets (e.g., scatter, reflect).")]
    public bool canInherit = true;

    public virtual void Initialize(Bullet_Base bullet, int rarity) { }
    
    public virtual void OnSpawn(Bullet_Base bullet, int rarity, ref float stateTimer) { }
    
    public virtual void OnFlight(Bullet_Base bullet, int rarity, ref float stateTimer, float deltaTime) { }
    
    public virtual void OnHit(Bullet_Base bullet, int rarity, Collider2D collision) { }
    
    public virtual void OnReturnToPool(Bullet_Base bullet, int rarity) { }
}
