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
    /// ポーズ画面の「ライブラリ（旧Control）」タブ内に表示される、
    /// 左横型サブタブ（チュートリアル、敵、弾、エフェクト）と詳細閲覧UI
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

        [Header("Colors")]
        public Color activeSubTabColor = new Color(0.2f, 0.85f, 0.8f, 1f);
        public Color inactiveSubTabColor = new Color(0.6f, 0.65f, 0.7f, 1f);
        public Color activeSubTabBgColor = new Color(0.15f, 0.25f, 0.35f, 0.9f);
        public Color inactiveSubTabBgColor = new Color(0.08f, 0.1f, 0.14f, 0.8f);

        public Color selectedItemColor = new Color(0.25f, 0.85f, 0.6f, 1f);
        public Color normalItemColor = Color.white;
        public Color lockedItemColor = new Color(0.45f, 0.45f, 0.5f, 0.8f);

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
            // 操作デバイス（キーボード vs パッド）の変化を監視して詳細文面をリアルタイム更新
            bool isGamepad = PlayerInputManager_Alpha.Instance != null && PlayerInputManager_Alpha.Instance.IsUsingGamepad;
            if (isGamepad != lastGamepadState)
            {
                lastGamepadState = isGamepad;
                UpdateDetailView();
            }
        }

        /// <summary>
        /// ライブラリUI全体を構築（まだUIが生成されていない場合）
        /// </summary>
        public void BuildLayoutIfNeeded()
        {
            if (subTabContainer != null) return;
            LoadFontAssetIfNeeded();

            // 全体背景
            Image bg = GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.07f, 0.1f, 0.96f);

            // メインヘッダー
            CreateHeader();

            // メイン横並びレイアウト（左: サブタブ / 右: コンテンツ領域）
            GameObject bodyGO = new GameObject("LibraryBody", typeof(RectTransform));
            bodyGO.transform.SetParent(transform, false);
            RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = new Vector2(25f, 25f);
            bodyRT.offsetMax = new Vector2(-25f, -65f); // ヘッダー分空ける

            // 1. 左側サブタブコンテナ（幅: 190px）
            GameObject subTabGO = new GameObject("SubTabContainer", typeof(RectTransform));
            subTabGO.transform.SetParent(bodyGO.transform, false);
            subTabContainer = subTabGO.GetComponent<RectTransform>();
            subTabContainer.anchorMin = new Vector2(0f, 0f);
            subTabContainer.anchorMax = new Vector2(0f, 1f);
            subTabContainer.pivot = new Vector2(0f, 0.5f);
            subTabContainer.sizeDelta = new Vector2(190f, 0f);
            subTabContainer.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup subVlg = subTabGO.AddComponent<VerticalLayoutGroup>();
            subVlg.spacing = 10f;
            subVlg.childControlWidth = true;
            subVlg.childControlHeight = false;
            subVlg.childForceExpandWidth = true;
            subVlg.childForceExpandHeight = false;

            // サブタブボタン群を作成
            CreateSubTabButton("チュートリアル", SubTabType.Tutorial);
            CreateSubTabButton("敵 (Enemies)", SubTabType.Enemy);
            CreateSubTabButton("弾 (Bullets)", SubTabType.Bullet);
            CreateSubTabButton("エフェクト (Matrix)", SubTabType.Effect);

            // 2. 右側コンテンツ領域（幅: 残り全域）
            GameObject contentAreaGO = new GameObject("ContentArea", typeof(RectTransform));
            contentAreaGO.transform.SetParent(bodyGO.transform, false);
            RectTransform contentAreaRT = contentAreaGO.GetComponent<RectTransform>();
            contentAreaRT.anchorMin = new Vector2(0f, 0f);
            contentAreaRT.anchorMax = new Vector2(1f, 1f);
            contentAreaRT.offsetMin = new Vector2(210f, 0f);
            contentAreaRT.offsetMax = Vector2.zero;

            // 2-A. チュートリアル専用ビュー（2カラム構造）
            BuildTutorialView(contentAreaRT);

            // 2-B. 汎用ビュー（敵、弾、エフェクトなどの未実装・プレビュー用）
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

            // タイトル
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform));
            titleGO.transform.SetParent(headerGO.transform, false);
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.offsetMin = new Vector2(30f, 0f);
            titleRT.offsetMax = Vector2.zero;
            var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) titleTxt.font = customFontAsset;
            titleTxt.text = "◆ ARCHIVE & LIBRARY ◆";
            titleTxt.fontSize = 24f;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(0.35f, 0.9f, 0.85f, 1f);
            titleTxt.alignment = TextAlignmentOptions.Left;

            // デバイスステータステキスト（右上）
            GameObject devGO = new GameObject("DeviceStatusText", typeof(RectTransform));
            devGO.transform.SetParent(headerGO.transform, false);
            RectTransform devRT = devGO.GetComponent<RectTransform>();
            devRT.anchorMin = new Vector2(0.5f, 0f);
            devRT.anchorMax = new Vector2(1f, 1f);
            devRT.offsetMin = Vector2.zero;
            devRT.offsetMax = new Vector2(-30f, 0f);
            inputDeviceStatusText = devGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) inputDeviceStatusText.font = customFontAsset;
            inputDeviceStatusText.fontSize = 15f;
            inputDeviceStatusText.color = new Color(0.7f, 0.8f, 0.9f, 0.85f);
            inputDeviceStatusText.alignment = TextAlignmentOptions.Right;

            // 区切り線
            GameObject lineGO = new GameObject("HeaderLine", typeof(RectTransform));
            lineGO.transform.SetParent(transform, false);
            RectTransform lineRT = lineGO.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0f, 1f);
            lineRT.anchorMax = new Vector2(1f, 1f);
            lineRT.pivot = new Vector2(0.5f, 1f);
            lineRT.sizeDelta = new Vector2(-50f, 2f);
            lineRT.anchoredPosition = new Vector2(0f, -60f);
            Image lineImg = lineGO.AddComponent<Image>();
            lineImg.color = new Color(0.2f, 0.4f, 0.5f, 0.5f);
        }

        private void CreateSubTabButton(string tabName, SubTabType type)
        {
            GameObject btnGO = new GameObject($"SubTab_{type}", typeof(RectTransform));
            btnGO.transform.SetParent(subTabContainer, false);
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(190f, 48f);

            Image bg = btnGO.AddComponent<Image>();
            bg.color = inactiveSubTabBgColor;

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = cb;

            btn.onClick.AddListener(() => SelectSubTab(type));

            // テキスト
            GameObject txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(btnGO.transform, false);
            RectTransform txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(15f, 0f);
            txtRT.offsetMax = new Vector2(-15f, 0f);

            TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) tmp.font = customFontAsset;
            tmp.text = tabName;
            tmp.fontSize = 17f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = inactiveSubTabColor;
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

            // 1. 左カラム: チュートリアル一覧リスト（幅: 320px）
            GameObject listScrollGO = new GameObject("ListScrollView", typeof(RectTransform));
            listScrollGO.transform.SetParent(tutorialView.transform, false);
            RectTransform listScrollRT = listScrollGO.GetComponent<RectTransform>();
            listScrollRT.anchorMin = new Vector2(0f, 0f);
            listScrollRT.anchorMax = new Vector2(0f, 1f);
            listScrollRT.pivot = new Vector2(0f, 0.5f);
            listScrollRT.sizeDelta = new Vector2(320f, 0f);

            Image listBg = listScrollGO.AddComponent<Image>();
            listBg.color = new Color(0.04f, 0.06f, 0.09f, 0.9f);

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
            vpRT.offsetMin = new Vector2(5f, 5f);
            vpRT.offsetMax = new Vector2(-5f, -5f);
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
            listVlg.spacing = 6f;
            listVlg.childControlWidth = true;
            listVlg.childControlHeight = false;
            listVlg.childForceExpandWidth = true;
            listVlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = tutorialListContent;

            // 2. 右カラム: 詳細プレビューカード（幅: 残り領域）
            GameObject cardGO = new GameObject("DetailCard", typeof(RectTransform));
            cardGO.transform.SetParent(tutorialView.transform, false);
            RectTransform cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0f, 0f);
            cardRT.anchorMax = new Vector2(1f, 1f);
            cardRT.offsetMin = new Vector2(335f, 0f);
            cardRT.offsetMax = Vector2.zero;

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = new Color(0.07f, 0.09f, 0.13f, 0.95f);

            // カード内レイアウト
            VerticalLayoutGroup cardVlg = cardGO.AddComponent<VerticalLayoutGroup>();
            cardVlg.padding = new RectOffset(35, 35, 30, 30);
            cardVlg.spacing = 16f;
            cardVlg.childControlWidth = true;
            cardVlg.childControlHeight = false;
            cardVlg.childForceExpandWidth = true;
            cardVlg.childForceExpandHeight = false;

            // カテゴリバッジ
            GameObject catGO = new GameObject("DetailCategory", typeof(RectTransform));
            catGO.transform.SetParent(cardGO.transform, false);
            detailCategoryText = catGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) detailCategoryText.font = customFontAsset;
            detailCategoryText.fontSize = 14f;
            detailCategoryText.fontStyle = FontStyles.Bold;
            detailCategoryText.color = new Color(0.35f, 0.9f, 0.85f, 1f);

            // タイトル
            GameObject titGO = new GameObject("DetailTitle", typeof(RectTransform));
            titGO.transform.SetParent(cardGO.transform, false);
            detailTitleText = titGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) detailTitleText.font = customFontAsset;
            detailTitleText.fontSize = 24f;
            detailTitleText.fontStyle = FontStyles.Bold;
            detailTitleText.color = Color.white;

            // 区切り線
            GameObject sepGO = new GameObject("Separator", typeof(RectTransform));
            sepGO.transform.SetParent(cardGO.transform, false);
            RectTransform sepRT = sepGO.GetComponent<RectTransform>();
            sepRT.sizeDelta = new Vector2(0f, 2f);
            Image sepImg = sepGO.AddComponent<Image>();
            sepImg.color = new Color(0.25f, 0.4f, 0.5f, 0.4f);

            // 概要・本文
            GameObject bodyTxtGO = new GameObject("DetailBody", typeof(RectTransform));
            bodyTxtGO.transform.SetParent(cardGO.transform, false);
            detailBodyText = bodyTxtGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) detailBodyText.font = customFontAsset;
            detailBodyText.fontSize = 18f;
            detailBodyText.lineSpacing = 15f;
            detailBodyText.color = new Color(0.92f, 0.94f, 0.96f, 1f);

            // 操作対応ショートカット枠
            GameObject scBoxGO = new GameObject("ShortcutBox", typeof(RectTransform));
            scBoxGO.transform.SetParent(cardGO.transform, false);
            Image scBg = scBoxGO.AddComponent<Image>();
            scBg.color = new Color(0.12f, 0.16f, 0.22f, 0.9f);
            VerticalLayoutGroup scVlg = scBoxGO.AddComponent<VerticalLayoutGroup>();
            scVlg.padding = new RectOffset(16, 16, 12, 12);
            scVlg.childControlWidth = true;
            scVlg.childControlHeight = true;

            detailShortcutText = new GameObject("ShortcutText", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            detailShortcutText.transform.SetParent(scBoxGO.transform, false);
            if (customFontAsset != null) detailShortcutText.font = customFontAsset;
            detailShortcutText.fontSize = 15f;
            detailShortcutText.color = new Color(1f, 0.88f, 0.45f, 1f);

            // ワンポイントTips枠
            GameObject tipsBoxGO = new GameObject("TipsBox", typeof(RectTransform));
            tipsBoxGO.transform.SetParent(cardGO.transform, false);
            Image tipsBg = tipsBoxGO.AddComponent<Image>();
            tipsBg.color = new Color(0.1f, 0.2f, 0.18f, 0.85f);
            VerticalLayoutGroup tipsVlg = tipsBoxGO.AddComponent<VerticalLayoutGroup>();
            tipsVlg.padding = new RectOffset(16, 16, 12, 12);
            tipsVlg.childControlWidth = true;
            tipsVlg.childControlHeight = true;

            detailTipsText = new GameObject("TipsText", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            detailTipsText.transform.SetParent(tipsBoxGO.transform, false);
            if (customFontAsset != null) detailTipsText.font = customFontAsset;
            detailTipsText.fontSize = 15f;
            detailTipsText.color = new Color(0.45f, 0.95f, 0.7f, 1f);
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
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

            VerticalLayoutGroup vlg = genericSubView.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(50, 50, 60, 60);
            vlg.spacing = 20f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // サブタイトル
            GameObject titGO = new GameObject("GenericTitle", typeof(RectTransform));
            titGO.transform.SetParent(genericSubView.transform, false);
            genericViewTitleText = titGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) genericViewTitleText.font = customFontAsset;
            genericViewTitleText.fontSize = 24f;
            genericViewTitleText.fontStyle = FontStyles.Bold;
            genericViewTitleText.color = new Color(0.35f, 0.85f, 0.95f, 1f);

            // 説明文
            GameObject descGO = new GameObject("GenericDesc", typeof(RectTransform));
            descGO.transform.SetParent(genericSubView.transform, false);
            genericViewDescText = descGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) genericViewDescText.font = customFontAsset;
            genericViewDescText.fontSize = 18f;
            genericViewDescText.lineSpacing = 14f;
            genericViewDescText.color = new Color(0.8f, 0.85f, 0.9f, 0.9f);

            genericSubView.SetActive(false);
        }

        public void SelectSubTab(SubTabType type)
        {
            currentSubTab = type;

            for (int i = 0; i < subTabButtons.Count; i++)
            {
                bool isSelected = (i == (int)type);
                if (subTabBgImages[i] != null) subTabBgImages[i].color = isSelected ? activeSubTabBgColor : inactiveSubTabBgColor;
                if (subTabTextList[i] != null) subTabTextList[i].color = isSelected ? activeSubTabColor : inactiveSubTabColor;
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
                        genericViewTitleText.text = "◆ 敵データ図鑑 (ENEMY ARCHIVE) ◆";
                        genericViewDescText.text = "【解析中 / COMING SOON】\n\n遭遇した敵の戦闘記録、ドロップ情報、撃破数、行動パターンを記録するアーカイブです。\n今後のアップデートで実装予定です。";
                        break;
                    case SubTabType.Bullet:
                        genericViewTitleText.text = "◆ 弾丸データベース (BULLET ARCHIVE) ◆";
                        genericViewDescText.text = "【解析中 / COMING SOON】\n\n自機および敵弾の弾速・拡散率・貫通力・ダメージ係数などの弾道データを一覧確認できる機能です。\n今後のアップデートで実装予定です。";
                        break;
                    case SubTabType.Effect:
                        genericViewTitleText.text = "◆ マトリクス効果 (EFFECT MATRIX) ◆";
                        genericViewDescText.text = "【解析中 / COMING SOON】\n\n装備やパーツの組み合わせによって生じるシナジー効果、セットボーナスの効果一覧を確認できるシステムです。\n今後のアップデートで実装予定です。";
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

            // 既存のボタンをクリア
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
                btnRT.sizeDelta = new Vector2(0f, 44f);

                Image btnBg = btnGO.AddComponent<Image>();
                bool isCurrentSelected = (item.id == selectedTutorialId);
                btnBg.color = isCurrentSelected ? new Color(0.18f, 0.3f, 0.4f, 0.95f) : new Color(0.08f, 0.1f, 0.15f, 0.85f);

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
                tmp.fontSize = 16f;
                tmp.alignment = TextAlignmentOptions.Left;

                if (isUnlocked)
                {
                    tmp.text = item.title;
                    tmp.color = isCurrentSelected ? selectedItemColor : normalItemColor;
                    string capturedId = item.id;
                    btn.onClick.AddListener(() => OnSelectTutorialItem(capturedId));
                }
                else
                {
                    tmp.text = "？？？ (未遭遇)";
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
                inputDeviceStatusText.text = isGamepad ? "操作系: [ ゲームパッド ]" : "操作系: [ キーボード / マウス ]";
            }

            var item = LibraryManager_Alpha.Instance.GetTutorialData(selectedTutorialId);
            if (item == null) return;

            bool isUnlocked = LibraryManager_Alpha.Instance.IsItemUnlocked(item.id);

            if (detailCategoryText != null) detailCategoryText.text = $"◆ {item.category} ◆";
            if (detailTitleText != null) detailTitleText.text = isUnlocked ? item.title : "？？？";

            if (detailBodyText != null)
            {
                if (isUnlocked)
                {
                    detailBodyText.text = isGamepad ? item.gamepadDesc : item.keyboardDesc;
                }
                else
                {
                    detailBodyText.text = "この項目はまだゲーム内で遭遇していません。\nゲームを進行することで解放されます。";
                }
            }

            if (detailShortcutText != null)
            {
                detailShortcutText.text = isUnlocked ? $"【対応操作】 {item.shortcutHint}" : "【対応操作】 ？？？";
            }

            if (detailTipsText != null)
            {
                detailTipsText.text = isUnlocked ? $"💡 ワンポイント: {item.tips}" : "💡 ワンポイント: ？？？";
            }
        }
    }
}
