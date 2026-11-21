using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alpha.Data;
using Alpha.Managers; // PlayerInputManager_Alpha

namespace Alpha.UI
{
    /// <summary>
    /// ポーズ画面の「ライブラリ」内に表示される、
    /// Modern Botanical UI / Herbarium（植物標本×神格条項）デザインの目録閲覧UI
    /// </summary>
    public class LibraryPanelUI_Alpha : MonoBehaviour
    {
        public enum SubTabType
        {
            Tutorial,
            Enemy,
            Bullet,
            Effect
        }

        [Header("Font Settings")]
        public TMP_FontAsset customFontAsset;

        [Header("Modern Botanical Palette (指示書準拠)")]
        // ベース (70%): 墨黒・深森の暗緑・すりガラス調
        public static readonly Color ColorBaseInk        = new Color(0.040f, 0.055f, 0.048f, 0.96f); // #0a0e0c 深森墨黒
        public static readonly Color ColorBasePlate      = new Color(0.055f, 0.080f, 0.068f, 0.94f); // #0e1411 標本台座
        public static readonly Color ColorBaseCard       = new Color(0.048f, 0.070f, 0.060f, 0.95f); // #0c120f 詳細カード
        public static readonly Color ColorBaseList       = new Color(0.035f, 0.050f, 0.042f, 0.90f); // #090d0b リスト背景

        // メイン (25%): 鈍い真鍮ゴールド・淡い象牙色・落ち着いた苔色
        public static readonly Color ColorBrassGold      = new Color(0.80f, 0.68f, 0.44f, 1.0f);     // #ccad70 アンティーク真鍮
        public static readonly Color ColorBrassMuted     = new Color(0.55f, 0.46f, 0.32f, 0.65f);    // #8c7552 枠線・幾何学ライン
        public static readonly Color ColorBrassFaint     = new Color(0.55f, 0.46f, 0.32f, 0.25f);    // 極細スライド枠
        public static readonly Color ColorIvoryWhite     = new Color(0.93f, 0.91f, 0.86f, 1.0f);     // #ece8dc 生成り白（本文）
        public static readonly Color ColorIvoryMuted     = new Color(0.68f, 0.68f, 0.64f, 0.85f);    // #adada3 未選択テキスト
        public static readonly Color ColorMossSlate      = new Color(0.14f, 0.21f, 0.17f, 0.95f);    // #24362b 選択中背景・苔色
        public static readonly Color ColorDarkMoss       = new Color(0.08f, 0.12f, 0.10f, 0.85f);    // #141f1a 通常リスト背景

        // アクセント (5%): 各神格の花弁色・若葉
        public static readonly Color ColorLotusJade      = new Color(0.32f, 0.72f, 0.53f, 1.0f);     // #52b788 蓮・翡翠バッジ
        public static readonly Color ColorSproutLeaf     = new Color(0.48f, 0.78f, 0.56f, 1.0f);     // #7bc78f 新緑Tips
        public static readonly Color ColorSunflowerGold  = new Color(0.92f, 0.75f, 0.32f, 1.0f);     // #ebbf52 向日葵・操作キー
        public static readonly Color ColorCamelliaCrimson= new Color(0.72f, 0.22f, 0.24f, 1.0f);     // #b8383d 椿・深紅
        public static readonly Color ColorWisteriaViolet = new Color(0.58f, 0.50f, 0.75f, 1.0f);     // #9480bf 藤・藤紫

        [Header("Legacy Color Compatibility")]
        public Color activeSubTabColor   = new Color(0.80f, 0.68f, 0.44f, 1f);
        public Color inactiveSubTabColor = new Color(0.68f, 0.68f, 0.64f, 0.85f);
        public Color activeSubTabBgColor = new Color(0.14f, 0.21f, 0.17f, 0.95f);
        public Color inactiveSubTabBgColor = new Color(0.06f, 0.09f, 0.075f, 0.85f);

        public Color selectedItemColor = new Color(0.93f, 0.91f, 0.86f, 1f);
        public Color normalItemColor   = new Color(0.75f, 0.75f, 0.72f, 0.9f);
        public Color lockedItemColor   = new Color(0.42f, 0.45f, 0.43f, 0.7f);

        [Header("UI Containers")]
        [SerializeField] private RectTransform subTabContainer;
        [SerializeField] private GameObject tutorialView;
        [SerializeField] private GameObject genericSubView;

        // チュートリアル用UI
        [SerializeField] private RectTransform tutorialListContent;
        [SerializeField] private TextMeshProUGUI detailTitleText;
        [SerializeField] private TextMeshProUGUI detailCategoryText;
        [SerializeField] private TextMeshProUGUI detailBodyText;
        [SerializeField] private TextMeshProUGUI detailShortcutText;
        [SerializeField] private TextMeshProUGUI detailTipsText;
        [SerializeField] private TextMeshProUGUI inputDeviceStatusText;

        // 汎用サブタブ用UI（敵、弾、エフェクト）
        [SerializeField] private TextMeshProUGUI genericViewTitleText;
        [SerializeField] private TextMeshProUGUI genericViewDescText;

        private SubTabType currentSubTab = SubTabType.Tutorial;
        private string selectedTutorialId = "001";
        private List<Button> subTabButtons = new List<Button>();
        private List<Image> subTabBgImages = new List<Image>();
        private List<TextMeshProUGUI> subTabTextList = new List<TextMeshProUGUI>();
        private List<Image> subTabIndicatorBars = new List<Image>();

        private Dictionary<string, Button> tutorialListButtons = new Dictionary<string, Button>();
        private bool lastGamepadState = false;

        private void Awake()
        {
            LoadFontAssetIfNeeded();
        }

        public void LoadFontAssetIfNeeded()
        {
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

        private void OnEnable()
        {
            RefreshUI();
        }

        private void Update()
        {
            bool isGamepad = PlayerInputManager_Alpha.Instance != null && PlayerInputManager_Alpha.Instance.IsUsingGamepad;
            if (isGamepad != lastGamepadState)
            {
                lastGamepadState = isGamepad;
                UpdateDetailView();
            }
        }

        /// <summary>
        /// ライブラリUI全体を構築（Modern Botanical UI / Herbarium 幾何学標本スタイル）
        /// </summary>
        public void BuildLayoutIfNeeded()
        {
            if (subTabContainer != null) return;
            LoadFontAssetIfNeeded();

            // 1. 全体背景（深森の暗緑・墨黒・すりガラス調）
            Image bg = GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color = ColorBaseInk;

            RectTransform mainRT = GetComponent<RectTransform>();
            if (mainRT != null)
            {
                // 四隅に植物標本プレートの幾何学L字コーナーオーナメントを配置
                AddCornerAccents(mainRT, ColorBrassMuted, 20f, 1.5f);
            }

            // 2. メインヘッダー
            CreateHeader();

            // 3. メイン横並びレイアウト（左: サブタブ / 右: コンテンツ領域）
            GameObject bodyGO = new GameObject("LibraryBody", typeof(RectTransform));
            bodyGO.transform.SetParent(transform, false);
            RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = new Vector2(25f, 25f);
            bodyRT.offsetMax = new Vector2(-25f, -65f); // ヘッダー分空ける

            // 4. 左側サブタブコンテナ（幅: 195px）
            GameObject subTabGO = new GameObject("SubTabContainer", typeof(RectTransform));
            subTabGO.transform.SetParent(bodyGO.transform, false);
            subTabContainer = subTabGO.GetComponent<RectTransform>();
            subTabContainer.anchorMin = new Vector2(0f, 0f);
            subTabContainer.anchorMax = new Vector2(0f, 1f);
            subTabContainer.pivot = new Vector2(0f, 0.5f);
            subTabContainer.sizeDelta = new Vector2(195f, 0f);
            subTabContainer.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup subVlg = subTabGO.AddComponent<VerticalLayoutGroup>();
            subVlg.spacing = 8f;
            subVlg.childControlWidth = true;
            subVlg.childControlHeight = false;
            subVlg.childForceExpandWidth = true;
            subVlg.childForceExpandHeight = false;

            // サブタブボタン群を作成（世界観に合わせた条項目録名）
            CreateSubTabButton("草花教本 (Tutorial)", SubTabType.Tutorial);
            CreateSubTabButton("神格標本 (Divinity)", SubTabType.Enemy);
            CreateSubTabButton("兵装種子 (Seeds)", SubTabType.Bullet);
            CreateSubTabButton("条項裁定 (Divine Law)", SubTabType.Effect);

            // 5. 右側コンテンツ領域（幅: 残り全域）
            GameObject contentAreaGO = new GameObject("ContentArea", typeof(RectTransform));
            contentAreaGO.transform.SetParent(bodyGO.transform, false);
            RectTransform contentAreaRT = contentAreaGO.GetComponent<RectTransform>();
            contentAreaRT.anchorMin = new Vector2(0f, 0f);
            contentAreaRT.anchorMax = new Vector2(1f, 1f);
            contentAreaRT.offsetMin = new Vector2(215f, 0f);
            contentAreaRT.offsetMax = Vector2.zero;

            // 5-A. チュートリアル専用ビュー（2カラム構造）
            BuildTutorialView(contentAreaRT);

            // 5-B. 汎用ビュー（敵、弾、エフェクトなどの未実装・プレビュー用）
            BuildGenericSubView(contentAreaRT);

            SelectSubTab(SubTabType.Tutorial);
        }

        private void CreateHeader()
        {
            GameObject headerGO = new GameObject("Header", typeof(RectTransform));
            headerGO.transform.SetParent(transform, false);
            RectTransform headerRT = headerGO.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 55f);
            headerRT.anchoredPosition = new Vector2(0f, -10f);

            // タイトル（装甲明朝 ＋ アンティーク真鍮）
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform));
            titleGO.transform.SetParent(headerGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0f);
            titleRT.anchorMax = new Vector2(0.6f, 1f);
            titleRT.offsetMin = new Vector2(30f, 0f);
            titleRT.offsetMax = Vector2.zero;
            var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) titleTxt.font = customFontAsset;
            titleTxt.text = "<size=65%><color=#8c7552>HERBARIUM SPECIMEN ARCHIVE // </color></size>神格草花条項目録";
            titleTxt.fontSize = 22f;
            titleTxt.color = ColorBrassGold;
            titleTxt.alignment = TextAlignmentOptions.Left;

            // デバイスステータステキスト（右上）
            GameObject devGO = new GameObject("DeviceStatusText", typeof(RectTransform));
            devGO.transform.SetParent(headerGO.transform, false);
            RectTransform devRT = devGO.GetComponent<RectTransform>();
            devRT.anchorMin = new Vector2(0.6f, 0f);
            devRT.anchorMax = new Vector2(1f, 1f);
            devRT.offsetMin = Vector2.zero;
            devRT.offsetMax = new Vector2(-30f, 0f);
            inputDeviceStatusText = devGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) inputDeviceStatusText.font = customFontAsset;
            inputDeviceStatusText.fontSize = 14f;
            inputDeviceStatusText.color = ColorIvoryMuted;
            inputDeviceStatusText.alignment = TextAlignmentOptions.Right;

            // 区切り線（細い真鍮ライン ＋ 中央ノード）
            GameObject lineGO = new GameObject("HeaderLine", typeof(RectTransform));
            lineGO.transform.SetParent(transform, false);
            RectTransform lineRT = lineGO.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0f, 1f);
            lineRT.anchorMax = new Vector2(1f, 1f);
            lineRT.pivot = new Vector2(0.5f, 1f);
            lineRT.sizeDelta = new Vector2(-50f, 1.5f);
            lineRT.anchoredPosition = new Vector2(0f, -60f);
            Image lineImg = lineGO.AddComponent<Image>();
            lineImg.color = ColorBrassMuted;

            // 中央幾何学◇ノード
            GameObject diamondGO = new GameObject("DiamondNode", typeof(RectTransform));
            diamondGO.transform.SetParent(lineGO.transform, false);
            RectTransform dRT = diamondGO.GetComponent<RectTransform>();
            dRT.sizeDelta = new Vector2(8f, 8f);
            dRT.anchoredPosition = Vector2.zero;
            dRT.localRotation = Quaternion.Euler(0, 0, 45f);
            Image dImg = diamondGO.AddComponent<Image>();
            dImg.color = ColorBrassGold;
        }

        private void CreateSubTabButton(string tabName, SubTabType type)
        {
            GameObject btnGO = new GameObject($"SubTab_{type}", typeof(RectTransform));
            btnGO.transform.SetParent(subTabContainer, false);
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(195f, 46f);

            Image bg = btnGO.AddComponent<Image>();
            bg.color = ColorDarkMoss;

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.1f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            btn.onClick.AddListener(() => SelectSubTab(type));

            // 左端アクティブインジケーター（真鍮の縦線）
            GameObject barGO = new GameObject("IndicatorBar", typeof(RectTransform));
            barGO.transform.SetParent(btnGO.transform, false);
            RectTransform barRT = barGO.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(0f, 1f);
            barRT.sizeDelta = new Vector2(3f, 0f);
            barRT.anchoredPosition = new Vector2(1.5f, 0f);
            Image barImg = barGO.AddComponent<Image>();
            barImg.color = ColorBrassGold;
            barImg.gameObject.SetActive(false);
            subTabIndicatorBars.Add(barImg);

            // テキスト
            GameObject txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(16f, 0f);
            txtRT.offsetMax = new Vector2(-12f, 0f);

            TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) tmp.font = customFontAsset;
            tmp.text = tabName;
            tmp.fontSize = 15f;
            tmp.color = ColorIvoryMuted;
            tmp.alignment = TextAlignmentOptions.Left;

            subTabButtons.Add(btn);
            subTabBgImages.Add(bg);
            subTabTextList.Add(tmp);
        }

        private void BuildTutorialView(RectTransform parent)
        {
            tutorialView = new GameObject("TutorialView", typeof(RectTransform));
            tutorialView.transform.SetParent(parent, false);
            RectTransform tvRT = tutorialView.GetComponent<RectTransform>();
            tvRT.anchorMin = Vector2.zero;
            tvRT.anchorMax = Vector2.one;
            tvRT.sizeDelta = Vector2.zero;

            // 1. 左カラム: チュートリアル目録リスト（幅: 320px）
            GameObject listScrollGO = new GameObject("ListScrollView", typeof(RectTransform));
            listScrollGO.transform.SetParent(tutorialView.transform, false);
            RectTransform listScrollRT = listScrollGO.GetComponent<RectTransform>();
            listScrollRT.anchorMin = new Vector2(0f, 0f);
            listScrollRT.anchorMax = new Vector2(0f, 1f);
            listScrollRT.pivot = new Vector2(0f, 0.5f);
            listScrollRT.sizeDelta = new Vector2(320f, 0f);

            Image listBg = listScrollGO.AddComponent<Image>();
            listBg.color = ColorBaseList;

            // リスト枠の幾何学コーナー
            AddCornerAccents(listScrollRT, ColorBrassMuted, 12f, 1f);

            ScrollRect sr = listScrollGO.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 25f;

            // Viewport
            GameObject vpGO = new GameObject("Viewport", typeof(RectTransform));
            vpGO.transform.SetParent(listScrollGO.transform, false);
            RectTransform vpRT = vpGO.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = new Vector2(6f, 6f);
            vpRT.offsetMax = new Vector2(-6f, -6f);
            vpGO.AddComponent<RectMask2D>();
            sr.viewport = vpRT;

            // Content
            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(vpGO.transform, false);
            tutorialListContent = contentGO.GetComponent<RectTransform>();
            tutorialListContent.anchorMin = new Vector2(0f, 1f);
            tutorialListContent.anchorMax = new Vector2(1f, 1f);
            tutorialListContent.pivot = new Vector2(0.5f, 1f);
            tutorialListContent.sizeDelta = new Vector2(0f, 500f);

            VerticalLayoutGroup listVlg = contentGO.AddComponent<VerticalLayoutGroup>();
            listVlg.spacing = 5f;
            listVlg.childControlWidth = true;
            listVlg.childControlHeight = false;
            listVlg.childForceExpandWidth = true;
            listVlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = tutorialListContent;

            // 2. 右カラム: 詳細プレビュー標本プレート（幅: 残り領域）
            GameObject cardGO = new GameObject("DetailCard", typeof(RectTransform));
            cardGO.transform.SetParent(tutorialView.transform, false);
            RectTransform cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0f, 0f);
            cardRT.anchorMax = new Vector2(1f, 1f);
            cardRT.offsetMin = new Vector2(335f, 0f);
            cardRT.offsetMax = Vector2.zero;

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = ColorBaseCard;

            // 標本プレートの幾何学L字コーナー（繊細な真鍮ライン）
            AddCornerAccents(cardRT, ColorBrassGold, 16f, 1.5f);

            // カード内レイアウト
            VerticalLayoutGroup cardVlg = cardGO.AddComponent<VerticalLayoutGroup>();
            cardVlg.padding = new RectOffset(35, 35, 30, 30);
            cardVlg.spacing = 16f;
            cardVlg.childControlWidth = true;
            cardVlg.childControlHeight = false;
            cardVlg.childForceExpandWidth = true;
            cardVlg.childForceExpandHeight = false;

            // カテゴリバッジ（蓮・翡翠色）
            GameObject catGO = new GameObject("DetailCategory", typeof(RectTransform));
            catGO.transform.SetParent(cardGO.transform, false);
            detailCategoryText = catGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) detailCategoryText.font = customFontAsset;
            detailCategoryText.fontSize = 13f;
            detailCategoryText.color = ColorLotusJade;

            // タイトル（生成り白・装甲明朝）
            GameObject titGO = new GameObject("DetailTitle", typeof(RectTransform));
            titGO.transform.SetParent(cardGO.transform, false);
            detailTitleText = titGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) detailTitleText.font = customFontAsset;
            detailTitleText.fontSize = 24f;
            detailTitleText.color = ColorIvoryWhite;

            // 区切り線（真鍮ゴールド極細線）
            GameObject sepGO = new GameObject("Separator", typeof(RectTransform));
            sepGO.transform.SetParent(cardGO.transform, false);
            RectTransform sepRT = sepGO.GetComponent<RectTransform>();
            sepRT.sizeDelta = new Vector2(0f, 1.5f);
            Image sepImg = sepGO.AddComponent<Image>();
            sepImg.color = ColorBrassMuted;

            // 概要・本文（生成り白・可読性重視）
            GameObject bodyTxtGO = new GameObject("DetailBody", typeof(RectTransform));
            bodyTxtGO.transform.SetParent(cardGO.transform, false);
            detailBodyText = bodyTxtGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) detailBodyText.font = customFontAsset;
            detailBodyText.fontSize = 17f;
            detailBodyText.lineSpacing = 16f;
            detailBodyText.color = ColorIvoryWhite;

            // 操作対応枠（深森苔色背景 ＋ 幾何学枠線 ＋ 向日葵ゴールドキー）
            GameObject scBoxGO = new GameObject("ShortcutBox", typeof(RectTransform));
            scBoxGO.transform.SetParent(cardGO.transform, false);
            RectTransform scRT = scBoxGO.GetComponent<RectTransform>();
            Image scBg = scBoxGO.AddComponent<Image>();
            scBg.color = ColorDarkMoss;
            AddCornerAccents(scRT, ColorBrassMuted, 10f, 1f);

            VerticalLayoutGroup scVlg = scBoxGO.AddComponent<VerticalLayoutGroup>();
            scVlg.padding = new RectOffset(16, 16, 12, 12);
            scVlg.childControlWidth = true;
            scVlg.childControlHeight = true;

            detailShortcutText = new GameObject("ShortcutText", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            detailShortcutText.transform.SetParent(scBoxGO.transform, false);
            if (customFontAsset != null) detailShortcutText.font = customFontAsset;
            detailShortcutText.fontSize = 15f;
            detailShortcutText.color = ColorSunflowerGold;

            // ワンポイントTips枠（深森翡翠背景 ＋ 若葉色アクセント）
            GameObject tipsBoxGO = new GameObject("TipsBox", typeof(RectTransform));
            tipsBoxGO.transform.SetParent(cardGO.transform, false);
            RectTransform tipsRT = tipsBoxGO.GetComponent<RectTransform>();
            Image tipsBg = tipsBoxGO.AddComponent<Image>();
            tipsBg.color = new Color(0.06f, 0.12f, 0.09f, 0.9f);
            AddCornerAccents(tipsRT, ColorLotusJade, 10f, 1f);

            VerticalLayoutGroup tipsVlg = tipsBoxGO.AddComponent<VerticalLayoutGroup>();
            tipsVlg.padding = new RectOffset(16, 16, 12, 12);
            tipsVlg.childControlWidth = true;
            tipsVlg.childControlHeight = true;

            detailTipsText = new GameObject("TipsText", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            detailTipsText.transform.SetParent(tipsBoxGO.transform, false);
            if (customFontAsset != null) detailTipsText.font = customFontAsset;
            detailTipsText.fontSize = 15f;
            detailTipsText.color = ColorSproutLeaf;
        }

        private void BuildGenericSubView(RectTransform parent)
        {
            genericSubView = new GameObject("GenericSubView", typeof(RectTransform));
            genericSubView.transform.SetParent(parent, false);
            RectTransform gsRT = genericSubView.GetComponent<RectTransform>();
            gsRT.anchorMin = Vector2.zero;
            gsRT.anchorMax = Vector2.one;
            gsRT.sizeDelta = Vector2.zero;

            Image bg = genericSubView.AddComponent<Image>();
            bg.color = ColorBaseCard;
            AddCornerAccents(gsRT, ColorBrassGold, 18f, 1.5f);

            VerticalLayoutGroup vlg = genericSubView.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(50, 50, 60, 60);
            vlg.spacing = 20f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // サブタイトル（真鍮ゴールド）
            GameObject titGO = new GameObject("GenericTitle", typeof(RectTransform));
            titGO.transform.SetParent(genericSubView.transform, false);
            genericViewTitleText = titGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) genericViewTitleText.font = customFontAsset;
            genericViewTitleText.fontSize = 24f;
            genericViewTitleText.color = ColorBrassGold;

            // 説明文（生成り白）
            GameObject descGO = new GameObject("GenericDesc", typeof(RectTransform));
            descGO.transform.SetParent(genericSubView.transform, false);
            genericViewDescText = descGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) genericViewDescText.font = customFontAsset;
            genericViewDescText.fontSize = 17f;
            genericViewDescText.lineSpacing = 16f;
            genericViewDescText.color = ColorIvoryWhite;

            genericSubView.SetActive(false);
        }

        public void SelectSubTab(SubTabType type)
        {
            currentSubTab = type;

            for (int i = 0; i < subTabButtons.Count; i++)
            {
                bool isSelected = (i == (int)type);
                if (subTabBgImages[i] != null)
                {
                    subTabBgImages[i].color = isSelected ? ColorMossSlate : ColorDarkMoss;
                }
                if (subTabTextList[i] != null)
                {
                    subTabTextList[i].color = isSelected ? ColorBrassGold : ColorIvoryMuted;
                }
                if (i < subTabIndicatorBars.Count && subTabIndicatorBars[i] != null)
                {
                    subTabIndicatorBars[i].gameObject.SetActive(isSelected);
                }
            }

            if (type == SubTabType.Tutorial)
            {
                if (tutorialView != null) tutorialView.SetActive(true);
                if (genericSubView != null) genericSubView.SetActive(false);
                PopulateTutorialList();
                UpdateDetailView();
            }
            else
            {
                if (tutorialView != null) tutorialView.SetActive(false);
                if (genericSubView != null) genericSubView.SetActive(true);

                switch (type)
                {
                    case SubTabType.Enemy:
                        genericViewTitleText.text = "◆ 神格標本図録 (DIVINITY & FLORA) ◆";
                        genericViewDescText.text = "【条項未採取 / SPECIMEN UNREGISTERED】\n\n遭遇した神格および敵性存在の生態記録、花弁ドロップ、撃破条項、行動様式を記録・保存する標本アーカイブです。\n今後のアップデートで順次解錠されます。";
                        break;
                    case SubTabType.Bullet:
                        genericViewTitleText.text = "◆ 兵装種子目録 (SEED & BULLETS) ◆";
                        genericViewDescText.text = "【条項未採取 / SPECIMEN UNREGISTERED】\n\n自機および神格が射出する弾丸種子の弾速・拡散率・貫通力・ダメージ係数などの弾道データを閲覧・解析する目録です。\n今後のアップデートで順次解錠されます。";
                        break;
                    case SubTabType.Effect:
                        genericViewTitleText.text = "◆ 条項裁定効果 (DIVINE LAW & MATRIX) ◆";
                        genericViewDescText.text = "【条項未採取 / SPECIMEN UNREGISTERED】\n\n兵装やパーツの共鳴によって発現するシナジー効果、花弁セットボーナスの裁定条項一覧を確認できるシステムです。\n今後のアップデートで順次解錠されます。";
                        break;
                }
            }
        }

        public void RefreshUI()
        {
            BuildLayoutIfNeeded();
            if (currentSubTab == SubTabType.Tutorial)
            {
                PopulateTutorialList();
                UpdateDetailView();
            }
        }

        private void PopulateTutorialList()
        {
            if (tutorialListContent == null || LibraryManager_Alpha.Instance == null) return;

            foreach (Transform child in tutorialListContent)
            {
                Destroy(child.gameObject);
            }
            tutorialListButtons.Clear();

            var database = LibraryManager_Alpha.Instance.tutorialDatabase;

            foreach (var item in database)
            {
                bool isUnlocked = LibraryManager_Alpha.Instance.IsItemUnlocked(item.id);

                GameObject btnGO = new GameObject($"Item_{item.id}", typeof(RectTransform));
                btnGO.transform.SetParent(tutorialListContent, false);
                RectTransform btnRT = btnGO.GetComponent<RectTransform>();
                btnRT.sizeDelta = new Vector2(0f, 42f);

                Image btnBg = btnGO.AddComponent<Image>();
                bool isCurrentSelected = (item.id == selectedTutorialId);
                btnBg.color = isCurrentSelected ? ColorMossSlate : ColorDarkMoss;

                Button btn = btnGO.AddComponent<Button>();

                // ボタンテキスト
                GameObject txtGO = new GameObject("Text", typeof(RectTransform));
                txtGO.transform.SetParent(btnGO.transform, false);
                RectTransform txtRT = txtGO.GetComponent<RectTransform>();
                txtRT.anchorMin = Vector2.zero;
                txtRT.anchorMax = Vector2.one;
                txtRT.offsetMin = new Vector2(14f, 0f);
                txtRT.offsetMax = new Vector2(-14f, 0f);

                TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
                if (customFontAsset != null) tmp.font = customFontAsset;
                tmp.fontSize = 15f;
                tmp.alignment = TextAlignmentOptions.Left;

                if (isUnlocked)
                {
                    tmp.text = isCurrentSelected ? $"◆ {item.title}" : $"   {item.title}";
                    tmp.color = isCurrentSelected ? ColorIvoryWhite : ColorIvoryMuted;
                    string capturedId = item.id;
                    btn.onClick.AddListener(() => OnSelectTutorialItem(capturedId));
                }
                else
                {
                    tmp.text = "   ？？？ [ 条項未採取 ]";
                    tmp.color = lockedItemColor;
                    btn.interactable = false;
                }

                tutorialListButtons[item.id] = btn;
            }
        }

        private void OnSelectTutorialItem(string id)
        {
            selectedTutorialId = id;
            PopulateTutorialList();
            UpdateDetailView();
        }

        private void UpdateDetailView()
        {
            if (LibraryManager_Alpha.Instance == null) return;

            bool isGamepad = PlayerInputManager_Alpha.Instance != null && PlayerInputManager_Alpha.Instance.IsUsingGamepad;
            if (inputDeviceStatusText != null)
            {
                inputDeviceStatusText.text = isGamepad ? "操作系: [ 操縦桿 / GAMEPAD ]" : "操作系: [ 鍵盤・鼠 / KEYBOARD ]";
            }

            var item = LibraryManager_Alpha.Instance.GetTutorialData(selectedTutorialId);
            if (item == null) return;

            bool isUnlocked = LibraryManager_Alpha.Instance.IsItemUnlocked(item.id);

            if (detailCategoryText != null) detailCategoryText.text = $"[ 神裁条項 / {item.category} ]";
            if (detailTitleText != null) detailTitleText.text = isUnlocked ? item.title : "？？？ [ 未遭遇 ]";

            if (detailBodyText != null)
            {
                if (isUnlocked)
                {
                    detailBodyText.text = isGamepad ? item.gamepadDesc : item.keyboardDesc;
                }
                else
                {
                    detailBodyText.text = "この条項はまだゲーム内で遭遇していません。\n草花たちとの交戦を進めることで解錠されます。";
                }
            }

            if (detailShortcutText != null)
            {
                detailShortcutText.text = isUnlocked ? $"【操作条項】 {item.shortcutHint}" : "【操作条項】 ？？？";
            }

            if (detailTipsText != null)
            {
                detailTipsText.text = isUnlocked ? $"◆ 草花伝承・戦術知見: {item.tips}" : "◆ 草花伝承・戦術知見: ？？？";
            }
        }

        /// <summary>
        /// アールヌーヴォー×幾何学の細い真鍮コーナー（L字フレーム）を四隅に生成するヘルパー
        /// </summary>
        private static void AddCornerAccents(RectTransform targetParent, Color color, float size = 14f, float thickness = 1.5f)
        {
            if (targetParent == null) return;

            // 既存のコーナーがあれば削除
            for (int i = targetParent.childCount - 1; i >= 0; i--)
            {
                var c = targetParent.GetChild(i);
                if (c.name.StartsWith("Corner_"))
                {
                    Destroy(c.gameObject);
                }
            }

            CreateSingleCorner(targetParent, "Corner_TL", new Vector2(0f, 1f), new Vector2(1f, -1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_TR", new Vector2(1f, 1f), new Vector2(-1f, -1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BL", new Vector2(0f, 0f), new Vector2(1f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BR", new Vector2(1f, 0f), new Vector2(-1f, 1f), color, size, thickness);
        }

        private static void CreateSingleCorner(RectTransform parent, string name, Vector2 anchor, Vector2 direction, Color color, float size, float thickness)
        {
            GameObject cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            RectTransform crt = cornerGO.GetComponent<RectTransform>();
            crt.anchorMin = anchor;
            crt.anchorMax = anchor;
            crt.pivot = anchor;
            crt.sizeDelta = new Vector2(size, size);
            crt.anchoredPosition = Vector2.zero;

            // 水平バー
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

            // 垂直バー
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
