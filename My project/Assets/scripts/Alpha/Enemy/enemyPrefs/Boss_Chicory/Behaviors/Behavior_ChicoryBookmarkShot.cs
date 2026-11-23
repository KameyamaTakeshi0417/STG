using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    /// <summary>
    /// チコリ本体の直接射撃ビヘイビア: 自機狙い栞弾（Bookmark Bullet）
    /// Phase 1: 3way（地雷との緩急）
    /// Phase 2: 単発〜2連（根ラインの補助）
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_ChicoryBookmarkShot", menuName = "EnemyAI/Behaviors/Chicory Bookmark Shot")]
    public class Behavior_ChicoryBookmarkShot : EnemyBehaviorData_Base
    {
        [Header("Bullet Prefab")]
        [SerializeField] private GameObject bulletPrefab;

        [Header("Barrage Settings")]
        [Tooltip("一度に発射するway数（1なら単発、3なら3way）")]
        [SerializeField] private int ways = 3;
        [Tooltip("way間の扇状スプレッド角度")]
        [SerializeField] private float spreadAngle = 35f;
        [Tooltip("1セット内のバースト連射数（1なら単射、2なら2連射）")]
        [SerializeField] private int burstCount = 1;
        [Tooltip("バースト間のディレイ（秒）")]
        [SerializeField] private float burstDelay = 0.12f;
        [Tooltip("発射サイクル間隔（秒）")]
        [SerializeField] private float fireInterval = 2.0f;
        [Tooltip("初動待機時間（秒）")]
        [SerializeField] private float initialDelay = 1.2f;
        [Tooltip("弾速")]
        [SerializeField] private float bulletSpeed = 6.0f;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (bulletPrefab == null) yield break;

            yield return new WaitForSeconds(initialDelay);

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            while (true)
            {
                if (player == null)
                {
                    player = GameObject.FindGameObjectWithTag("Player");
                }

                if (player != null && ai != null)
                {
                    for (int b = 0; b < burstCount; b++)
                    {
                        FireBarrageAtPlayer(ai, player.transform.position);
                        if (burstDelay > 0f && b < burstCount - 1)
                        {
                            yield return new WaitForSeconds(burstDelay);
                        }
                    }
                }

                yield return new WaitForSeconds(fireInterval);
            }
        }

        private void FireBarrageAtPlayer(Alpha_EnemyAI ai, Vector3 targetPos)
        {
            Vector3 spawnPos = ai.transform.position;
            Vector3 centerDir = (targetPos - spawnPos).normalized;
            if (centerDir == Vector3.zero) centerDir = Vector3.down;

            float centerAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;
            float startAngle = ways > 1 ? centerAngle - (spreadAngle / 2f) : centerAngle;
            float angleStep = ways > 1 ? spreadAngle / (ways - 1) : 0f;

            for (int i = 0; i < ways; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 dir = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0f);

                GameObject bullet = null;
                if (Alpha_ObjectPoolManager.Instance != null)
                {
                    bullet = Alpha_ObjectPoolManager.Instance.Rent(bulletPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
                }

                if (bullet != null)
                {
                    bullet.transform.up = dir;

                    Bullet_Base bBase = bullet.GetComponent<Bullet_Base>();
                    if (bBase != null)
                    {
                        bBase.sourcePrefab = bulletPrefab;
                        bBase.isEnemyBullet = true;
                        float finalSpeed = (bulletSpeed > 50f) ? bulletSpeed : (bulletSpeed * 100f);
                        bBase.setStatus(dir, finalSpeed, bBase.dmg > 0 ? bBase.dmg : 9f);
                        bBase.setRotate(dir);
                        bBase.DestroyTime = 3000f; // 壁衝突まで長距離生存
                        bBase.preventAutoDestroy = true; // 自然消滅抑制
                        bBase.shoot();
                    }
                    else
                    {
                        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.gravityScale = 0f;
                            rb.velocity = dir * bulletSpeed;
                        }
                    }
                }
            }
        }
    }
}
