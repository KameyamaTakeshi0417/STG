using UnityEngine;

namespace Alpha.Enemy
{
    public class EnemyReward_Alpha : MonoBehaviour
    {
        [Tooltip("ドロップする経験値量")]
        public int expAmount = 10;
        
        [Tooltip("オーブ（宝箱）のドロップ率（0.0〜1.0）")]
        [Range(0f, 1f)]
        public float orbDropRate = 0.05f;

        [Tooltip("オーブのプレハブ")]
        public GameObject orbPrefab;

        /// <summary>
        /// 敵がプレイヤーの攻撃等で倒された時に呼ばれる（逃亡時は呼ばない）
        /// </summary>
        public void DropRewards()
        {
            // 経験値付与の処理（仮実装：ログ出力）
            Debug.Log($"[{gameObject.name}] Dropped {expAmount} EXP.");
            
            // 実際のEXPマネージャー等に送る場合はここで行う
            // 例: GameManager.Instance.AddExp(expAmount);

            // オーブのドロップ判定
            if (orbPrefab != null && Random.value <= orbDropRate)
            {
                Instantiate(orbPrefab, transform.position, Quaternion.identity);
                Debug.Log($"[{gameObject.name}] Dropped an Orb!");
            }
        }
    }
}
