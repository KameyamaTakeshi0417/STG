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
    [Tooltip("プレイヤーのゲージに対して喰らわせる倍率 (1.1で1本と少し)")]
    public float playerGaugeDamageMultiplier = 1.1f;
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
                    // プレイヤーへの大ダメージ
                    PlayerHealth playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        // 1.1ゲージ分のダメージを計算（HPプロパティは現在の最大HP/ゲージ最大を保持すると想定）
                        float damage = playerHealth.HP * playerGaugeDamageMultiplier;
                        playerHealth.TakeDamage(damage);
                        Debug.Log($"[GiantMissileDrop] Missile impact! Dealt {damage} damage to player.");
                    }

                    // 自身への50%ダメージ
                    Alpha_EliteHealth bossHealth = ai.GetComponent<Alpha_EliteHealth>();
                    if (bossHealth != null)
                    {
                        float selfDamage = bossHealth.HP * selfDamageRatio;
                        bossHealth.TakeDamage(selfDamage);
                        Debug.Log($"[GiantMissileDrop] Dealt {selfDamage} damage to Boss itself.");
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
