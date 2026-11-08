using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Data
{
    [CreateAssetMenu(fileName = "NewRewardTable", menuName = "Alpha/Reward/Reward Table Data")]
    public class RewardTableData_Alpha : ScriptableObject
    {
        [Header("Table Info")]
        public string tableName = "Default Reward Table";

        [Header("Weapon Series Pool")]
        [Tooltip("このテーブルから排出される武器シリーズ一覧")]
        public List<WeaponSeriesData_Alpha> seriesPool = new List<WeaponSeriesData_Alpha>();

        [Header("Probability Settings")]
        [Tooltip("オーブの品質ごとの各レアリティの抽選確率")]
        public List<Flow.WeaponGenerator_Alpha.QualityProbability> qualityProbabilities = new List<Flow.WeaponGenerator_Alpha.QualityProbability>();

        [Header("Boss Reward Mappings")]
        [Tooltip("ボスIDごとの確定/優先排出シリーズ")]
        public List<Flow.WeaponGenerator_Alpha.BossRewardMapping> bossRewardMappings = new List<Flow.WeaponGenerator_Alpha.BossRewardMapping>();

        [Header("Effects Pool")]
        [Tooltip("ランダム付与される追加バフ効果")]
        public List<WeaponEffectSO_Alpha> buffEffects = new List<WeaponEffectSO_Alpha>();
    }
}
