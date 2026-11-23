using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    /// <summary>
    /// 三女・チコリの空間設置ギミック「花時計地雷 (Clock Mine)」
    /// 予兆(時計盤拡大・12時予告) -> 針出現＆反時計回りにギュンと一回転(判定あり) -> 消滅
    /// </summary>
    public class ChicoryClockMine : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer dialRenderer;
        [SerializeField] private Transform handTransform;
        [SerializeField] private SpriteRenderer handRenderer;
        [SerializeField] private Collider2D handCollider;
        [SerializeField] private TrailRenderer handTrail;
        [SerializeField] private ParticleSystem vanishVfx;

        [Header("Timeline Settings")]
        [Tooltip("予兆（時計盤展開）の時間（秒）")]
        [SerializeField] private float telegraphDuration = 1.1f;
        [Tooltip("針の一回転にかける時間（秒）")]
        [SerializeField] private float spinDuration = 0.55f;
        [Tooltip("攻撃ダメージ")]
        [SerializeField] private float damage = 12f;

        private bool hasHitPlayer = false;
        private bool isForceStopped = false;

        private void Awake()
        {
            if (handCollider != null) handCollider.enabled = false;
            if (handRenderer != null) handRenderer.enabled = false;
            if (handTrail != null) handTrail.emitting = false;
        }

        public void ActivateMine(float delay = 0f)
        {
            StartCoroutine(MineRoutine(delay));
        }

        private IEnumerator MineRoutine(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (isForceStopped) yield break;

            // 1. 予兆：時計盤がフワッと拡大出現
            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = Vector3.one * 1.5f;

            if (dialRenderer != null)
            {
                Color dialColor = dialRenderer.color;
                dialColor.a = 0f;
                dialRenderer.color = dialColor;
                dialRenderer.transform.localScale = startScale;

                while (elapsed < telegraphDuration)
                {
                    if (isForceStopped) yield break;
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / telegraphDuration);
                    // スムーズなイージング展開
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);
                    dialRenderer.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
                    dialColor.a = Mathf.Lerp(0f, 0.85f, smoothT);
                    dialRenderer.color = dialColor;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(telegraphDuration);
            }

            if (isForceStopped) yield break;

            // 2. 針出現＆反時計回りにギュンと高速一回転（時間を遡るアーカイブの伏線）
            if (handTransform != null)
            {
                handTransform.localRotation = Quaternion.Euler(0, 0, 0); // 12時方向
                if (handRenderer != null) handRenderer.enabled = true;
                if (handCollider != null) handCollider.enabled = true;
                if (handTrail != null)
                {
                    handTrail.Clear();
                    handTrail.emitting = true;
                }

                // 軽い発動SE演出（あれば）
                if (Alpha.Audio.EnemyShotSoundManager_Alpha.Instance != null)
                {
                    Alpha.Audio.EnemyShotSoundManager_Alpha.Instance.PlaySingleBulletSE(transform.position, false);
                }

                float spinElapsed = 0f;
                float startZ = 0f;
                float totalSpinAngle = 360f; // 反時計回り（+Z方向）

                while (spinElapsed < spinDuration)
                {
                    if (isForceStopped) yield break;
                    spinElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(spinElapsed / spinDuration);
                    // ギュン！と初速が速いイージング
                    float angle = Mathf.Lerp(startZ, startZ + totalSpinAngle, Mathf.Pow(t, 0.85f));
                    handTransform.localRotation = Quaternion.Euler(0, 0, angle);
                    yield return null;
                }
            }

            // 判定終了
            if (handCollider != null) handCollider.enabled = false;
            if (handTrail != null) handTrail.emitting = false;

            // 3. 消滅演出（青い花弁・光粒の散布）
            if (vanishVfx != null)
            {
                vanishVfx.Play();
            }

            // フェードアウト
            elapsed = 0f;
            float fadeDuration = 0.25f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.85f, 0f, elapsed / fadeDuration);
                if (dialRenderer != null)
                {
                    Color c = dialRenderer.color;
                    c.a = alpha;
                    dialRenderer.color = c;
                }
                if (handRenderer != null)
                {
                    Color c = handRenderer.color;
                    c.a = alpha;
                    handRenderer.color = c;
                }
                yield return null;
            }

            Destroy(gameObject, 0.5f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasHitPlayer || isForceStopped) return;

            if (collision.CompareTag("Player"))
            {
                hasHitPlayer = true;
                Health playerHealth = collision.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        /// <summary>
        /// ボス決着演出時などに外部から一斉停止・消滅させる
        /// </summary>
        public void ForceStopAndVanish()
        {
            isForceStopped = true;
            StopAllCoroutines();
            if (handCollider != null) handCollider.enabled = false;
            if (handTrail != null) handTrail.emitting = false;
            if (vanishVfx != null) vanishVfx.Play();
            Destroy(gameObject, 0.3f);
        }
    }
}
