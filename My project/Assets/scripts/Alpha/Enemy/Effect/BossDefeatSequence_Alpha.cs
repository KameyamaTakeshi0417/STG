using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.Effect
{
    public class BossDefeatSequence_Alpha : MonoBehaviour
    {
        [Header("Prefabs & References")]
        [Tooltip("爆発演出のプレハブ。AetherExplosionEffect_Alphaがアタッチされていること")]
        public GameObject aetherExplosionPrefab;
        public string bossId; // 撃破報酬などで使用するボスID
        public Sprite defeatSprite;
        public SpriteRenderer targetSpriteRenderer;

        [Header("Explosion Settings")]
        public float explosionMinInterval = 0.15f;
        public float explosionMaxInterval = 0.35f;
        public int maxExplosionCount = 3;

        public Vector2 explosionOffsetRange = new Vector2(1.0f, 0.8f);
        public Vector2 explosionScaleRange = new Vector2(0.8f, 1.2f);

        [Header("Time Settings")]
        public float slowTimeScale = 0.3f;
        public float slowDuration = 0.8f;
        public float clearTextDelay = 1.0f;

        private bool isDefeatSequence = false;
        private bool canSpawnExplosion = false;
        private int activeExplosionCount = 0;

        /// <summary>
        /// ボスのHPが0になった際に外部（Alpha_EliteHealth等）から呼び出します
        /// </summary>
        public void StartDefeatSequence(string id, Sprite sprite, SpriteRenderer renderer, GameObject explosionPrefab)
        {
            if (isDefeatSequence) return;

            isDefeatSequence = true;

            this.bossId = id;
            this.defeatSprite = sprite;
            this.targetSpriteRenderer = renderer;
            
            if (explosionPrefab != null)
            {
                this.aetherExplosionPrefab = explosionPrefab;
            }
            else
            {
                // フォールバック（設定されていなければResourcesから読み込みを試みる）
                this.aetherExplosionPrefab = Resources.Load<GameObject>("Objects/Effect/Effect_AetherExplosion");
            }

            StopBossBehavior();

            StartCoroutine(DefeatSequenceCoroutine());
            StartCoroutine(AetherExplosionCoroutine());
        }

        private IEnumerator DefeatSequenceCoroutine()
        {
            // スロー演出開始
            Time.timeScale = slowTimeScale;
            canSpawnExplosion = true;

            // やられスプライト表示までの待機（リアルタイム）
            yield return new WaitForSecondsRealtime(slowDuration);

            ShowDefeatSprite();

            // クリアテキスト表示までの待機（リアルタイム）
            yield return new WaitForSecondsRealtime(clearTextDelay);

            ShowClearText();

            // 新規の爆発生成を停止（すでに再生中のものは自然消滅に任せる）
            canSpawnExplosion = false;

            // スロー演出終了
            Time.timeScale = 1f;
        }

        private IEnumerator AetherExplosionCoroutine()
        {
            while (canSpawnExplosion)
            {
                if (activeExplosionCount < maxExplosionCount && aetherExplosionPrefab != null)
                {
                    SpawnAetherExplosion();
                }

                float wait = Random.Range(explosionMinInterval, explosionMaxInterval);
                yield return new WaitForSecondsRealtime(wait);
            }
        }

        private void SpawnAetherExplosion()
        {
            Vector3 basePos = transform.position;

            Vector3 offset = new Vector3(
                Random.Range(-explosionOffsetRange.x, explosionOffsetRange.x),
                Random.Range(-explosionOffsetRange.y, explosionOffsetRange.y),
                0f
            );

            GameObject effect = null;
            if (global::Alpha_ObjectPoolManager.Instance != null)
            {
                effect = global::Alpha_ObjectPoolManager.Instance.Rent(aetherExplosionPrefab, basePos + offset, Quaternion.identity);
            }
            else
            {
                effect = Instantiate(aetherExplosionPrefab, basePos + offset, Quaternion.identity);
            }

            float scale = Random.Range(explosionScaleRange.x, explosionScaleRange.y);
            effect.transform.localScale = Vector3.one * scale;

            // SortingOrder をボスより手前にする（必要に応じて）
            SpriteRenderer effectSr = effect.GetComponent<SpriteRenderer>();
            if (effectSr != null && targetSpriteRenderer != null)
            {
                effectSr.sortingOrder = targetSpriteRenderer.sortingOrder + 10;
            }

            activeExplosionCount++;

            AetherExplosionEffect_Alpha explosionScript = effect.GetComponent<AetherExplosionEffect_Alpha>();
            if (explosionScript != null)
            {
                explosionScript.sourcePrefab = aetherExplosionPrefab;
                explosionScript.OnFinished += () =>
                {
                    activeExplosionCount--;
                };
            }
            else
            {
                // スクリプトがアタッチされていない場合のフェイルセーフ
                Destroy(effect, 2f);
                activeExplosionCount--;
            }
        }

        private void StopBossBehavior()
        {
            // AI・攻撃・移動の停止
            var ai = GetComponent<global::Alpha_EliteEnemyAI>();
            if (ai != null)
            {
                ai.StopAllBehaviors();
            }

            // 被弾判定の停止（弾に当たらないように）
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 画面上の敵弾を削除
            if (Alpha.Flow.StageManager_Alpha.Instance != null)
            {
                Alpha.Flow.StageManager_Alpha.Instance.ClearAllEnemyBullets();
            }

            // HPバー等のUIを非表示
            var hpBar = GetComponentInChildren<Alpha.UI.Alpha_EliteCircleHPBar>();
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
        }

        private void ShowDefeatSprite()
        {
            if (defeatSprite != null)
            {
                var sr = targetSpriteRenderer != null ? targetSpriteRenderer : GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.sprite = defeatSprite;
            }
        }

        private void ShowClearText()
        {
            // ボス報酬のドロップ
            if (Alpha.Flow.RewardManager_Alpha.Instance != null)
            {
                Alpha.Flow.RewardManager_Alpha.Instance.DropBossReward(transform.position, bossId);
            }

            // クリア進行
            if (Alpha.Flow.StageManager_Alpha.Instance != null)
            {
                Alpha.Flow.StageManager_Alpha.Instance.OnBossDefeated();
            }

            Debug.Log($"[{gameObject.name}] Boss defeat sequence finished. Proceeding to stage clear.");
        }
    }
}
