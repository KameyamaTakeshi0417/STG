using UnityEngine;
using DG.Tweening;

namespace Alpha.UI
{
    public class UIPopupAnimator_Alpha : MonoBehaviour
    {
        public float duration = 0.35f;
        public Vector3 startScale = new Vector3(0.7f, 0.7f, 1f);
        public Ease easeType = Ease.OutBack;

        private Vector3 originalScale;
        private bool isInitialized = false;
        private Tween currentTween;

        private void Awake()
        {
            InitScale();
        }

        private void InitScale()
        {
            if (isInitialized) return;
            originalScale = transform.localScale;
            if (originalScale.sqrMagnitude < 0.01f)
            {
                originalScale = Vector3.one;
            }
            isInitialized = true;
        }

        private void OnEnable()
        {
            InitScale();
            
            if (currentTween != null && currentTween.IsActive())
            {
                currentTween.Kill();
            }

            transform.localScale = startScale;
            currentTween = transform.DOScale(originalScale, duration).SetEase(easeType).SetUpdate(true);
        }
    }
}
