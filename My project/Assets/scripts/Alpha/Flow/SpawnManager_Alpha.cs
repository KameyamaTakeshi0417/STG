using UnityEngine;
using System.Collections.Generic;
using Alpha.Data;

namespace Alpha.Flow
{
    public class SpawnManager_Alpha : MonoBehaviour
    {
        private StageSequenceData_Alpha currentSequence;
        private int currentWaveIndex = 0;
        private int currentRushIndex = 0;

        public class EnemyRushState
        {
            public EnemyRushData_Alpha data;
            public float nextSpawnTime;
            public EnemyRushState(EnemyRushData_Alpha d)
            {
                data = d;
                nextSpawnTime = d.startTime;
            }
        }
        private List<EnemyRushState> activeRushes = new List<EnemyRushState>();
        
        // フィールド上に存在する敵（雑魚）のリスト
        private List<GameObject> activeMobs = new List<GameObject>();
        // 予兆中でまだスポーンしていない敵の数
        private int pendingSpawns = 0;
        private int activeIndicatorCount = 0;
        public int maxRushIndicators = 30;

        [Tooltip("敵出現の2秒前に表示する予兆マーカー（未設定時はResourcesからロードします）")]
        public GameObject spawnIndicatorPrefab;

        public void SetupSequence(StageSequenceData_Alpha sequence)
        {
            currentSequence = sequence;
            currentWaveIndex = 0;
            currentRushIndex = 0;
            if (spawnIndicatorPrefab == null)
            {
                spawnIndicatorPrefab = Resources.Load<GameObject>("Objects/SpawnSign");
            }
            pendingSpawns = 0;
            activeIndicatorCount = 0;
            activeMobs.Clear();
            activeRushes.Clear();
        }

        public void ClearActiveRushes() { activeRushes.Clear(); }

        public void CheckSpawn(float currentTime)
        {
            if (currentSequence == null) return;

            // スキップで時間が飛んだ場合も考慮し、currentTime以下の未スポーンウェーブを全て処理
            while (currentWaveIndex < currentSequence.waves.Count && 
                   currentTime >= currentSequence.waves[currentWaveIndex].time)
            {
                SpawnWave(currentSequence.waves[currentWaveIndex]);
                currentWaveIndex++;
            }

            // ラッシュの開始判定
            if (currentSequence.enemyRushes != null)
            {
                while (currentRushIndex < currentSequence.enemyRushes.Count && 
                       currentTime >= currentSequence.enemyRushes[currentRushIndex].startTime)
                {
                    activeRushes.Add(new EnemyRushState(currentSequence.enemyRushes[currentRushIndex]));
                    currentRushIndex++;
                }
            }

            // アクティブラッシュの更新処理
            for (int i = activeRushes.Count - 1; i >= 0; i--)
            {
                var rush = activeRushes[i];
                if (currentTime > rush.data.endTime)
                {
                    activeRushes.RemoveAt(i);
                    continue;
                }

                while (currentTime >= rush.nextSpawnTime)
                {
                    SpawnRushEnemy(rush);
                    rush.nextSpawnTime += Mathf.Max(0.01f, rush.data.spawnInterval); // 0除算や無限ループ防止
                }
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

        public float GetNextWaveTime()
        {
            if (currentSequence == null) return float.MaxValue;

            float nextWaveTime = float.MaxValue;
            if (currentWaveIndex < currentSequence.waves.Count)
            {
                nextWaveTime = currentSequence.waves[currentWaveIndex].time;
            }

            float nextRushTime = float.MaxValue;
            if (currentSequence.enemyRushes != null && currentRushIndex < currentSequence.enemyRushes.Count)
            {
                nextRushTime = currentSequence.enemyRushes[currentRushIndex].startTime;
            }

            float nextTime = Mathf.Min(nextWaveTime, nextRushTime);

            if (nextTime == float.MaxValue)
            {
                // 次のウェーブ・ラッシュがない場合はシーケンスの終了時間を返す
                return currentSequence.duration;
            }

            return nextTime;
        }

        public bool IsRushActive()
        {
            return activeRushes.Count > 0;
        }

        /// <summary>
        /// 雑魚が全滅しているか判定する
        /// </summary>
        public bool IsMobCleared()
        {
            CleanUpDeadMobs();
            return activeMobs.Count == 0 && pendingSpawns <= 0 && activeRushes.Count == 0;
        }

        private void CleanUpDeadMobs()
        {
            activeMobs.RemoveAll(mob => mob == null);
        }

        private void SpawnRushEnemy(EnemyRushState rush)
        {
            if (rush.data.enemyPrefabs == null || rush.data.enemyPrefabs.Count == 0) return;

            int count = UnityEngine.Random.Range(rush.data.minSpawnCountPerInterval, rush.data.maxSpawnCountPerInterval + 1);
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = rush.data.enemyPrefabs[UnityEngine.Random.Range(0, rush.data.enemyPrefabs.Count)];
                if (prefab == null) continue;

                Vector2 spawnPos = new Vector2(
                    rush.data.spawnCenter.x + UnityEngine.Random.Range(-rush.data.spawnAreaSize.x / 2f, rush.data.spawnAreaSize.x / 2f),
                    rush.data.spawnCenter.y + UnityEngine.Random.Range(-rush.data.spawnAreaSize.y / 2f, rush.data.spawnAreaSize.y / 2f)
                );

                StartCoroutine(SpawnRushEnemyDelayed(prefab, spawnPos));
            }
        }

        private System.Collections.IEnumerator SpawnRushEnemyDelayed(GameObject prefab, Vector2 spawnPos)
        {
            pendingSpawns++;
            bool useIndicator = activeIndicatorCount < maxRushIndicators;
            GameObject indicator = null;

            if (useIndicator && spawnIndicatorPrefab != null)
            {
                activeIndicatorCount++;
                indicator = Instantiate(spawnIndicatorPrefab, spawnPos, Quaternion.identity);
            }

            if (useIndicator)
            {
                yield return new WaitForSeconds(2.0f);
            }

            if (indicator != null)
            {
                Destroy(indicator);
                activeIndicatorCount--;
            }

            pendingSpawns--;
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeMobs.Add(enemy);
        }
    }
}

