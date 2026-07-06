using System;
using System.Collections;
using UnityEngine;

namespace Alpha.Enemy.Effect
{
    public class AetherExplosionEffect_Alpha : MonoBehaviour, IAlphaPoolable
    {
        public event Action OnFinished;
        public GameObject sourcePrefab; // プール返却用

        private SpriteRenderer spriteRenderer;

        private Animator animator;

        [Header("Fade Out Settings")]
        [Tooltip("アニメーション終了後の自然消滅にかける時間")]
        public float fadeDuration = 0.2f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            // 初回生成時も動作させるが、プールからRentされた時はOnRentFromPoolが呼ばれる
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AutoFadeOutRoutine());
            }
        }

        public void OnRentFromPool()
        {
            // プールから呼び出された時の初期化
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 1f); // アルファ値を元に戻す
            }

            if (animator != null)
            {
                // アニメーションを最初から再生し直す（必要に応じて）
                animator.Play(0, -1, 0f);
            }

            // 新しくコルーチンを開始
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AutoFadeOutRoutine());
            }
        }

        public void OnReturnToPool()
        {
            // 返却時のクリーンアップ
            StopAllCoroutines();
            OnFinished = null; // デリゲートのクリア
        }

        // AnimationClip に残っているイベントマーカーのエラーを防ぐためのダミーメソッド
        public void OnAnimationFinished()
        {
            // スクリプト側で自動時間計測（AutoFadeOutRoutine）を行っているため、ここでは何もしません。
            // これにより "has no receiver!" エラーを抑止します。
        }

        private IEnumerator AutoFadeOutRoutine()
        {
            float animLength = 0.5f; // デフォルトの待機時間

            // Animatorから現在のアニメーションの長さを取得する
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    animLength = clips[0].length;
                }
            }

            // アニメーションの長さ分だけリアルタイムで待機（スローの影響を受けない）
            yield return new WaitForSecondsRealtime(animLength);

            // フェードアウト処理
            if (spriteRenderer != null)
            {
                Color startColor = spriteRenderer.color;
                float time = 0f;

                while (time < fadeDuration)
                {
                    time += Time.unscaledDeltaTime; // 演出中はTimeScaleが0.3等になるため、unscaledを使用
                    float alpha = Mathf.Lerp(startColor.a, 0f, time / fadeDuration);
                    spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            OnFinished?.Invoke();

            if (sourcePrefab != null && Alpha_ObjectPoolManager.Instance != null)
            {
                Alpha_ObjectPoolManager.Instance.Return(gameObject, sourcePrefab);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
