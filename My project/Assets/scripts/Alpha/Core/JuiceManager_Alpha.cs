using System.Collections;
using UnityEngine;

namespace Alpha.Core
{
    public class JuiceManager_Alpha : MonoBehaviour
    {
        public static JuiceManager_Alpha Instance { get; private set; }

        private float originalTimeScale = 1f;
        private Coroutine hitStopCoroutine;
        private Coroutine screenShakeCoroutine;
        private Coroutine slowMoCoroutine;

        private CameraControl cameraControl;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // In case we persist across scenes
            // DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                cameraControl = Camera.main.GetComponent<CameraControl>();
            }
        }

        /// <summary>
        /// Briefly pause the game to emphasize a hit.
        /// </summary>
        public void HitStop(float duration, float targetTimeScale = 0.0f)
        {
            if (hitStopCoroutine != null)
                StopCoroutine(hitStopCoroutine);
            
            hitStopCoroutine = StartCoroutine(HitStopRoutine(duration, targetTimeScale));
        }

        private IEnumerator HitStopRoutine(float duration, float targetTimeScale)
        {
            // If there's an ongoing slow-mo, don't mess with originalTimeScale
            float currentScale = Time.timeScale;
            if (currentScale > 0.01f && currentScale <= 1f)
            {
                Time.timeScale = targetTimeScale;
                yield return new WaitForSecondsRealtime(duration);
                Time.timeScale = originalTimeScale;
            }
            else
            {
                // If we are already paused for menus etc, don't do anything
                yield break;
            }
        }

        /// <summary>
        /// Prolonged slow motion for dramatic effects (like boss deaths).
        /// </summary>
        public void SlowMotion(float duration, float targetScale = 0.2f)
        {
            if (slowMoCoroutine != null)
                StopCoroutine(slowMoCoroutine);

            slowMoCoroutine = StartCoroutine(SlowMotionRoutine(duration, targetScale));
        }

        private IEnumerator SlowMotionRoutine(float duration, float targetScale)
        {
            originalTimeScale = targetScale;
            Time.timeScale = targetScale;
            
            yield return new WaitForSecondsRealtime(duration);
            
            originalTimeScale = 1f;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Shake the screen using Perlin Noise.
        /// </summary>
        public void ScreenShake(float duration, float magnitude)
        {
            if (cameraControl == null)
            {
                if (Camera.main != null)
                    cameraControl = Camera.main.GetComponent<CameraControl>();
                
                if (cameraControl == null) return;
            }

            if (screenShakeCoroutine != null)
                StopCoroutine(screenShakeCoroutine);

            screenShakeCoroutine = StartCoroutine(ScreenShakeRoutine(duration, magnitude));
        }

        private IEnumerator ScreenShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            // Generate a random seed for Perlin noise so every shake is different
            float seedX = Random.Range(0f, 100f);
            float seedY = Random.Range(0f, 100f);

            while (elapsed < duration)
            {
                // Smoothly decay magnitude over time
                float currentMag = Mathf.Lerp(magnitude, 0f, elapsed / duration);
                
                // Fast changing coordinates for Perlin noise
                float xOffset = (Mathf.PerlinNoise(seedX + Time.unscaledTime * 50f, 0f) - 0.5f) * 2f * currentMag;
                float yOffset = (Mathf.PerlinNoise(0f, seedY + Time.unscaledTime * 50f) - 0.5f) * 2f * currentMag;

                cameraControl.shakeOffset = new Vector3(xOffset, yOffset, 0f);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            cameraControl.shakeOffset = Vector3.zero;
        }
    }
}
