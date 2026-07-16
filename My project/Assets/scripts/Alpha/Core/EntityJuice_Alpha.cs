using System.Collections;
using UnityEngine;

namespace Alpha.Core
{
    public class EntityJuice_Alpha : MonoBehaviour
    {
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;

        private Coroutine flashCoroutine;
        private Coroutine squashCoroutine;

        private Vector3 originalScale;

        private void Awake()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
            }

            originalScale = transform.localScale;
        }

        /// <summary>
        /// Flashes the sprite renderers with the given color for a brief duration.
        /// </summary>
        public void FlashColor(Color flashColor, float duration = 0.1f)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            
            flashCoroutine = StartCoroutine(FlashRoutine(flashColor, duration));
        }

        private IEnumerator FlashRoutine(Color flashColor, float duration)
        {
            // Apply flash color
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = flashColor;
            }

            yield return new WaitForSeconds(duration);

            // Revert back to original colors
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = originalColors[i];
            }
        }

        /// <summary>
        /// Squashes and stretches the entity by the specified scale multiplier before bouncing back.
        /// e.g. squashMulti = new Vector3(1.2f, 0.8f, 1f) makes it wider and shorter.
        /// </summary>
        public void SquashAndStretch(Vector3 squashMulti, float duration = 0.15f)
        {
            if (squashCoroutine != null)
                StopCoroutine(squashCoroutine);
                
            squashCoroutine = StartCoroutine(SquashRoutine(squashMulti, duration));
        }

        private IEnumerator SquashRoutine(Vector3 squashMulti, float duration)
        {
            Vector3 targetScale = new Vector3(
                originalScale.x * squashMulti.x,
                originalScale.y * squashMulti.y,
                originalScale.z * squashMulti.z
            );

            float halfDuration = duration / 2f;
            float elapsed = 0f;

            // Squash down
            while (elapsed < halfDuration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Stretch back
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}
