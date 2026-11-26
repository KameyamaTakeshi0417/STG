using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 3: 『雪中寒椿・冬吹雪』の花弁弾（Camellia Petal Bullet）
    /// 深紅の花弁が風に乗って滑らかにうねりながら流れる。
    /// コライダーは見た目より少し小さめにして回避の快感を確保。
    /// </summary>
    public class TsubakiPetalBullet : Bullet_Base
    {
        [Header("Petal Settings")]
        [SerializeField] private float baseSpeed = 4.5f;
        [SerializeField] private float flutterSpeed = 3f;
        [SerializeField] private float flutterAmount = 0.8f;
        [SerializeField] private float damage = 12f;
        [SerializeField] private float lifeTime = 8f;

        private Vector2 windVelocity = Vector2.down;
        private float flutterTimer = 0f;
        private float lifeTimer = 0f;
        private bool isVanishing = false;
        private SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            isEnemyBullet = true;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Initialize(Vector2 currentWind, float speed = 4.5f, float bulletDamage = 12f)
        {
            windVelocity = currentWind.normalized * speed;
            baseSpeed = speed;
            damage = bulletDamage;
            flutterTimer = Random.Range(0f, Mathf.PI * 2f);
            lifeTimer = 0f;
            isVanishing = false;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        public void UpdateWind(Vector2 newWind)
        {
            windVelocity = newWind.normalized * baseSpeed;
        }

        protected override void Update()
        {
            if (isVanishing) return;

            flutterTimer += Time.deltaTime * flutterSpeed;
            lifeTimer += Time.deltaTime;

            // 風の流れ + 垂直方向へのサイン波の揺れ
            Vector2 perp = new Vector2(-windVelocity.y, windVelocity.x).normalized;
            Vector2 flutter = perp * Mathf.Sin(flutterTimer) * flutterAmount;

            transform.position += (Vector3)((windVelocity + flutter) * Time.deltaTime);

            // 花弁の回転
            transform.Rotate(0, 0, 45f * Time.deltaTime);

            if (lifeTimer >= lifeTime)
            {
                RecycleOrDestroy();
            }
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (isVanishing) return;

            if (collision.CompareTag("Player"))
            {
                var health = collision.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                RecycleOrDestroy();
            }
        }

        public void ForceStopAndVanish()
        {
            if (isVanishing) return;
            isVanishing = true;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            StartCoroutine(VanishRoutine());
        }

        private IEnumerator VanishRoutine()
        {
            float elapsed = 0f;
            float duration = 0.8f;
            Vector3 startPos = transform.position;
            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // ハラハラと落ちる
                transform.position = startPos + new Vector3(Mathf.Sin(elapsed * 4f) * 0.1f, -elapsed * 0.6f, 0);

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                }
                yield return null;
            }

            RecycleOrDestroy();
        }

        private void RecycleOrDestroy()
        {
            if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
            {
                Alpha_ObjectPoolManager.Instance.Return(gameObject, sourcePrefab);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
