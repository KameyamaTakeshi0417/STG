using UnityEngine;
using System.Collections.Generic;

namespace Alpha.Data
{
    [System.Serializable]
    public class TutorialEventData_Alpha
    {
        [Tooltip("チュートリアルを起動するタイミング（秒）")]
        public float time;
        [Tooltip("チュートリアルのID（Canvas内の対応するオブジェクト名）")]
        public string tutorialId;
        
        [Tooltip("フェードインして自動で消えるモードを使用するか")]
        public bool useFadeMode = false;
        [Tooltip("フェードモード時の表示時間（秒）")]
        public float displayDuration = 3f;
    }

    [CreateAssetMenu(fileName = "NewStageSequence", menuName = "Alpha/Stage Sequence Data")]
    public class StageSequenceData_Alpha : ScriptableObject
    {
        [Tooltip("このフェーズ（前半または後半）の全体の長さ（秒）")]
        public float duration;
        
        [Tooltip("ウェーブのリスト")]
        public List<WaveData_Alpha> waves = new List<WaveData_Alpha>();
        
        [Tooltip("シークエンスバーに表示するマーカーのリスト")]
        public List<MarkerData_Alpha> markers = new List<MarkerData_Alpha>();

        [Tooltip("チュートリアルイベントのリスト")]
        public List<TutorialEventData_Alpha> tutorialEvents = new List<TutorialEventData_Alpha>();
        
        [Tooltip("フェーズの最後に出現するボス/中ボスのプレハブ")]
        public GameObject bossPrefab;
        
        [Tooltip("このフェーズ完了時（ボス・中ボス撃破時）の基本報酬オーブドロップ数")]
        public int rewardDropCount = 1;
    }
}
