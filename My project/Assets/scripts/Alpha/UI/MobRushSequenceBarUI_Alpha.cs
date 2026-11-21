using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Alpha.UI
{
    /// <summary>
    /// 雑魚ラッシュ（エネミーラッシュ）期間中にタイムラインゲージと切り替えて表示される、
    /// 椿の深紅（警告）×真鍮ゴールド基調の専用ラッシュタイムラインUI
    /// </summary>
    public class MobRushSequenceBarUI_Alpha : MonoBehaviour
    {
        private static MobRushSequenceBarUI_Alpha _instance;
        public static MobRushSequenceBarUI_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MobRushSequenceBarUI_Alpha>();
                    if (_instance == null)
                    {
                        CreateDefaultUI();
                    }
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        [Header("UI References")]
        [Tooltip("ラッシュ残り時間を示すスライダー")]
        public Slider progressSlider;
        [Tooltip("残り時間を表示するテキスト")]
        public TextMeshProUGUI timeText;
        [Tooltip("ラッシュ中の撃破数を表示するテキスト")]
        public TextMeshProUGUI defeatText;

        [Header("Font Settings")]
        public TMP_FontAsset customFontAsset;

        [Header("Animation Settings")]
        public float baseScale = 1.0f;
        public float bounceScaleAdd = 0.12f;
        public float bounceDuration = 0.25f;

        // Modern Botanical Palette
        private static readonly Color ColorBaseInk = new Color(0.040f, 0.055f, 0.048f, 0.96f);
        private static readonly Color ColorCamelliaCrimson = new Color(0.72f, 0.22f, 0.24f, 1.0f); // 椿・深紅
        private static readonly Color ColorSunflowerGold = new Color(0.92f, 0.75f, 0.32f, 1.0f);  // 向日葵・ゴールド
        private static readonly Color ColorIvoryWhite = new Color(0.93f, 0.91f, 0.86f, 1.0f);     // 生成り白
        private static readonly Color ColorLotusJade = new Color(0.32f, 0.72f, 0.53f, 1.0f);       // 蓮・翡翠

        private int lastDisplayedSecond = -1;
        private Tweener textBounceTween;
        private int currentDefeatCount = 0;
        private bool isRushActive = false;
        public bool IsRushActive => isRushActive;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            LoadFontAssetIfNeeded();

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "SystemUI";
            canvas.sortingOrder = 505;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            gameObject.SetActive(false);
        }

        private void LoadFontAssetIfNeeded()
        {
            if (customFontAsset != null) return;
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("SoukouMincho SDF t:TMP_FontAsset");
            if (guids != null && guids.Length > 0)
            {
                string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                customFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
            }
#endif
            if (customFontAsset == null) customFontAsset = Resources.Load<TMP_FontAsset>("Fonts/SoukouMincho SDF");
        }

        public void Show(float duration)
        {
            LoadFontAssetIfNeeded();
            isRushActive = true;
            currentDefeatCount = 0;
            lastDisplayedSecond = -1;

            if (SequenceBarUI_Alpha.Instance != null)
            {
                SequenceBarUI_Alpha.Instance.SetBarVisible(false);
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (progressSlider != null)
            {
                progressSlider.maxValue = 1f;
                progressSlider.minValue = 0f;
                progressSlider.value = 1f;
            }

            UpdateDefeatText();

            // 登場演出（少し上からスッと降りてくる、またはポップ）
            transform.localScale = Vector3.one * 0.95f;
            transform.DOScale(Vector3.one, 0.2f).SetUpdate(true);
        }

        public void Hide()
        {
            isRushActive = false;
            gameObject.SetActive(false);

            if (textBounceTween != null && textBounceTween.IsActive())
            {
                textBounceTween.Kill();
            }

            // 通常のタイムラインバーを再表示
            if (SequenceBarUI_Alpha.Instance != null)
            {
                SequenceBarUI_Alpha.Instance.SetBarVisible(true);
            }
        }

        public void UpdateProgress(float remainingTime, float totalDuration)
        {
            if (totalDuration <= 0f) return;

            float progress = Mathf.Clamp01(remainingTime / totalDuration);
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            int currentSecond = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));

            if (timeText != null)
            {
                timeText.text = $"<size=75%><color=#b8383d>ENEMY RUSH // </color></size>残り <color=#ece8dc>{currentSecond:D2}</color> 秒！";

                // 残り5秒以下なら点滅・色変化
                if (remainingTime <= 5f)
                {
                    float colorT = 1f - (remainingTime / 5f);
                    timeText.color = Color.Lerp(ColorSunflowerGold, ColorCamelliaCrimson, colorT);
                }
                else
                {
                    timeText.color = ColorIvoryWhite;
                }

                // 1秒ごとに跳ねる演出
                if (currentSecond != lastDisplayedSecond && currentSecond > 0)
                {
                    lastDisplayedSecond = currentSecond;
                    timeText.transform.DOKill();
                    timeText.transform.localScale = Vector3.one * baseScale;
                    textBounceTween = timeText.transform
                        .DOScale(baseScale + bounceScaleAdd, bounceDuration)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetUpdate(true);
                }
            }
        }

        public void IncrementDefeatCount()
        {
            if (!isRushActive) return;
            currentDefeatCount++;
            UpdateDefeatText();

            if (defeatText != null)
            {
                defeatText.transform.DOKill();
                defeatText.transform.localScale = Vector3.one;
                defeatText.transform.DOScale(1.2f, 0.15f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
            }
        }

        private void UpdateDefeatText()
        {
            if (defeatText != null)
            {
                defeatText.text = $"<size=75%><color=#ccad70>DEFEATED // </color></size><color=#52b788><b>{currentDefeatCount}</b></color> 体";
            }
        }

        /// <summary>
        /// シーン内にオブジェクトが存在しない場合に自動構築するフェイルセーフ
        /// </summary>
        private static void CreateDefaultUI()
        {
            Canvas targetCanvas = null;
            if (SequenceBarUI_Alpha.Instance != null)
            {
                targetCanvas = SequenceBarUI_Alpha.Instance.GetComponentInParent<Canvas>();
            }
            if (targetCanvas == null)
            {
                var canvasObj = GameObject.Find("SystemUICanvas");
                if (canvasObj != null) targetCanvas = canvasObj.GetComponent<Canvas>();
            }
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }
            if (targetCanvas == null) return;

            GameObject go = new GameObject("MobRushSequenceBarUI", typeof(RectTransform));
            go.transform.SetParent(targetCanvas.transform, false);

            // 独自のCanvasコンポーネントを付与してSystemUI内の最前面（SortingOrder 505）に配置
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "SystemUI";
            canvas.sortingOrder = 505;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            go.AddComponent<GraphicRaycaster>();

            RectTransform rt = go.GetComponent<RectTransform>();
            if (SequenceBarUI_Alpha.Instance != null && SequenceBarUI_Alpha.Instance.progressSlider != null)
            {
                RectTransform seqRT = SequenceBarUI_Alpha.Instance.progressSlider.GetComponent<RectTransform>();
                rt.anchorMin = seqRT.anchorMin;
                rt.anchorMax = seqRT.anchorMax;
                rt.pivot = seqRT.pivot;
                rt.anchoredPosition = seqRT.anchoredPosition;
                rt.sizeDelta = new Vector2(seqRT.sizeDelta.x, Mathf.Max(seqRT.sizeDelta.y, 36f));
            }
            else
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 473.7f);
                rt.sizeDelta = new Vector2(1652.2f, 36f);
            }

            // 背景イメージ（深森墨黒すりガラス）
            Image bg = go.AddComponent<Image>();
            bg.color = ColorBaseInk;
            AddCornerAccents(rt, ColorCamelliaCrimson, 14f, 1.5f);

            var mobUI = go.AddComponent<MobRushSequenceBarUI_Alpha>();

            // スライダー本体
            GameObject sliderGO = new GameObject("Slider", typeof(RectTransform));
            sliderGO.transform.SetParent(go.transform, false);
            RectTransform sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = Vector2.zero;
            sliderRT.anchorMax = Vector2.one;
            sliderRT.offsetMin = new Vector2(4f, 4f);
            sliderRT.offsetMax = new Vector2(-4f, -4f);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Fill Area
            GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGO.transform.SetParent(sliderGO.transform, false);
            RectTransform fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = Vector2.zero;

            // Fill
            GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.72f, 0.22f, 0.24f, 0.90f); // 椿・深紅
            slider.fillRect = fillRT;

            mobUI.progressSlider = slider;

            // 残り時間テキスト（中央）
            GameObject textGO = new GameObject("TimeText", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0f, 0f);
            textRT.anchorMax = new Vector2(0.7f, 1f);
            textRT.offsetMin = new Vector2(16f, 0f);
            textRT.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "ENEMY RUSH // 残り -- 秒";
            tmp.fontSize = 17f;
            tmp.color = ColorIvoryWhite;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            mobUI.timeText = tmp;

            // 撃破カウントテキスト（右側）
            GameObject defGO = new GameObject("DefeatText", typeof(RectTransform));
            defGO.transform.SetParent(go.transform, false);
            RectTransform defRT = defGO.GetComponent<RectTransform>();
            defRT.anchorMin = new Vector2(0.7f, 0f);
            defRT.anchorMax = new Vector2(1f, 1f);
            defRT.offsetMin = Vector2.zero;
            defRT.offsetMax = new Vector2(-16f, 0f);

            var defTmp = defGO.AddComponent<TextMeshProUGUI>();
            defTmp.text = "DEFEATED // 0 体";
            defTmp.fontSize = 16f;
            defTmp.color = ColorLotusJade;
            defTmp.alignment = TextAlignmentOptions.MidlineRight;
            mobUI.defeatText = defTmp;

            _instance = mobUI;
            go.SetActive(false);
            Debug.Log("[MobRushSequenceBarUI] Auto-created rush UI successfully.");
        }

        private static void AddCornerAccents(RectTransform targetParent, Color color, float size = 14f, float thickness = 1.5f)
        {
            if (targetParent == null) return;
            CreateSingleCorner(targetParent, "Corner_TL", new Vector2(0f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_TR", new Vector2(1f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BL", new Vector2(0f, 0f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BR", new Vector2(1f, 0f), color, size, thickness);
        }

        private static void CreateSingleCorner(RectTransform parent, string name, Vector2 anchor, Color color, float size, float thickness)
        {
            GameObject cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            RectTransform crt = cornerGO.GetComponent<RectTransform>();
            crt.anchorMin = anchor;
            crt.anchorMax = anchor;
            crt.pivot = anchor;
            crt.sizeDelta = new Vector2(size, size);
            crt.anchoredPosition = Vector2.zero;

            GameObject hGO = new GameObject("H", typeof(RectTransform));
            hGO.transform.SetParent(cornerGO.transform, false);
            RectTransform hrt = hGO.GetComponent<RectTransform>();
            hrt.anchorMin = anchor;
            hrt.anchorMax = anchor;
            hrt.pivot = anchor;
            hrt.sizeDelta = new Vector2(size, thickness);
            hrt.anchoredPosition = Vector2.zero;
            Image hImg = hGO.AddComponent<Image>();
            hImg.color = color;
            hImg.raycastTarget = false;

            GameObject vGO = new GameObject("V", typeof(RectTransform));
            vGO.transform.SetParent(cornerGO.transform, false);
            RectTransform vrt = vGO.GetComponent<RectTransform>();
            vrt.anchorMin = anchor;
            vrt.anchorMax = anchor;
            vrt.pivot = anchor;
            vrt.sizeDelta = new Vector2(thickness, size);
            vrt.anchoredPosition = Vector2.zero;
            Image vImg = vGO.AddComponent<Image>();
            vImg.color = color;
            vImg.raycastTarget = false;
        }
    }
}
