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

        [System.Serializable]
        public struct QualityProbability
        {
            [Tooltip("オーブのレアリティ、またはパーツの品質")]
            public int baseQuality; 

            [Tooltip("Common(1)が選ばれる確率")] public float prob1;
            [Tooltip("Uncommon(2)が選ばれる確率")] public float prob2;
            [Tooltip("Rare(3)が選ばれる確率")] public float prob3;
            [Tooltip("Divine(4)が選ばれる確率")] public float prob4;
        }

        [Header("Probability Settings")]
        [Tooltip("ベースの品質ごとの各レアリティの抽選確率（オーブドロップ・エフェクト付与で共有）")]
        public List<QualityProbability> qualityProbabilities = new List<QualityProbability>();

        private void Awake()
        {
            if (qualityProbabilities == null || qualityProbabilities.Count == 0)
            {
                qualityProbabilities = new List<QualityProbability>()
                {
                    new QualityProbability { baseQuality = 1, prob1 = 70f, prob2 = 25f, prob3 = 5f, prob4 = 0f },
                    new QualityProbability { baseQuality = 2, prob1 = 40f, prob2 = 50f, prob3 = 9.5f, prob4 = 0.5f },
                    new QualityProbability { baseQuality = 3, prob1 = 10f, prob2 = 40f, prob3 = 49f, prob4 = 1f },
                    new QualityProbability { baseQuality = 4, prob1 = 0f, prob2 = 30f, prob3 = 50f, prob4 = 20f }
                };
            }
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

        private int RollQuality(int baseQuality)
        {
            QualityProbability prob = qualityProbabilities.Find(q => q.baseQuality == baseQuality);
            if (prob.baseQuality == 0) // struct default
            {
                Debug.LogWarning($"[WeaponGenerator] No QualityProbability found for baseQuality {baseQuality}. Using fallback.");
                return 1;
            }

            float rand = Random.value * 100f;
            if (rand <= prob.prob1) return 1;
            if (rand <= prob.prob1 + prob.prob2) return 2;
            if (rand <= prob.prob1 + prob.prob2 + prob.prob3) return 3;
            return 4;
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

            // パーツごとの固有効果の付与
            List<WeaponEffectSO_Alpha> partSpecificEffects = null;
            switch (partType)
            {
                case WeaponPartType_Alpha.Bullet:
                    partSpecificEffects = series.bulletSpecificEffects;
                    break;
                case WeaponPartType_Alpha.Casing:
                    partSpecificEffects = series.casingSpecificEffects;
                    break;
                case WeaponPartType_Alpha.Primer:
                    partSpecificEffects = series.primerSpecificEffects;
                    break;
            }

            if (partSpecificEffects != null)
            {
                foreach (var effect in partSpecificEffects)
                {
                    if (effect != null)
                    {
                        instance.currentEffects.Add(effect);
                    }
                }
            }

            // グローバルバフのランダム付与
            if (globalBuffEffects != null && globalBuffEffects.Count > 0)
            {
                int maxRandomEffects = quality; // コモン(1)=1, アンコモン(2)=2, レア(3)=3, ディバイン(4)=4
                int addedCount = 0;

                while (addedCount < maxRandomEffects)
                {
                    int targetMinQuality = RollQuality(quality);
                    List<WeaponEffectSO_Alpha> availableEffects = new List<WeaponEffectSO_Alpha>();

                    // 指定されたレアリティ、またはそれ以下のエフェクトを探す（フォールバック用）
                    for (int q = targetMinQuality; q >= 1; q--)
                    {
                        foreach (var effect in globalBuffEffects)
                        {
                            // 既に付与されているエフェクトは除外する
                            if (effect != null && effect.minQuality == q && !instance.currentEffects.Contains(effect))
                            {
                                availableEffects.Add(effect);
                            }
                        }
                        if (availableEffects.Count > 0) break; // 見つかればそのレアリティから抽選
                    }

                    if (availableEffects.Count == 0)
                    {
                        // それでも見つからなければ全ての中からランダム（重複は避ける）
                        foreach (var effect in globalBuffEffects)
                        {
                            if (effect != null && !instance.currentEffects.Contains(effect))
                            {
                                availableEffects.Add(effect);
                            }
                        }
                    }

                    if (availableEffects.Count > 0)
                    {
                        var randomBuff = availableEffects[Random.Range(0, availableEffects.Count)];
                        if (randomBuff != null)
                        {
                            instance.currentEffects.Add(randomBuff);
                            addedCount++;
                            
                            // 次のエンチャントが付与可能なら、30%の確率で追加判定を行う
                            if (addedCount < maxRandomEffects)
                            {
                                if (Random.value > 0.3f)
                                {
                                    break; // 30%の抽選に漏れたら終了
                                }
                                // 当たりを引いた場合はループを継続し、さらに追加付与を行う
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break; // 付与可能なエフェクトがなくなったら終了
                    }
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
