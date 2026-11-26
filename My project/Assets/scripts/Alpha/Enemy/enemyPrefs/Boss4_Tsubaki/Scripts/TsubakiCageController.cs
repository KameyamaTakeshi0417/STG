using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 2: 『多層・零落の椿檻』マネージャー
    /// プレイヤーを中心に八重椿の3重同心円包囲網を展開。
    /// - 1層目: 一部（2〜3個）がひび入り弱点弾（YaeConeCracked）
    /// - 2層目・3層目: 1層目の弱点弾の延長線上にあたる箇所が最初から欠落（出口）
    /// カウントダウン（約4.5秒）内に脱出できなければ中心へ高速収束圧壊（特大ダメージ）。
    /// </summary>
    public class TsubakiCageController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject coneNormalPrefab;
        [SerializeField] private GameObject coneCrackedPrefab;
        [SerializeField] private GameObject collapseVfxPrefab;
        [SerializeField] private GameObject missingSlotVfxPrefab;

        [Header("Cage Parameters")]
        [SerializeField] private float countdownDuration = 4.5f;
        [SerializeField] private float layer1Radius = 2.4f;
        [SerializeField] private float layer2Radius = 3.8f;
        [SerializeField] private float layer3Radius = 5.2f;

        [SerializeField] private int layer1Count = 14;
        [SerializeField] private int layer2Count = 18;
        [SerializeField] private int layer3Count = 22;

        [SerializeField] private float exitAngleWidth = 35f; // 欠損出口の角度幅（度）
        [SerializeField] private float collapseDamage = 80f; // 時間切れ特大ダメージ

        private List<TsubakiYaeConeBullet> activeCones = new List<TsubakiYaeConeBullet>();
        private Vector3 cageCenter;
        private Transform playerTransform;
        private bool isResolved = false;
        private Coroutine cageRoutine;

        public void DeployCage(Vector3 center)
        {
            cageCenter = center;
            transform.position = center;
            isResolved = false;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;

            SpawnLayers();
            cageRoutine = StartCoroutine(CageCountdownRoutine());
        }

        private void SpawnLayers()
        {
            // 出口の基準角度（プレイヤーの右上または左上など斜め上に設定して回避しやすくする）
            float exitBaseAngle = 45f + Random.Range(-15f, 15f);

            // 1層目（最内層: プレイヤーが直接攻撃可能）
            SpawnLayerCones(1, layer1Radius, layer1Count, exitBaseAngle, exitAngleWidth, true);

            // 2層目（中層: 出口部分は欠損）
            SpawnLayerCones(2, layer2Radius, layer2Count, exitBaseAngle, exitAngleWidth, false);

            // 3層目（外層: 出口部分は欠損）
            SpawnLayerCones(3, layer3Radius, layer3Count, exitBaseAngle, exitAngleWidth, false);
        }

        private void SpawnLayerCones(int layerIndex, float radius, int count, float exitAngle, float exitWidth, bool allowCracked)
        {
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float currentAngle = i * angleStep;
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, exitAngle));

                // 2層目・3層目は出口角度範囲内なら欠損スロット（生成しない）
                if (layerIndex > 1 && angleDiff <= exitWidth * 0.5f)
                {
                    // 欠損スロット演出（一瞬赤いノイズVFXを出す）
                    if (missingSlotVfxPrefab != null)
                    {
                        Vector3 missingPos = cageCenter + (Vector3)(GetDirFromAngle(currentAngle) * radius);
                        Instantiate(missingSlotVfxPrefab, missingPos, Quaternion.identity);
                    }
                    continue;
                }

                // 1層目の出口角度範囲内は「ひび入り弱点弾」
                bool isCracked = allowCracked && (angleDiff <= exitWidth * 0.5f);

                GameObject prefabToUse = isCracked ? coneCrackedPrefab : coneNormalPrefab;
                if (prefabToUse == null) continue;

                Vector2 dir = GetDirFromAngle(currentAngle);
                Vector3 spawnPos = cageCenter + (Vector3)(dir * radius);

                // 内側（中心）を向く回転
                float rotAngle = currentAngle + 90f; // 三角錐の先端を中心に向ける
                Quaternion rot = Quaternion.Euler(0, 0, rotAngle);

                GameObject obj = Instantiate(prefabToUse, spawnPos, rot, transform);
                var cone = obj.GetComponent<TsubakiYaeConeBullet>();
                if (cone != null)
                {
                    cone.Setup(isCracked, isCracked ? 60f : 9999f, 20f);
                    activeCones.Add(cone);
                }
            }
        }

        private Vector2 GetDirFromAngle(float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        private IEnumerator CageCountdownRoutine()
        {
            float elapsed = 0f;
            float maxSafeRadius = layer3Radius + 0.8f;

            while (elapsed < countdownDuration)
            {
                elapsed += Time.deltaTime;

                // プレイヤーが包囲網の外へ脱出したかを判定
                if (playerTransform != null)
                {
                    float dist = Vector2.Distance(playerTransform.position, cageCenter);
                    if (dist > maxSafeRadius)
                    {
                        // 突破・脱出成功！
                        OnPlayerEscaped();
                        yield break;
                    }
                }

                // じりじりと中心へ少しずつ迫る緊張感
                float scaleMod = Mathf.Lerp(1.0f, 0.9f, elapsed / countdownDuration);
                transform.localScale = Vector3.one * scaleMod;

                yield return null;
            }

            // カウントダウン終了: 収束圧壊（特大ダメージ）
            TriggerCollapse();
        }

        private void OnPlayerEscaped()
        {
            if (isResolved) return;
            isResolved = true;

            // 檻解除: 全三角錐が花弁となって霧散
            ForceStopAndVanish();
        }

        /// <summary>
        /// 時間切れ時の高速収束圧壊
        /// </summary>
        private void TriggerCollapse()
        {
            if (isResolved) return;
            isResolved = true;

            StartCoroutine(CollapseSequenceRoutine());
        }

        private IEnumerator CollapseSequenceRoutine()
        {
            // 一瞬で中心へ全三角錐が収束
            float collapseTime = 0.25f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < collapseTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / collapseTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);
                yield return null;
            }

            // 中心で大輪の椿が咲く圧壊エフェクト + 特大ダメージ
            if (collapseVfxPrefab != null)
            {
                Instantiate(collapseVfxPrefab, cageCenter, Quaternion.identity);
            }

            // プレイヤーがまだ中心付近にいる場合は特大ダメージ
            if (playerTransform != null)
            {
                float dist = Vector2.Distance(playerTransform.position, cageCenter);
                if (dist < layer3Radius)
                {
                    var health = playerTransform.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(collapseDamage);
                    }
                }
            }

            // 後片付け
            foreach (var cone in activeCones)
            {
                if (cone != null) Destroy(cone.gameObject);
            }
            activeCones.Clear();
            Destroy(gameObject, 0.5f);
        }

        public void ForceStopAndVanish()
        {
            if (isResolved) return;
            isResolved = true;
            if (cageRoutine != null) StopCoroutine(cageRoutine);

            foreach (var cone in activeCones)
            {
                if (cone != null)
                {
                    cone.ForceStopAndVanish();
                }
            }
            activeCones.Clear();
            Destroy(gameObject, 0.6f);
        }
    }
}
