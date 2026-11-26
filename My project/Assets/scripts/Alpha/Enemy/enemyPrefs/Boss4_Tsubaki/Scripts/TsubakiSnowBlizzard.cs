using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 3: 『雪中寒椿・冬吹雪』システム
    /// 画面全体に当たり判定のない粉雪を舞わせ、画面外から大量の椿花弁弾を吹雪のように流し込む。
    /// 一定間隔（4〜5秒ごと）で風向きが変化し、直前（1秒前）に風向き予兆ライン（WindShiftVFX）で予告する。
    /// </summary>
    public class TsubakiSnowBlizzard : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject petalBulletPrefab;
        [SerializeField] private GameObject windShiftVfxPrefab;
        [SerializeField] private GameObject snowBgObj;

        [Header("Blizzard Parameters")]
        [SerializeField] private float windChangeInterval = 4.5f;
        [SerializeField] private float spawnRate = 0.2f; // 花弁弾の生成間隔（秒）
        [SerializeField] private float petalSpeed = 5f;
        [SerializeField] private float petalDamage = 12f;

        [Header("Field Bounds")]
        [SerializeField] private Vector2 xBounds = new Vector2(-7.5f, 7.5f);
        [SerializeField] private Vector2 yBounds = new Vector2(-4.5f, 4.5f);

        private Vector2 currentWindDirection = new Vector2(-0.5f, -1f).normalized;
        private bool isBlizzardActive = false;
        private bool isGlitchStallActive = false; // HP終盤の傷による吹雪一時停止
        private Coroutine blizzardRoutine;
        private Coroutine windCycleRoutine;

        public Vector2 CurrentWindDirection => currentWindDirection;

        public void StartBlizzard()
        {
            if (isBlizzardActive) return;
            isBlizzardActive = true;
            isGlitchStallActive = false;

            if (snowBgObj != null) snowBgObj.SetActive(true);

            blizzardRoutine = StartCoroutine(BlizzardSpawnRoutine());
            windCycleRoutine = StartCoroutine(WindCycleRoutine());
        }

        public void StopBlizzard()
        {
            isBlizzardActive = false;
            if (blizzardRoutine != null) StopCoroutine(blizzardRoutine);
            if (windCycleRoutine != null) StopCoroutine(windCycleRoutine);
            if (snowBgObj != null) snowBgObj.SetActive(false);
        }

        /// <summary>
        /// HP終盤（20〜30%以下）でのストーリー演出:
        /// 傷の限界で吹雪が一瞬（約2秒）フッと途切れ、ツバキに大きな隙が生じる
        /// </summary>
        public void TriggerExhaustionStall(float stallDuration = 2.0f)
        {
            StartCoroutine(ExhaustionStallRoutine(stallDuration));
        }

        private IEnumerator ExhaustionStallRoutine(float duration)
        {
            isGlitchStallActive = true;
            yield return new WaitForSeconds(duration);
            isGlitchStallActive = false;
        }

        private IEnumerator BlizzardSpawnRoutine()
        {
            while (isBlizzardActive)
            {
                if (!isGlitchStallActive)
                {
                    SpawnPetalFromEdge();
                }
                yield return new WaitForSeconds(spawnRate);
            }
        }

        private void SpawnPetalFromEdge()
        {
            if (petalBulletPrefab == null) return;

            // 現在の風上（風と逆方向）の画面端からスポーン
            Vector3 spawnPos = Vector3.zero;
            if (currentWindDirection.y < 0)
            {
                // 上から流れてくる
                spawnPos = new Vector3(Random.Range(xBounds.x, xBounds.y), yBounds.y + 0.8f, 0);
            }
            else
            {
                // 下から流れてくる
                spawnPos = new Vector3(Random.Range(xBounds.x, xBounds.y), yBounds.x - 0.8f, 0);
            }

            // 風の横成分に応じて横端からもスポーン
            if (Mathf.Abs(currentWindDirection.x) > 0.3f && Random.value > 0.5f)
            {
                float x = currentWindDirection.x > 0 ? xBounds.x - 0.8f : xBounds.y + 0.8f;
                spawnPos = new Vector3(x, Random.Range(yBounds.x, yBounds.y), 0);
            }

            GameObject petalObj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                petalObj = Alpha_ObjectPoolManager.Instance.Rent(petalBulletPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                petalObj = Instantiate(petalBulletPrefab, spawnPos, Quaternion.identity);
            }

            if (petalObj != null)
            {
                var petal = petalObj.GetComponent<TsubakiPetalBullet>();
                if (petal != null)
                {
                    petal.Initialize(currentWindDirection, petalSpeed, petalDamage);
                }
            }
        }

        private IEnumerator WindCycleRoutine()
        {
            // 風向きのプリセット（斜め左下、斜め右下、真下、左など）
            Vector2[] windPresets = new Vector2[]
            {
                new Vector2(-0.7f, -1.0f).normalized,
                new Vector2(0.7f, -1.0f).normalized,
                new Vector2(0f, -1.0f).normalized,
                new Vector2(-1.0f, -0.4f).normalized,
                new Vector2(1.0f, -0.4f).normalized
            };

            int index = 0;
            while (isBlizzardActive)
            {
                yield return new WaitForSeconds(windChangeInterval - 1.0f);

                if (!isBlizzardActive) yield break;

                // 次の風向きを選択
                index = (index + 1) % windPresets.Length;
                Vector2 nextWind = windPresets[index];

                // 1秒前: 風向き予兆ライン（WindShiftVFX）を走らせる
                ShowWindShiftWarning(nextWind);

                yield return new WaitForSeconds(1.0f);

                // 風向きを切り替え
                currentWindDirection = nextWind;

                // 既存の画面内花弁弾の風向きも徐々に更新
                UpdateActivePetalsWind(nextWind);
            }
        }

        private void ShowWindShiftWarning(Vector2 nextWind)
        {
            if (windShiftVfxPrefab != null)
            {
                // 画面中央付近に風の流れる方向を予兆するストリークを生成
                GameObject vfx = Instantiate(windShiftVfxPrefab, Vector3.zero, Quaternion.identity);
                float angle = Mathf.Atan2(nextWind.y, nextWind.x) * Mathf.Rad2Deg;
                vfx.transform.rotation = Quaternion.Euler(0, 0, angle);
                Destroy(vfx, 1.2f);
            }
        }

        private void UpdateActivePetalsWind(Vector2 newWind)
        {
            TsubakiPetalBullet[] petals = FindObjectsOfType<TsubakiPetalBullet>();
            foreach (var petal in petals)
            {
                if (petal != null) petal.UpdateWind(newWind);
            }
        }

        /// <summary>
        /// 外部（決着時など）からの安全消滅
        /// </summary>
        public void ForceStopAndVanish()
        {
            StopBlizzard();
            TsubakiPetalBullet[] petals = FindObjectsOfType<TsubakiPetalBullet>();
            foreach (var petal in petals)
            {
                if (petal != null) petal.ForceStopAndVanish();
            }
        }
    }
}
