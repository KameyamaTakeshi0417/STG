using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Core
{
    public class ProceduralJuiceManager_Alpha : MonoBehaviour
    {
        private static ProceduralJuiceManager_Alpha _instance;
        public static ProceduralJuiceManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ProceduralJuiceManager_Alpha>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("ProceduralJuiceManager_Alpha");
                        _instance = go.AddComponent<ProceduralJuiceManager_Alpha>();
                    }
                }
                return _instance;
            }
        }

        private Material defaultSpriteMaterial;
        private Sprite cachedCircleSprite;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

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

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
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
            // Time.timeScale の制御は呼び出し元（BossDefeatSequence_Alpha / PlayBossDefeatCinematic）に委譲
            float elapsed = 0f;
            float nextExplosionTime = 0f;

            while (elapsed < duration)
            {
                if (elapsed >= nextExplosionTime)
                {
                    Vector3 offset = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0f);
                    SpawnRipple(bossCenter + offset, Color.white, 1f, 4f, 0.3f);
                    SpawnHitSparks(bossCenter + offset, Color.yellow, 10);
                    PlayBossSubBlastSE();
                    
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
            // ポーズ中(0f)なら何もしない
            if (Time.timeScale <= 0f) yield break;
            
            Time.timeScale = 0.02f; // 極低速
            yield return new WaitForSecondsRealtime(duration);

            // 終了時、もしポーズ中でなければ強制的に1fに戻す
            // これにより、複数ヒットが重なって0.02fのまま固まるバグを防ぎます
            if (Time.timeScale > 0f) 
            {
                Time.timeScale = 1f;
            }
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

        // =====================================================================
        // Boss / MidBoss Defeat Cinematic Suite (全要素入り撃破スロー演出)
        // =====================================================================
        private Sprite cachedSmoothRingSprite;
        private Sprite cachedPetalSprite;
        private Sprite cachedVignetteSprite;
        private Canvas cinematicOverlayCanvas;
        private UnityEngine.UI.Image cinematicFlashImage;
        private UnityEngine.UI.Image cinematicVignetteImage;

        private AudioClip cachedMidBossDefeatSE;
        private AudioClip cachedBigBossDefeatSE;
        private AudioClip cachedSubBlastSE;

        private Coroutine activeCinematicTimeCoroutine;
        private Coroutine activeCinematicZoomCoroutine;
        private Coroutine activeCinematicOverlayCoroutine;

        private void EnsureCinematicAssets()
        {
            // 1. 高精細リングスプライト (128x128)
            if (cachedSmoothRingSprite == null)
            {
                const int sz = 128;
                Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                Color[] px = new Color[sz * sz];
                Vector2 c = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.5f);
                float maxR = sz * 0.47f;
                float ringWidth = 5.5f;

                for (int y = 0; y < sz; y++)
                {
                    for (int x = 0; x < sz; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), c);
                        float distFromRing = Mathf.Abs(d - (maxR - ringWidth * 0.5f));
                        float a = Mathf.Clamp01(1f - distFromRing / (ringWidth * 0.65f));
                        // 内側に淡いグロー尾を引く
                        if (d < maxR - ringWidth * 0.5f)
                        {
                            float innerGlow = Mathf.Clamp01(1f - ((maxR - ringWidth * 0.5f) - d) / (sz * 0.14f)) * 0.35f;
                            a = Mathf.Max(a, innerGlow);
                        }
                        px[y * sz + x] = new Color(1f, 1f, 1f, a * a);
                    }
                }
                tex.SetPixels(px);
                tex.Apply();
                cachedSmoothRingSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
            }

            // 2. 花弁シルエットスプライト (32x64)
            if (cachedPetalSprite == null)
            {
                const int w = 32;
                const int h = 64;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                Color[] px = new Color[w * h];

                for (int y = 0; y < h; y++)
                {
                    float ny = (y / (float)(h - 1)) * 2f - 1f; // -1 (底) .. +1 (先端)
                    // 涙型／花弁カーブ：中央やや上が広く、上下が尖る
                    float halfWidth = Mathf.Pow(Mathf.Clamp01(1f - ny * ny), 0.75f) * (1f - 0.18f * ny);
                    for (int x = 0; x < w; x++)
                    {
                        float nx = Mathf.Abs((x / (float)(w - 1)) * 2f - 1f);
                        float edge = halfWidth - nx;
                        float a = Mathf.Clamp01(edge * 6.5f);
                        // わずかに先端に切れ込み（桜花弁風）
                        if (ny > 0.72f && nx < (ny - 0.72f) * 1.1f)
                        {
                            a *= 0.15f;
                        }
                        px[y * w + x] = new Color(1f, 1f, 1f, a);
                    }
                }
                tex.SetPixels(px);
                tex.Apply();
                cachedPetalSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 48f);
            }

            // 3. 放射状シネマティックビネット (128x128)
            if (cachedVignetteSprite == null)
            {
                const int sz = 128;
                Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                Color[] px = new Color[sz * sz];
                for (int y = 0; y < sz; y++)
                {
                    float ny = (y / (float)(sz - 1)) * 2f - 1f;
                    for (int x = 0; x < sz; x++)
                    {
                        float nx = (x / (float)(sz - 1)) * 2f - 1f;
                        float r = Mathf.Sqrt(nx * nx + ny * ny);
                        float a = Mathf.SmoothStep(0.45f, 1.32f, r);
                        px[y * sz + x] = new Color(0.02f, 0.03f, 0.06f, a);
                    }
                }
                tex.SetPixels(px);
                tex.Apply();
                cachedVignetteSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
            }

            // 4. 専用シネマティックオーバーレイCanvas
            if (cinematicOverlayCanvas == null)
            {
                GameObject cObj = new GameObject("BossDefeatCinematicCanvas");
                cObj.transform.SetParent(transform, false);
                cinematicOverlayCanvas = cObj.AddComponent<Canvas>();
                cinematicOverlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                cinematicOverlayCanvas.sortingOrder = 950;

                // ビネットImage
                GameObject vigObj = new GameObject("CinematicVignette");
                vigObj.transform.SetParent(cObj.transform, false);
                cinematicVignetteImage = vigObj.AddComponent<UnityEngine.UI.Image>();
                cinematicVignetteImage.sprite = cachedVignetteSprite;
                cinematicVignetteImage.color = new Color(1f, 1f, 1f, 0f);
                cinematicVignetteImage.raycastTarget = false;
                RectTransform vrt = vigObj.GetComponent<RectTransform>();
                vrt.anchorMin = Vector2.zero;
                vrt.anchorMax = Vector2.one;
                vrt.offsetMin = Vector2.zero;
                vrt.offsetMax = Vector2.zero;

                // 白フラッシュImage
                GameObject flObj = new GameObject("CinematicFlash");
                flObj.transform.SetParent(cObj.transform, false);
                cinematicFlashImage = flObj.AddComponent<UnityEngine.UI.Image>();
                cinematicFlashImage.color = new Color(1f, 1f, 1f, 0f);
                cinematicFlashImage.raycastTarget = false;
                RectTransform frt = flObj.GetComponent<RectTransform>();
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = Vector2.one;
                frt.offsetMin = Vector2.zero;
                frt.offsetMax = Vector2.zero;
            }
        }

        public void PlayBossDefeatSE(bool isBigBoss)
        {
            if (Alpha.Audio.SoundManager_Alpha.Instance == null) return;

            if (isBigBoss)
            {
                if (cachedBigBossDefeatSE == null)
                    cachedBigBossDefeatSE = Resources.Load<AudioClip>("SE/SE_BossDefeat_Finish");
                if (cachedBigBossDefeatSE != null)
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(cachedBigBossDefeatSE, 0.95f, 1.0f);
            }
            else
            {
                if (cachedMidBossDefeatSE == null)
                    cachedMidBossDefeatSE = Resources.Load<AudioClip>("SE/SE_BossDefeat_MidBoss");
                if (cachedMidBossDefeatSE != null)
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(cachedMidBossDefeatSE, 0.90f, 1.0f);
            }
        }

        public void PlayBossSubBlastSE()
        {
            if (Alpha.Audio.SoundManager_Alpha.Instance == null) return;
            if (cachedSubBlastSE == null)
                cachedSubBlastSE = Resources.Load<AudioClip>("SE/SE_BossDefeat_SubBlast");
            if (cachedSubBlastSE != null)
            {
                float pitch = Random.Range(0.88f, 1.14f);
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(cachedSubBlastSE, 0.65f, pitch);
            }
        }

        /// <summary>
        /// （中）ボス撃破時のフル演出（SE・ヒットストップ・深いスロー・白フラッシュ・ビネット・カメラズーム＆シェイク・2重衝撃波・花弁＆光粒バースト）
        /// </summary>
        public void PlayBossDefeatCinematic(Vector3 bossPos, bool isBigBoss, float slowDuration = 2.2f, bool manageTimeScale = true)
        {
            EnsureCinematicAssets();

            // 1. 専用撃破SE（ベースドロップ＋結晶残響）
            PlayBossDefeatSE(isBigBoss);

            // 2. カメラ強めシェイク
            if (JuiceManager_Alpha.Instance != null)
            {
                JuiceManager_Alpha.Instance.ScreenShake(isBigBoss ? 0.55f : 0.42f, isBigBoss ? 0.85f : 0.52f);
            }

            // 3. ヒットストップ → 深いスロー → 滑らか復帰
            if (manageTimeScale)
            {
                if (activeCinematicTimeCoroutine != null) StopCoroutine(activeCinematicTimeCoroutine);
                activeCinematicTimeCoroutine = StartCoroutine(BossDefeatTimeScaleRoutine(isBigBoss, slowDuration));
            }

            // 4. 白フラッシュ ＋ シネマティックビネット（四隅暗転）
            if (activeCinematicOverlayCoroutine != null) StopCoroutine(activeCinematicOverlayCoroutine);
            activeCinematicOverlayCoroutine = StartCoroutine(BossDefeatOverlayRoutine(isBigBoss, slowDuration));

            // 5. カメラの軽いズームイン → スロー終了に合わせて復帰
            if (activeCinematicZoomCoroutine != null) StopCoroutine(activeCinematicZoomCoroutine);
            activeCinematicZoomCoroutine = StartCoroutine(BossDefeatCameraZoomRoutine(isBigBoss, slowDuration));

            // 6. 2重の衝撃波リング
            StartCoroutine(ShockwaveRingRoutine(bossPos, new Color(1f, 0.98f, 0.92f, 0.92f), 0.8f, isBigBoss ? 30f : 24f, 0.65f, 0f));
            StartCoroutine(ShockwaveRingRoutine(bossPos, new Color(0.65f, 0.95f, 1f, 0.72f), 0.6f, isBigBoss ? 24f : 18f, 0.78f, 0.08f));

            // 7. 花弁・光粒のバースト放出
            int petalCount = isBigBoss ? 48 : 30;
            int sparkCount = isBigBoss ? 36 : 22;
            SpawnDefeatPetalBurst(bossPos, petalCount, sparkCount, isBigBoss);
        }

        private IEnumerator BossDefeatTimeScaleRoutine(bool isBigBoss, float totalDuration)
        {
            if (Time.timeScale <= 0f) yield break;

            float hitStopSec = isBigBoss ? 0.10f : 0.08f;
            float deepSlowScale = isBigBoss ? 0.22f : 0.26f;
            float holdDuration = Mathf.Max(0.4f, totalDuration * 0.62f);
            float recoverDuration = Mathf.Max(0.35f, totalDuration - hitStopSec - holdDuration);

            // (a) 一瞬の完全ヒットストップ
            Time.timeScale = 0.02f;
            yield return new WaitForSecondsRealtime(hitStopSec);

            if (Time.timeScale <= 0f) yield break;

            // (b) 深いスローモーション
            Time.timeScale = deepSlowScale;
            float elapsed = 0f;
            while (elapsed < holdDuration)
            {
                if (Time.timeScale <= 0f) yield break; // ポーズ／報酬フェーズ突入時は邪魔しない
                elapsed += Time.unscaledDeltaTime;
                // スロー中もわずかに時間を進めて停滞感を防ぐ
                float t = Mathf.Clamp01(elapsed / holdDuration);
                Time.timeScale = Mathf.Lerp(deepSlowScale, deepSlowScale + 0.14f, t);
                yield return null;
            }

            // (c) 通常速度へ滑らかに復帰 (SmoothStep)
            float startScale = Time.timeScale;
            elapsed = 0f;
            while (elapsed < recoverDuration)
            {
                if (Time.timeScale <= 0f) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / recoverDuration));
                Time.timeScale = Mathf.Lerp(startScale, 1f, t);
                yield return null;
            }

            if (Time.timeScale > 0f)
            {
                Time.timeScale = 1f;
            }
            activeCinematicTimeCoroutine = null;
        }

        private IEnumerator BossDefeatOverlayRoutine(bool isBigBoss, float slowDuration)
        {
            float flashPeakAlpha = isBigBoss ? 0.68f : 0.50f;
            float flashDuration = isBigBoss ? 0.30f : 0.22f;
            float vignettePeakAlpha = isBigBoss ? 0.62f : 0.48f;
            float vignetteInSec = 0.15f;
            float vignetteOutSec = Mathf.Max(0.4f, slowDuration * 0.35f);
            float vignetteHoldSec = Mathf.Max(0.3f, slowDuration - vignetteInSec - vignetteOutSec);

            Color flashBase = new Color(0.96f, 0.99f, 1f, 1f);
            float totalSec = Mathf.Max(flashDuration, vignetteInSec + vignetteHoldSec + vignetteOutSec);
            float elapsed = 0f;

            while (elapsed < totalSec)
            {
                elapsed += Time.unscaledDeltaTime;

                // 白フラッシュ（指数的にスッと引く）
                if (cinematicFlashImage != null)
                {
                    if (elapsed < flashDuration)
                    {
                        float ft = elapsed / flashDuration;
                        float a = flashPeakAlpha * Mathf.Pow(1f - ft, 2f);
                        cinematicFlashImage.color = new Color(flashBase.r, flashBase.g, flashBase.b, a);
                    }
                    else
                    {
                        cinematicFlashImage.color = new Color(flashBase.r, flashBase.g, flashBase.b, 0f);
                    }
                }

                // ビネット（スロー中だけ四隅をじんわり暗転）
                if (cinematicVignetteImage != null)
                {
                    float va = 0f;
                    if (elapsed < vignetteInSec)
                    {
                        va = Mathf.Lerp(0f, vignettePeakAlpha, elapsed / vignetteInSec);
                    }
                    else if (elapsed < vignetteInSec + vignetteHoldSec)
                    {
                        va = vignettePeakAlpha;
                    }
                    else if (elapsed < vignetteInSec + vignetteHoldSec + vignetteOutSec)
                    {
                        float ot = (elapsed - vignetteInSec - vignetteHoldSec) / vignetteOutSec;
                        va = Mathf.Lerp(vignettePeakAlpha, 0f, Mathf.SmoothStep(0f, 1f, ot));
                    }
                    cinematicVignetteImage.color = new Color(1f, 1f, 1f, va);
                }

                yield return null;
            }

            if (cinematicFlashImage != null) cinematicFlashImage.color = new Color(1f, 1f, 1f, 0f);
            if (cinematicVignetteImage != null) cinematicVignetteImage.color = new Color(1f, 1f, 1f, 0f);
            activeCinematicOverlayCoroutine = null;
        }

        private IEnumerator BossDefeatCameraZoomRoutine(bool isBigBoss, float slowDuration)
        {
            Camera cam = Camera.main;
            if (cam == null) yield break;

            CameraControl camCtrl = cam.GetComponent<CameraControl>();
            float zoomDepth = isBigBoss ? -1.15f : -0.75f; // 約8〜12%ズームイン
            float inDuration = 0.14f;
            float holdDuration = Mathf.Max(0.35f, slowDuration * 0.50f);
            float outDuration = Mathf.Max(0.45f, slowDuration - inDuration - holdDuration);

            if (camCtrl != null)
            {
                float elapsed = 0f;
                while (elapsed < inDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / inDuration), 3f);
                    camCtrl.zoomOffset = Mathf.Lerp(0f, zoomDepth, t);
                    yield return null;
                }
                camCtrl.zoomOffset = zoomDepth;

                yield return new WaitForSecondsRealtime(holdDuration);

                elapsed = 0f;
                while (elapsed < outDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / outDuration));
                    camCtrl.zoomOffset = Mathf.Lerp(zoomDepth, 0f, t);
                    yield return null;
                }
                camCtrl.zoomOffset = 0f;
            }

            activeCinematicZoomCoroutine = null;
        }

        private IEnumerator ShockwaveRingRoutine(Vector3 pos, Color color, float startScale, float endScale, float duration, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            EnsureCinematicAssets();

            GameObject ring = new GameObject("DefeatShockwaveRing");
            ring.transform.position = pos;

            SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial;
            sr.sprite = cachedSmoothRingSprite;
            sr.color = color;
            sr.sortingOrder = 180;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // EaseOutQuart：初速が勢いよく広がり、画面端で余韻を残して消える
                float easeT = 1f - Mathf.Pow(1f - t, 4f);
                float curScale = Mathf.Lerp(startScale, endScale, easeT);
                ring.transform.localScale = Vector3.one * curScale;

                float alpha = color.a * Mathf.Pow(1f - t, 1.7f);
                sr.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            Destroy(ring);
        }

        private void SpawnDefeatPetalBurst(Vector3 center, int petalCount, int sparkCount, bool isBigBoss)
        {
            EnsureCinematicAssets();

            Color[] palette = new Color[]
            {
                new Color(1.00f, 0.92f, 0.96f, 0.95f), // 淡桜
                new Color(0.98f, 0.78f, 0.88f, 0.90f), // 桜ピンク
                new Color(0.82f, 0.97f, 1.00f, 0.90f), // エーテルシアン
                new Color(1.00f, 0.98f, 0.88f, 0.95f)  // 白金
            };

            for (int i = 0; i < petalCount; i++)
            {
                Color c = palette[Random.Range(0, palette.Length)];
                float speed = Random.Range(isBigBoss ? 4.5f : 3.5f, isBigBoss ? 14.0f : 10.5f);
                float scale = Random.Range(0.28f, isBigBoss ? 0.65f : 0.52f);
                float life = Random.Range(1.2f, 2.3f);
                StartCoroutine(PetalParticleRoutine(center, c, speed, scale, life));
            }

            for (int i = 0; i < sparkCount; i++)
            {
                Color c = (i % 2 == 0) ? new Color(1f, 0.96f, 0.78f, 1f) : new Color(0.75f, 0.96f, 1f, 1f);
                StartCoroutine(LightGrainRoutine(center, c, Random.Range(5f, 16f), Random.Range(0.55f, 1.25f)));
            }
        }

        /// <summary>
        /// 衝撃波で敵弾が消滅した位置に、光の粒＆花弁をパッと散らす
        /// </summary>
        public void SpawnBulletClearBurst(Vector3 pos, float delay)
        {
            EnsureCinematicAssets();
            StartCoroutine(BulletClearBurstRoutine(pos, delay));
        }

        private IEnumerator BulletClearBurstRoutine(Vector3 pos, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

            Color petalCol = (Random.value > 0.45f)
                ? new Color(1.0f, 0.85f, 0.93f, 0.90f)
                : new Color(0.80f, 0.97f, 1.0f, 0.90f);

            StartCoroutine(PetalParticleRoutine(pos, petalCol, Random.Range(1.8f, 4.5f), Random.Range(0.20f, 0.36f), Random.Range(0.65f, 1.15f)));
            StartCoroutine(LightGrainRoutine(pos, new Color(1f, 0.98f, 0.88f, 0.95f), Random.Range(2.5f, 6.0f), Random.Range(0.28f, 0.55f)));
        }

        private IEnumerator PetalParticleRoutine(Vector3 startPos, Color color, float initialSpeed, float baseScale, float duration)
        {
            GameObject pObj = new GameObject("DefeatPetal");
            pObj.transform.position = startPos + (Vector3)Random.insideUnitCircle * 0.25f;

            SpriteRenderer sr = pObj.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial;
            sr.sprite = cachedPetalSprite;
            sr.color = color;
            sr.sortingOrder = 160;

            float angleRad = Random.Range(0f, Mathf.PI * 2f);
            Vector3 vel = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * initialSpeed;
            float zRot = Random.Range(0f, 360f);
            float zRotSpeed = Random.Range(-220f, 220f);
            float flutterFreq = Random.Range(4f, 8f);
            float flutterPhase = Random.Range(0f, Mathf.PI * 2f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // スロー中も優雅に舞うよう、unscaledDeltaTime と deltaTime をブレンド
                float dt = Mathf.Lerp(Time.unscaledDeltaTime, Time.deltaTime, 0.35f);
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // 空気抵抗でふわっと減速しつつ、わずかに下へ舞い落ちる
                vel = Vector3.Lerp(vel, new Vector3(vel.x * 0.15f, -0.8f, 0f), dt * 2.8f);
                pObj.transform.position += vel * dt;

                zRot += zRotSpeed * dt;
                pObj.transform.rotation = Quaternion.Euler(0f, 0f, zRot);

                // ひらひらと裏返るような3D風スケール揺らぎ
                float flutter = 0.35f + 0.65f * Mathf.Abs(Mathf.Cos(elapsed * flutterFreq + flutterPhase));
                float fadeScale = baseScale * Mathf.Lerp(1.0f, 0.55f, t);
                pObj.transform.localScale = new Vector3(fadeScale * flutter, fadeScale, 1f);

                float alpha = color.a * (1f - Mathf.Pow(t, 2.2f));
                sr.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            Destroy(pObj);
        }

        private IEnumerator LightGrainRoutine(Vector3 startPos, Color color, float initialSpeed, float duration)
        {
            GameObject gObj = new GameObject("DefeatLightGrain");
            gObj.transform.position = startPos;

            SpriteRenderer sr = gObj.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial;
            sr.sprite = cachedCircleSprite;
            sr.color = color;
            sr.sortingOrder = 165;

            float baseScale = Random.Range(0.08f, 0.22f);
            float angleRad = Random.Range(0f, Mathf.PI * 2f);
            Vector3 vel = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * initialSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float dt = Mathf.Lerp(Time.unscaledDeltaTime, Time.deltaTime, 0.3f);
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                vel *= Mathf.Clamp01(1f - dt * 3.5f);
                gObj.transform.position += vel * dt;
                gObj.transform.localScale = Vector3.one * (baseScale * (1f - t * 0.7f));
                sr.color = new Color(color.r, color.g, color.b, color.a * (1f - t));

                yield return null;
            }

            Destroy(gObj);
        }

        // =====================================================================
        // Mob Kill HitStop (雑魚撃破ヒットストップ: 小 / 中 / 大)
        // =====================================================================
        [Header("Mob Kill HitStop Settings")]
        [Tooltip("小ヒットストップ（mob1, TutorialFirstEnemy, SwarmMob系）の timeScale")]
        public float smallHitStopTimeScale = 0.25f;
        [Tooltip("小ヒットストップの持続時間（リアルタイム秒）")]
        public float smallHitStopDuration = 0.040f;

        [Tooltip("中ヒットストップ（mob2, mob3等の通常雑魚）の timeScale")]
        public float mediumHitStopTimeScale = 0.08f;
        [Tooltip("中ヒットストップの持続時間（リアルタイム秒）")]
        public float mediumHitStopDuration = 0.065f;

        [Tooltip("大ヒットストップ（mob4〜6等の強敵雑魚）の timeScale")]
        public float largeHitStopTimeScale = 0.02f;
        [Tooltip("大ヒットストップの持続時間（リアルタイム秒）")]
        public float largeHitStopDuration = 0.110f;
        [Tooltip("大ヒットストップ時に添える画面揺れの強さ")]
        public float largeHitStopShakeMagnitude = 0.25f;

        [Tooltip("同時撃破・連殺時のカクつきを防ぐクールダウン（秒）")]
        public float mobKillHitStopCooldown = 0.08f;

        private Coroutine activeMobHitStopCoroutine;
        private int activeMobHitStopLevel = 0;
        private int lastMobHitStopLevel = 0;
        private float lastMobHitStopTime = -10f;

        /// <summary>
        /// 雑魚敵撃破時のヒットストップを発動する（1=Small, 2=Medium, 3=Large）
        /// </summary>
        public void TriggerMobKillHitStop(int sizeLevel)
        {
            if (sizeLevel <= 0) return;

            // ポーズ中（timeScale == 0）やボス撃破シネマティック中は邪魔しない
            if (Time.timeScale <= 0f || activeCinematicTimeCoroutine != null) return;

            // 既に同等以上のヒットストップが発動中ならスキップ
            if (activeMobHitStopCoroutine != null && sizeLevel <= activeMobHitStopLevel) return;

            // クールダウン中の連打防止（ただし直前より大きいサイズでの上書きは許可）
            float now = Time.unscaledTime;
            if (now - lastMobHitStopTime < mobKillHitStopCooldown && sizeLevel <= lastMobHitStopLevel) return;

            float targetScale;
            float duration;
            switch (sizeLevel)
            {
                case 1: // Small
                    targetScale = smallHitStopTimeScale;
                    duration = smallHitStopDuration;
                    break;
                case 3: // Large
                    targetScale = largeHitStopTimeScale;
                    duration = largeHitStopDuration;
                    break;
                default: // Medium (2)
                    targetScale = mediumHitStopTimeScale;
                    duration = mediumHitStopDuration;
                    break;
            }

            if (activeMobHitStopCoroutine != null)
            {
                StopCoroutine(activeMobHitStopCoroutine);
            }

            activeMobHitStopLevel = sizeLevel;
            lastMobHitStopLevel = sizeLevel;
            lastMobHitStopTime = now;

            // 大ヒットストップ時は軽い画面揺れも添える
            if (sizeLevel == 3 && largeHitStopShakeMagnitude > 0f && JuiceManager_Alpha.Instance != null)
            {
                JuiceManager_Alpha.Instance.ScreenShake(0.12f, largeHitStopShakeMagnitude);
            }

            activeMobHitStopCoroutine = StartCoroutine(MobKillHitStopRoutine(targetScale, duration));
        }

        private IEnumerator MobKillHitStopRoutine(float targetScale, float duration)
        {
            Time.timeScale = targetScale;
            yield return new WaitForSecondsRealtime(duration);

            // ポーズ中やボス撃破シネマティック中でなければ通常速度へ復帰
            if (Time.timeScale > 0f && activeCinematicTimeCoroutine == null)
            {
                Time.timeScale = 1f;
            }

            activeMobHitStopCoroutine = null;
            activeMobHitStopLevel = 0;
        }

        // =====================================================================
        // MidBoss Appear Effect Helpers (案D：空間収束・吸引・解放バースト)
        // =====================================================================

        /// <summary>
        /// シネマティックビネットの透明度を直接設定する（中ボス出現演出等から利用）
        /// </summary>
        public void SetCinematicVignetteAlpha(float alpha)
        {
            EnsureCinematicAssets();
            if (cinematicVignetteImage != null)
            {
                cinematicVignetteImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }

        /// <summary>
        /// 周囲から中心座標へ向かって光の粒と花弁が吸い込まれていく吸引パーティクルを再生する
        /// </summary>
        public void SpawnImplosionParticles(Vector3 center, float duration)
        {
            EnsureCinematicAssets();
            StartCoroutine(ImplosionSpawnerRoutine(center, duration));
        }

        private IEnumerator ImplosionSpawnerRoutine(Vector3 center, float duration)
        {
            float elapsed = 0f;
            float spawnTimer = 0f;

            Color[] palette = new Color[]
            {
                new Color(1.00f, 0.35f, 0.45f, 0.90f), // クリムゾン桜
                new Color(1.00f, 0.85f, 0.90f, 0.85f), // 淡桜
                new Color(0.65f, 0.95f, 1.00f, 0.85f), // エーテルシアン
                new Color(1.00f, 0.94f, 0.72f, 0.90f)  // 白金
            };

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                spawnTimer += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);
                // 終盤に向けて吸引パーティクルの密度が急増
                float interval = Mathf.Lerp(0.055f, 0.018f, progress);

                if (spawnTimer >= interval)
                {
                    spawnTimer = 0f;
                    bool isPetal = Random.value > 0.4f;
                    Color col = palette[Random.Range(0, palette.Length)];
                    float travelTime = Mathf.Clamp(Mathf.Lerp(0.55f, 0.25f, progress), 0.18f, Mathf.Max(0.18f, duration - elapsed + 0.08f));
                    StartCoroutine(SingleImplosionParticleRoutine(center, col, isPetal, travelTime));
                }

                yield return null;
            }
        }

        private IEnumerator SingleImplosionParticleRoutine(Vector3 targetCenter, Color color, bool isPetal, float travelDuration)
        {
            GameObject pObj = new GameObject(isPetal ? "ImplosionPetal" : "ImplosionGrain");
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float startDist = Random.Range(5.5f, 11.0f);
            Vector3 startPos = targetCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * startDist;
            pObj.transform.position = startPos;

            SpriteRenderer sr = pObj.AddComponent<SpriteRenderer>();
            sr.material = defaultSpriteMaterial;
            sr.sprite = isPetal ? cachedPetalSprite : cachedCircleSprite;
            sr.color = new Color(color.r, color.g, color.b, 0f);
            sr.sortingOrder = 162;

            float baseScale = isPetal ? Random.Range(0.24f, 0.48f) : Random.Range(0.08f, 0.18f);
            float swirlSign = Random.value > 0.5f ? 1f : -1f;
            float swirlAmount = Random.Range(0.9f, 2.2f) * swirlSign;
            float zRot = Random.Range(0f, 360f);
            float zRotSpeed = Random.Range(240f, 480f) * swirlSign;

            float elapsed = 0f;
            while (elapsed < travelDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / travelDuration);
                // EaseInCubic で中心に近づくほど急加速して吸い込まれる
                float easeT = t * t * t;
                float curDist = Mathf.Lerp(startDist, 0.05f, easeT);
                float curAngle = angle + swirlAmount * easeT;

                pObj.transform.position = targetCenter + new Vector3(Mathf.Cos(curAngle), Mathf.Sin(curAngle), 0f) * curDist;
                zRot += zRotSpeed * Time.unscaledDeltaTime;
                pObj.transform.rotation = Quaternion.Euler(0f, 0f, zRot);

                float scaleFactor = Mathf.Lerp(1.0f, 0.2f, easeT);
                pObj.transform.localScale = Vector3.one * (baseScale * scaleFactor);

                // 現れてから中心でスッと消える
                float fadeIn = Mathf.Clamp01(t * 4f);
                float fadeOut = 1f - Mathf.Clamp01((t - 0.82f) / 0.18f);
                sr.color = new Color(color.r, color.g, color.b, color.a * fadeIn * fadeOut);

                yield return null;
            }

            Destroy(pObj);
        }

        /// <summary>
        /// 中ボス降臨瞬間の解放バースト（白フラッシュ＋赤金＆白金の2重衝撃波＋花弁バースト）
        /// </summary>
        public void TriggerMidBossAppearBurst(Vector3 center)
        {
            EnsureCinematicAssets();

            // 白フラッシュ（0.25秒）
            StartCoroutine(QuickWhiteFlashRoutine(0.58f, 0.26f));

            // 2重の衝撃波リング（赤金＆白金）
            StartCoroutine(ShockwaveRingRoutine(center, new Color(1f, 0.94f, 0.85f, 0.95f), 0.5f, 26f, 0.55f, 0f));
            StartCoroutine(ShockwaveRingRoutine(center, new Color(1f, 0.28f, 0.38f, 0.78f), 0.4f, 20f, 0.65f, 0.06f));

            // 花弁・光粒の解放バースト
            SpawnDefeatPetalBurst(center, petalCount: 22, sparkCount: 18, isBigBoss: false);
        }

        private IEnumerator QuickWhiteFlashRoutine(float peakAlpha, float duration)
        {
            EnsureCinematicAssets();
            if (cinematicFlashImage == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float a = peakAlpha * Mathf.Pow(1f - t, 2f);
                cinematicFlashImage.color = new Color(1f, 0.98f, 0.96f, a);
                yield return null;
            }
            cinematicFlashImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
