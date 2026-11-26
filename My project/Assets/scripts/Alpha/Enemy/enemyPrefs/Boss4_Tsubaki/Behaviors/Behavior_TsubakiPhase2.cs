using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 2 攻撃ビヘイビア: 『多層・零落の椿檻』
    /// プレイヤーを中心に多層（3層）の八重椿檻を展開。
    /// 1層目にひび入り弱点弾、2・3層目に欠損抜け穴が配置され、脱出DPSチェックとなる。
    /// 檻の合間にツバキ本体は軽い牽制弾を差し込む。
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_TsubakiPhase2", menuName = "EnemyAI/Behaviors/Tsubaki Phase 2")]
    public class Behavior_TsubakiPhase2 : EnemyBehaviorData_Base
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject cagePrefab;
        [SerializeField] private GameObject needleBulletPrefab;

        [Header("Cage Cycle Settings")]
        [SerializeField] private float initialDelay = 1.0f;
        [SerializeField] private float postCageCooldown = 3.0f;

        [Header("Host Poke Shots")]
        [SerializeField] private int pokeCount = 2;
        [SerializeField] private float pokeInterval = 0.8f;
        [SerializeField] private float pokeNeedleSpeed = 8f;
        [SerializeField] private float pokeDamage = 12f;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (ai == null || cagePrefab == null) yield break;

            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                // 1. プレイヤー中心に多層椿檻を展開
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector3 cagePos = player != null ? player.transform.position : ai.transform.position + Vector3.down * 2.5f;

                GameObject cageObj = Instantiate(cagePrefab, cagePos, Quaternion.identity);
                var cage = cageObj.GetComponent<TsubakiCageController>();
                if (cage != null)
                {
                    cage.DeployCage(cagePos);
                }

                // 2. 檻展開中、ツバキ本体が軽い牽制弾を放つ
                for (int i = 0; i < pokeCount; i++)
                {
                    yield return new WaitForSeconds(pokeInterval);
                    if (ai == null) yield break;

                    ShootPokeNeedle(ai);
                }

                // 檻が解決（脱出または圧壊）するまで待機 + クールダウン
                yield return new WaitForSeconds(postCageCooldown + 4.5f);
            }
        }

        private void ShootPokeNeedle(Alpha_EnemyAI ai)
        {
            if (needleBulletPrefab == null || ai == null) return;

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
                    needle.Initialize(dir, pokeNeedleSpeed, pokeDamage);
                }
            }
        }
    }
}
