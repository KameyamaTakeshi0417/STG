using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 1 攻撃ビヘイビア: 『落ち椿・時間差トラップ』
    /// フィールド要所に典型的な赤椿アンカー（CamelliaTrap）を4〜5個設置。
    /// 本体は自機狙い精密弾（Precision Needle）を3連射してプレイヤーを動かす。
    /// 時間差で椿がポトリと沈み込んで自機へ交差弾を発射する（一部は不発）。
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_TsubakiPhase1", menuName = "EnemyAI/Behaviors/Tsubaki Phase 1")]
    public class Behavior_TsubakiPhase1 : EnemyBehaviorData_Base
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private GameObject needleBulletPrefab;

        [Header("Cycle Settings")]
        [SerializeField] private float cycleInterval = 4.0f;
        [SerializeField] private float initialDelay = 1.0f;
        [SerializeField] private int trapCountPerWave = 4;
        [SerializeField] private float trapActivateDelay = 2.4f;

        [Header("Host Precision Shots")]
        [SerializeField] private int hostBurstCount = 3;
        [SerializeField] private float hostBurstInterval = 0.18f;
        [SerializeField] private float hostNeedleSpeed = 11f;
        [SerializeField] private float hostNeedleDamage = 15f;

        [Header("Field Bounds for Traps")]
        [SerializeField] private Vector2 xBounds = new Vector2(-6.0f, 6.0f);
        [SerializeField] private Vector2 yBounds = new Vector2(-3.5f, 3.5f);

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (ai == null) yield break;

            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                // 1. 椿トラップをフィールド要所に設置
                SpawnTraps(ai);

                // 2. 本体からの自機狙い精密弾 3連射（プレイヤーを追い立てる）
                yield return ai.StartCoroutine(HostShootRoutine(ai));

                yield return new WaitForSeconds(cycleInterval);
            }
        }

        private void SpawnTraps(Alpha_EnemyAI ai)
        {
            if (trapPrefab == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector2 playerPos = player != null ? (Vector2)player.transform.position : Vector2.zero;

            for (int i = 0; i < trapCountPerWave; i++)
            {
                // プレイヤーの周囲や死角に散らす
                Vector2 spawnPos;
                if (i == 0)
                {
                    // 1個目はプレイヤーの少し先（予測退路）
                    spawnPos = playerPos + new Vector2(Random.Range(-2.5f, 2.5f), Random.Range(1.5f, 2.5f));
                }
                else
                {
                    spawnPos = new Vector2(
                        Random.Range(xBounds.x, xBounds.y),
                        Random.Range(yBounds.x, yBounds.y)
                    );
                }

                spawnPos.x = Mathf.Clamp(spawnPos.x, xBounds.x, xBounds.y);
                spawnPos.y = Mathf.Clamp(spawnPos.y, yBounds.x, yBounds.y);

                GameObject trapObj = Instantiate(trapPrefab, spawnPos, Quaternion.identity);
                var trap = trapObj.GetComponent<TsubakiTrapCamellia>();
                if (trap != null)
                {
                    // 1waveにつき最低1つは不発になるようにして不完全演出を見せる
                    bool forceMisfire = (i == trapCountPerWave - 1);
                    trap.Initialize(forceMisfire, trapActivateDelay);
                }
            }
        }

        private IEnumerator HostShootRoutine(Alpha_EnemyAI ai)
        {
            if (needleBulletPrefab == null || ai == null) yield break;

            for (int i = 0; i < hostBurstCount; i++)
            {
                if (ai == null) yield break;

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector2 targetPos = player != null ? (Vector2)player.transform.position : (Vector2)ai.transform.position + Vector2.down;
                Vector2 dir = (targetPos - (Vector2)ai.transform.position).normalized;

                GameObject bulletObj = null;
                if (Alpha_ObjectPoolManager.Instance != null)
                {
                    bulletObj = Alpha_ObjectPoolManager.Instance.Rent(needleBulletPrefab, ai.transform.position, Quaternion.identity);
                }
                else
                {
                    bulletObj = Instantiate(needleBulletPrefab, ai.transform.position, Quaternion.identity);
                }

                if (bulletObj != null)
                {
                    var needle = bulletObj.GetComponent<TsubakiNeedleBullet>();
                    if (needle != null)
                    {
                        needle.Initialize(dir, hostNeedleSpeed, hostNeedleDamage);
                    }
                }

                yield return new WaitForSeconds(hostBurstInterval);
            }
        }
    }
}
