using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Alpha.Managers; // PlayerInputManager_Alpha, GameSettingsManager_Alpha
using Alpha.Audio;

namespace Alpha.UI
{
    public class PauseMenuManager_Alpha : MonoBehaviour
    {
        private static PauseMenuManager_Alpha _instance;
        public static PauseMenuManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PauseMenuManager_Alpha>();
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        public enum TabType
        {
            Equipment,
            Settings,
            QuitToTitle,
            Controls,
            Custom
        }

        [Serializable]
        public class PauseTabEntry
        {
            public TabType tabType;
            public string tabName = "Tab";
            public Button tabButton;
            public GameObject tabContentPanel;
            public KeyCode directShortcutKey = KeyCode.None;
            public Action onTabOpened;
            public Action onTabClosed;
        }

        [Header("UI References")]
        public GameObject pauseMenuContainer;

        [Header("Legacy Direct Button Bindings (Optional)")]
        public Button equipmentTabButton;
        public Button settingsTabButton;
        public Button controlsTabButton;
        public Button quitTabButton;

        [Header("Legacy Panel Bindings (Optional)")]
        public InventoryUI_Alpha inventoryUI;
        public GameObject settingsPanel;
        public GameObject controlsPanel;
        public GameObject quitConfirmPanel;

        [Header("Extensible Tab Configuration")]
        [Tooltip("ポーズ画面のタブ一覧。インスペクターから自由に追加・並び替え可能です。")]
        public List<PauseTabEntry> tabs = new List<PauseTabEntry>();

        [Header("Font Settings")]
        [Tooltip("ポーズ画面で使用するフォント（未指定時はSoukouMincho SDFを自動ロード）")]
        public TMP_FontAsset customFontAsset;

        [Header("Audio Settings")]
        public AudioClip tabSwitchSEClip;
        [Range(0f, 1f)] public float tabSwitchSEVolume = 0.8f;

        [Header("Tab Visual Settings")]
        public Color activeTabColor = new Color(0.80f, 0.68f, 0.44f, 1.0f);   // アンティーク真鍮
        public Color inactiveTabColor = new Color(0.68f, 0.68f, 0.64f, 0.85f); // 淡い象牙色

        [Header("State (Read Only)")]
        public int currentTabIndex = 0;
        private bool isPaused = false;
        public bool IsPaused => isPaused;

        // Escキー連打・判定残りによる即時終了を防止するクールダウン
        private float lastToggleTime = -1f;
        private const float ToggleCooldown = 0.25f;


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
                    if (customFontAsset != null)
                    {
                        Debug.Log("[PauseMenuManager] Auto-loaded SoukouMincho SDF from: " + p);
                    }
                }
            }
#endif
        }

        public void ApplyFontToAllUI()
        {
            LoadFontAssetIfNeeded();
            if (customFontAsset == null) return;

            Transform targetRoot = (pauseMenuContainer != null) ? pauseMenuContainer.transform : transform;
            var allTexts = targetRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t != null) t.font = customFontAsset;
            }

            if (settingsPanel != null)
            {
                var sTexts = settingsPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in sTexts)
                {
                    if (t != null) t.font = customFontAsset;
                }
            }
            if (quitConfirmPanel != null)
            {
                var qTexts = quitConfirmPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in qTexts)
                {
                    if (t != null) t.font = customFontAsset;
                }
            }
        }
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            EnsureCanvasSortingOrder();
            LoadFontAssetIfNeeded();
            InitializeTabs();
        }

        private void Start()
        {
            EnsureCanvasSortingOrder();

            if (pauseMenuContainer != null)
            {
                pauseMenuContainer.SetActive(false);
            }

            if (inventoryUI == null)
            {
                inventoryUI = FindObjectOfType<InventoryUI_Alpha>();
                var equipTab = tabs.Find(t => t.tabType == TabType.Equipment);
                if (equipTab != null && inventoryUI != null)
                {
                    equipTab.tabContentPanel = inventoryUI.gameObject;
                }
            }

            // SEのデフォルトロード
            if (tabSwitchSEClip == null)
            {
                tabSwitchSEClip = Resources.Load<AudioClip>("SE/SE_WeaponSwitch_4");
                if (tabSwitchSEClip == null)
                {
                    tabSwitchSEClip = Resources.Load<AudioClip>("SE/ボタン切り替え_1");
                }
            }

            EnsurePanelsConstructed();
            ApplyFontToAllUI();
        }

        /// <summary>
        /// Canvasの描画レイヤーとSortingOrderを確実に最前面（SystemUI / 1010）に調整する
        /// </summary>
        public void EnsureCanvasSortingOrder()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = "SystemUI";
                canvas.sortingOrder = 1010;
                canvas.planeDistance = 100f;
            }

            // PauseMenuCanvas 直下の透明背景パネルなどの RaycastTarget を false に設定し入力を遮断しない
            if (pauseMenuContainer != null)
            {
                var pImg = pauseMenuContainer.GetComponent<Image>();
                if (pImg != null) pImg.raycastTarget = false;
            }
            var rootImg = GetComponent<Image>();
            if (rootImg != null) rootImg.raycastTarget = false;

            // タブバー（TabsBar）を確実に全画面パネルの最前面（SystemUI / 1050）に維持
            Transform tabParent = null;
            if (equipmentTabButton != null) tabParent = equipmentTabButton.transform.parent;
            else if (tabs.Count > 0 && tabs[0].tabButton != null) tabParent = tabs[0].tabButton.transform.parent;

            if (tabParent != null)
            {
                Canvas tabCanvas = tabParent.GetComponent<Canvas>();
                if (tabCanvas == null) tabCanvas = tabParent.gameObject.AddComponent<Canvas>();
                tabCanvas.overrideSorting = true;
                tabCanvas.sortingLayerName = "SystemUI";
                tabCanvas.sortingOrder = 1050;
                tabCanvas.planeDistance = 100f;
                if (canvas != null && canvas.worldCamera != null)
                {
                    tabCanvas.worldCamera = canvas.worldCamera;
                }

                GraphicRaycaster gr = tabParent.GetComponent<GraphicRaycaster>();
                if (gr == null) gr = tabParent.gameObject.AddComponent<GraphicRaycaster>();
                gr.ignoreReversedGraphics = true;
            }
        }

        /// <summary>
        /// タブ定義の自動初期化（インスペクターが空でもデフォルトの3タブを自動登録）
        /// </summary>
        private void InitializeTabs()
        {
            if (tabs.Count == 0)
            {
                // 1. 装備タブ
                tabs.Add(new PauseTabEntry
                {
                    tabType = TabType.Equipment,
                    tabName = "装備 (Equip)",
                    tabButton = equipmentTabButton,
                    tabContentPanel = (inventoryUI != null) ? inventoryUI.gameObject : null,
                    directShortcutKey = KeyCode.Alpha1
                });

                // 2. 設定タブ
                tabs.Add(new PauseTabEntry
                {
                    tabType = TabType.Settings,
                    tabName = "環境調律 (Settings)",
                    tabButton = settingsTabButton,
                    tabContentPanel = settingsPanel,
                    directShortcutKey = KeyCode.Alpha2
                });

                // 3. ライブラリタブ（旧Controls枠）
                if (controlsTabButton != null || controlsPanel != null)
                {
                    tabs.Add(new PauseTabEntry
                    {
                        tabType = TabType.Controls,
                        tabName = "神格目録 (Library)",
                        tabButton = controlsTabButton,
                        tabContentPanel = controlsPanel,
                        directShortcutKey = KeyCode.Alpha3
                    });
                }

                // 4. タイトルに戻るタブ
                tabs.Add(new PauseTabEntry
                {
                    tabType = TabType.QuitToTitle,
                    tabName = "帰還 (Title)",
                    tabButton = quitTabButton,
                    tabContentPanel = quitConfirmPanel,
                    directShortcutKey = KeyCode.Alpha4
                });
            }

            // ボタンのクリックイベントを一括登録 ＆ 付箋タブのテキスト・フォント設定
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                if (tabs[index].tabType == TabType.Controls)
                {
                    tabs[index].tabName = "神格目録 (Library)";
                }

                if (tabs[index].tabButton != null)
                {
                    tabs[index].tabButton.onClick.RemoveAllListeners();
                    tabs[index].tabButton.onClick.AddListener(() => SwitchTab(index));

                    // タブボタンのテキストとフォントを自動設定
                    var tmp = tabs[index].tabButton.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        tmp.text = tabs[index].tabName;
                        if (customFontAsset != null) tmp.font = customFontAsset;
                        tmp.fontSize = 20f;
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 13f;
                        tmp.fontSizeMax = 20f;
                        tmp.alignment = TextAlignmentOptions.Center;
                    }
                    else
                    {
                        var txt = tabs[index].tabButton.GetComponentInChildren<UnityEngine.UI.Text>(true);
                        if (txt != null) txt.text = tabs[index].tabName;
                    }
                }
            }
        }

        /// <summary>
        /// 将来の拡張用：外部スクリプトから新しいタブを動的に追加するAPI
        /// </summary>
        public void RegisterNewTab(string name, Button button, GameObject panel, TabType type = TabType.Custom, Action onOpen = null, Action onClose = null)
        {
            PauseTabEntry newEntry = new PauseTabEntry
            {
                tabType = type,
                tabName = name,
                tabButton = button,
                tabContentPanel = panel,
                onTabOpened = onOpen,
                onTabClosed = onClose
            };

            int newIndex = tabs.Count;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SwitchTab(newIndex));
            }
            tabs.Add(newEntry);
            Debug.Log($"[PauseMenuManager] Registered new tab: {name} (Index: {newIndex})");
        }

        private void Update()
        {
            // Escキーによるポーズ開閉判定
            bool toggleRequested = false;
            if (PlayerInputManager_Alpha.Instance != null && PlayerInputManager_Alpha.Instance.WasPausePressed)
            {
                toggleRequested = true;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                toggleRequested = true;
            }

            if (toggleRequested)
            {
                // 連打・判定残りによる即時終了を防止
                if (Time.unscaledTime - lastToggleTime >= ToggleCooldown)
                {
                    lastToggleTime = Time.unscaledTime;
                    TogglePauseMenu();
                }
                return;
            }

            // ポーズ中のタブ切り替えキー入力（LShift, RShift, Q, E, Tab, 数字キーなど）
            if (isPaused && tabs.Count > 0)
            {
                HandleTabNavigationInput();
            }
        }

        /// <summary>
        /// ポーズ中のキーボード/ゲームパッドによるタブ切り替え操作
        /// </summary>
        private void HandleTabNavigationInput()
        {
            // 左Shift または Q キー: 前のタブ
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PrevTab();
                return;
            }

            // 右Shift または E キー または Tab キー: 次のタブ
            if (Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextTab();
                return;
            }

            // 数字キーによる直接タブジャンプ
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].directShortcutKey != KeyCode.None && Input.GetKeyDown(tabs[i].directShortcutKey))
                {
                    SwitchTab(i);
                    return;
                }
            }
        }

        public void NextTab()
        {
            if (tabs.Count == 0) return;
            int next = (currentTabIndex + 1) % tabs.Count;
            SwitchTab(next);
            PlayTabSE();
        }

        public void PrevTab()
        {
            if (tabs.Count == 0) return;
            int prev = (currentTabIndex - 1 + tabs.Count) % tabs.Count;
            SwitchTab(prev);
            PlayTabSE();
        }

        public void TogglePauseMenu()
        {
            if (inventoryUI != null && inventoryUI.currentMode == InventoryUI_Alpha.InventoryUIMode.SelectForBlacksmith)
            {
                Debug.Log("[PauseMenuManager] Blocked because Inventory is in Blacksmith mode.");
                return;
            }

            isPaused = !isPaused;
            if (isPaused)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }

        private void OpenMenu()
        {
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.PauseBGM();
            }
            Time.timeScale = 0f;
            EnsureCanvasSortingOrder();

            if (pauseMenuContainer != null)
            {
                pauseMenuContainer.SetActive(true);
            }

            if (TutorialManager_Alpha.Instance != null && TutorialManager_Alpha.Instance.IsShowing)
            {
                TutorialManager_Alpha.Instance.isQueuePaused = true;
                TutorialManager_Alpha.Instance.ForceCloseCurrentTutorial();
            }

            // 閉じるアニメーション等が走らないよう初期化してからタブ0を開く
            ApplyFontToAllUI();
            currentTabIndex = -1;
            SwitchTab(0);
        }

        public void CloseMenu()
        {
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.ResumeBGM();
            }
            isPaused = false;
            Time.timeScale = 1f;

            // 各タブパネルの非表示化と設定の自動保存
            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
            {
                CloseCurrentTabPanel(currentTabIndex);
            }

            if (GameSettingsManager_Alpha.Instance != null)
            {
                GameSettingsManager_Alpha.Instance.SaveSettings();
            }

            if (pauseMenuContainer != null)
            {
                pauseMenuContainer.SetActive(false);
            }

            if (TutorialManager_Alpha.Instance != null)
            {
                TutorialManager_Alpha.Instance.isQueuePaused = false;
            }
        }

        public void SwitchTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= tabs.Count) return;

            // 直前のタブパネルと異なる場合のみ前のパネルを閉じる
            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count && currentTabIndex != tabIndex)
            {
                CloseCurrentTabPanel(currentTabIndex);
            }

            currentTabIndex = tabIndex;
            var targetTab = tabs[currentTabIndex];

            // すべてのタブボタンの色をリセット
            for (int i = 0; i < tabs.Count; i++)
            {
                SetButtonColor(tabs[i].tabButton, (i == currentTabIndex) ? activeTabColor : inactiveTabColor);
            }

            // 新しいタブを開く
            OpenTabPanel(targetTab);
        }

        private void OpenTabPanel(PauseTabEntry tab)
        {
            switch (tab.tabType)
            {
                case TabType.Equipment:
                    if (inventoryUI != null)
                    {
                        inventoryUI.OpenAsTab();
                    }
                    else if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(true);
                    }
                    break;

                case TabType.Controls:
                    if (controlsPanel == null) EnsurePanelsConstructed();
                    ApplyFontToAllUI();
                    if (tab.tabContentPanel == null && controlsPanel != null) tab.tabContentPanel = controlsPanel;
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(true);
                        var pc = tab.tabContentPanel.GetComponent<Canvas>();
                        if (pc != null && pc.worldCamera == null)
                        {
                            pc.worldCamera = Camera.main;
                            pc.planeDistance = 100f;
                        }
                        var libUI = tab.tabContentPanel.GetComponent<LibraryPanelUI_Alpha>();
                        if (libUI != null)
                        {
                            libUI.RefreshUI();
                        }
                    }
                    break;

                case TabType.Settings:
                    if (settingsPanel == null) EnsurePanelsConstructed();
            ApplyFontToAllUI();
                    if (tab.tabContentPanel == null && settingsPanel != null) tab.tabContentPanel = settingsPanel;
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(true);
                        var pc = tab.tabContentPanel.GetComponent<Canvas>();
                        if (pc != null && pc.worldCamera == null)
                        {
                            pc.worldCamera = Camera.main;
                            pc.planeDistance = 100f;
                        }
                        var optUI = tab.tabContentPanel.GetComponent<OptionsUI_Alpha>();
                        if (optUI != null)
                        {
                            optUI.OpenOptions();
                        }
                    }
                    break;

                case TabType.QuitToTitle:
                    if (quitConfirmPanel == null) EnsurePanelsConstructed();
            ApplyFontToAllUI();
                    if (tab.tabContentPanel == null && quitConfirmPanel != null) tab.tabContentPanel = quitConfirmPanel;
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(true);
                        var pc = tab.tabContentPanel.GetComponent<Canvas>();
                        if (pc != null && pc.worldCamera == null)
                        {
                            pc.worldCamera = Camera.main;
                            pc.planeDistance = 100f;
                        }
                    }
                    else
                    {
                        OnQuitToTitleDirect();
                    }
                    break;

                default:
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(true);
                    }
                    break;
            }

            tab.onTabOpened?.Invoke();
        }

        private void CloseCurrentTabPanel(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= tabs.Count) return;
            var tab = tabs[tabIndex];

            switch (tab.tabType)
            {
                case TabType.Equipment:
                    if (inventoryUI != null)
                    {
                        inventoryUI.CloseAsTab();
                    }
                    else if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(false);
                    }
                    break;

                case TabType.Controls:
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(false);
                    }
                    break;

                case TabType.Settings:
                    if (GameSettingsManager_Alpha.Instance != null)
                    {
                        GameSettingsManager_Alpha.Instance.SaveSettings();
                    }
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(false);
                    }
                    break;

                default:
                    if (tab.tabContentPanel != null)
                    {
                        tab.tabContentPanel.SetActive(false);
                    }
                    break;
            }

            tab.onTabClosed?.Invoke();
        }

        private void SetButtonColor(Button btn, Color col)
        {
            if (btn != null)
            {
                var colors = btn.colors;
                colors.normalColor = col;
                colors.highlightedColor = col;
                colors.selectedColor = col;
                btn.colors = colors;
            }
        }

        private void PlayTabSE()
        {
            if (tabSwitchSEClip != null && SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.PlaySE(tabSwitchSEClip, tabSwitchSEVolume);
            }
        }

        // --- パネル自動構築・フォールバック ---

        private void EnsurePanelsConstructed()
        {
            Transform parentTransform = (pauseMenuContainer != null) ? pauseMenuContainer.transform : transform;

            // 1. Settings Panel の確保・OptionsUI_Alpha の自動生成
            if (settingsPanel == null)
            {
                settingsPanel = CreateDefaultSettingsPanel(parentTransform);
                var settingTab = tabs.Find(t => t.tabType == TabType.Settings);
                if (settingTab != null) settingTab.tabContentPanel = settingsPanel;
            }

            // 2. Quit Confirm Panel の確保
            if (quitConfirmPanel == null)
            {
                quitConfirmPanel = CreateDefaultQuitConfirmPanel(parentTransform);
                var quitTab = tabs.Find(t => t.tabType == TabType.QuitToTitle);
                if (quitTab != null) quitTab.tabContentPanel = quitConfirmPanel;
            }

            // 3. Library Panel（旧Controls枠）の確保・LibraryPanelUI_Alpha の自動生成
            if (controlsPanel == null)
            {
                controlsPanel = CreateDefaultLibraryPanel(parentTransform);
                var ctrlTab = tabs.Find(t => t.tabType == TabType.Controls);
                if (ctrlTab != null) ctrlTab.tabContentPanel = controlsPanel;
            }
        }

        /// <summary>
        /// ライブラリ（チュートリアル反復確認、敵、弾、エフェクト等）のUIパネルをコードから動的生成（全画面フルスクリーン）
        /// </summary>
        private GameObject CreateDefaultLibraryPanel(Transform parent)
        {
            GameObject panelGO = new GameObject("PauseLibraryPanel", typeof(RectTransform));
            panelGO.transform.SetParent(parent, false);

            RectTransform rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localEulerAngles = Vector3.zero;

            // レイヤー設定 (SystemUI / 1005: 最前面タブバー 1050 の背面)
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null) rootCanvas = GetComponent<Canvas>();

            Canvas panelCanvas = panelGO.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingLayerName = "SystemUI";
            panelCanvas.sortingOrder = 1005;
            panelCanvas.planeDistance = 100f;
            if (rootCanvas != null && rootCanvas.worldCamera != null)
            {
                panelCanvas.worldCamera = rootCanvas.worldCamera;
            }
            else
            {
                panelCanvas.worldCamera = Camera.main;
            }

            GraphicRaycaster gr = panelGO.AddComponent<GraphicRaycaster>();
            gr.ignoreReversedGraphics = true;

            // 背景イメージ（深森墨黒・すりガラス調 全画面）
            Image bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0.040f, 0.055f, 0.048f, 0.96f);
            AddCornerAccents(rt, new Color(0.80f, 0.68f, 0.44f, 0.9f), 24f, 1.5f);

            LibraryPanelUI_Alpha libUI = panelGO.AddComponent<LibraryPanelUI_Alpha>();
            libUI.customFontAsset = customFontAsset;
            libUI.BuildLayoutIfNeeded();

            panelGO.SetActive(false);
            return panelGO;
        }

        /// <summary>
        /// オプション画面（音量3種、画面揺れ・ダメージ表示・VSync等）のUIパネルをコードから動的生成（全画面フルスクリーン）
        /// </summary>
        private GameObject CreateDefaultSettingsPanel(Transform parent)
        {
            GameObject panelGO = new GameObject("PauseSettingsPanel", typeof(RectTransform));
            panelGO.transform.SetParent(parent, false);

            RectTransform rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localEulerAngles = Vector3.zero;

            // レイヤー設定 (SystemUI / 1005: 最前面タブバー 1050 の背面)
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null) rootCanvas = GetComponent<Canvas>();

            Canvas panelCanvas = panelGO.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingLayerName = "SystemUI";
            panelCanvas.sortingOrder = 1005;
            panelCanvas.planeDistance = 100f;
            if (rootCanvas != null && rootCanvas.worldCamera != null)
            {
                panelCanvas.worldCamera = rootCanvas.worldCamera;
            }
            else
            {
                panelCanvas.worldCamera = Camera.main;
            }

            GraphicRaycaster gr = panelGO.AddComponent<GraphicRaycaster>();
            gr.ignoreReversedGraphics = true;

            // 背景イメージ（深森墨黒・すりガラス調 全画面）
            Image bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0.040f, 0.055f, 0.048f, 0.96f);
            AddCornerAccents(rt, new Color(0.80f, 0.68f, 0.44f, 0.9f), 24f, 1.5f);

            // 1. ヘッダー（上部タブバーの下 Y: -65f に配置）
            GameObject headerGO = new GameObject("Header", typeof(RectTransform));
            headerGO.transform.SetParent(panelGO.transform, false);
            RectTransform headerRT = headerGO.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 50f);
            headerRT.anchoredPosition = new Vector2(0f, -65f);

            var titleTmp = CreateTextRow(headerGO.transform, "<size=65%><color=#8c7552>ENVIRONMENTAL TUNING // </color></size>環境調律", 24, FontStyles.Bold, TextAlignmentOptions.Center, 40f);
            titleTmp.color = new Color(0.80f, 0.68f, 0.44f, 1f);

            // 区切り線（細い真鍮ライン ＋ 中央◇ノード）
            GameObject lineGO = new GameObject("HeaderLine", typeof(RectTransform));
            lineGO.transform.SetParent(panelGO.transform, false);
            RectTransform lineRT = lineGO.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0f, 1f);
            lineRT.anchorMax = new Vector2(1f, 1f);
            lineRT.pivot = new Vector2(0.5f, 1f);
            lineRT.sizeDelta = new Vector2(-80f, 1.5f);
            lineRT.anchoredPosition = new Vector2(0f, -115f);
            Image lineImg = lineGO.AddComponent<Image>();
            lineImg.color = new Color(0.55f, 0.46f, 0.32f, 0.65f);

            GameObject diamondGO = new GameObject("DiamondNode", typeof(RectTransform));
            diamondGO.transform.SetParent(lineGO.transform, false);
            RectTransform dRT = diamondGO.GetComponent<RectTransform>();
            dRT.sizeDelta = new Vector2(8f, 8f);
            dRT.anchoredPosition = Vector2.zero;
            dRT.localRotation = Quaternion.Euler(0, 0, 45f);
            Image dImg = diamondGO.AddComponent<Image>();
            dImg.color = new Color(0.80f, 0.68f, 0.44f, 1f);

            // 2. 中央カードコンテナ（幅: 1040px, 高さ: 580px）
            GameObject cardGO = new GameObject("SettingsCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            RectTransform cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(1040f, 580f);
            cardRT.anchoredPosition = new Vector2(0f, -30f);

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = new Color(0.055f, 0.080f, 0.068f, 0.94f);
            AddCornerAccents(cardRT, new Color(0.55f, 0.46f, 0.32f, 0.65f), 18f, 1.5f);

            // 左右分割 HorizontalLayoutGroup
            HorizontalLayoutGroup cardHlg = cardGO.AddComponent<HorizontalLayoutGroup>();
            cardHlg.padding = new RectOffset(45, 45, 40, 40);
            cardHlg.spacing = 50;
            cardHlg.childControlWidth = true;
            cardHlg.childControlHeight = true;
            cardHlg.childForceExpandWidth = true;
            cardHlg.childForceExpandHeight = true;

            // OptionsUI_Alpha コンポーネントをアタッチ
            OptionsUI_Alpha optionsUI = panelGO.AddComponent<OptionsUI_Alpha>();
            optionsUI.optionsPanel = panelGO;

            // --- 左カラム: 音響調律 ---
            GameObject leftCol = new GameObject("AudioColumn", typeof(RectTransform));
            leftCol.transform.SetParent(cardGO.transform, false);
            VerticalLayoutGroup leftVlg = leftCol.AddComponent<VerticalLayoutGroup>();
            leftVlg.spacing = 22;
            leftVlg.childControlWidth = true;
            leftVlg.childControlHeight = false;
            leftVlg.childForceExpandWidth = true;
            leftVlg.childForceExpandHeight = false;

            CreateTextRow(leftCol.transform, "<color=#ccad70>◆ 音響条項 (Sound Calibration)</color>", 19, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 34f);
            optionsUI.masterVolumeSlider = CreateSliderRow(leftCol.transform, "主音量 (Master)", out optionsUI.masterVolumeText);
            optionsUI.bgmVolumeSlider = CreateSliderRow(leftCol.transform, "奏楽 (BGM)", out optionsUI.bgmVolumeText);
            optionsUI.seVolumeSlider = CreateSliderRow(leftCol.transform, "効果音 (SE)", out optionsUI.seVolumeText);

            // --- 右カラム: 演出・画面条項 ---
            GameObject rightCol = new GameObject("VisualColumn", typeof(RectTransform));
            rightCol.transform.SetParent(cardGO.transform, false);
            VerticalLayoutGroup rightVlg = rightCol.AddComponent<VerticalLayoutGroup>();
            rightVlg.spacing = 22;
            rightVlg.childControlWidth = true;
            rightVlg.childControlHeight = false;
            rightVlg.childForceExpandWidth = true;
            rightVlg.childForceExpandHeight = false;

            CreateTextRow(rightCol.transform, "<color=#ccad70>◆ 演出・画面条項 (Visual Calibration)</color>", 19, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 34f);
            optionsUI.screenShakeToggle = CreateToggleRow(rightCol.transform, "画面揺れ (Screen Shake)");
            optionsUI.damageNumbersToggle = CreateToggleRow(rightCol.transform, "被弾数値 (Damage Numbers)");
            optionsUI.vSyncToggle = CreateToggleRow(rightCol.transform, "垂直同期 (VSync)");

            SetLayerRecursively(panelGO, 5); // 5: UI
            panelGO.SetActive(false);
            Debug.Log("[PauseMenuManager] Created fullscreen Options panel successfully.");
            return panelGO;
        }

        /// <summary>
        /// タイトルに戻る確認パネルをコードから動的生成（全画面フルスクリーン）
        /// </summary>
        private GameObject CreateDefaultQuitConfirmPanel(Transform parent)
        {
            GameObject panelGO = new GameObject("QuitConfirmPanel", typeof(RectTransform));
            panelGO.transform.SetParent(parent, false);

            RectTransform rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localEulerAngles = Vector3.zero;

            // レイヤー設定 (SystemUI / 1005: 最前面タブバー 1050 の背面)
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null) rootCanvas = GetComponent<Canvas>();

            Canvas panelCanvas = panelGO.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingLayerName = "SystemUI";
            panelCanvas.sortingOrder = 1005;
            panelCanvas.planeDistance = 100f;
            if (rootCanvas != null && rootCanvas.worldCamera != null)
            {
                panelCanvas.worldCamera = rootCanvas.worldCamera;
            }
            else
            {
                panelCanvas.worldCamera = Camera.main;
            }

            GraphicRaycaster gr = panelGO.AddComponent<GraphicRaycaster>();
            gr.ignoreReversedGraphics = true;

            // 全画面背景（深森墨黒・すりガラス調）
            Image bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0.040f, 0.055f, 0.048f, 0.96f);
            AddCornerAccents(rt, new Color(0.80f, 0.68f, 0.44f, 0.9f), 24f, 1.5f);

            // 1. ヘッダー（上部タブバーの下 Y: -65f に配置）
            GameObject headerGO = new GameObject("Header", typeof(RectTransform));
            headerGO.transform.SetParent(panelGO.transform, false);
            RectTransform headerRT = headerGO.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 50f);
            headerRT.anchoredPosition = new Vector2(0f, -65f);

            var titleTmp = CreateTextRow(headerGO.transform, "<size=65%><color=#8c7552>SANCTUARY RETURN // </color></size>拠点帰還", 24, FontStyles.Bold, TextAlignmentOptions.Center, 40f);
            titleTmp.color = new Color(0.80f, 0.68f, 0.44f, 1f);

            // 区切り線（細い真鍮ライン ＋ 中央ノード）
            GameObject lineGO = new GameObject("HeaderLine", typeof(RectTransform));
            lineGO.transform.SetParent(panelGO.transform, false);
            RectTransform lineRT = lineGO.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0f, 1f);
            lineRT.anchorMax = new Vector2(1f, 1f);
            lineRT.pivot = new Vector2(0.5f, 1f);
            lineRT.sizeDelta = new Vector2(-80f, 1.5f);
            lineRT.anchoredPosition = new Vector2(0f, -115f);
            Image lineImg = lineGO.AddComponent<Image>();
            lineImg.color = new Color(0.55f, 0.46f, 0.32f, 0.65f);

            GameObject diamondGO = new GameObject("DiamondNode", typeof(RectTransform));
            diamondGO.transform.SetParent(lineGO.transform, false);
            RectTransform dRT = diamondGO.GetComponent<RectTransform>();
            dRT.sizeDelta = new Vector2(8f, 8f);
            dRT.anchoredPosition = Vector2.zero;
            dRT.localRotation = Quaternion.Euler(0, 0, 45f);
            Image dImg = diamondGO.AddComponent<Image>();
            dImg.color = new Color(0.80f, 0.68f, 0.44f, 1f);

            // 2. 中央確認ダイアログカード（幅: 760px, 高さ: 360px）
            GameObject cardGO = new GameObject("QuitDialogCard", typeof(RectTransform));
            cardGO.transform.SetParent(panelGO.transform, false);
            RectTransform cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(760f, 360f);
            cardRT.anchoredPosition = new Vector2(0f, -30f);

            Image cardBg = cardGO.AddComponent<Image>();
            cardBg.color = new Color(0.055f, 0.080f, 0.068f, 0.95f);
            // 椿色（深紅）の四隅アクセント
            AddCornerAccents(cardRT, new Color(0.72f, 0.22f, 0.24f, 0.85f), 18f, 1.5f);

            VerticalLayoutGroup vlg = cardGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 45, 45);
            vlg.spacing = 25;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var msgTitle = CreateTextRow(cardGO.transform, "タイトル（拠点）へ帰還しますか？", 26, FontStyles.Bold, TextAlignmentOptions.Center, 38f);
            msgTitle.color = new Color(0.93f, 0.91f, 0.86f, 1f);

            var msgSub = CreateTextRow(cardGO.transform, "※オートセーブされた直近の記録条項まで戻ります", 16, FontStyles.Normal, TextAlignmentOptions.Center, 28f);
            msgSub.color = new Color(0.68f, 0.68f, 0.64f, 0.85f);

            // ボタン配置用 HorizontalLayout
            GameObject btnRow = new GameObject("Buttons", typeof(RectTransform));
            btnRow.transform.SetParent(cardGO.transform, false);
            btnRow.layer = 5;
            LayoutElement btnRowLE = btnRow.AddComponent<LayoutElement>();
            btnRowLE.minHeight = 52f;
            btnRowLE.preferredHeight = 52f;

            HorizontalLayoutGroup hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var yesBtn = CreateSimpleButton(btnRow.transform, "タイトルへ帰還", new Color(0.72f, 0.22f, 0.24f, 1f));
            yesBtn.onClick.AddListener(OnQuitToTitleDirect);

            var noBtn = CreateSimpleButton(btnRow.transform, "交戦を継続", new Color(0.14f, 0.21f, 0.17f, 0.95f));
            noBtn.onClick.AddListener(() => SwitchTab(0)); // 装備タブへ戻る

            SetLayerRecursively(panelGO, 5); // 5: UI
            panelGO.SetActive(false);
            return panelGO;
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child != null) SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private TextMeshProUGUI CreateTextRow(Transform parent, string content, float fontSize, FontStyles style, TextAlignmentOptions alignment, float height = 30f)
        {
            GameObject go = new GameObject("TextRow", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = 5;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null)
            {
                tmp.font = customFontAsset;
            }
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return tmp;
        }

        private Slider CreateSliderRow(Transform parent, string label, out TextMeshProUGUI valText)
        {
            GameObject row = new GameObject("SliderRow_" + label, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.layer = 5;
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            var labelTmp = CreateTextRow(row.transform, label, 17, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            labelTmp.rectTransform.sizeDelta = new Vector2(160, 30);

            // スライダー本体 (190 x 24)
            GameObject sliderGO = new GameObject("Slider", typeof(RectTransform));
            sliderGO.transform.SetParent(row.transform, false);
            sliderGO.layer = 5;
            RectTransform sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.sizeDelta = new Vector2(190, 24);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // 1. スライダー背景トラック（深森苔色）
            GameObject bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(sliderGO.transform, false);
            bgGO.layer = 5;
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.12f, 0.10f, 0.90f);
            RectTransform bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.25f);
            bgRT.anchorMax = new Vector2(1f, 0.75f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // 2. Fill Area & Fill (蓮の翡翠色ゲージ)
            GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGO.transform.SetParent(sliderGO.transform, false);
            fillAreaGO.layer = 5;
            RectTransform fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRT.offsetMin = new Vector2(5f, 0f);
            fillAreaRT.offsetMax = new Vector2(-5f, 0f);

            GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            fillGO.layer = 5;
            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.32f, 0.72f, 0.53f, 1.0f);
            RectTransform fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            slider.fillRect = fillRT;

            // 3. Handle Slide Area & Handle (真鍮ゴールド操作つまみ)
            GameObject handleAreaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGO.transform.SetParent(sliderGO.transform, false);
            handleAreaGO.layer = 5;
            RectTransform handleAreaRT = handleAreaGO.GetComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.offsetMin = new Vector2(10f, 0f);
            handleAreaRT.offsetMax = new Vector2(-10f, 0f);

            GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            handleGO.layer = 5;
            Image handleImg = handleGO.AddComponent<Image>();
            handleImg.color = new Color(0.80f, 0.68f, 0.44f, 1.0f);
            RectTransform handleRT = handleGO.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(18f, 22f);

            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;

            // パーセント表示テキスト（真鍮ゴールド）
            valText = CreateTextRow(row.transform, "100%", 17, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            valText.color = new Color(0.80f, 0.68f, 0.44f, 1.0f);
            valText.rectTransform.sizeDelta = new Vector2(55, 30);

            return slider;
        }

        private Toggle CreateToggleRow(Transform parent, string label)
        {
            GameObject row = new GameObject("ToggleRow_" + label, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.layer = 5;
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            GameObject toggleGO = new GameObject("Toggle", typeof(RectTransform));
            toggleGO.transform.SetParent(row.transform, false);
            toggleGO.layer = 5;
            Toggle toggle = toggleGO.AddComponent<Toggle>();
            toggleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(24, 24);

            Image bg = toggleGO.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.10f, 0.90f);

            GameObject checkGO = new GameObject("Checkmark", typeof(RectTransform));
            checkGO.transform.SetParent(toggleGO.transform, false);
            checkGO.layer = 5;
            Image checkImg = checkGO.AddComponent<Image>();
            checkImg.color = new Color(0.32f, 0.72f, 0.53f, 1.0f);
            toggle.graphic = checkImg;
            checkGO.GetComponent<RectTransform>().sizeDelta = new Vector2(16, 16);

            var labelTmp = CreateTextRow(row.transform, label, 17, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            labelTmp.color = new Color(0.93f, 0.91f, 0.86f, 1.0f);
            labelTmp.rectTransform.sizeDelta = new Vector2(300, 30);

            return toggle;
        }

        private Button CreateSimpleButton(Transform parent, string text, Color color)
        {
            GameObject btnGO = new GameObject("Btn_" + text, typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);
            btnGO.layer = 5;
            btnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 45);

            Image img = btnGO.AddComponent<Image>();
            img.color = color;

            Button btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            btn.colors = colors;

            var txt = CreateTextRow(btnGO.transform, text, 17, FontStyles.Bold, TextAlignmentOptions.Center);
            txt.color = new Color(0.93f, 0.91f, 0.86f, 1.0f);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.sizeDelta = Vector2.zero;

            return btn;
        }

        private void OnQuitToTitleDirect()
        {
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.StopBGM(0.2f);
            }
            Time.timeScale = 1f;
            if (GameSettingsManager_Alpha.Instance != null)
            {
                GameSettingsManager_Alpha.Instance.SaveSettings();
            }
            SceneManager.LoadScene("Title_Alpha");
        }

        private static void AddCornerAccents(RectTransform targetParent, Color color, float size = 14f, float thickness = 1.5f)
        {
            if (targetParent == null) return;

            for (int i = targetParent.childCount - 1; i >= 0; i--)
            {
                var c = targetParent.GetChild(i);
                if (c.name.StartsWith("Corner_"))
                {
                    Destroy(c.gameObject);
                }
            }

            CreateSingleCorner(targetParent, "Corner_TL", new Vector2(0f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_TR", new Vector2(1f, 1f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BL", new Vector2(0f, 0f), color, size, thickness);
            CreateSingleCorner(targetParent, "Corner_BR", new Vector2(1f, 0f), color, size, thickness);
        }

        private static void CreateSingleCorner(RectTransform parent, string name, Vector2 anchor, Color color, float size, float thickness)
        {
            GameObject cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            cornerGO.layer = 5;
            RectTransform crt = cornerGO.GetComponent<RectTransform>();
            crt.anchorMin = anchor;
            crt.anchorMax = anchor;
            crt.pivot = anchor;
            crt.sizeDelta = new Vector2(size, size);
            crt.anchoredPosition = Vector2.zero;

            GameObject hGO = new GameObject("H", typeof(RectTransform));
            hGO.transform.SetParent(cornerGO.transform, false);
            hGO.layer = 5;
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
            vGO.layer = 5;
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
