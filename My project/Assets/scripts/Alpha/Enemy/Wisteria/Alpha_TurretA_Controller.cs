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
        public float fireInterval = 2.0f;
        [Tooltip("弾の速度")]
        public float bulletSpeed = 5f;
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
            while (true)
            {
                yield return new WaitForSeconds(fireInterval);

                if (bulletPrefab != null && playerTransform != null)
                {
                    FireAtPlayer();
                }
            }
        }

        private void FireAtPlayer()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector3 direction = (playerTransform.position - spawnPos).normalized;

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
                // 弾のコンポーネント（Bullet_Base等）に速度や方向をセットする
                // 既存の弾の仕様に合わせて適宜調整
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.velocity = direction * bulletSpeed;
                }
                
                // 弾の「上」方向を進行方向に向ける（弾の画像が上向き前提の場合）
                bullet.transform.up = direction;
                // ※もし画像が右向き前提なら bullet.transform.right = direction; となります
            }
        }
    }
}
