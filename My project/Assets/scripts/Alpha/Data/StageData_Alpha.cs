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

        [Header("ADV Data")]
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

    }
}
