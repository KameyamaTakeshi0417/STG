using UnityEngine;

namespace Alpha.Data
{
    [CreateAssetMenu(fileName = "NewStageData", menuName = "Alpha/Stage Data")]
    public class StageData_Alpha : ScriptableObject
    {
        [Tooltip("ステージ前半のシーケンスデータ")]
        public StageSequenceData_Alpha firstHalf;
        
        [Tooltip("ステージ後半のシーケンスデータ")]
        public StageSequenceData_Alpha secondHalf;

        [Header("ADV Data")]
        [Tooltip("ボス戦前に再生されるADVデータ（任意）")]
        public ADVData_Alpha preBossADV;
        
        [Tooltip("ボス撃破後に再生されるADVデータ（任意）")]
        public ADVData_Alpha postBossADV;
    }
}
