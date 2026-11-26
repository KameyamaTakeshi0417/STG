using UnityEngine;

namespace Alpha.Data
{
    [CreateAssetMenu(fileName = "NewStageData", menuName = "Alpha/Stage Data")]
    public class StageData_Alpha : ScriptableObject
    {
        [Header("Stage Info")]
        public string stageName = "Stage 1";

        [Tooltip("ステージ前半のシーケンスデータ")]
        public StageSequenceData_Alpha firstHalf;
        
        [Tooltip("ステージ後半のシーケンスデータ")]
        public StageSequenceData_Alpha secondHalf;

        [Header("Reward Table Data")]
        [Tooltip("このステージでドロップする報酬テーブル（未設定時はWeaponGeneratorのデフォルトを使用）")]
        public RewardTableData_Alpha stageRewardTable;

        [Header("ADV Data")]
        [Tooltip("前半戦の報酬フェーズ後に再生するADVデータ（中ボス前等、任意）")]
        public ADVData_Alpha postFirstHalfRewardADV;

        [Tooltip("鍛冶フェーズ前に再生するADVデータ（任意）")]
        public ADVData_Alpha preBlacksmithADV;

        [Tooltip("鍛冶フェーズ後に再生するADVデータ（任意）")]
        public ADVData_Alpha postBlacksmithADV;

        [Tooltip("ボス前に再生するADVデータ（任意）")]
        public ADVData_Alpha preBossADV;
        
        [Tooltip("ボス後に再生するADVデータ（任意）")]
        public ADVData_Alpha postBossADV;

        [Header("BGM Data")]
        [Tooltip("道中・ウェーブ進行時のBGM")]
        public AudioClip stageBGM;

        [Tooltip("中ボス時のBGM（任意）")]
        public AudioClip midBossBGM;

        [Tooltip("ボス出現時に鳴らすBGM（途切れずシームレスに移行できます）")]
        public AudioClip bossBGM;

        [Tooltip("ステージクリアのテキスト表示時に鳴らすSE")]
        public AudioClip stageClearSE;

        [Header("Stage Flow Customization (Optional)")]
        [Tooltip("ステージ前半終了時の中ボス戦をスキップするかどうか（ステージ4の前半雑魚のみフロー用）")]
        public bool skipFirstHalfMidBoss = false;

        [Tooltip("ステージ後半開始時に道中雑魚を挟まず即座に中ボス戦を開始するかどうか（ステージ4用）")]
        public bool startSecondHalfWithMidBoss = false;

        [Tooltip("中ボスプレハブ（未設定時は firstHalf.bossPrefab を使用）")]
        public GameObject midBossPrefab;

        [Tooltip("大ボスプレハブ（未設定時は secondHalf.bossPrefab を使用）")]
        public GameObject stageBossPrefab;

    }
}
