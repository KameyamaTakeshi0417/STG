using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Alpha.UI
{
    public class BossPhaseSequenceBarUI_Alpha : MonoBehaviour
    {
        public static BossPhaseSequenceBarUI_Alpha Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("制限時間の進行率を表示するスライダー（0→1で満たされる）")]
        public Slider progressSlider;
        [Tooltip("残り時間を表示するテキスト")]
        public TextMeshProUGUI timeText;

        [Header("Animation Settings")]
        [Tooltip("テキストの基本スケール")]
        public float baseScale = 1.0f;
        [Tooltip("テキストが跳ねるときのスケール加算値")]
        public float bounceScaleAdd = 0.1f;
        [Tooltip("テキストの跳ねる時間")]
        public float bounceDuration = 0.3f;

        private int lastDisplayedSecond = -1;
        private Tweener textBounceTween;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            gameObject.SetActive(false); // 初期状態は非表示
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (progressSlider != null)
            {
                progressSlider.value = 0f;
            }
            lastDisplayedSecond = -1;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// フェーズ中の毎フレームの更新
        /// </summary>
        public void UpdateProgress(float elapsed, float timeLimit)
        {
            if (timeLimit <= 0) return;

            // スライダーは 0 -> 1 に進む
            float progress = Mathf.Clamp01(elapsed / timeLimit);
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            // 残り時間の計算
            float remaining = Mathf.Max(0f, timeLimit - elapsed);
            int currentSecond = Mathf.CeilToInt(remaining);

            if (timeText != null)
            {
                // テキスト更新（2桁まで）
                timeText.text = $"残り{currentSecond:D2}秒！";

                // 色の更新（10秒以下なら黄色→赤へ遷移）
                if (remaining <= 10f)
                {
                    float colorT = 1f - (remaining / 10f); // 10秒=0, 0秒=1
                    timeText.color = Color.Lerp(Color.yellow, Color.red, colorT);
                }
                else
                {
                    timeText.color = Color.white;
                }

                // 1秒ごとにブルンとアニメーション
                if (currentSecond != lastDisplayedSecond && currentSecond > 0)
                {
                    lastDisplayedSecond = currentSecond;

                    // 既存のTweenがあればキルして新しく再生
                    textBounceTween?.Kill();
                    timeText.transform.localScale = Vector3.one * baseScale;
                    
                    textBounceTween = timeText.transform.DOScale(baseScale + bounceScaleAdd, bounceDuration / 2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            textBounceTween = timeText.transform.DOScale(baseScale, bounceDuration / 2f)
                                .SetEase(Ease.InQuad);
                        });
                }
            }
        }

        /// <summary>
        /// フェーズがHPブレイクで早期終了した際、現在の割合から0へ1秒かけて戻す
        /// </summary>
        public void DrainToZero()
        {
            if (progressSlider != null)
            {
                float currentVal = progressSlider.value;
                progressSlider.DOValue(0f, 1.0f).SetEase(Ease.Linear);
            }
            
            if (timeText != null)
            {
                timeText.text = "";
            }
        }
    }
}
