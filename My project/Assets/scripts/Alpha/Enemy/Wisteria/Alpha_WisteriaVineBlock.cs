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
        
        private float lifeTimer = 0f;

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
