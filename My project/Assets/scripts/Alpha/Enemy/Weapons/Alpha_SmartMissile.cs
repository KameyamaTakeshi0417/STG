using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

namespace Alpha.Enemy.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Alpha_SmartMissile : MonoBehaviour, IBombDestructible
    {
        public enum MissileType
        {
            Straight,
            SmallHoming,
            EliteHoming, // Predictive
            Baka // Wobbly/Dummy
        }

        [Header("Missile Settings")]
        public MissileType type = MissileType.Straight;
        public float turnSpeed = 90f;
        public float wobbleFrequency = 5f;
        public float wobbleAmplitude = 30f;

        [Header("Phase 1: Initial Spread")]
        public float initialFlightDuration = 0.5f;
        public float initialSpeed = 15f;
        public float minSpeed = 2f; // 展開終了時の最低速度

        [Header("Phase 2 & 3: Homing & Straight")]
        public float homingDuration = 2f;
        public float homingSpeed = 12f;

        private Rigidbody2D rb;
        private Transform playerTarget;
        private Rigidbody2D playerRb;
        
        private float aliveTime = 0f;
        private float wobbleOffset = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            wobbleOffset = Random.Range(0f, 100f);

            // 飛行機雲（残像）の追加
            TrailRenderer tr = GetComponent<TrailRenderer>();
            if (tr == null)
            {
                tr = gameObject.AddComponent<TrailRenderer>();
            }
            tr.time = 0.4f; // 残像の長さ（秒）
            tr.startWidth = 0.3f;
            tr.endWidth = 0f;
            tr.material = new Material(Shader.Find("Sprites/Default"));
            tr.startColor = new Color(1f, 1f, 1f, 0.7f);
            tr.endColor = new Color(1f, 1f, 1f, 0f);
            tr.sortingOrder = -1; // 弾より奥に描画
        }

        void Start()
        {
            // プレイヤーを取得
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                playerRb = player.GetComponent<Rigidbody2D>();
            }

            // 初期速度設定
            rb.velocity = transform.up * initialSpeed;
        }

        void FixedUpdate()
        {
            aliveTime += Time.fixedDeltaTime;
            
            float currentSpeed = initialSpeed;

            // --- Phase 1: 初動航行（直進・減速） ---
            if (aliveTime <= initialFlightDuration)
            {
                // initialSpeed から minSpeed へ徐々に減速する
                float t = aliveTime / initialFlightDuration;
                // Ease-out (最初早く減速して最後ゆっくりになる)
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                currentSpeed = Mathf.Lerp(initialSpeed, minSpeed, t);
                
                rb.velocity = transform.up * currentSpeed;
                return;
            }

            // --- Phase 2 & 3 以降の速度管理 ---
            // Phase2に入ったら minSpeed から homingSpeed へ徐々に加速
            float homingPassedTime = aliveTime - initialFlightDuration;
            float homingT = Mathf.Clamp01(homingPassedTime / 0.5f); // 0.5秒かけて加速
            currentSpeed = Mathf.Lerp(minSpeed, homingSpeed, homingT);

            if (playerTarget == null)
            {
                rb.velocity = transform.up * currentSpeed;
                return;
            }

            // --- Phase 2: 追尾航行 ---
            if (homingPassedTime <= homingDuration)
            {
                switch (type)
                {
                    case MissileType.Straight:
                        // 直進
                        rb.velocity = transform.up * currentSpeed;
                        break;

                    case MissileType.SmallHoming:
                        Homing(playerTarget.position, currentSpeed);
                        break;

                    case MissileType.EliteHoming:
                        Vector2 targetPos = playerTarget.position;
                        if (playerRb != null)
                        {
                            float dist = Vector2.Distance(transform.position, targetPos);
                            float timeToReach = dist / currentSpeed;
                            targetPos += playerRb.velocity * (timeToReach * 0.5f);
                        }
                        Homing(targetPos, currentSpeed);
                        break;

                    case MissileType.Baka:
                        Vector2 baseDir = (playerTarget.position - transform.position).normalized;
                        float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                        float wobble = Mathf.Sin(homingPassedTime * wobbleFrequency + wobbleOffset) * wobbleAmplitude;
                        angle += wobble - 90f;

                        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnSpeed * Time.fixedDeltaTime);
                        rb.velocity = transform.up * currentSpeed;
                        break;
                }
            }
            // --- Phase 3: 最終航行（直進） ---
            else
            {
                rb.velocity = transform.up * currentSpeed;
            }
        }

        private void Homing(Vector2 targetPos, float currentSpeed)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnSpeed * Time.fixedDeltaTime);
            rb.velocity = transform.up * currentSpeed;
        }

        [Header("Damage Settings")]
        public float damage = 1f;

        // --- IBombDestructible Implementation ---
        public bool canDestructByBomb { get; set; } = true;

        public void OnBombDestruct()
        {
            if (canDestructByBomb)
            {
                // ボムで消される時の処理
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerHealth ph = collision.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}
