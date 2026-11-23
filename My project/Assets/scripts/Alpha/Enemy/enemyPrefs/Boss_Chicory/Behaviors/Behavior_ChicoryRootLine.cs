using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    /// <summary>
    /// Phase 2 召喚ビヘイビア: 3種AIカクカク根ライン（Root Trace Line）展開
    /// 画面左端から4本（秀才1・真面目2・マヌケ1）を同時発射。
    /// 最後のステップが開始された瞬間に次の波を生成し、途切れない連続攻防を作る。
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_ChicoryRootLine", menuName = "EnemyAI/Behaviors/Chicory Root Line")]
    public class Behavior_ChicoryRootLine : EnemyBehaviorData_Base
    {
        [Header("Prefab")]
        [SerializeField] private GameObject rootLinePrefab;

        [Header("Spawn Settings")]
        [Tooltip("初動待機時間（秒）")]
        [SerializeField] private float initialDelay = 1.0f;
        [Tooltip("画面左端のX座標（ワールド座標目安）")]
        [SerializeField] private float spawnLeftX = -10.5f;
        [Tooltip("Y座標の分散範囲（ワールド座標目安）")]
        [SerializeField] private Vector2 ySpawnRange = new Vector2(-4.5f, 4.5f);
        [Tooltip("フォールバック用最大リピート間隔（秒）")]
        [SerializeField] private float maxIntervalFallback = 6.0f;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (rootLinePrefab == null) yield break;

            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                bool nextWaveTriggered = false;

                // 4本（秀才1・真面目2・マヌケ1）のラインを生成
                SpawnLineWave(ai, () => nextWaveTriggered = true);

                // 最終ステップが始まった通知を待機（またはフォールバック時間）
                float elapsed = 0f;
                while (!nextWaveTriggered && elapsed < maxIntervalFallback)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        private void SpawnLineWave(Alpha_EnemyAI ai, System.Action onWaveLastStepCallback)
        {
            // 4本のAI割り当て: [Smart, Loyal, Loyal, Goofy]
            RootAiType[] types = new RootAiType[]
            {
                RootAiType.Smart,
                RootAiType.Loyal,
                RootAiType.Loyal,
                RootAiType.Goofy
            };

            // Y座標を均等かつわずかにランダムに分散
            float yStep = (ySpawnRange.y - ySpawnRange.x) / (types.Length + 1);

            bool callbackFired = false;

            for (int i = 0; i < types.Length; i++)
            {
                float targetY = ySpawnRange.x + yStep * (i + 1) + Random.Range(-0.35f, 0.35f);
                Vector3 spawnPos = new Vector3(spawnLeftX, targetY, 0f);

                GameObject lineObj = Instantiate(rootLinePrefab, spawnPos, Quaternion.identity);
                if (ai != null && ai.PhaseSpawnedObjects != null)
                {
                    ai.PhaseSpawnedObjects.Add(lineObj);
                }

                ChicoryRootLine rootLine = lineObj.GetComponent<ChicoryRootLine>();
                if (rootLine != null)
                {
                    // 代表の1本（または最初に最終ステップに入ったライン）が次ウェーブの合図を送る
                    System.Action notifyAction = () =>
                    {
                        if (!callbackFired)
                        {
                            callbackFired = true;
                            onWaveLastStepCallback?.Invoke();
                        }
                    };

                    rootLine.Initialize(types[i], notifyAction);
                }
            }
        }
    }
}
