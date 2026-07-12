using UnityEngine;
using DG.Tweening;

namespace Alpha.Enemy.Wisteria
{
    public class Alpha_WisteriaVineBlock : Health
    {
        [Tooltip("ツタブロックの生存時間（秒）。0以下の場合は時間経過で消滅しない。")]
        public float lifetime = 10f;
        
        [Header("Growth Animation (Sprite Mask)")]
        [Tooltip("徐々に表示させるためのSpriteMaskのTransform（※空の正方形画像を設定し、Pivotを左端にしておく）")]
        public Transform maskTransform;
        
        [Header("Damage & Knockback Settings")]
        [Tooltip("プレイヤーに与えるダメージ")]
        public float damageAmount = 20f;
        [Tooltip("プレイヤーを吹き飛ばす初速度（QuickStepと同じくらいの速さ）")]
        public float knockbackForce = 20f;
        [Tooltip("吹っ飛びにかかる時間")]
        public float knockbackDuration = 0.5f;
        [Tooltip("吹っ飛び後の無敵時間（連続ヒット防止）")]
        public float invincibilityDuration = 1.0f;

        private float lifeTimer = 0f;

        private void HandlePlayerCollision(GameObject playerObj)
        {
            if (isDead) return;

            // PlayerHealthを取得
            PlayerHealth pHealth = playerObj.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                // すでに無敵状態なら何もしない
                if (pHealth.isInvincible) return;

                // 1. ダメージを与える
                pHealth.TakeDamage(damageAmount);

                // 2. 無敵時間を付与 (吹っ飛び時間 + その後の猶予時間)
                pHealth.MakeInvincible(knockbackDuration + invincibilityDuration);

                // 3. 吹っ飛び（ノックバック）処理
                Player_Control_Alpha pControl = playerObj.GetComponent<Player_Control_Alpha>();
                if (pControl != null)
                {
                    // ツタの中心からプレイヤーに向かうベクトル
                    Vector2 dir = (playerObj.transform.position - transform.position).normalized;
                    if (dir == Vector2.zero) dir = Vector2.up; // 万が一完全に重なっていた場合
                    
                    float actualForce = knockbackForce;
                    // ダッシュと同じ速度を計算して適用する
                    if (global::playerStatusManager_Alpha.Instance != null)
                    {
                        var status = global::playerStatusManager_Alpha.Instance;
                        if (status.dashDuration > 0f)
                        {
                            actualForce = status.dashDistance / status.dashDuration;
                        }
                    }

                    pControl.ApplyKnockback(dir, actualForce, knockbackDuration);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                HandlePlayerCollision(collision.gameObject);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                HandlePlayerCollision(collision.gameObject);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            // 物理挙動を無効化
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }
        }

        public override void setSlideHPBar()
        {
            // ツタブロックには個別のHPバーを表示しない
        }

        /// <summary>
        /// ブロック生成時に呼ばれ、マスクのスケールをアニメーションしてツタを徐々に表示する
        /// </summary>
        /// <param name="duration">アニメーションにかける時間</param>
        public void Grow(float duration)
        {
            if (maskTransform != null)
            {
                // 初期状態：マスクの幅を0にして完全に隠す
                Vector3 initialScale = maskTransform.localScale;
                maskTransform.localScale = new Vector3(0f, initialScale.y, initialScale.z);
                
                // DOTweenで元の幅(initialScale.x)まで伸ばす
                maskTransform.DOScaleX(initialScale.x, duration).SetEase(Ease.Linear);
            }
        }

        protected override void Update()
        {
            base.Update();
            
            if (lifetime > 0f)
            {
                lifeTimer += Time.deltaTime;
                if (lifeTimer >= lifetime)
                {
                    Die();
                }
            }
        }

        protected override void Die()
        {
            // アイテムドロップ等はしない
            isDead = true;
            
            // ツタ消滅時はアニメーションをキルする
            if (maskTransform != null)
            {
                maskTransform.DOKill();
            }
            
            Destroy(gameObject);
        }
    }
}
