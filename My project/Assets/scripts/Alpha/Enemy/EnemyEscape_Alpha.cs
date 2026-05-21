using UnityEngine;

namespace Alpha.Enemy
{
    public class EnemyEscape_Alpha : MonoBehaviour
    {
        [Tooltip("出現してから逃亡するまでの時間（秒）")]
        public float escapeTime = 20f;
        
        private float timer;
        private bool isEscaping;

        void Update()
        {
            if (isEscaping) return;

            timer += Time.deltaTime;
            if (timer >= escapeTime)
            {
                Escape();
            }
        }

        private void Escape()
        {
            isEscaping = true;
            Debug.Log($"[{gameObject.name}] Escaped!");
            
            // 逃亡の演出（フェードアウトなど）を入れる場合はここに追加
            
            // 報酬を落とさずに自身を破棄する
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 外部（キル時など）から手動で呼び出される用（体力0で倒された時など）
        /// 通常のキルではEnemyReward_AlphaのDropRewardsを呼んでからDestroyする
        /// </summary>
        public void KillByPlayer()
        {
            if (isEscaping) return;
            
            var reward = GetComponent<EnemyReward_Alpha>();
            if (reward != null)
            {
                reward.DropRewards();
            }
            Destroy(gameObject);
        }
    }
}
