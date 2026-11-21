using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Alpha.Core;

namespace Alpha.Effect
{
    /// <summary>
    /// 中ボス出現演出（案Dハイブリッド：空間収束・花弁吸引 ＋ シックなWARNING帯＆黒帯 ＋ 白フラッシュ・衝撃波・カメラ演出 ＋ 専用SE）
    /// </summary>
    public class Alpha_MidBossAppearEffect : MonoBehaviour
    {
        [Header("Animation Settings (Legacy Compatible)")]
        [Tooltip("旧シリアライズ互換用（useCompactTimeline が ON の場合は下記の convergeDuration を使用）")]
        public float duration = 5f;
        [Tooltip("渦巻きパネルの演出開始時スケール")]
        public float startScale = 15f;

        [Header("Cinematic Timeline Settings (Plan D)")]
        [Tooltip("ONでテンポの良い約2.15秒シネマティック演出を使用（OFFでdurationを使用）")]
        public bool useCompactTimeline = true;
        [Tooltip("空間収束＆警告表示にかける時間（秒）")]
        public float convergeDuration = 2.0f;
        [Tooltip("中心の1点へ極小圧縮した直後の「タメ」時間（秒）")]
        public float holdCompressionDuration = 0.14f;

        [Header("Warning Banner Settings")]
        public string warningHeaderText = "— WARNING —";
        public string warningSubText = "ELITE APPROACHING";
        public TMP_FontAsset customFontAsset;

        [Header("Audio Settings")]
        public AudioClip warningSEClip;
        public AudioClip appearImpactSEClip;
        [Range(0f, 1f)] public float warningSEVolume = 0.85f;
        [Range(0f, 1f)] public float appearSEVolume = 0.92f;

        [Header("Screen Shake & Camera Settings")]
        [Tooltip("出現瞬間の画面揺れの持続時間")]
        public float shakeDuration = 0.45f;
        [Tooltip("出現瞬間の画面揺れの強さ")]
        public float shakeIntensity = 0.55f;
        [Tooltip("収束中のカメラズームイン量（負値でズームイン）")]
        public float cameraZoomDepth = -0.5f;

        private Material effectMat;
        private RectTransform rectTransform;
        private Graphic targetGraphic;
        private Color baseGraphicColor = Color.white;

        // 動的生成するシックWARNING帯＆上下黒帯UI
        private Canvas overlayCanvas;
        private RectTransform topBarRT;
        private RectTransform bottomBarRT;
        private RectTransform warningBannerRT;
        private CanvasGroup warningBannerCG;
        private TextMeshProUGUI warningHeaderTMP;
        private TextMeshProUGUI warningSubTMP;
        private Image warningTopLine;
        private Image warningBottomLine;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            targetGraphic = GetComponent<Graphic>();
            HideLegacyPanelGraphics();

            LoadAssetsIfNeeded();

            // 初動で非アクティブにしておく
            gameObject.SetActive(false);
        }

        private void HideLegacyPanelGraphics()
        {
            // 旧・収縮パネル（Image / RawImage 等）の描画を完全に無効化
            Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                if (g != null)
                {
                    g.enabled = false;
                }
            }
        }

        private void LoadAssetsIfNeeded()
        {
            if (warningSEClip == null)
            {
                warningSEClip = Resources.Load<AudioClip>("SE/SE_MidBoss_Warning");
            }
            if (appearImpactSEClip == null)
            {
                appearImpactSEClip = Resources.Load<AudioClip>("SE/SE_MidBoss_Appear");
            }

#if UNITY_EDITOR
            if (customFontAsset == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("SoukouMincho SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    customFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
                }
            }
#endif
            if (customFontAsset == null)
            {
                customFontAsset = Resources.Load<TMP_FontAsset>("Fonts/SoukouMincho SDF");
            }
        }

        private void EnsureOverlayUI()
        {
            if (overlayCanvas != null) return;

            GameObject cObj = new GameObject("MidBossWarningOverlayCanvas");
            overlayCanvas = cObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 960;

            CanvasScaler scaler = cObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // --- 上下のシネマティック黒帯（レターボックス） ---
            topBarRT = CreateBar(cObj.transform, "TopLetterbox", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), 68f, new Color(0.02f, 0.02f, 0.04f, 0.92f));
            bottomBarRT = CreateBar(cObj.transform, "BottomLetterbox", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), 68f, new Color(0.02f, 0.02f, 0.04f, 0.92f));

            // --- シックなWARNING横帯（中央やや上寄り） ---
            GameObject bannerObj = new GameObject("WarningBanner", typeof(RectTransform));
            bannerObj.transform.SetParent(cObj.transform, false);
            warningBannerRT = bannerObj.GetComponent<RectTransform>();
            warningBannerRT.anchorMin = new Vector2(0f, 0.5f);
            warningBannerRT.anchorMax = new Vector2(1f, 0.5f);
            warningBannerRT.pivot = new Vector2(0.5f, 0.5f);
            warningBannerRT.sizeDelta = new Vector2(0f, 118f);
            warningBannerRT.anchoredPosition = new Vector2(0f, 180f);

            Image bannerBg = bannerObj.AddComponent<Image>();
            bannerBg.color = new Color(0.04f, 0.03f, 0.06f, 0.78f);
            bannerBg.raycastTarget = false;

            warningBannerCG = bannerObj.AddComponent<CanvasGroup>();
            warningBannerCG.alpha = 0f;
            warningBannerCG.blocksRaycasts = false;
            warningBannerCG.interactable = false;

            // 上下の極細クリムゾンアクセントライン
            Color lineCol = new Color(0.92f, 0.22f, 0.32f, 0.95f);
            warningTopLine = CreateAccentLine(bannerObj.transform, "TopLine", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), 2.5f, lineCol);
            warningBottomLine = CreateAccentLine(bannerObj.transform, "BottomLine", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), 2.5f, lineCol);

            // メインテキスト（— WARNING —）
            GameObject headerObj = new GameObject("WarningHeaderText", typeof(RectTransform));
            headerObj.transform.SetParent(bannerObj.transform, false);
            RectTransform hrt = headerObj.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 0.42f);
            hrt.anchorMax = new Vector2(1f, 0.95f);
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;

            warningHeaderTMP = headerObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) warningHeaderTMP.font = customFontAsset;
            warningHeaderTMP.text = warningHeaderText;
            warningHeaderTMP.fontSize = 42f;
            warningHeaderTMP.fontStyle = FontStyles.Bold;
            warningHeaderTMP.characterSpacing = 14f;
            warningHeaderTMP.color = new Color(0.98f, 0.30f, 0.38f, 1f);
            warningHeaderTMP.alignment = TextAlignmentOptions.Center;
            warningHeaderTMP.raycastTarget = false;

            // サブテキスト（ELITE APPROACHING）
            GameObject subObj = new GameObject("WarningSubText", typeof(RectTransform));
            subObj.transform.SetParent(bannerObj.transform, false);
            RectTransform srt = subObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0.06f);
            srt.anchorMax = new Vector2(1f, 0.46f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;

            warningSubTMP = subObj.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) warningSubTMP.font = customFontAsset;
            warningSubTMP.text = warningSubText;
            warningSubTMP.fontSize = 20f;
            warningSubTMP.characterSpacing = 10f;
            warningSubTMP.color = new Color(0.95f, 0.92f, 0.88f, 0.92f);
            warningSubTMP.alignment = TextAlignmentOptions.Center;
            warningSubTMP.raycastTarget = false;

            cObj.SetActive(false);
        }

        private RectTransform CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, float height, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        private Image CreateAccentLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, float height, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// 演出を開始します。終了時に onComplete コールバックが呼ばれます。
        /// </summary>
        public void PlayEffect(System.Action onComplete)
        {
            HideLegacyPanelGraphics();
            LoadAssetsIfNeeded();
            EnsureOverlayUI();
            gameObject.SetActive(true);
            StartCoroutine(EffectRoutine(onComplete));
        }

        private Vector3 GetAppearWorldCenter()
        {
            // カメラ中心（中ボス出現位置付近）を基準に吸引＆衝撃波を発生させる
            if (Camera.main != null)
            {
                Vector3 c = Camera.main.transform.position;
                return new Vector3(c.x, c.y, 0f);
            }
            return Vector3.zero;
        }

        private IEnumerator EffectRoutine(System.Action onComplete)
        {
            float activeConvergeDur = useCompactTimeline ? convergeDuration : Mathf.Max(0.8f, duration);
            Vector3 worldCenter = GetAppearWorldCenter();

            // 1. 予兆ライザー＋シック警告脈動SE再生
            if (warningSEClip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(warningSEClip, warningSEVolume, 1.0f);
            }

            // 2. 吸引パーティクル開始（周囲から中心へ花弁＆光粒を吸い込む）
            var juice = ProceduralJuiceManager_Alpha.Instance;
            if (juice != null)
            {
                juice.SpawnImplosionParticles(worldCenter, activeConvergeDur);
            }

            // 3. オーバーレイUI（黒帯・WARNING帯）初期状態セット
            if (overlayCanvas != null)
            {
                overlayCanvas.gameObject.SetActive(true);
                if (topBarRT != null) topBarRT.anchoredPosition = new Vector2(0f, 68f);
                if (bottomBarRT != null) bottomBarRT.anchoredPosition = new Vector2(0f, -68f);
                if (warningBannerRT != null) warningBannerRT.localScale = new Vector3(1f, 0f, 1f);
                if (warningBannerCG != null) warningBannerCG.alpha = 0f;
            }

            CameraControl camCtrl = Camera.main != null ? Camera.main.GetComponent<CameraControl>() : null;

            // --- Phase 1: 空間収束（花弁吸引） ＋ シックWARNING帯 ＋ 緩やかなズーム＆地鳴り ---
            float elapsed = 0f;
            float rumbleTimer = 0f;

            while (elapsed < activeConvergeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / activeConvergeDur);

                // (a) 上下シネマティック黒帯：最初の0.28秒でスッと入り、最後0.18秒前まで維持
                float barInT = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / 0.28f), 3f);
                if (topBarRT != null) topBarRT.anchoredPosition = new Vector2(0f, Mathf.Lerp(68f, 0f, barInT));
                if (bottomBarRT != null) bottomBarRT.anchoredPosition = new Vector2(0f, Mathf.Lerp(-68f, 0f, barInT));

                // (c) WARNING帯：0.12〜0.32秒で横一閃展開、脈動（約3回）に合わせて明滅、終盤0.2秒でスッと引く
                float bannerScaleY = 1f;
                float bannerAlpha = 1f;
                if (elapsed < 0.10f)
                {
                    bannerScaleY = 0f;
                    bannerAlpha = 0f;
                }
                else if (elapsed < 0.30f)
                {
                    float bt = (elapsed - 0.10f) / 0.20f;
                    bannerScaleY = 1f - Mathf.Pow(1f - bt, 3f);
                    bannerAlpha = bt;
                }
                else if (elapsed > activeConvergeDur - 0.20f)
                {
                    float outT = Mathf.Clamp01((elapsed - (activeConvergeDur - 0.20f)) / 0.20f);
                    bannerScaleY = Mathf.Lerp(1f, 0f, outT * outT);
                    bannerAlpha = 1f - outT;
                }
                else
                {
                    // 3回の警告脈動に合わせた落ち着いた明滅
                    float pulse = 0.78f + 0.22f * Mathf.Sin((elapsed - 0.15f) * 10.0f);
                    bannerAlpha = pulse;
                }

                if (warningBannerRT != null) warningBannerRT.localScale = new Vector3(1f, bannerScaleY, 1f);
                if (warningBannerCG != null) warningBannerCG.alpha = bannerAlpha;

                // (d) ビネット暗転：じんわり深まる
                if (juice != null)
                {
                    juice.SetCinematicVignetteAlpha(Mathf.Lerp(0f, 0.52f, Mathf.Clamp01(t * 1.4f)));
                }

                // (e) カメラズームイン（後半に向けてじわっと寄る）
                if (camCtrl != null)
                {
                    camCtrl.zoomOffset = Mathf.Lerp(0f, cameraZoomDepth, t * t);
                }

                // (f) 後半（60%以降）に微細な地鳴りシェイク
                if (t > 0.60f)
                {
                    rumbleTimer += Time.unscaledDeltaTime;
                    if (rumbleTimer >= 0.08f)
                    {
                        rumbleTimer = 0f;
                        float rumbleMag = Mathf.Lerp(0.04f, 0.16f, (t - 0.60f) / 0.40f);
                        if (JuiceManager_Alpha.Instance != null)
                        {
                            JuiceManager_Alpha.Instance.ScreenShake(0.09f, rumbleMag);
                        }
                    }
                }

                yield return null;
            }

            // --- Phase 2: 一瞬の「タメ」（静寂と緊張の頂点） ---
            if (warningBannerRT != null) warningBannerRT.localScale = new Vector3(1f, 0f, 1f);
            if (warningBannerCG != null) warningBannerCG.alpha = 0f;
            if (holdCompressionDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdCompressionDuration);
            }

            // --- Phase 3: 解放・中ボス降臨インパクト！ ---

            // (a) 降臨インパクトSE
            if (appearImpactSEClip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(appearImpactSEClip, appearSEVolume, 1.0f);
            }

            // (b) 白フラッシュ ＋ 赤金＆白金の2重衝撃波リング ＋ 花弁バースト
            Vector3 releaseCenter = GetAppearWorldCenter();
            if (juice != null)
            {
                juice.TriggerMidBossAppearBurst(releaseCenter);
            }

            // (c) 強めのカメラ揺れ
            if (JuiceManager_Alpha.Instance != null)
            {
                JuiceManager_Alpha.Instance.ScreenShake(shakeDuration, shakeIntensity);
            }

            // (d) 完了コールバック呼び出し（ここで中ボスがドーンとスポーン＆中ボスBGM開始！）
            onComplete?.Invoke();

            // (e) 黒帯・ビネット・カメラズームを滑らかに退場（中ボス戦闘を止めずに裏で0.35秒フェードアウト）
            // ※自身を非アクティブ化する前に、退場コルーチンは ProceduralJuiceManager 側で走らせる
            if (juice != null)
            {
                juice.StartCoroutine(CleanupOverlayAndZoomRoutine(camCtrl, juice));
            }
            else
            {
                if (camCtrl != null) camCtrl.zoomOffset = 0f;
                if (overlayCanvas != null) overlayCanvas.gameObject.SetActive(false);
            }

            // パネル本体を非表示にする
            gameObject.SetActive(false);
        }

        private IEnumerator CleanupOverlayAndZoomRoutine(CameraControl camCtrl, ProceduralJuiceManager_Alpha juice)
        {
            float outDur = 0.35f;
            float elapsed = 0f;
            float startZoom = camCtrl != null ? camCtrl.zoomOffset : 0f;

            while (elapsed < outDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / outDur));

                if (topBarRT != null) topBarRT.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, 68f, t));
                if (bottomBarRT != null) bottomBarRT.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, -68f, t));
                if (juice != null) juice.SetCinematicVignetteAlpha(Mathf.Lerp(0.52f, 0f, t));
                if (camCtrl != null) camCtrl.zoomOffset = Mathf.Lerp(startZoom, 0f, t);

                yield return null;
            }

            if (camCtrl != null) camCtrl.zoomOffset = 0f;
            if (juice != null) juice.SetCinematicVignetteAlpha(0f);
            if (overlayCanvas != null) overlayCanvas.gameObject.SetActive(false);
        }

        private void SetPanelScale(float s)
        {
            Vector3 v = new Vector3(s, s, 1f);
            if (rectTransform != null) rectTransform.localScale = v;
            else transform.localScale = v;
        }

        private void SetPanelAlpha(float alpha)
        {
            if (targetGraphic != null)
            {
                Color c = targetGraphic.color;
                c.a = alpha;
                targetGraphic.color = c;
            }
        }

        private void OnDestroy()
        {
            if (overlayCanvas != null)
            {
                Destroy(overlayCanvas.gameObject);
            }
        }
    }
}
