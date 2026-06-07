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
            if (fadeGroup.alpha >= 1f)
            {
                fadeGroup.alpha = 1f;
                fadeGroup.blocksRaycasts = true;
                onComplete?.Invoke();
            }
            else
            {
                StartCoroutine(FadeCoroutine(fadeGroup.alpha, 1f, onComplete));
            }
        }

        public void FadeIn(Action onComplete = null)
        {
            if (fadeGroup.alpha <= 0f)
            {
                fadeGroup.alpha = 0f;
                fadeGroup.blocksRaycasts = false;
                onComplete?.Invoke();
            }
            else
            {
                StartCoroutine(FadeCoroutine(fadeGroup.alpha, 0f, onComplete));
            }
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
