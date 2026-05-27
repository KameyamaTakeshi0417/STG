using UnityEngine;
using System.Collections;
using System;

namespace Alpha.UI
{
    public class FadeController_Alpha : MonoBehaviour
    {
        public CanvasGroup fadeGroup;
        public float fadeDuration = 1f;

        private void Start()
        {
            if (fadeGroup == null)
            {
                fadeGroup = GetComponent<CanvasGroup>();
            }
        }

        public void FadeOut(Action onComplete = null)
        {
            StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
        }

        public void FadeIn(Action onComplete = null)
        {
            StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
        }

        private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, Action onComplete)
        {
            fadeGroup.blocksRaycasts = true;
            float time = 0;
            
            while (time < fadeDuration)
            {
                time += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
                yield return null;
            }

            fadeGroup.alpha = endAlpha;
            fadeGroup.blocksRaycasts = (endAlpha > 0f);
            
            onComplete?.Invoke();
        }
    }
}
