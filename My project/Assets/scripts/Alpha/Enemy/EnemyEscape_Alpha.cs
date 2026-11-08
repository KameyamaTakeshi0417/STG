using UnityEngine;

namespace Alpha.Enemy
{
    public class EnemyEscape_Alpha : MonoBehaviour
    {
        [Tooltip("出現してから逃亡するまでの時間（秒）")]
        public float escapeTime = 20f;
        
        [Header("Escape Audio Settings")]
        [Tooltip("逃走時の虫の足音SE（未設定時は Resources/SE/SE_Enemy_Escape_Insect_01_Kasa が自動ロードされます）")]
        public AudioClip escapeSEClip;
        [Range(0f, 1f)]
        public float escapeSEVolume = 0.75f;

        private static float lastEscapeSoundTime = -1f;
        private const float MinEscapeSoundInterval = 0.08f;

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

            PlayEscapeSE();
            
            // 逃亡の演出（フェードアウトなど）を入れる場合はここに追加
            
            // 報酬を落とさずに自身を破棄する
            Destroy(gameObject);
        }

        private void PlayEscapeSE()
        {
            if (Time.unscaledTime - lastEscapeSoundTime < MinEscapeSoundInterval) return;
            lastEscapeSoundTime = Time.unscaledTime;

            if (escapeSEClip == null)
            {
                escapeSEClip = Resources.Load<AudioClip>("SE/SE_Enemy_Escape_Insect_01_Kasa");
            }

            if (escapeSEClip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                float pitch = Random.Range(0.95f, 1.05f);
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(escapeSEClip, escapeSEVolume, pitch);
            }
        }
        
        /// <summary>
        /// 外部（キル時など）から手動で呼び出される用（体力0で倒された時など）
        /// 通常のキルではEnemyReward_AlphaのDropRewardsを呼んでからDestroyする
        /// </summary>
        public void KillByPlayer()
        {
            if (isEscaping) return;
            
            var health = GetComponent<_Health_Base>();
            if (health != null)
            {
                health.DropEnemyRewards();
            }
            Destroy(gameObject);
        }
    }
}
