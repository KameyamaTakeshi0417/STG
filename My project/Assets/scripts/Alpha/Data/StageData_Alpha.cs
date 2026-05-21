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
    }
}
