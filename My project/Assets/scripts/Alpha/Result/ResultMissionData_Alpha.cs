using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Result
{
    public enum ResultMissionType
    {
        GameClear,             // ゲームクリア（全5ステージクリア・中央確定用）
        MobKills,              // 雑魚敵撃破数（全7回雑魚戦累積）
        OrbsOpened,            // オーブ開封数（1ゲーム通算）
        GrazePoints,           // グレイズ獲得ポイント累計
        JustEscapeKills,       // 逃走寸止めキル数累計
        QuickReloads,          // クイックリロード発動回数累計
        MaxBulletSize,         // 発射した最大弾サイズ
        MaxSingleDamage,       // 単発最大ダメージ
        LowStaminaExhaustion,  // 息切れ（スタミナ枯渇）回数が一定以下
        BossKills,             // ボス（中ボス・大ボス）累計撃破数
        SizeSynergyActive      // スノーボールまたはスナイパー装備
    }

    [System.Serializable]
    public class ResultMission
    {
        public string id;
        public string title;
        public string description;
        public int difficulty; // 1: 銅, 2: 銀, 3: 金, Special: 中央
        public int scorePoints; // 100, 300, 500, 1000
        public ResultMissionType missionType;
        public float targetValue;

        public ResultMission(string id, string title, string description, int difficulty, int scorePoints, ResultMissionType type, float targetVal)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.difficulty = difficulty;
            this.scorePoints = scorePoints;
            this.missionType = type;
            this.targetValue = targetVal;
        }

        /// <summary>
        /// 1ゲーム通算の実績値をもとに課題が達成されたか判定
        /// </summary>
        public bool IsCompleted(RunStatsTracker_Alpha stats)
        {
            if (stats == null) return false;

            switch (missionType)
            {
                case ResultMissionType.GameClear:
                    return stats.isGameCleared;

                case ResultMissionType.MobKills:
                    return stats.totalMobKills >= (int)targetValue;

                case ResultMissionType.OrbsOpened:
                    return stats.orbsOpened >= (int)targetValue;

                case ResultMissionType.GrazePoints:
                    return stats.totalGrazePoints >= (int)targetValue;

                case ResultMissionType.JustEscapeKills:
                    return stats.justEscapeKills >= (int)targetValue;

                case ResultMissionType.QuickReloads:
                    return stats.quickReloadCount >= (int)targetValue;

                case ResultMissionType.MaxBulletSize:
                    return stats.maxBulletSize >= targetValue;

                case ResultMissionType.MaxSingleDamage:
                    return stats.maxDamageDealt >= targetValue;

                case ResultMissionType.LowStaminaExhaustion:
                    return stats.staminaExhaustionCount <= (int)targetValue;

                case ResultMissionType.BossKills:
                    return stats.bossesDefeatedCount >= (int)targetValue;

                case ResultMissionType.SizeSynergyActive:
                    var pStatus = playerStatusManager_Alpha.Instance;
                    return pStatus != null && (pStatus.hasSnowballEffect || pStatus.hasSniperEffect);

                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// 1ゲーム通算（全5ステージ・雑魚戦7回＋ボス戦想定）の課題プール生成ファクトリ
    /// </summary>
    public static class ResultMissionFactory_Alpha
    {
        public static ResultMission CreateCenterMission()
        {
            return new ResultMission(
                "GAME_CLEAR",
                "ゲームクリア",
                "全5ステージをクリアし生還する",
                2, // Special
                1000,
                ResultMissionType.GameClear,
                1f
            );
        }

        public static List<ResultMission> GetAllPoolMissions()
        {
            return new List<ResultMission>
            {
                // ★1 (Easy / 銅・100pt - ゲーム中盤で到達しやすい目標)
                new ResultMission("MOB_50", "雑魚掃討", "雑魚敵を累計50体以上撃破", 1, 100, ResultMissionType.MobKills, 50f),
                new ResultMission("ORB_8", "兵装収集", "オーブを累計8個以上開封", 1, 100, ResultMissionType.OrbsOpened, 8f),
                new ResultMission("GRAZE_50P", "弾幕の狭間", "グレイズで累計50P獲得", 1, 100, ResultMissionType.GrazePoints, 50f),
                new ResultMission("ESCAPE_2", "追撃の一撃", "逃走加速中の敵を累計2体撃破", 1, 100, ResultMissionType.JustEscapeKills, 2f),
                new ResultMission("RELOAD_15", "スワップ射撃", "クイックリロードを累計15回発動", 1, 100, ResultMissionType.QuickReloads, 15f),
                new ResultMission("SIZE_20", "巨大弾丸", "弾サイズ2.0倍以上の弾を発射", 1, 100, ResultMissionType.MaxBulletSize, 2.0f),
                new ResultMission("DMG_100", "痛打", "単発ダメージ100以上を記録", 1, 100, ResultMissionType.MaxSingleDamage, 100f),
                new ResultMission("BOSS_2", "激闘の幕開け", "ボス（中ボス含む）を累計2体撃破", 1, 100, ResultMissionType.BossKills, 2f),

                // ★2 (Medium / 銀・300pt - ゲーム終盤で腕前や意識が試される目標)
                new ResultMission("MOB_100", "大量殲滅", "雑魚敵を累計100体以上撃破", 2, 300, ResultMissionType.MobKills, 100f),
                new ResultMission("ORB_12", "装備拡充", "オーブを累計12個以上開封", 2, 300, ResultMissionType.OrbsOpened, 12f),
                new ResultMission("GRAZE_120P", "紙一重の舞", "グレイズで累計120P獲得", 2, 300, ResultMissionType.GrazePoints, 120f),
                new ResultMission("ESCAPE_5", "逃走阻止網", "逃走加速中の敵を累計5体撃破", 2, 300, ResultMissionType.JustEscapeKills, 5f),
                new ResultMission("RELOAD_30", "銃撃のロンド", "クイックリロードを累計30回発動", 2, 300, ResultMissionType.QuickReloads, 30f),
                new ResultMission("SIZE_35", "超弩級弾", "弾サイズ3.5倍以上の弾を発射", 2, 300, ResultMissionType.MaxBulletSize, 3.5f),
                new ResultMission("DMG_250", "必殺の一撃", "単発ダメージ250以上を記録", 2, 300, ResultMissionType.MaxSingleDamage, 250f),
                new ResultMission("NO_EXHAUST", "冷静沈着", "スタミナ枯渇（息切れ）累計2回以内", 2, 300, ResultMissionType.LowStaminaExhaustion, 2f),
                new ResultMission("SYNERGY", "特化兵装", "スノーボールまたはスナイパーを装備", 2, 300, ResultMissionType.SizeSynergyActive, 1f),
                new ResultMission("BOSS_4", "死線突破", "ボス（中ボス含む）を累計4体撃破", 2, 300, ResultMissionType.BossKills, 4f),

                // ★3 (Hard / 金・500pt - ごく稀に出現する極限のやり込み目標)
                new ResultMission("MOB_150", "完全駆逐", "雑魚敵を累計150体以上撃破", 3, 500, ResultMissionType.MobKills, 150f),
                new ResultMission("ORB_16", "武庫の主", "オーブを累計16個以上開封", 3, 500, ResultMissionType.OrbsOpened, 16f),
                new ResultMission("GRAZE_200P", "極限の見切り", "グレイズで累計200P獲得", 3, 500, ResultMissionType.GrazePoints, 200f),
                new ResultMission("ESCAPE_8", "絶対包囲", "逃走加速中の敵を累計8体撃破", 3, 500, ResultMissionType.JustEscapeKills, 8f),
                new ResultMission("RELOAD_50", "兵装マスター", "クイックリロードを累計50回発動", 3, 500, ResultMissionType.QuickReloads, 50f),
                new ResultMission("SIZE_50", "天変地異", "弾サイズ5.0倍以上の弾を発射", 3, 500, ResultMissionType.MaxBulletSize, 5.0f),
                new ResultMission("DMG_500", "破壊神", "単発ダメージ500以上を記録", 3, 500, ResultMissionType.MaxSingleDamage, 500f)
            };
        }

        /// <summary>
        /// N×Nグリッド用のミッション配列を生成。中央はクリア確定枠、その他は難易度1〜2中心（まれに3）でランダム抽選
        /// </summary>
        public static ResultMission[] GenerateGridMissions(int gridSize = 3)
        {
            int totalCells = gridSize * gridSize;
            ResultMission[] grid = new ResultMission[totalCells];
            int centerIndex = (gridSize / 2) * gridSize + (gridSize / 2);

            // 中央マスをセット
            grid[centerIndex] = CreateCenterMission();

            var pool = GetAllPoolMissions();
            List<ResultMission> tier1 = pool.FindAll(m => m.difficulty == 1);
            List<ResultMission> tier2 = pool.FindAll(m => m.difficulty == 2);
            List<ResultMission> tier3 = pool.FindAll(m => m.difficulty == 3);

            HashSet<string> usedMissionIds = new HashSet<string>();

            for (int i = 0; i < totalCells; i++)
            {
                if (i == centerIndex) continue;

                // 難易度抽選: ★1(60%), ★2(35%), ★3(5%)
                float r = UnityEngine.Random.value;
                int targetTier = 1;
                if (r < 0.60f) targetTier = 1;
                else if (r < 0.95f) targetTier = 2;
                else targetTier = 3;

                ResultMission picked = PickFromTier(targetTier, tier1, tier2, tier3, usedMissionIds);
                if (picked == null)
                {
                    picked = PickFromTier(1, tier1, tier2, tier3, usedMissionIds);
                }

                if (picked != null)
                {
                    usedMissionIds.Add(picked.id);
                    grid[i] = picked;
                }
                else
                {
                    grid[i] = new ResultMission($"GEN_{i}", "兵装稼働", "戦闘を継続する", 1, 100, ResultMissionType.MobKills, 30f);
                }
            }

            return grid;
        }

        private static ResultMission PickFromTier(int tier, List<ResultMission> t1, List<ResultMission> t2, List<ResultMission> t3, HashSet<string> used)
        {
            List<ResultMission> targetList = tier == 3 ? t3 : (tier == 2 ? t2 : t1);
            var available = targetList.FindAll(m => !used.Contains(m.id));
            if (available.Count > 0)
            {
                return available[UnityEngine.Random.Range(0, available.Count)];
            }
            var allAvailable = new List<ResultMission>();
            allAvailable.AddRange(t1.FindAll(m => !used.Contains(m.id)));
            allAvailable.AddRange(t2.FindAll(m => !used.Contains(m.id)));
            allAvailable.AddRange(t3.FindAll(m => !used.Contains(m.id)));

            if (allAvailable.Count > 0)
            {
                return allAvailable[UnityEngine.Random.Range(0, allAvailable.Count)];
            }
            return null;
        }
    }
}
