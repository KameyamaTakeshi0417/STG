using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    /// <summary>
    /// Phase 1 召喚ビヘイビア: 花時計地雷（Clock Mine）展開
    /// 「プレイヤーの足元に必ず1個」＋「フィールドのランダム位置に2〜3個」を同時展開
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_ChicoryClockMine", menuName = "EnemyAI/Behaviors/Chicory Clock Mine")]
    public class Behavior_ChicoryClockMine : EnemyBehaviorData_Base
    {
        [Header("Prefab")]
        [SerializeField] private GameObject clockMinePrefab;

        [Header("Spawn Settings")]
        [Tooltip("地雷を展開する周期（秒）")]
        [SerializeField] private float spawnInterval = 3.2f;
        [Tooltip("ランダム配置する個数（足元とは別）")]
        [SerializeField] private int randomMinesCount = 2;
        [Tooltip("地雷同士の最小マージン（重なり防止）")]
        [SerializeField] private float minDistanceBetweenMines = 3.2f;
        [Tooltip("初動待機時間（秒）")]
        [SerializeField] private float initialDelay = 1.0f;

        [Header("Field Bounds")]
        [SerializeField] private Vector2 xBounds = new Vector2(-6.5f, 6.5f);
        [SerializeField] private Vector2 yBounds = new Vector2(-4f, 4f);

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (clockMinePrefab == null) yield break;

            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                SpawnClockMinesSet(ai);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnClockMinesSet(Alpha_EnemyAI ai)
        {
            List<Vector2> spawnPoints = new List<Vector2>();

            // 1. プレイヤー足元に必ず1個
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector2 playerPos = player != null ? (Vector2)player.transform.position : Vector2.zero;
            spawnPoints.Add(playerPos);

            // 2. フィールドランダム位置に重なりを避けて配置
            for (int i = 0; i < randomMinesCount; i++)
            {
                Vector2 randomPos = Vector2.zero;
                bool valid = false;
                int attempts = 0;

                while (!valid && attempts < 15)
                {
                    attempts++;
                    randomPos = new Vector2(
                        Random.Range(xBounds.x, xBounds.y),
                        Random.Range(yBounds.x, yBounds.y)
                    );

                    valid = true;
                    foreach (var pt in spawnPoints)
                    {
                        if (Vector2.Distance(randomPos, pt) < minDistanceBetweenMines)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    spawnPoints.Add(randomPos);
                }
            }

            // 3. 地雷を生成・起動
            foreach (var pos in spawnPoints)
            {
                GameObject mineObj = Instantiate(clockMinePrefab, pos, Quaternion.identity);
                if (ai != null && ai.PhaseSpawnedObjects != null)
                {
                    ai.PhaseSpawnedObjects.Add(mineObj);
                }

                ChicoryClockMine mine = mineObj.GetComponent<ChicoryClockMine>();
                if (mine != null)
                {
                    float slightStagger = Random.Range(0f, 0.12f);
                    mine.ActivateMine(slightStagger);
                }
            }
        }
    }
}
