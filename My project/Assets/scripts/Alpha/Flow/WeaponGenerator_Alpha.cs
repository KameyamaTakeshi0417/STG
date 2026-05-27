using System.Collections.Generic;
using UnityEngine;
using Alpha.Data;

namespace Alpha.Flow
{
    public class WeaponGenerator_Alpha : MonoBehaviour
    {
        public static WeaponGenerator_Alpha Instance { get; private set; }

        [Header("Series Pool")]
        [Tooltip("実装済みの武器シリーズをすべて登録する")]
        public List<WeaponSeriesData_Alpha> allSeriesPool = new List<WeaponSeriesData_Alpha>();

        // BossId -> Series mapping
        [System.Serializable]
        public class BossRewardMapping
        {
            public string bossId;
            public List<WeaponSeriesData_Alpha> bossSeries;
        }
        public List<BossRewardMapping> bossRewardMappings = new List<BossRewardMapping>();

        [Header("Global Random Effects")]
        [Tooltip("ランダム付与されるバフエフェクトのプール")]
        public List<WeaponEffectSO_Alpha> globalBuffEffects = new List<WeaponEffectSO_Alpha>();

        [Tooltip("将来用：ランダム付与されるデバフエフェクトのプール")]
        public List<WeaponEffectSO_Alpha> globalDebuffEffects = new List<WeaponEffectSO_Alpha>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// オーブ1つにつき、3つのパーツ候補を生成する
        /// </summary>
        public List<WeaponPartInstance_Alpha> GenerateChoices(OrbData_Alpha orbData)
        {
            List<WeaponPartInstance_Alpha> choices = new List<WeaponPartInstance_Alpha>();

            // 枠1: Quality が OrbRarity に確定
            int quality1 = orbData.orbRarity;
            WeaponSeriesData_Alpha series1 = GetRandomSeries(quality1, orbData.bossId, true);
            choices.Add(CreatePartInstance(series1, quality1));

            // 枠2: 抽選
            int quality2 = RollQuality(orbData.orbRarity);
            WeaponSeriesData_Alpha series2 = GetRandomSeries(quality2, "");
            choices.Add(CreatePartInstance(series2, quality2));

            // 枠3: 抽選
            int quality3 = RollQuality(orbData.orbRarity);
            WeaponSeriesData_Alpha series3 = GetRandomSeries(quality3, "");
            choices.Add(CreatePartInstance(series3, quality3));

            return choices;
        }

        private int RollQuality(int orbRarity)
        {
            float rand = Random.value * 100f;
            switch (orbRarity)
            {
                case 1:
                    // 70 / 25 / 5 / 0
                    if (rand <= 70f) return 1;
                    if (rand <= 95f) return 2;
                    return 3;
                case 2:
                    // 40 / 50 / 9.5 / 0.5
                    if (rand <= 40f) return 1;
                    if (rand <= 90f) return 2;
                    if (rand <= 99.5f) return 3;
                    return 4;
                case 3:
                    // 10 / 40 / 49 / 1
                    if (rand <= 10f) return 1;
                    if (rand <= 50f) return 2;
                    if (rand <= 99f) return 3;
                    return 4;
                case 4:
                    // 0 / 30 / 50 / 20
                    if (rand <= 30f) return 2;
                    if (rand <= 80f) return 3;
                    return 4;
                default:
                    return 1;
            }
        }

        private WeaponSeriesData_Alpha GetRandomSeries(int quality, string bossId, bool preferBossSeries = false)
        {
            List<WeaponSeriesData_Alpha> validPool = new List<WeaponSeriesData_Alpha>();

            // ボス対応枠の処理
            if (preferBossSeries && !string.IsNullOrEmpty(bossId))
            {
                var mapping = bossRewardMappings.Find(m => m.bossId == bossId);
                if (mapping != null && mapping.bossSeries.Count > 0)
                {
                    // ボスのプール内からQuality条件を満たすものを探す
                    foreach (var s in mapping.bossSeries)
                    {
                        if (s.minQuality <= quality) validPool.Add(s);
                    }
                    if (validPool.Count > 0)
                    {
                        return validPool[Random.Range(0, validPool.Count)];
                    }
                }
            }

            // 通常のプールから抽選
            foreach (var s in allSeriesPool)
            {
                // Hybrid構成になったため、ベストスロットかどうかの追加プールはなく、パッシブ効果プールからのみ引く
                // アクティブ効果（弾への付与）はシリーズ自身が持っている activeEffectClassName を元に生成される
                if (s.minQuality <= quality)
                {
                    validPool.Add(s);
                }
            }

            if (validPool.Count > 0)
            {
                return validPool[Random.Range(0, validPool.Count)];
            }

            // フォールバック（条件を満たすものがない場合）
            Debug.LogWarning($"[WeaponGenerator] No valid series found for quality {quality}. Returning random.");
            return allSeriesPool.Count > 0 ? allSeriesPool[Random.Range(0, allSeriesPool.Count)] : null;
        }

        private WeaponPartInstance_Alpha CreatePartInstance(WeaponSeriesData_Alpha series, int quality)
        {
            if (series == null) return null;

            // 部位の決定 (bestSlot が 50%, それ以外が 25% ずつ)
            WeaponPartType_Alpha partType;
            float rand = Random.value;
            if (rand <= 0.5f)
            {
                partType = series.bestSlot;
            }
            else
            {
                // 残りの2部位からランダムに選ぶ（それぞれ25%）
                List<WeaponPartType_Alpha> otherParts = new List<WeaponPartType_Alpha>();
                foreach (WeaponPartType_Alpha type in System.Enum.GetValues(typeof(WeaponPartType_Alpha)))
                {
                    if (type != series.bestSlot) otherParts.Add(type);
                }
                partType = otherParts[Random.Range(0, otherParts.Count)];
            }
            
            WeaponPartInstance_Alpha instance = new WeaponPartInstance_Alpha(series, partType, quality);

            // 固有効果の付与（無制限で全て付与）
            if (series.passiveEffects != null)
            {
                foreach (var effect in series.passiveEffects)
                {
                    if (effect != null)
                    {
                        instance.currentEffects.Add(effect);
                    }
                }
            }

            // グローバルバフのランダム付与（基本1つ）
            if (globalBuffEffects != null && globalBuffEffects.Count > 0)
            {
                var randomBuff = globalBuffEffects[Random.Range(0, globalBuffEffects.Count)];
                if (randomBuff != null)
                {
                    instance.currentEffects.Add(randomBuff);
                }
            }

            // 将来用：グローバルデバフの付与
            // if (globalDebuffEffects != null && globalDebuffEffects.Count > 0)
            // {
            //     // 難易度や品質などに応じてデバフを抽選・付与する処理をここに追加
            // }

            return instance;
        }
    }
}
