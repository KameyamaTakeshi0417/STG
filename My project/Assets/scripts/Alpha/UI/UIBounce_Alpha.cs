using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Alpha.UI
{
    public class UIBounce_Alpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.05f;
        public float clickScale = 0.9f;
        public float duration = 0.1f;

        private Vector3 originalScale;
        private Coroutine scaleCoroutine;
        private bool isInitialized = false;

        private void Awake()
        {
            InitOriginalScale();
        }

        private void OnEnable()
        {
            InitOriginalScale();
            transform.localScale = originalScale;
        }

        private void InitOriginalScale()
        {
            if (isInitialized) return;
            originalScale = transform.localScale;
            // ゼロスケールのまま初期化されるのを防ぐフェイルセーフ
            if (originalScale.sqrMagnitude < 0.01f)
            {
                originalScale = Vector3.one;
            }
            isInitialized = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ScaleTo(originalScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ScaleTo(originalScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ScaleTo(originalScale * clickScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // If mouse is still over it, go back to hover scale, else original
            if (eventData.pointerEnter == gameObject)
            {
                ScaleTo(originalScale * hoverScale);
            }
            else
            {
                ScaleTo(originalScale);
            }
        }

        private void ScaleTo(Vector3 target)
        {
            if (!gameObject.activeInHierarchy) return;
            
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);
            
            scaleCoroutine = StartCoroutine(ScaleRoutine(target));
        }

        private IEnumerator ScaleRoutine(Vector3 target)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(startScale, target, elapsed / duration);
                elapsed += Time.unscaledDeltaTime; // Unscaled so it works during HitStop
                yield return null;
            }

            transform.localScale = target;
        }
    }
}
