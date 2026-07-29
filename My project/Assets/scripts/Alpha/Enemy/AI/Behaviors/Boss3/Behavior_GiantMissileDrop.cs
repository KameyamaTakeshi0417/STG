using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Enemy.Weapons;

[CreateAssetMenu(fileName = "Behavior_GiantMissileDrop", menuName = "Alpha/Enemy AI/Behaviors/Boss3/Giant Missile Drop")]
public class Behavior_GiantMissileDrop : EnemyBehaviorData_Base
{
    [Header("Missile Setup")]
    public GameObject giantMissilePrefab;
    public float fallDuration = 5f;
    public float missileHP = 3000f;
    public Vector2 spawnOffset = new Vector2(0, 15f);

    [Header("Damage Setup")]
    [Tooltip("ボス自身が負うダメージ（自身の最大HPの割合、0.5で50%）")]
    public float selfDamageRatio = 0.5f;

    public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
    {
        // ボスは中央・上方に留まる
        if (ai.Rb != null)
        {
            ai.Rb.velocity = Vector2.zero;
        }

        Vector3 startPos = ai.transform.position + (Vector3)spawnOffset;
        Vector3 targetPos = ai.transform.position; // または画面中央

        if (giantMissilePrefab != null)
        {
            GameObject missileObj = Instantiate(giantMissilePrefab, startPos, Quaternion.identity);
            Alpha_GiantMissile missile = missileObj.GetComponent<Alpha_GiantMissile>();
            if (missile != null)
            {
                missile.HP = missileHP;
                missile.fallDuration = fallDuration;
                missile.targetPosition = targetPos;
                
                bool? resultDestroyed = null;
                missile.OnMissileEnd += (isDestroyed) => {
                    resultDestroyed = isDestroyed;
                };

                // 結果が出るまで待機
                while (!resultDestroyed.HasValue && missile != null)
                {
                    yield return null;
                }

                // 時間切れで落下した場合の処理
                if (resultDestroyed.HasValue && !resultDestroyed.Value)
                {
                    // プレイヤーへの大ダメージ (普通のダメージ処理として巨大な爆発判定を生成)
                    PlayerHealth playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
                    float damage = (playerHealth != null) ? playerHealth.HP : 100f;

                    GameObject explosionObj = new GameObject("GiantMissileExplosion");
                    explosionObj.transform.position = Vector3.zero; // 画面全体
                    
                    CircleCollider2D col = explosionObj.AddComponent<CircleCollider2D>();
                    col.isTrigger = true;
                    col.radius = 50f; // 画面を覆うサイズ
                    
                    Alpha_GiantMissileExplosion explosion = explosionObj.AddComponent<Alpha_GiantMissileExplosion>();
                    explosion.damage = damage;
                    explosion.duration = 3f; // 3秒間持続して無敵切れを狙う
                    
                    Debug.Log($"[GiantMissileDrop] Missile impact! Created giant explosion dealing {damage} damage.");

                    // 自身への50%ダメージ
                    Alpha_EliteHealth bossHealth = ai.GetComponent<Alpha_EliteHealth>();
                    if (bossHealth != null)
                    {
                        float selfDamage = bossHealth.HP * selfDamageRatio;
                        bossHealth.TakeDamage(selfDamage);
                        Debug.Log($"[GiantMissileDrop] Dealt {selfDamage} damage to Boss itself.");
                    }
                }
                // プレイヤーによって破壊された場合の処理
                else if (resultDestroyed.HasValue && resultDestroyed.Value)
                {
                    // ボスのみにダメージを与える（プレイヤーへのダメージは無し）
                    Alpha_EliteHealth bossHealth = ai.GetComponent<Alpha_EliteHealth>();
                    if (bossHealth != null)
                    {
                        float selfDamage = bossHealth.HP * selfDamageRatio;
                        bossHealth.TakeDamage(selfDamage);
                        Debug.Log($"[GiantMissileDrop] Missile destroyed by player! Dealt {selfDamage} damage to Boss itself.");
                    }
                }
            }
        }

        // ミサイル着弾・破壊後もボスは行動せず動けなくなる（フェーズダウン待ち）
        while (true)
        {
            yield return null;
        }
    }
}
