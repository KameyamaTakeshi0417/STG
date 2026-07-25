using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Alpha_SmartMissile : MonoBehaviour
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
        public float speed = 15f;
        public float homingDuration = 2f; // For SmallHoming
        public float turnSpeed = 180f; // degrees per second
        public float wobbleFrequency = 5f;
        public float wobbleAmplitude = 30f;

        private Rigidbody2D rb;
        private Transform playerTarget;
        private Rigidbody2D playerRb;
        
        private float aliveTime = 0f;
        private Vector2 currentVelocity;
        private float wobbleOffset = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            wobbleOffset = Random.Range(0f, 100f);
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
            currentVelocity = transform.up * speed;
            rb.velocity = currentVelocity;
        }

        void FixedUpdate()
        {
            aliveTime += Time.fixedDeltaTime;
            
            if (playerTarget == null)
            {
                rb.velocity = transform.up * speed;
                return;
            }

            switch (type)
            {
                case MissileType.Straight:
                    // 直進（初期の向きのまま）
                    rb.velocity = transform.up * speed;
                    break;

                case MissileType.SmallHoming:
                    if (aliveTime <= homingDuration)
                    {
                        Homing(playerTarget.position);
                    }
                    else
                    {
                        rb.velocity = transform.up * speed;
                    }
                    break;

                case MissileType.EliteHoming:
                    // 予測エイム：相手の移動ベクトルを考慮した未来位置
                    Vector2 targetPos = playerTarget.position;
                    if (playerRb != null)
                    {
                        // 弾が相手に到達するまでの概算時間を計算し、その分だけ未来位置を予測
                        float dist = Vector2.Distance(transform.position, targetPos);
                        float timeToReach = dist / speed;
                        targetPos += playerRb.velocity * (timeToReach * 0.5f); // 予測しすぎないよう調整
                    }
                    Homing(targetPos);
                    break;

                case MissileType.Baka:
                    // 対象の大まかな方向に向かいつつ、サイン波でブレる
                    Vector2 baseDir = (playerTarget.position - transform.position).normalized;
                    float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                    
                    // ブレを加算
                    float wobble = Mathf.Sin(aliveTime * wobbleFrequency + wobbleOffset) * wobbleAmplitude;
                    angle += wobble - 90f; // -90 for transform.up alignment

                    Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnSpeed * Time.fixedDeltaTime);
                    rb.velocity = transform.up * speed;
                    break;
            }
        }

        private void Homing(Vector2 targetPos)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            rb.velocity = transform.up * speed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("PlayerBullet"))
            {
                // ボムやプレイヤー接触で消える場合
                // ダメージ処理はAlpha_Bullet_Controllerなどの別のスクリプトに任せるか、ここで実装
                Destroy(gameObject);
            }
        }
    }
}
