using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularObject : Bullet_Base
{
    [Header("Circular Settings")]
    public float stoppingTime = 5.0f;
    public float fireInterval = 0.5f;
    public GameObject subBulletPrefab;

    private float currentDeceleration = 0f;
    private float fireTimer = 0f;
    private bool hasStopped = false;

    void Start() 
    { 
        if (subBulletPrefab == null)
        {
            subBulletPrefab = Resources.Load<GameObject>("Objects/Bullet/CircularBullet") 
                           ?? Resources.Load<GameObject>("Objects/Bullet/NormalBullet");
        }
        preventAutoDestroy = true;
    }

    public override void OnRentFromPool()
    {
        base.OnRentFromPool();
        fireTimer = 0f;
        hasStopped = false;
        currentDeceleration = 0f;
        preventAutoDestroy = true;
    }

    protected override void Update() 
    { 
        base.Update();

        if (hasStopped || rb == null) return;

        if (currentDeceleration == 0 && initialSpeed > 0)
        {
            currentDeceleration = initialSpeed / stoppingTime;
        }

        if (Speed > 0)
        {
            Speed = Mathf.Max(0, Speed - currentDeceleration * Time.deltaTime);

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer -= fireInterval;
                FireAtNearestEnemy();
            }

            if (Speed == 0)
            {
                hasStopped = true;
                FireInSixDirections();
                DestroyAction(); 
            }
        }
    }

    private void FireAtNearestEnemy()
    {
        Vector3 baseAimDir;
        GameObject target = FindNearestEnemy();
        if (target != null)
        {
            baseAimDir = (target.transform.position - transform.position).normalized;
        }
        else
        {
            baseAimDir = rb != null && rb.velocity.magnitude > 0 ? (Vector3)rb.velocity.normalized : transform.up;
        }

        int totalShotCount = 1 + this.extraShotCount;
        float lateralSpacing = 0.5f; // プレイヤーの弾と同じように弾間を空ける
        Vector3 rightDir = new Vector3(baseAimDir.y, -baseAimDir.x, 0).normalized;

        for (int i = 0; i < totalShotCount; i++)
        {
            float offset = 0f;
            if (totalShotCount > 1)
            {
                if (totalShotCount % 2 == 1)
                {
                    int step = (i + 1) / 2;
                    float sign = (i % 2 == 1) ? 1f : -1f;
                    if (i == 0) step = 0;
                    offset = step * lateralSpacing * sign;
                }
                else
                {
                    int step = i / 2;
                    float sign = (i % 2 == 0) ? 1f : -1f;
                    offset = (step + 0.5f) * lateralSpacing * sign;
                }
            }
            Vector3 spawnPosOffset = rightDir * offset;
            FireSubBullet(baseAimDir, spawnPosOffset);
        }
    }

    private void FireInSixDirections()
    {
        int numBullets = Mathf.Max(1, 6 + this.extraShotCount);
        float angleStep = 360f / numBullets;
        for (int i = 0; i < numBullets; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.up;
            FireSubBullet(dir);
        }
    }

    private void FireSubBullet(Vector3 dir, Vector3 spawnPosOffset = default)
    {
        if (subBulletPrefab == null) return;

        Vector3 spawnPos = transform.position + spawnPosOffset;
        GameObject bulletObj = null;
        if (Alpha_ObjectPoolManager.Instance != null)
        {
            bulletObj = Alpha_ObjectPoolManager.Instance.Rent(subBulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            bulletObj = Instantiate(subBulletPrefab, spawnPos, Quaternion.identity);
        }

        Bullet_Base bulletScript = bulletObj.GetComponent<Bullet_Base>();
        if (bulletScript != null)
        {
            bulletScript.sourcePrefab = subBulletPrefab;
            bulletScript.originalAimDirection = dir;

            bulletScript.setStatus(dir, this.initialSpeed > 0 ? this.initialSpeed : 200f, this.dmg);
            bulletScript.DestroyTime = this.DestroyTime;
            bulletScript.piercingCount = this.piercingCount;

            List<Alpha_Effect_Base> clonedEffects = new List<Alpha_Effect_Base>();
            if (this.activeEffects != null)
            {
                foreach (var effect in this.activeEffects)
                {
                    if (effect != null)
                    {
                        var clonedEffect = effect.Clone();
                        clonedEffect.isSubBullet = true;
                        clonedEffects.Add(clonedEffect);
                    }
                }
            }
            
            bool parentCanUseAll = false;
            if (this.activeEffects != null && this.activeEffects.Count > 0)
            {
                parentCanUseAll = this.activeEffects[0].canUseAllEffects;
            }
            bulletScript.SetWeaponEffects(clonedEffects, parentCanUseAll);

            float rotationAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bulletObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle));

            bulletScript.shoot();
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = float.MaxValue;
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("wall"))
        {
            DestroyAction();
        }
    }
}
