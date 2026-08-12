using UnityEngine;
using System.Collections.Generic;

namespace Alpha.Data
{
    [System.Serializable]
    public class EnemyRushData_Alpha
    {
        [Tooltip("ラッシュの開始時間（秒）")]
        public float startTime;
        [Tooltip("ラッシュの終了時間（秒）")]
        public float endTime;
        [Tooltip("敵がスポーンする間隔（秒）")]
        public float spawnInterval = 1.0f;
        [Tooltip("スポーンさせる敵のプレハブ（複数登録でランダム抽選）")]
        public List<GameObject> enemyPrefabs = new List<GameObject>();
        [Tooltip("1回の間隔でスポーンする敵の最小数")]
        public int minSpawnCountPerInterval = 1;
        [Tooltip("1回の間隔でスポーンする敵の最大数")]
        public int maxSpawnCountPerInterval = 1;
        [Tooltip("スポーン範囲の中心座標")]
        public Vector2 spawnCenter;
        [Tooltip("スポーン範囲のサイズ（X軸、Y軸のブレ幅）")]
        public Vector2 spawnAreaSize = new Vector2(5f, 5f);
    }

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

        [Tooltip("チュートリアル表示中にタイムラインの進行を一時停止するか")]
        public bool pauseTimeline = true;
    }

    [CreateAssetMenu(fileName = "NewStageSequence", menuName = "Alpha/Stage Sequence Data")]
    public class StageSequenceData_Alpha : ScriptableObject
    {
        [Tooltip("このフェーズ（前半または後半）の全体の長さ（秒）")]
        public float duration;
        
        [Tooltip("ウェーブのリスト")]
        public List<WaveData_Alpha> waves = new List<WaveData_Alpha>();

        [Tooltip("エネミーラッシュのリスト")]
        public List<EnemyRushData_Alpha> enemyRushes = new List<EnemyRushData_Alpha>();

        [Tooltip("チュートリアルイベントのリスト")]
        public List<TutorialEventData_Alpha> tutorialEvents = new List<TutorialEventData_Alpha>();
        
        [Tooltip("フェーズの最後に出現するボス/中ボスのプレハブ")]
        public GameObject bossPrefab;
        
        [Tooltip("このフェーズ完了時（ボス・中ボス撃破時）の基本報酬オーブドロップ数")]
        public int rewardDropCount = 1;
    }
}
