using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 3 攻撃ビヘイビア: 『雪中寒椿・冬吹雪』
    /// 画面全体の冬吹雪＆粉雪システムを起動し、
    /// ツバキ本体は残像ダッシュ移動を交えながら自機狙いの一閃精密弾を差し込む。
    /// </summary>
    [CreateAssetMenu(fileName = "Behavior_TsubakiPhase3", menuName = "EnemyAI/Behaviors/Tsubaki Phase 3")]
    public class Behavior_TsubakiPhase3 : EnemyBehaviorData_Base
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject blizzardPrefab;
        [SerializeField] private GameObject needleBulletPrefab;

        [Header("Host Slash Settings")]
        [SerializeField] private float slashInterval = 2.5f;
        [SerializeField] private float needleSpeed = 13f; // 吹雪に紛れる高速一閃
        [SerializeField] private float needleDamage = 18f;
        [SerializeField] private int needleCount = 3;

        private GameObject blizzardInstance;

        public override IEnumerator RunBehavior(Alpha_EnemyAI ai)
        {
            if (ai == null) yield break;

            // 1. 冬吹雪システムの起動
            if (blizzardPrefab != null && blizzardInstance == null)
            {
                blizzardInstance = Instantiate(blizzardPrefab, Vector3.zero, Quaternion.identity);
                var blizzard = blizzardInstance.GetComponent<TsubakiSnowBlizzard>();
                if (blizzard != null)
                {
                    blizzard.StartBlizzard();
                }
            }

            yield return new WaitForSeconds(1.5f);

            while (true)
            {
                // 吹雪の合間に自機狙いの一閃（3連射または高速弾）
                yield return ai.StartCoroutine(SlashRoutine(ai));
                yield return new WaitForSeconds(slashInterval);
            }
        }

        private IEnumerator SlashRoutine(Alpha_EnemyAI ai)
        {
            if (needleBulletPrefab == null || ai == null) yield break;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector2 targetPos = player != null ? (Vector2)player.transform.position : (Vector2)ai.transform.position + Vector2.down;
            Vector2 baseDir = (targetPos - (Vector2)ai.transform.position).normalized;

            for (int i = 0; i < needleCount; i++)
            {
                if (ai == null) yield break;

                float spread = (i - (needleCount - 1) * 0.5f) * 10f;
                Vector2 dir = Quaternion.Euler(0, 0, spread) * baseDir;

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
                        needle.Initialize(dir, needleSpeed, needleDamage);
                    }
                }

                yield return new WaitForSeconds(0.12f);
            }
        }
    }
}
