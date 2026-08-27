using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Alpha.Core.Utils;

public class Bullet_Base : MonoBehaviour, IAlphaPoolable, IBombDestructible
{
    public GameObject sourcePrefab;

    [Header("Alignment Settings")]
    [Tooltip("Enemy Bullet Flag")]
    public bool isEnemyBullet = false;
    [Tooltip("Can Hit Both Flag")]
    public bool canHitBoth = false;
 
    public Transform lockedTarget;
    public System.Collections.Generic.List<ActiveBulletBehavior_Alpha> activeBehaviors;
    public Vector3 originalAimDirection;

    public virtual void OnRentFromPool()
    {
        piercingCount = 0; 
        extraShotCount = 0; 
        voltTickReduceCount = 0; 
        secondaryDamageMultiplier = 1.0f; 
        hitCountsPerEnemy.Clear();
        ignoredColliders.Clear();
        
        if (bulletCollider != null) bulletCollider.enabled = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public virtual void OnReturnToPool()
    {
        StopAllCoroutines();
        if (activeBehaviors != null) { foreach (var ab in activeBehaviors) ab.behaviorSO.OnReturnToPool(this, ab.rarity); }
    }
    
    public string Objname;
    protected Rigidbody2D rb;
    public float dmg; 
    public float Speed; 
    public float DestroyTime; 
    public float bullettype = 0; 
    public Vector3 rotate; 

    public int rarelity; 
    public string bulletName;
    public float addDmg; 
    public int piercingCount = 0;
    public int extraShotCount = 0; 
    public int voltTickReduceCount = 0; 
    public float secondaryDamageMultiplier = 1.0f; 

    protected float initialDmg; 
    protected float basePrefabSpeed; 
    protected Dictionary<GameObject, int> hitCountsPerEnemy = new Dictionary<GameObject, int>(); 
    protected Collider2D bulletCollider; 

    public bool canDestructByBomb { get; set; } = true;

    protected virtual void Awake()
    {
        // behaviors = GetComponents<Alpha_BulletBehavior_Base>();
        basePrefabSpeed = Speed;
        if (basePrefabSpeed <= 0f) basePrefabSpeed = 1f; 
    }

    protected virtual void Start() { }
    protected virtual void Update() { }

    public string getBulletName()
    {
        return bulletName;
    }

    public void setDmg(float damage)
    {
        dmg = damage;
    }

    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }

    public void setBulletSpeed(float mag) { }

    [Tooltip("Prevent Auto Destroy")]
    public bool preventAutoDestroy = false;

    public float localPierceDamageReductionRate = -1f;

    public void setStatus(Vector3 Prot, float pSpeed, float pDmg)
    {
        rotate = Prot;
        Speed = pSpeed;
        dmg = pDmg;
    }

    protected float initialSpeed; 

    public void shoot()
    {
        initialDmg = dmg; 
        initialSpeed = Speed; 
        bulletCollider = GetComponent<Collider2D>(); 

        if (activeBehaviors != null) { foreach (var ab in activeBehaviors) ab.behaviorSO.OnSpawn(this, ab.rarity, ref ab.stateTimer); }
        StartCoroutine(move());
    }

    public void fire()
    {
        gameObject.GetComponent<Case_Base>().setStatus(rotate, Speed, dmg);
        gameObject.GetComponent<Case_Base>().ApplyCaseEffect(this.gameObject);
    }

    protected virtual IEnumerator move()
    {
        int count = 0;

        rb = gameObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = rotate.normalized * (Speed * 0.01f);
            
            if ((Speed / basePrefabSpeed) >= 3.0f) 
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }

        while (count <= DestroyTime || preventAutoDestroy)
        {
            if (!preventAutoDestroy)
            {
                count++;
            }

            if (rb != null)
            {
                rb.velocity = rotate.normalized * (Speed * 0.01f);
            }

            if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance != null)
            {
                if (Alpha.Core.ScreenBoundaryManager_Alpha.Instance.IsOutOfBounds(transform.position))
                {
                    break; 
                }
            }

            if (activeBehaviors != null) { foreach (var ab in activeBehaviors) ab.behaviorSO.OnFlight(this, ab.rarity, ref ab.stateTimer, 0.01f); }
            yield return new WaitForSeconds(0.01f);
        }

        if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
        {
            Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
        }
        else
        {
           DestroyAction();
        }
        yield break;
    }

    public void callHitEffect()
    {
        if (!gameObject.activeInHierarchy) return;

        DrainEffect targetScript;
        targetScript = GetComponent<DrainEffect>();
        if (targetScript != null)
        {
            targetScript.MakeEffect();
        }
        StartCoroutine(hitEffect());
    }

    protected IEnumerator hitEffect()
    {
        yield return null;
    }

    protected void DestroyCheck()
    {
        piercingCount--;

        if (piercingCount <= 0)
        {
            if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
            {
                Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
            }
            else
            {
               DestroyAction();
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (ignoredColliders.Contains(collision)) return;

        bool hitSomething = false;
        if (activeBehaviors != null) { foreach (var ab in activeBehaviors) ab.behaviorSO.OnHit(this, ab.rarity, collision); }

        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            if (!canHitBoth)
            {
                if (isEnemyBullet && collision.CompareTag("Enemy")) return;
                if (!isEnemyBullet && collision.CompareTag("Player")) return;
            }

            _Health_Base health = collision.GetComponentInParent<_Health_Base>();
            if (health != null)
            {
                GameObject targetObj = health.gameObject; 
                if (!hitCountsPerEnemy.ContainsKey(targetObj))
                {
                    hitCountsPerEnemy[targetObj] = 0;
                }

                int prevHitCount = hitCountsPerEnemy[targetObj];
                
                // Penetration hits
                int actualHits = 1;
                
                float reductionRate = 0.5f; 
                if (localPierceDamageReductionRate >= 0f)
                {
                    reductionRate = localPierceDamageReductionRate;
                }
                else
                {
                    GameObject manager = (playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.gameObject : null);
                    if (manager != null)
                    {
                        var pStatus = manager.GetComponent<playerStatusManager_Alpha>();
                        if (pStatus != null) reductionRate = pStatus.pierceDamageReductionRate;
                    }
                }

                for (int i = 0; i < actualHits; i++)
                {
                    float velocityRatio = Speed / basePrefabSpeed;
                    float damageMultiplier = Mathf.Clamp(velocityRatio, 0.1f, 5.0f);
                    float finalImpactDamage = dmg * damageMultiplier;

                    health.ApplyDamage(finalImpactDamage, this);
                    hitCountsPerEnemy[targetObj]++;

                    piercingCount--;

                    dmg -= initialDmg * reductionRate;
                    if (dmg <= initialDmg * 0.1f) dmg = initialDmg * 0.1f;
                }
            }
            hitSomething = true;
        }
        else if (collision.CompareTag("wall"))
        {
            hitSomething = true;
        }

        if (hitSomething)
        {
            if (piercingCount < 0 || collision.CompareTag("wall"))
            {
                if (preventAutoDestroy && GetComponent<CircularObject>() != null)
                {
                    StartCoroutine(TemporaryDisableCollider(collision));
                }
                else
                {
                    if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
                    {
                        Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
                    }
                    else
                    {
                       DestroyAction();
                    }
                }
            }
            else
            {
                StartCoroutine(TemporaryDisableCollider(collision));
            }
        }
    }

    private HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();

    protected IEnumerator TemporaryDisableCollider(Collider2D targetCollider)
    {
        if (bulletCollider != null && targetCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, targetCollider, true);
            ignoredColliders.Add(targetCollider);
            
            yield return new WaitForSeconds(0.5f);
            
            if (this != null && this.gameObject != null && bulletCollider != null && targetCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, targetCollider, false);
                ignoredColliders.Remove(targetCollider);
            }
        }
    }

    protected float damageCaluculator(float pow, float mag)
    {
        float ret = 0f;
        ret = addDmg + (pow + dmg) * mag;

        return ret;
    }

    public float getDmg()
    {
        return dmg;
    }

    public float getSpeed()
    {
        return Speed;
    }

    public int getRarelity()
    {
        return rarelity;
    }
    
    public virtual void DestroyAction() {
        if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
        {
            Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnRipple(transform.position, isEnemyBullet ? Color.white : new Color(0.8f, 0.4f, 0.4f), 0.2f, 0.8f, 0.15f);
        }
        Destroy(this.gameObject);
    }
    
    public void GenerateAnotherChildBullet() { }

    public void OnBombDestruct()
    {
        if (!isEnemyBullet) return;

        if (canDestructByBomb)
        {
            if (isEnemyBullet)
            {
                int currentMNE = PlayerPrefs.GetInt("MoneyAndExp", 0);
                PlayerPrefs.SetInt("MoneyAndExp", currentMNE + 1);
            }
            if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
            {
                Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
            }
            else
            {
                DestroyAction();
            }
        }
    }
}




