using UnityEngine;
using System.Collections.Generic;

namespace Alpha.Data
{
    [CreateAssetMenu(fileName = "NewStageSequence", menuName = "Alpha/Stage Sequence Data")]
    public class StageSequenceData_Alpha : ScriptableObject
    {
        [Tooltip("このフェーズ（前半または後半）の全体の長さ（秒）")]
        public float duration;
        
        [Tooltip("ウェーブのリスト")]
        public List<WaveData_Alpha> waves = new List<WaveData_Alpha>();
        
        [Tooltip("シークエンスバーに表示するマーカーのリスト")]
        public List<MarkerData_Alpha> markers = new List<MarkerData_Alpha>();
        
        [Tooltip("フェーズの最後に出現するボス/中ボスのプレハブ")]
        public GameObject bossPrefab;
    }
}
