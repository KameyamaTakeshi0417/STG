using UnityEngine;
using System.Collections.Generic;
using Alpha.Data;

namespace Alpha.Flow
{
    public class SpawnManager_Alpha : MonoBehaviour
    {
        private StageSequenceData_Alpha currentSequence;
        private int currentWaveIndex = 0;
        
        // フィールド上に存在する敵（雑魚）のリスト
        private List<GameObject> activeMobs = new List<GameObject>();
        // 予兆中でまだスポーンしていない敵の数
        private int pendingSpawns = 0;

        [Tooltip("敵出現の2秒前に表示する予兆マーカー（未設定時はResourcesからロードします）")]
        public GameObject spawnIndicatorPrefab;

        public void SetupSequence(StageSequenceData_Alpha sequence)
        {
            currentSequence = sequence;
            currentWaveIndex = 0;
            if (spawnIndicatorPrefab == null)
            {
                spawnIndicatorPrefab = Resources.Load<GameObject>("Objects/SpawnSign");
            }
            pendingSpawns = 0;
            activeMobs.Clear();
        }

        public void CheckSpawn(float currentTime)
        {
            if (currentSequence == null || currentWaveIndex >= currentSequence.waves.Count)
                return;

            // スキップで時間が飛んだ場合も考慮し、currentTime以下の未スポーンウェーブを全て処理
            while (currentWaveIndex < currentSequence.waves.Count && 
                   currentTime >= currentSequence.waves[currentWaveIndex].time)
            {
                SpawnWave(currentSequence.waves[currentWaveIndex]);
                currentWaveIndex++;
            }
        }

        private void SpawnWave(WaveData_Alpha wave)
        {
            Debug.Log($"[SpawnManager] Spawning Wave at {wave.time}s");
            StartCoroutine(SpawnWaveDelayed(wave));
        }

        private System.Collections.IEnumerator SpawnWaveDelayed(WaveData_Alpha wave)
        {
            List<GameObject> indicators = new List<GameObject>();

            // 1. 予兆マーカーの生成
            foreach (var spawn in wave.spawns)
            {
                if (spawn.enemyPrefab != null)
                {
                    pendingSpawns++;
                    if (spawnIndicatorPrefab != null)
                    {
                        GameObject indicator = Instantiate(spawnIndicatorPrefab, spawn.spawnPosition, Quaternion.identity);
                        indicators.Add(indicator);
                    }
                }
            }

            // 2秒待機
            yield return new WaitForSeconds(2.0f);

            // 2. マーカーを削除し、実際の敵を生成
            foreach (var indicator in indicators)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
            }

            foreach (var spawn in wave.spawns)
            {
                if (spawn.enemyPrefab != null)
                {
                    pendingSpawns--;
                    GameObject enemy = Instantiate(spawn.enemyPrefab, spawn.spawnPosition, Quaternion.identity);
                    activeMobs.Add(enemy);
                }
            }
            
            // 破壊されたオブジェクトのリスト整理は IsMobCleared など適当なタイミングで行う
            CleanUpDeadMobs();
        }

        public void SpawnBoss(GameObject bossPrefab)
        {
            if (bossPrefab != null)
            {
                Debug.Log($"[SpawnManager] Spawning Boss!");
                // ボスの出現位置は固定（あるいはデータ定義に拡張）とするが、今回は原点付近
                Instantiate(bossPrefab, new Vector2(0, 3f), Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[SpawnManager] Boss Prefab is missing!");
            }
        }

        /// <summary>
        /// 次のウェーブの時間を返す（スキップ用）
        /// </summary>
        public float GetNextWaveTime()
        {
            if (currentSequence == null || currentWaveIndex >= currentSequence.waves.Count)
            {
                // 次のウェーブがない場合はシーケンスの終了時間を返す
                return currentSequence != null ? currentSequence.duration : float.MaxValue;
            }
            return currentSequence.waves[currentWaveIndex].time;
        }

        /// <summary>
        /// 雑魚が全滅しているか判定する
        /// </summary>
        public bool IsMobCleared()
        {
            CleanUpDeadMobs();
            return activeMobs.Count == 0 && pendingSpawns <= 0;
        }

        private void CleanUpDeadMobs()
        {
            activeMobs.RemoveAll(mob => mob == null);
        }
    }
}
