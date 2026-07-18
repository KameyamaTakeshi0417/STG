using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Homing_Alpha : Alpha_Effect_Base
{
    public Transform currentTarget { get; private set; }
    private float homingStrength = 0f; // 0〜100
    private float trackingDamageTimer = 0f;

    private Transform lockedTargetOnFire;

    public Effect_Homing_Alpha(int pos, int rarity, float homingStrength) : base(pos, rarity) 
    {
        flightEffectInterval = 0f;
        this.homingStrength = Mathf.Clamp(homingStrength, 0f, 100f);
    }

    // 古い仕様(activeEffectClassName)からの呼び出しやFactoryからのReflection生成時のためのフォールバック
    public Effect_Homing_Alpha(int pos, int rarity) : base(pos, rarity)
    {
        flightEffectInterval = 0f;
        
        // 武器自体の固有エフェクトとして呼ばれる場合、レアリティ(1〜4)に応じて旋回力を決定する
        switch (rarity)
        {
            case 1: this.homingStrength = 0.3f; break; // 大回りで重力的に引かれる
            case 2: this.homingStrength = 1.0f; break; // ゆったり曲がる
            case 3: this.homingStrength = 4.0f; break; // 標準的な追尾
            case 4: this.homingStrength = 20f; break;  // かなり鋭く曲がる
            default: this.homingStrength = 20f; break;
        }
    }

    public override void Setup(Bullet_Base bullet, playerStatusManager_Alpha playerStatus)
    {
        base.Setup(bullet, playerStatus);

        Alpha.PointerLineSystem pointerSystem = Object.FindAnyObjectByType<Alpha.PointerLineSystem>();
        if (pointerSystem != null && pointerSystem.CurrentTarget != null)
        {
            lockedTargetOnFire = pointerSystem.CurrentTarget;
        }
        else if (bullet.lockedTarget != null)
        {
            lockedTargetOnFire = bullet.lockedTarget;
        }
        
        currentTarget = lockedTargetOnFire;
    }

    public override void OnFlight(Bullet_Base bullet, float deltaTime)
    {
        DoFlightEffect(bullet, deltaTime);
    }

    private void DoFlightEffect(Bullet_Base bullet, float deltaTime)
    {
        if (bullet.GetComponent<CircularObject>() != null) return;
        if (homingStrength <= 0f) return;

        UpdateTarget(bullet);

        bool isTracking = false;

        if (currentTarget != null)
        {
            Vector2 directionToTarget = (Vector2)currentTarget.position - (Vector2)bullet.transform.position;
            directionToTarget.Normalize();

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 currentVelocityDir = bullet.rotate.normalized;
                if (currentVelocityDir == Vector2.zero) currentVelocityDir = Vector2.up;

                float currentHomingStrength = homingStrength;
                if (bullet.piercingCount > 0)
                {
                    currentHomingStrength *= 0.9f;
                }

                // RotateTowardsによる一定速度の旋回
                // currentHomingStrength を「1秒間に旋回できるラジアン角」として扱う
                float maxRadiansDelta = currentHomingStrength * deltaTime;
                Vector2 newDirection = Vector3.RotateTowards(currentVelocityDir, directionToTarget, maxRadiansDelta, 0f).normalized;

                rb.velocity = newDirection * (bullet.Speed * 0.02f);

                float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                bullet.rotate = newDirection;
                
                isTracking = true;
            }
        }

        // 追尾中のダメージ上昇処理 (旋回力が25以上の時とする)
        if (homingStrength >= 25f && isTracking)
        {
            trackingDamageTimer += deltaTime;
            float interval = 10f / 60f;
            if (trackingDamageTimer >= interval)
            {
                // 旧仕様: homingLevel 2 = 1.03倍。今は10/60秒ごとに3%上昇固定とする
                float multiplier = 1.03f; 
                while (trackingDamageTimer >= interval)
                {
                    bullet.dmg *= multiplier;
                    trackingDamageTimer -= interval;
                }
            }
        }
    }

    private void UpdateTarget(Bullet_Base bullet)
    {
        // 既にターゲットがいて有効なら継続
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            _Health_Base h = currentTarget.GetComponentInParent<_Health_Base>();
            if (h != null && h.getCurrentHP() > 0) return;
        }

        // ロックオン対象が存在し有効ならそれを優先する
        if (lockedTargetOnFire != null && lockedTargetOnFire.gameObject.activeInHierarchy)
        {
            _Health_Base lockedH = lockedTargetOnFire.GetComponentInParent<_Health_Base>();
            if (lockedH != null && lockedH.getCurrentHP() > 0)
            {
                currentTarget = lockedTargetOnFire;
                return;
            }
        }

        // 新しいターゲットを検索（一番近い敵）
        currentTarget = FindClosestEnemy(bullet.transform.position);
    }

    private Transform FindClosestEnemy(Vector3 currentPos)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        float closestDistSq = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            _Health_Base h = enemy.GetComponent<_Health_Base>();
            if (h == null || h.getCurrentHP() <= 0) continue;

            float distSq = (enemy.transform.position - currentPos).sqrMagnitude;
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                bestTarget = enemy.transform;
            }
        }
        return bestTarget;
    }

    protected override void DoHitEffect(Bullet_Base bullet, Collider2D target)
    {
        // 貫通時の挙動は特に変更なし
    }
}
