using System.Collections;
using UnityEngine;
using TMPro;
using Alpha.Core;

namespace Alpha.UI
{
    public class AutoSaveIndicatorUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI indicatorText;

        [Header("Display Settings")]
        public float displayDuration = 1.8f;
        public float fadeDuration = 0.4f;

        private Coroutine currentRoutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void OnEnable()
        {
            SaveManager_Alpha.OnAutoSaveNotification += ShowIndicator;
        }

        private void OnDisable()
        {
            SaveManager_Alpha.OnAutoSaveNotification -= ShowIndicator;
        }

        public void ShowIndicator(string message)
        {
            if (canvasGroup == null) return;

            if (indicatorText != null && !string.IsNullOrEmpty(message))
            {
                indicatorText.text = message;
            }

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            currentRoutine = StartCoroutine(DisplayRoutine());
        }

        private IEnumerator DisplayRoutine()
        {
            // フェードイン
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // 表示維持
            yield return new WaitForSecondsRealtime(displayDuration);

            // フェードアウト
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            currentRoutine = null;
        }
    }
}
