using UnityEngine;
using System.Collections.Generic;
using Alpha.Core.Utils;

namespace Alpha.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class Alpha_PlayerGraze : MonoBehaviour
    {
        [Tooltip("1tickごとに獲得する報酬ポイント")]
        public int pointsPerTick = 1;

        // 現在コライダー内に入っている敵弾のリスト
        private HashSet<Collider2D> grazingBullets = new HashSet<Collider2D>();

        private void OnEnable()
        {
            if (Alpha_TickManager.Instance != null)
            {
                Alpha_TickManager.Instance.OnTick += HandleTick;
            }
        }

        private void OnDisable()
        {
            if (Alpha_TickManager.Instance != null)
            {
                Alpha_TickManager.Instance.OnTick -= HandleTick;
            }
        }

        private void Start()
        {
            // インスタンスが存在しないかもしれないので、Start時にも念のため購読を試みる
            // （OnEnable時にまだAlpha_TickManagerがAwakeされていない場合への対応）
            if (Alpha_TickManager.Instance != null)
            {
                Alpha_TickManager.Instance.OnTick -= HandleTick; // 二重登録防止
                Alpha_TickManager.Instance.OnTick += HandleTick;
            }
        }

        private void HandleTick()
        {
            // Nullまたは非アクティブになった弾を取り除く
            grazingBullets.RemoveWhere(b => b == null || !b.gameObject.activeInHierarchy);

            if (grazingBullets.Count > 0)
            {
                // グレイズ中なのでポイントを加算
                if (Alpha.Flow.RewardManager_Alpha.Instance != null)
                {
                    Alpha.Flow.RewardManager_Alpha.Instance.AddPoints(pointsPerTick);
                    // Debug.Log($"[Graze] Added {pointsPerTick} points! Grazing {grazingBullets.Count} bullets.");
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Bullet_Base bullet = collision.GetComponent<Bullet_Base>();
            if (bullet != null && bullet.isEnemyBullet)
            {
                if (!grazingBullets.Contains(collision))
                {
                    if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
                    {
                        Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnHitSparks(bullet.transform.position, new Color(0.4f, 0.8f, 1f), 2);
                    }
                }
                grazingBullets.Add(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Bullet_Base bullet = collision.GetComponent<Bullet_Base>();
            if (bullet != null && bullet.isEnemyBullet)
            {
                grazingBullets.Remove(collision);
            }
        }
    }
}
