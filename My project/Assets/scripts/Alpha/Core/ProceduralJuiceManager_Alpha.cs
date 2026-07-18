using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Core
{
    public class ProceduralJuiceManager_Alpha : MonoBehaviour
    {
        public static ProceduralJuiceManager_Alpha Instance { get; private set; }

        private Material defaultSpriteMaterial;
        private Sprite cachedCircleSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create a default material for procedural sprites
            defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));

            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            // Set PPU to 4 so that the 4x4 texture is exactly 1x1 world units
            cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        // 1a. Hit Sparks
        public void SpawnHitSparks(Vector3 pos, Color color, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(SparkRoutine(pos, color));
            }
        }

        private IEnumerator SparkRoutine(Vector3 pos, Color color)
        {
            GameObject spark = new GameObject("HitSpark");
            spark.transform.position = pos;
            
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial;
            sr.color = color;
            sr.sortingOrder = 100;
            
            // Generate a 1x1 white texture sprite if we don't have one
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for(int i=0; i<16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            // Set PPU to 4 so that base size is 1x1 world units
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

            spark.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);

            Vector3 velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized * Random.Range(3f, 8f);
            float duration = Random.Range(0.2f, 0.4f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                
                spark.transform.position += velocity * Time.unscaledDeltaTime;
                velocity = Vector3.Lerp(velocity, Vector3.zero, elapsed / duration); // Drag
                
                sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(1f, 0f, elapsed / duration));
                
                yield return null;
            }

            Destroy(tex);
            Destroy(spark);
        }

        // 1b. Ripple
        public void SpawnRipple(Vector3 pos, Color color, float startScale = 0.5f, float endScale = 2.0f, float duration = 0.2f)
        {
            StartCoroutine(RippleRoutine(pos, color, startScale, endScale, duration));
        }

        private IEnumerator RippleRoutine(Vector3 pos, Color color, float startScale, float endScale, float duration)
        {
            GameObject ripple = new GameObject("Ripple");
            ripple.transform.position = pos;
            ripple.transform.localScale = Vector3.one * startScale;

            SpriteRenderer sr = ripple.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial;
            sr.color = color;
            sr.sortingOrder = 90;

            // Simple hollow circle by generating a texture (basic approach: full square but transparent center? No, let's just make a thin line circle)
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size/2f, size/2f);
            float radius = size / 2f - 1f;
            float thickness = 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius && dist >= radius - thickness)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                ripple.transform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, t);
                sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(1f, 0f, t));

                yield return null;
            }

            Destroy(tex);
            Destroy(ripple);
        }

        // 1c. Boss Explosion Sequence
        public void SpawnBossExplosionSequence(Vector3 bossCenter, float radius, float duration)
        {
            StartCoroutine(BossExplosionRoutine(bossCenter, radius, duration));
        }

        private IEnumerator BossExplosionRoutine(Vector3 bossCenter, float radius, float duration)
        {
            // Slow motion
            if (JuiceManager_Alpha.Instance != null)
            {
                JuiceManager_Alpha.Instance.SlowMotion(duration, 0.2f);
            }

            float elapsed = 0f;
            float nextExplosionTime = 0f;

            while (elapsed < duration)
            {
                if (elapsed >= nextExplosionTime)
                {
                    Vector3 offset = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0f);
                    SpawnRipple(bossCenter + offset, Color.white, 1f, 4f, 0.3f);
                    SpawnHitSparks(bossCenter + offset, Color.yellow, 10);
                    
                    if (JuiceManager_Alpha.Instance != null)
                    {
                        JuiceManager_Alpha.Instance.ScreenShake(0.1f, 0.3f);
                    }
                    
                    nextExplosionTime = elapsed + Random.Range(0.1f, 0.25f);
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Final Massive Flash
            if (JuiceManager_Alpha.Instance != null)
            {
                JuiceManager_Alpha.Instance.ScreenShake(0.5f, 1.0f);
            }
            SpawnRipple(bossCenter, Color.white, 1f, 15f, 0.5f);
        }

        // 4a. UI Score Particles
        public void SpawnUIParticles(RectTransform targetRect)
        {
            StartCoroutine(UIParticleRoutine(targetRect));
        }

        private IEnumerator UIParticleRoutine(RectTransform parentRect)
        {
            GameObject pObj = new GameObject("UIParticle");
            pObj.transform.SetParent(parentRect.transform, false);
            
            UnityEngine.UI.Image img = pObj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.8f);
            
            RectTransform rt = pObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(8f, 8f);
            rt.anchoredPosition = new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));

            Vector2 velocity = new Vector2(Random.Range(-20f, 20f), Random.Range(50f, 100f));
            float duration = Random.Range(0.5f, 1.0f);
            float elapsed = 0f;

            while(elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                rt.anchoredPosition += velocity * Time.unscaledDeltaTime;
                img.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.8f, 0f, t));
                rt.localEulerAngles += new Vector3(0, 0, 180f * Time.unscaledDeltaTime); // Spin
                
                yield return null;
            }

            Destroy(pObj);
        }

        // Phase 2: Muzzle Flash
        public void SpawnMuzzleFlash(Vector3 position, Vector3 direction)
        {
            StartCoroutine(MuzzleFlashRoutine(position, direction));
        }

        private IEnumerator MuzzleFlashRoutine(Vector3 position, Vector3 direction)
        {
            GameObject flashObj = new GameObject("ProceduralMuzzleFlash");
            flashObj.transform.position = position;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            flashObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            SpriteRenderer sr = flashObj.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial; // Assign default sprite material
            sr.sprite = cachedCircleSprite;
            sr.color = new Color(0.6f, 0.9f, 1f, 1f); // Pale blue
            sr.sortingOrder = 100;

            // startScale is relative to the 1x1 world unit base
            Vector3 startScale = new Vector3(0.5f, 0.2f, 1f);
            flashObj.transform.localScale = startScale;

            float duration = 0.05f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                flashObj.transform.localScale = Vector3.Lerp(startScale, new Vector3(1.5f, 0.05f, 1f), t);
                sr.color = new Color(0.6f, 0.9f, 1f, Mathf.Lerp(1f, 0f, t));
                
                yield return null;
            }

            Destroy(flashObj);
        }

        // Phase 2: Hit Stop & Screen Flash
        public void TriggerPlayerDamageJuice()
        {
            StartCoroutine(HitStopRoutine(0.08f)); // 0.08秒のヒットストップ
            if (JuiceManager_Alpha.Instance != null)
            {
                JuiceManager_Alpha.Instance.ScreenShake(0.3f, 0.4f);
            }
            StartCoroutine(ScreenFlashRoutine(new Color(1f, 0f, 0f, 0.4f), 0.2f));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float originalTimeScale = Time.timeScale;
            if (originalTimeScale <= 0f) yield break; // 既に停止中なら何もしない
            
            Time.timeScale = 0.02f; // 極低速
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalTimeScale;
        }

        private IEnumerator ScreenFlashRoutine(Color flashColor, float duration)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                var uiRoot = GameObject.Find("UI_Root");
                if (uiRoot != null) canvasObj = uiRoot;
                else yield break;
            }

            GameObject flashObj = new GameObject("DamageFlashPanel");
            flashObj.transform.SetParent(canvasObj.transform, false);
            flashObj.transform.SetAsLastSibling(); // 手前に表示
            
            UnityEngine.UI.Image img = flashObj.AddComponent<UnityEngine.UI.Image>();
            img.color = flashColor;
            img.raycastTarget = false;

            RectTransform rt = flashObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            float elapsed = 0f;
            while(elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                img.color = new Color(flashColor.r, flashColor.g, flashColor.b, Mathf.Lerp(flashColor.a, 0f, t));
                yield return null;
            }

            Destroy(flashObj);
        }

        // Phase 2: Text Popup
        public void SpawnTextPopup(Vector3 position, string text, Color color)
        {
            StartCoroutine(TextPopupRoutine(position, text, color));
        }

        private IEnumerator TextPopupRoutine(Vector3 position, string text, Color color)
        {
            GameObject txtObj = new GameObject("ProceduralTextPopup");
            txtObj.transform.position = position;

            var tmp = txtObj.AddComponent<TMPro.TextMeshPro>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = 3f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.sortingOrder = 200;

            float duration = 0.8f;
            float elapsed = 0f;
            Vector3 startPos = position;
            Vector3 endPos = position + new Vector3(0, 1.0f, 0);

            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // イーズアウト (上に上がりながらゆっくりになる)
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                txtObj.transform.position = Vector3.Lerp(startPos, endPos, easeT);
                tmp.color = new Color(color.r, color.g, color.b, Mathf.Lerp(1f, 0f, easeT));
                
                yield return null;
            }

            Destroy(txtObj);
        }
    }
}
