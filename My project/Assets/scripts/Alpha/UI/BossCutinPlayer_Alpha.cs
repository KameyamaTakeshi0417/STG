using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    /// <summary>
    /// ボス戦カットイン演出コントローラー（Canvas方式）。
    /// Canvas内にあらかじめ配置されたImageオブジェクトでカットインアニメーションを再生する。
    ///
    /// 【演出フロー】
    /// ADV終了 → BossEntry再生（画面を覆う）
    /// → 最終フレームで停止 + テキスト表示 + 3秒待機
    /// → ★この間に裏でStartBossFight()が呼ばれる★
    /// → EntryIdle再生（フェードアウトしてボス戦が露出）
    /// → 非表示 → ボス行動開始
    ///
    /// 【セットアップ】
    /// 1. Canvas内にImageオブジェクトを作成（名前例: CutinImage）
    /// 2. CutinImageに以下をアタッチ:
    ///    - Animator（コントローラーは空でOK。スクリプトが切り替える）
    ///    - Alpha_SyncSpriteToImage（SpriteRendererは自動追加される）
    /// 3. CutinImageのRectTransformをStretch-Stretchにして画面全体を覆う
    /// 4. このスクリプトを任意オブジェクトにアタッチし、各フィールドを設定
    /// </summary>
    public class BossCutinPlayer_Alpha : MonoBehaviour
    {
        public static BossCutinPlayer_Alpha Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("カットイン表示用のImageオブジェクト（Animator + Alpha_SyncSpriteToImage付き）")]
        public GameObject cutinImageObject;
        
        [Tooltip("「devine Approaching / 神格接近」テキストオブジェクト")]
        public GameObject textObject;

        [Header("Animator Controllers")]
        [Tooltip("登場アニメーション用のAnimatorController")]
        public RuntimeAnimatorController entryController;
        [Tooltip("退場アニメーション用のAnimatorController")]
        public RuntimeAnimatorController idleController;

        [Header("Settings")]
        [Tooltip("テキスト表示後の待機時間（秒）")]
        public float waitTimeBeforeIdle = 3.0f;

        /// <summary>カットイン再生中かどうか。外部からボスAI等が参照する。</summary>
        public static bool IsCutinPlaying { get; private set; }

        private Animator cutinAnimator;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            Instance = this;
            IsCutinPlaying = false;

            if (cutinImageObject != null)
            {
                cutinAnimator = cutinImageObject.GetComponent<Animator>();
                
                // CanvasGroupで表示制御（SetActiveを使わない）
                canvasGroup = cutinImageObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = cutinImageObject.AddComponent<CanvasGroup>();
                }
            }

            // 初期状態は非表示
            SetCutinVisible(false);
            if (textObject != null) textObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            IsCutinPlaying = false;
        }

        private void SetCutinVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }
            // Animatorは常にアクティブにしておく（SetActiveしない）
        }

        /// <summary>
        /// カットイン演出を開始する。
        /// onMidPoint: カットインが画面を覆っている最中に呼ばれる（ここでStartBossFightを呼ぶ）
        /// onComplete: カットイン演出が全て終了した時に呼ばれる
        /// </summary>
        public void PlayCutin(System.Action onMidPoint = null, System.Action onComplete = null)
        {
            Debug.Log("[BossCutin] PlayCutin called");
            StartCoroutine(CutinSequence(onMidPoint, onComplete));
        }

        private IEnumerator CutinSequence(System.Action onMidPoint, System.Action onComplete)
        {
            IsCutinPlaying = true;

            // ===== Phase 1: BossEntry 再生 =====
            SetCutinVisible(true);
            if (cutinAnimator != null && entryController != null)
            {
                cutinAnimator.runtimeAnimatorController = entryController;
                cutinAnimator.speed = 1f;
                cutinAnimator.Play(0, 0, 0f);
                Debug.Log("[BossCutin] Playing entry animation");
            }

            yield return null;
            yield return null;

            // エントリーアニメーション再生完了を待つ
            if (cutinAnimator != null)
            {
                float safetyTimer = 30f;
                while (safetyTimer > 0f)
                {
                    var info = cutinAnimator.GetCurrentAnimatorStateInfo(0);
                    if (info.normalizedTime >= 0.95f)
                        break;
                    safetyTimer -= Time.deltaTime;
                    yield return null;
                }
                cutinAnimator.speed = 0f;
                Debug.Log("[BossCutin] Entry done. Holding last frame.");
            }

            // ===== Phase 2: テキスト表示 + 裏でボス起動 =====
            if (textObject != null) textObject.SetActive(true);

            if (onMidPoint != null)
            {
                Debug.Log("[BossCutin] Calling onMidPoint (StartBossFight)");
                onMidPoint.Invoke();
            }

            yield return new WaitForSeconds(waitTimeBeforeIdle);

            // ===== Phase 3: EntryIdle 再生（退場） =====
            if (textObject != null) textObject.SetActive(false);
            if (cutinAnimator != null && idleController != null)
            {
                cutinAnimator.runtimeAnimatorController = idleController;
                cutinAnimator.speed = 1f;
                cutinAnimator.Play(0, 0, 0f);
                Debug.Log("[BossCutin] Playing idle animation");
            }

            yield return null;
            yield return null;

            if (cutinAnimator != null)
            {
                float safetyTimer = 30f;
                while (safetyTimer > 0f)
                {
                    var info = cutinAnimator.GetCurrentAnimatorStateInfo(0);
                    if (info.normalizedTime >= 0.95f)
                        break;
                    safetyTimer -= Time.deltaTime;
                    yield return null;
                }
            }

            // ===== 完了 =====
            SetCutinVisible(false);
            IsCutinPlaying = false;
            Debug.Log("[BossCutin] Cutin COMPLETE");

            if (onComplete != null) onComplete.Invoke();
        }
    }
}
