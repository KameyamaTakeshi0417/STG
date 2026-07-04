using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.Wisteria
{
    public class Alpha_TurretB_Controller : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [Tooltip("発射する弾のプレハブ")]
        public GameObject bulletPrefab;
        [Tooltip("弾を発射する間隔（秒）")]
        public float fireInterval = 0.5f;
        [Tooltip("弾の速度")]
        public float bulletSpeed = 5f;
        [Tooltip("弾の発射方向（デフォは下）")]
        public Vector3 fireDirection = Vector3.down;
        [Tooltip("一度に発射する弾の数（バラマキ数）")]
        public int numberOfBullets = 5;
        [Tooltip("バラマキの扇状の角度")]
        public float spreadAngle = 60f;
        [Tooltip("連射時の1発ごとの間隔（秒）")]
        public float burstDelay = 0.05f;
        [Tooltip("弾の発射位置（指定がなければ自身の中央）")]
        public Transform firePoint;

        private void Start()
        {
            StartCoroutine(ShootingRoutine());
        }

        private IEnumerator ShootingRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireInterval);

                if (bulletPrefab != null)
                {
                    StartCoroutine(FireBarrageRoutine());
                }
            }
        }

        private IEnumerator FireBarrageRoutine()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector3 centerDir = fireDirection.normalized;
            float centerAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;

            float startAngle = centerAngle - (spreadAngle / 2f);
            float angleStep = numberOfBullets > 1 ? spreadAngle / (numberOfBullets - 1) : 0f;

            for (int i = 0; i < numberOfBullets; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 dir = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0);

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
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.gravityScale = 0f;
                        rb.velocity = dir * bulletSpeed;
                    }
                    
                    // 弾の「上」方向を進行方向に向ける（弾の画像が上向き前提の場合）
                    bullet.transform.up = dir;
                    // ※もし画像が右向き前提なら bullet.transform.right = dir; となります
                }

                if (burstDelay > 0f)
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }
        }
    }
}
