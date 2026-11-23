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
        public float fireInterval = 0.65f;
        [Tooltip("弾の速度")]
        public float bulletSpeed = 5.5f;
        [Tooltip("弾の発射方向（デフォは下）")]
        public Vector3 fireDirection = Vector3.down;
        [Tooltip("一度に発射する弾の数（バラマキ数）")]
        public int numberOfBullets = 5;
        [Tooltip("バラマキの扇状の角度")]
        public float spreadAngle = 60f;
        [Tooltip("連射時の1発ごとの間隔（秒）")]
        public float burstDelay = 0.06f;
        [Tooltip("弾の発射位置（指定がなければ自身の中央）")]
        public Transform firePoint;

        private void Start()
        {
            StartCoroutine(ShootingRoutine());
        }

        private IEnumerator ShootingRoutine()
        {
            // 開始時にわずかなランダムディレイを入れてタレット同士の同期を適度にずらす
            yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));

            while (true)
            {
                if (bulletPrefab != null)
                {
                    yield return StartCoroutine(FireBarrageRoutine());
                }

                yield return new WaitForSeconds(fireInterval);
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
                    Bullet_Base bBase = bullet.GetComponent<Bullet_Base>();
                    if (bBase != null)
                    {
                        bBase.sourcePrefab = bulletPrefab;
                        bBase.isEnemyBullet = true;
                        float finalSpeed = (bulletSpeed > 50f) ? bulletSpeed : (bulletSpeed * 100f);
                        bBase.setStatus(dir, finalSpeed, bBase.dmg > 0 ? bBase.dmg : 8f);
                        bBase.setRotate(dir);
                        bBase.DestroyTime = 3000f; // 30秒相当（壁衝突まで消滅しない）
                        bBase.preventAutoDestroy = true; // 時間経過で消えず、壁(wall)接触または画面外で消滅
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
                        bullet.transform.up = dir;
                    }
                }

                if (burstDelay > 0f)
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }
        }
    }
}
