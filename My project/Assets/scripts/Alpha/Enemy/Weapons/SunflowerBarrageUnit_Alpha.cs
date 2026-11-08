using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Weapons
{
    /// <summary>
    /// 画面下から回転しながら上昇し、円形弾とばらまき弾を発射する向日葵ユニット。
    /// プロモーション用・コンセプト用の華やかな弾幕を演出します。
    /// </summary>
    public class SunflowerBarrageUnit_Alpha : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("上昇速度")]
        public float riseSpeed = 2.5f;

        [Tooltip("自転速度（度/秒）")]
        public float rotationSpeed = 120f;

        [Tooltip("回転方向をランダムに反転するか")]
        public bool randomRotationDirection = true;

        [Tooltip("上昇時の横揺れ（Sway）の強さ。0なら直進")]
        public float swayAmount = 0.5f;

        [Tooltip("横揺れの周期速度")]
        public float swaySpeed = 2.0f;

        [Header("Lifetime Settings")]
        [Tooltip("画面上外に出て消滅するY座標")]
        public float destroyY = 12f;

        [Tooltip("最大生存時間（秒）")]
        public float maxLifeTime = 15f;

        [Header("Circle Barrage (円形弾発射)")]
        [Tooltip("円形弾を撃つかどうか")]
        public bool enableCircleBarrage = true;

        [Tooltip("円形弾の発射間隔（秒）")]
        public float circleInterval = 1.5f;

        [Tooltip("初回発射までの遅延時間（秒）")]
        public float circleInitialDelay = 0.5f;

        [Tooltip("1回の円形発射における弾の数（全方位）")]
        public int circleBulletCount = 16;

        [Tooltip("円形弾のプレハブ")]
        public GameObject circleBulletPrefab;

        [Tooltip("円形弾の弾速")]
        public float circleBulletSpeed = 3.5f;

        [Tooltip("円形弾の生存時間")]
        public float circleBulletLifeTime = 8f;

        [Tooltip("向日葵自身の回転角度を円形弾のオフセットに反映するか")]
        public bool useRotationOffsetForCircle = true;

        [Header("Scatter Barrage (ばらまき弾発射)")]
        [Tooltip("ばらまき弾を撃つかどうか")]
        public bool enableScatterBarrage = true;

        [Tooltip("ばらまき弾の発射間隔（秒）")]
        public float scatterInterval = 0.25f;

        [Tooltip("初回発射までの遅延時間（秒）")]
        public float scatterInitialDelay = 0.2f;

        [Tooltip("1回のばらまきで同時に放つ方向数（例: 2なら向日葵の両側、4なら十字）")]
        public int scatterWays = 2;

        [Tooltip("ばらまき弾のランダム拡散角度（±度）")]
        public float scatterRandomSpread = 15f;

        [Tooltip("ばらまき弾のプレハブ（未指定の場合は円形弾と同じプレハブを使用）")]
        public GameObject scatterBulletPrefab;

        [Tooltip("ばらまき弾の弾速")]
        public float scatterBulletSpeed = 2.8f;

        [Tooltip("ばらまき弾の生存時間")]
        public float scatterBulletLifeTime = 6f;

        [Header("Visual Juice")]
        [Tooltip("生成時のポップアップ（0から徐々に実サイズへ拡大）")]
        public bool enableSpawnPop = true;
        public float popDuration = 0.4f;

        private float currentZAngle = 0f;
        private float actualRotationSpeed;
        private float spawnTime;
        private float startX;
        private Vector3 initialScale;

        private void Start()
        {
            spawnTime = Time.time;
            startX = transform.position.x;
            initialScale = transform.localScale;

            // 回転方向の決定
            actualRotationSpeed = rotationSpeed;
            if (randomRotationDirection && Random.value > 0.5f)
            {
                actualRotationSpeed = -rotationSpeed;
            }

            // スポーン時のポップアニメーション
            if (enableSpawnPop)
            {
                StartCoroutine(PopRoutine());
            }

            // 弾幕ルーチンの開始
            if (enableCircleBarrage)
            {
                StartCoroutine(CircleBarrageRoutine());
            }

            if (enableScatterBarrage)
            {
                StartCoroutine(ScatterBarrageRoutine());
            }
        }

        private IEnumerator PopRoutine()
        {
            float elapsed = 0f;
            transform.localScale = Vector3.zero;

            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                // Back-Out イージング風のちょっと飛び出るポップ演出
                float scaleMod = Mathf.Sin(t * Mathf.PI * 0.5f);
                transform.localScale = initialScale * scaleMod;
                yield return null;
            }

            transform.localScale = initialScale;
        }

        private void Update()
        {
            float life = Time.time - spawnTime;

            // 1. 回転処理
            currentZAngle += actualRotationSpeed * Time.deltaTime;
            currentZAngle = Mathf.Repeat(currentZAngle, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, currentZAngle);

            // 2. 上昇＆左右ゆらぎ移動
            float xOffset = (swayAmount > 0f) ? Mathf.Sin(life * swaySpeed) * swayAmount : 0f;
            Vector3 pos = transform.position;
            pos.x = startX + xOffset;
            pos.y += riseSpeed * Time.deltaTime;
            transform.position = pos;

            // 3. 画面上端での消滅またはタイムアウト
            if (transform.position.y >= destroyY || life >= maxLifeTime)
            {
                DestroySelf();
            }
        }

        /// <summary>
        /// 一定間隔で円形（全方位リング）に弾を発射
        /// </summary>
        private IEnumerator CircleBarrageRoutine()
        {
            if (circleInitialDelay > 0f)
            {
                yield return new WaitForSeconds(circleInitialDelay);
            }

            GameObject prefab = circleBulletPrefab != null ? circleBulletPrefab : scatterBulletPrefab;
            if (prefab == null) yield break;

            while (true)
            {
                if (circleBulletCount > 0)
                {
                    float angleStep = 360f / circleBulletCount;
                    float baseOffset = useRotationOffsetForCircle ? currentZAngle : 0f;

                    for (int i = 0; i < circleBulletCount; i++)
                    {
                        float angle = baseOffset + (angleStep * i);
                        FireSingleBullet(prefab, angle, circleBulletSpeed, circleBulletLifeTime);
                    }
                }

                yield return new WaitForSeconds(circleInterval);
            }
        }

        /// <summary>
        /// 一定間隔でヒマワリの回転に合わせて弾をばらまき発射（花びら/種のようなスパイラル散弾）
        /// </summary>
        private IEnumerator ScatterBarrageRoutine()
        {
            if (scatterInitialDelay > 0f)
            {
                yield return new WaitForSeconds(scatterInitialDelay);
            }

            GameObject prefab = scatterBulletPrefab != null ? scatterBulletPrefab : circleBulletPrefab;
            if (prefab == null) yield break;

            while (true)
            {
                if (scatterWays > 0)
                {
                    float wayStep = 360f / scatterWays;

                    for (int w = 0; w < scatterWays; w++)
                    {
                        // 向日葵の現在の回転角を基準に、ランダムなばらつきを加算
                        float randomOffset = (scatterRandomSpread > 0f) ? Random.Range(-scatterRandomSpread, scatterRandomSpread) : 0f;
                        float angle = currentZAngle + (wayStep * w) + randomOffset;

                        FireSingleBullet(prefab, angle, scatterBulletSpeed, scatterBulletLifeTime);
                    }
                }

                yield return new WaitForSeconds(scatterInterval);
            }
        }

        /// <summary>
        /// 弾を発射し、発射方向へ正確に向ける共通処理
        /// </summary>
        private void FireSingleBullet(GameObject prefab, float angleDeg, float speed, float lifeTime)
        {
            Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
            Vector3 spawnPos = transform.position;

            // オブジェクトプール対応
            GameObject bObj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                bObj = Alpha_ObjectPoolManager.Instance.Rent(prefab, spawnPos, Quaternion.identity);
            }
            else
            {
                bObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            }

            if (bObj != null)
            {
                Bullet_Base bullet = bObj.GetComponent<Bullet_Base>();
                if (bullet != null)
                {
                    bullet.setStatus(dir, speed, bullet.dmg);
                    // 進行方向（発射方向）を向かせる
                    bullet.setRotate(dir);
                    bullet.DestroyTime = lifeTime;
                    bullet.shoot();
                }
                else
                {
                    // Bullet_Base が無い単純なプレハブでも進行方向に向ける
                    bObj.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
                    Rigidbody2D rb = bObj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = dir * speed;
                    }
                }
            }
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.8f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-10f, destroyY, 0f), new Vector3(10f, destroyY, 0f));
        }
    }
}
