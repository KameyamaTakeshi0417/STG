using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.Wisteria
{
    public class Alpha_TurretA_Controller : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [Tooltip("発射する弾のプレハブ")]
        public GameObject bulletPrefab;
        [Tooltip("弾を発射する間隔（秒）")]
        public float fireInterval = 0.65f;
        [Tooltip("弾の速度")]
        public float bulletSpeed = 7f;
        [Tooltip("初回発射の待機ディレイ（0なら即時発射）")]
        public float initialDelay = 0.15f;
        [Tooltip("弾の発射位置（指定がなければ自身の中央）")]
        public Transform firePoint;

        private bool isActivated = false;
        private Transform playerTransform;

        private void Start()
        {
            // プレイヤーを検索しておく
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        public void ActivateTurret()
        {
            if (isActivated) return;
            isActivated = true;
            StartCoroutine(ShootingRoutine());
        }

        private IEnumerator ShootingRoutine()
        {
            if (initialDelay > 0f)
            {
                yield return new WaitForSeconds(initialDelay);
            }

            while (true)
            {
                if (bulletPrefab != null && playerTransform != null)
                {
                    FireAtPlayer();
                }

                yield return new WaitForSeconds(fireInterval);
            }
        }

        private void FireAtPlayer()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector3 direction = (playerTransform.position - spawnPos).normalized;
            if (direction == Vector3.zero) direction = Vector3.down;

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
                    bBase.setStatus(direction, finalSpeed, bBase.dmg > 0 ? bBase.dmg : 10f);
                    bBase.setRotate(direction);
                    bBase.DestroyTime = 3000f; // 30秒相当（壁衝突まで消滅しない）
                    bBase.preventAutoDestroy = true; // 時間経過で消えず、壁(wall)接触または画面外で消滅
                    bBase.shoot();
                }
                else
                {
                    // フォールバック
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.gravityScale = 0f;
                        rb.velocity = direction * bulletSpeed;
                    }
                    bullet.transform.up = direction;
                }
            }
        }
    }
}
