using UnityEngine;
using System.Collections.Generic;

namespace Alpha.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class Alpha_PlayerGraze : MonoBehaviour
    {
        [Header("Graze Rewards")]
        [Tooltip("グレイズ1回ごとに獲得するオーブ（報酬）ポイント")]
        public int pointsPerGraze = 1;

        [Tooltip("グレイズ1回ごとに回復するスタミナ量")]
        public float staminaRecovery = 0.8f;

        [Header("Audio Settings")]
        [Tooltip("グレイズSE（跳弾チュン音）")]
        public AudioClip grazeSEClip;
        [Range(0f, 1f)]
        public float grazeSEVolume = 0.8f;

        // グレイズ済みの弾コライダーの記録（二重判定防止）
        private HashSet<Collider2D> grazedBullets = new HashSet<Collider2D>();
        private static float lastGrazeSoundTime = -1f;
        private const float MinGrazeSoundInterval = 0.04f; // 高速連続かすり時の耳障り防止

        private void Start()
        {
            if (grazeSEClip == null)
            {
                grazeSEClip = Resources.Load<AudioClip>("SE/SE_Player_Graze_Ricochet");
            }
        }

        private void Update()
        {
            // 非アクティブまたは破壊された弾の定期クリーンアップ
            if (grazedBullets.Count > 0)
            {
                grazedBullets.RemoveWhere(b => b == null || !b.gameObject.activeInHierarchy);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Bullet_Base bullet = collision.GetComponent<Bullet_Base>();
            if (bullet != null && bullet.isEnemyBullet)
            {
                // まだこの弾でグレイズしていない場合のみ判定
                if (!grazedBullets.Contains(collision))
                {
                    grazedBullets.Add(collision);
                    TriggerGraze(bullet, collision);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 弾がかすり判定エリアを出た場合、リストから除外
            if (grazedBullets.Contains(collision))
            {
                grazedBullets.Remove(collision);
            }
        }

        private void TriggerGraze(Bullet_Base bullet, Collider2D collision)
        {
            // 1. スタミナ微量回復
            var status = playerStatusManager_Alpha.Instance;
            if (status != null)
            {
                status.currentStamina = Mathf.Min(status.maxStamina, status.currentStamina + staminaRecovery);
                // 息切れ状態でもスタミナが一定値まで戻れば早期復帰
                if (status.isStaminaExhausted && status.currentStamina >= status.maxStamina * 0.25f)
                {
                    status.isStaminaExhausted = false;
                }
            }

            // 2. オーブゲージ +1
            if (Alpha.Flow.RewardManager_Alpha.Instance != null)
            {
                Alpha.Flow.RewardManager_Alpha.Instance.AddPoints(pointsPerGraze);
            }

            // 統計記録（回数と獲得P）
            if (Alpha.Result.RunStatsTracker_Alpha.Instance != null)
            {
                Alpha.Result.RunStatsTracker_Alpha.Instance.RecordGraze(pointsPerGraze);
            }

            // 3. グレイズSE再生（鉄板跳弾音：チュン！）
            PlayGrazeSE();

            // 4. 金属火花エフェクト
            if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
            {
                Vector3 contactPos = collision.ClosestPoint(transform.position);
                Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnHitSparks(contactPos, new Color(1f, 0.95f, 0.7f), 3);
            }
        }

        private void PlayGrazeSE()
        {
            if (Time.unscaledTime - lastGrazeSoundTime < MinGrazeSoundInterval) return;
            lastGrazeSoundTime = Time.unscaledTime;

            if (grazeSEClip == null)
            {
                grazeSEClip = Resources.Load<AudioClip>("SE/SE_Player_Graze_Ricochet");
            }

            if (grazeSEClip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                // 小気味よいピッチ揺らぎ (0.96 〜 1.12)
                float pitch = Random.Range(0.96f, 1.12f);
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(grazeSEClip, grazeSEVolume, pitch);
            }
        }
    }
}
