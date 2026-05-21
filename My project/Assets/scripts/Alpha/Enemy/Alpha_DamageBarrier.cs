using UnityEngine;

namespace Alpha.Enemy
{
    public class Alpha_DamageBarrier : MonoBehaviour
    {
        [Tooltip("バリアが現在アクティブかどうか")]
        public bool isActive = false;

        [Tooltip("プレイヤーが使う場合はtrue、敵が使う場合はfalse")]
        public bool isPlayerFriendly = false;

        [Tooltip("接触時のダメージ量")]
        public float damageAmount = 10f;

        [Tooltip("接触時に付与するスタン時間（秒）。0ならスタンなし")]
        public float stunDuration = 0f;

        [Tooltip("連続ヒットを防ぐためのクールダウン（秒）")]
        public float hitCooldown = 0.5f;

        private float lastHitTime = -100f;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!isActive) return;

            // クールダウンチェック
            if (Time.time < lastHitTime + hitCooldown) return;

            if (isPlayerFriendly)
            {
                // プレイヤーのバリア：敵に当たる
                if (collision.CompareTag("Enemy"))
                {
                    ApplyDamageAndStun(collision.gameObject);
                }
            }
            else
            {
                // 敵のバリア：プレイヤーに当たる
                if (collision.CompareTag("Player"))
                {
                    ApplyDamageAndStun(collision.gameObject);
                }
            }
        }

        private void ApplyDamageAndStun(GameObject target)
        {
            _Health_Base targetHealth = target.GetComponent<_Health_Base>();
            if (targetHealth != null)
            {
                // ダメージを与える
                targetHealth.TakeDamage(damageAmount);

                // スタンを付与する
                if (stunDuration > 0f)
                {
                    targetHealth.ApplyStun(stunDuration);
                }

                lastHitTime = Time.time;
                Debug.Log($"[DamageBarrier] Hit {target.name} for {damageAmount} damage and {stunDuration}s stun.");
            }
        }

        // 外部からバリアのON/OFFを切り替える用
        public void SetBarrierActive(bool active)
        {
            isActive = active;
            // 必要に応じてバリアの見た目（SpriteRendererやParticle）をここで切り替える
        }
    }
}
