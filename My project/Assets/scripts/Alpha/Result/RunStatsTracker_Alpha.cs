using UnityEngine;

namespace Alpha.Result
{
    /// <summary>
    /// 1ゲーム（全5ステージ・雑魚戦7回＋ボス戦）全体のプレイ統計をリアルタイムに累積記録・集計するトラッカー
    /// </summary>
    public class RunStatsTracker_Alpha : MonoBehaviour
    {
        private static RunStatsTracker_Alpha _instance;
        public static RunStatsTracker_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RunStatsTracker_Alpha>();
                    if (_instance == null)
                    {
                        var go = new GameObject("RunStatsTracker_Alpha");
                        _instance = go.AddComponent<RunStatsTracker_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Battle Stats (Game Total)")]
        public int totalMobKills = 0;
        public int bossesDefeatedCount = 0;
        public bool midBossDefeated = false;
        public bool isGameCleared = false; // 全5ステージ完全クリア
        public int totalGrazeCount = 0;
        public int totalGrazePoints = 0; // グレイズで稼いだ累計オーブポイント
        public int justEscapeKills = 0;
        public int quickReloadCount = 0;

        [Header("Damage & Size Stats")]
        public float maxDamageDealt = 0f;
        public float maxBulletSize = 1.0f;
        public int minBulletSizeKills = 0;

        [Header("Survival & Technical Stats")]
        public int staminaExhaustionCount = 0;
        public float damageTakenDuringMidBoss = 0f;
        public bool isFightingMidBoss = false;
        public int orbsOpened = 0;
        public bool hasSetBonus = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// ゲーム全体（新しいラン）の開始時に統計を完全にリセット
        /// </summary>
        public void ResetForNewRun()
        {
            totalMobKills = 0;
            bossesDefeatedCount = 0;
            midBossDefeated = false;
            isGameCleared = false;
            totalGrazeCount = 0;
            totalGrazePoints = 0;
            justEscapeKills = 0;
            quickReloadCount = 0;
            maxDamageDealt = 0f;
            maxBulletSize = 1.0f;
            minBulletSizeKills = 0;
            staminaExhaustionCount = 0;
            damageTakenDuringMidBoss = 0f;
            isFightingMidBoss = false;
            orbsOpened = 0;
            hasSetBonus = false;
            Debug.Log("[RunStatsTracker] Reset stats for NEW GAME RUN.");
        }

        /// <summary>
        /// 後方互換用（ステージ間ではリセットせず保持）
        /// </summary>
        public void ResetForNewStage()
        {
            // 1ゲーム通算集計のため、ステージ間ではリセットしない
            Debug.Log("[RunStatsTracker] Stage completed. Retaining cumulative run stats.");
        }

        // --- 記録用ヘルパーメソッド ---

        public void RecordMobKill(bool isNearEscape = false, float bulletSizeWhenKilled = 1.0f)
        {
            totalMobKills++;
            if (isNearEscape)
            {
                justEscapeKills++;
            }
            if (bulletSizeWhenKilled <= 0.65f)
            {
                minBulletSizeKills++;
            }
        }

        public void RecordGraze(int points = 1)
        {
            totalGrazeCount++;
            totalGrazePoints += points;
        }

        public void RecordQuickReload()
        {
            quickReloadCount++;
        }

        public void RecordDamageDealt(float dmg)
        {
            if (dmg > maxDamageDealt)
            {
                maxDamageDealt = dmg;
            }
        }

        public void RecordBulletSize(float size)
        {
            if (size > maxBulletSize)
            {
                maxBulletSize = size;
            }
        }

        public void RecordStaminaExhaustion()
        {
            staminaExhaustionCount++;
        }

        public void RecordDamageTaken(float dmg)
        {
            if (isFightingMidBoss)
            {
                damageTakenDuringMidBoss += dmg;
            }
        }

        public void RecordOrbOpened()
        {
            orbsOpened++;
        }

        public void SetMidBossState(bool active)
        {
            isFightingMidBoss = active;
        }

        public void SetMidBossDefeated()
        {
            midBossDefeated = true;
            isFightingMidBoss = false;
            bossesDefeatedCount++;
        }

        public void SetStageBossDefeated(bool isFinalBoss = false)
        {
            bossesDefeatedCount++;
            if (isFinalBoss)
            {
                isGameCleared = true;
            }
        }
    }
}
