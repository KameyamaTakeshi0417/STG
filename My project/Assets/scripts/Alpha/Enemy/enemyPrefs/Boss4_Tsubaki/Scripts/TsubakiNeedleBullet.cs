using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// ツバキの精密細弾（Precision Needle）
    /// 「低出力・高技術」を象徴する、シャープで無駄のない深紅の細弾。
    /// 直進性に優れ、自機狙い連射や落ち椿からの交差弾として使用される。
    /// </summary>
    public class TsubakiNeedleBullet : Bullet_Base
    {
        [Header("Needle Bullet Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private float damage = 15f;

        private Vector2 moveDirection = Vector2.down;
        private float currentLifeTimer = 0f;
        private bool isVanishing = false;

        protected override void Awake()
        {
            base.Awake();
            isEnemyBullet = true;
        }

        public void Initialize(Vector2 direction, float bulletSpeed = 10f, float bulletDamage = 15f)
        {
            moveDirection = direction.normalized;
            speed = bulletSpeed;
            damage = bulletDamage;
            currentLifeTimer = 0f;
            isVanishing = false;

            // 弾の進行方向に向ける
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        protected override void Update()
        {
            if (isVanishing) return;

            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

            currentLifeTimer += Time.deltaTime;
            if (currentLifeTimer >= lifeTime)
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

        /// <summary>
        /// 決着時等に判定を切り、静かに消滅する
        /// </summary>
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
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            float duration = 0.8f;
            float elapsed = 0f;
            Color startColor = sr != null ? sr.color : Color.white;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // ハラハラと静かに下へ落ちる
                transform.position = startPos + new Vector3(Mathf.Sin(elapsed * 5f) * 0.1f, -elapsed * 0.5f, 0);

                if (sr != null)
                {
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
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
