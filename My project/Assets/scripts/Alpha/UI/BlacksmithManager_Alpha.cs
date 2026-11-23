using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alpha.Data;
using Alpha.Flow;

namespace Alpha.UI
{
    public class BlacksmithManager_Alpha : MonoBehaviour
    {
        public static BlacksmithManager_Alpha Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject panel;
        public InventoryUI_Alpha inventoryUI;

        [Header("Popup Settings")]
        public GameObject popupPanel;
        public TextMeshProUGUI popupText;
        public EquipDetailPopupUI_Alpha detailPopup;

        [Header("Action Confirm UI")]
        public GameObject confirmPanel;
        public TextMeshProUGUI confirmExplainText;
        public Button confirmYesButton;
        public Button confirmNoButton;

        [Header("Confirm Text Formats")]
        [TextArea(3, 5)]
        public string sellConfirmFormat = "このパーツ（と付与スキル）を売却します。\n{0}\n計{1}エーテル\nよろしいですか？";
        [TextArea(3, 5)]
        public string upgradeConfirmFormat = "これをアップグレードします。\n消費エーテル: {0}\nよろしかったですか？";
        [TextArea(3, 5)]
        public string partBuyConfirmFormat = "このパーツを購入し、空きスロットに装備します。\n{0}\n消費エーテル: {1}\nよろしかったですか？";
        [TextArea(3, 5)]
        public string skillBuyConfirmFormat = "選択した装備にこのスキルを付与します。\n{0}\n消費エーテル: {1}\nよろしかったですか？";

        private System.Action pendingConfirmAction;

        [Header("Part Shop Settings")]
        public Transform partButtonsContainer;
        public GameObject partButtonPrefab;
        public int partShopCount = 3;
        public int[] partPrices = new int[] { 50, 100, 200, 300 };
        private List<GameObject> spawnedPartButtons = new List<GameObject>();

        [Header("Skill Shop Settings")]
        public Transform skillButtonsContainer;
        public GameObject skillButtonPrefab;
        public int skillShopCount = 3;
        private List<GameObject> spawnedSkillButtons = new List<GameObject>();

        [Header("General Buttons")]
        public Button sellModeButton;
        public Button upgradeModeButton;
        public Button nextPhaseButton;
        public TextMeshProUGUI emptySlotText;
        
        [Header("Costs")]
        [Tooltip("装備アップグレードのコスト倍率 (現在の品質 * n)")]
        public int upgradeCostMultiplier = 50;

        [Header("Modern Botanical Palette")]
        public static readonly Color ColorBaseInk = new Color(0.024f, 0.034f, 0.028f, 1.0f);       // 完全不透明な深森墨黒背景（背景完全遮蔽）
        public static readonly Color ColorBasePlate = new Color(0.060f, 0.090f, 0.075f, 0.94f);     // プレート背景
        public static readonly Color ColorCardBg = new Color(0.065f, 0.095f, 0.080f, 0.95f);       // 通常カード背景（落ち着いた深森墨黒）
        public static readonly Color ColorCardDimBg = new Color(0.040f, 0.055f, 0.048f, 0.45f);    // 装備不可カード背景（半透明・暗小）
        public static readonly Color ColorBrassGold = new Color(0.80f, 0.68f, 0.44f, 1.0f);        // 真鍮金（枠線・強調）
        public static readonly Color ColorBrassGoldDim = new Color(0.80f, 0.68f, 0.44f, 0.35f);    // 境界ライン
        public static readonly Color ColorPaleWhite = new Color(0.88f, 0.94f, 0.90f, 1.0f);        // 生成り白テキスト
        public static readonly Color ColorSubText = new Color(0.60f, 0.72f, 0.66f, 1.0f);          // 翡翠薄灰サブテキスト
        public static readonly Color ColorDimText = new Color(0.45f, 0.52f, 0.48f, 0.60f);         // 暗小テキスト
        public static readonly Color ColorDepartGold = new Color(0.88f, 0.72f, 0.32f, 1.0f);       // 出発ボタン金

        // レアリティカラー体系（濁りのない澄んだボタニカルカラー定義：紫変色を防止）
        public static Color GetBotanicalRarityColor(int quality)
        {
            switch (quality)
            {
                case 1: return new Color(0.65f, 0.72f, 0.68f, 1.0f); // 普: 落ち着いた真鍮鉄・薄銀緑
                case 2: return new Color(0.24f, 0.78f, 0.54f, 1.0f); // 良: 鮮やかな新緑・翡翠
                case 3: return new Color(0.25f, 0.62f, 0.96f, 1.0f); // 優: 透き通る瑠璃・蒼藍（澄んだブルーで紫変色を根絶）
                case 4: return new Color(0.96f, 0.76f, 0.26f, 1.0f); // 極: 絢爛たる琥珀金・神代真鍮黄金
                default: return ColorBrassGold;
            }
        }

        public static string GetRarityName(int quality)
        {
            switch (quality)
            {
                case 1: return "普";
                case 2: return "良";
                case 3: return "優";
                case 4: return "極";
                default: return "";
            }
        }

        private TextMeshProUGUI etherStatusText;
        private bool isStylingApplied = false;

        // 自機および自機UI（CircleHPBarなど）の退避リスト
        private List<Renderer> hiddenPlayerRenderers = new List<Renderer>();
        private List<Collider2D> hiddenPlayerColliders = new List<Collider2D>();
        private List<GameObject> hiddenCanvases = new List<GameObject>();

        // カード可否追跡
        private struct SkillCardEntry
        {
            public WeaponEffectSO_Alpha effectSO;
            public GameObject cardObj;
        }

        private struct PartCardEntry
        {
            public WeaponPartInstance_Alpha partInstance;
            public int price;
            public GameObject cardObj;
        }

        private List<SkillCardEntry> activeSkillEntries = new List<SkillCardEntry>();
        private List<PartCardEntry> activePartEntries = new List<PartCardEntry>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (panel != null) panel.SetActive(false);
            if (popupPanel != null) popupPanel.SetActive(false);
            if (confirmDimOverlay != null) confirmDimOverlay.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            if (sellModeButton != null)
            {
                sellModeButton.onClick.RemoveAllListeners();
                sellModeButton.onClick.AddListener(EnterSellMode);
            }
            if (upgradeModeButton != null)
            {
                upgradeModeButton.onClick.RemoveAllListeners();
                upgradeModeButton.onClick.AddListener(EnterUpgradeMode);
            }
            if (nextPhaseButton != null)
            {
                nextPhaseButton.onClick.RemoveAllListeners();
                nextPhaseButton.onClick.AddListener(CloseBlacksmith);
            }
            
            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.RemoveAllListeners();
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            }
            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.RemoveAllListeners();
                confirmNoButton.onClick.AddListener(OnConfirmNo);
            }
            
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
        }

        public void OpenBlacksmith()
        {
            Debug.Log("[Blacksmith] Blacksmith Phase started.");
            
            // 商店・休憩中はゲーム時間（戦闘・背景）を完全停止
            Time.timeScale = 0f;

            if (StageManager_Alpha.Instance != null)
            {
                StageManager_Alpha.Instance.ClearAllEnemyBullets();
            }

            // プレイヤーHPを全快（聖域の湧水）
            if (playerStatusManager_Alpha.Instance != null)
            {
                playerStatusManager_Alpha.Instance.Heal(playerStatusManager_Alpha.Instance.HP);
            }

            // 商店画面の裏で右下に被るプレイヤー自機・CircleHPBar等のUIを完全に非表示化
            HidePlayerAndInGameUI();

            if (panel != null) panel.SetActive(true);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            if (popupPanel != null) popupPanel.SetActive(false);

            ApplyModernBotanicalStyling();

            GenerateSkillShop();
            GeneratePartShop();
            UpdateStatusDisplay();
        }

        public void CloseBlacksmith()
        {
            Debug.Log("[Blacksmith] Blacksmith Phase closing.");

            if (panel != null) panel.SetActive(false);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            if (confirmDimOverlay != null) confirmDimOverlay.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (popupPanel != null) popupPanel.SetActive(false);

            // 非表示にしていたプレイヤー自機・HPバー等のUIを確実に復帰
            RestorePlayerAndInGameUI();
            
            // 出発時にゲーム時間を再開
            Time.timeScale = 1f;

            if (StageManager_Alpha.Instance != null)
            {
                StageManager_Alpha.Instance.OnShopPhaseClosed();
            }
        }

        public void UpdateStatusDisplay()
        {
            if (etherStatusText != null && playerStatusManager_Alpha.Instance != null)
            {
                etherStatusText.text = $"所持エーテル: <color=#ccad70>{playerStatusManager_Alpha.Instance.currentExp}</color>";
            }

            // エーテル残高や装備状況の変動に合わせて全カードの明暗・可否を再評価
            RefreshCardsAvailability();
        }

        /// <summary>
        /// 互換性のための空メソッド（空きスロット表示は不要になったため削除）
        /// </summary>
        public void UpdateEmptySlotDisplay()
        {
            if (emptySlotText != null)
            {
                emptySlotText.gameObject.SetActive(false);
            }
        }

        // ==========================
        // プレイヤー自機・頭上UIの非表示/復帰
        // ==========================
        private void HidePlayerAndInGameUI()
        {
            hiddenPlayerRenderers.Clear();
            hiddenPlayerColliders.Clear();
            hiddenCanvases.Clear();

            // 1. プレイヤー自機オブジェクトの探索
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.FindWithTag("Player");
            }
            if (playerObj == null)
            {
                var pHealth = FindObjectOfType<PlayerHealth>();
                if (pHealth != null) playerObj = pHealth.gameObject;
            }
            if (playerObj == null)
            {
                var pCtrl = FindObjectOfType<Player_Control_Alpha>();
                if (pCtrl != null) playerObj = pCtrl.gameObject;
            }

            if (playerObj != null)
            {
                // レンダラーの非表示
                var renderers = playerObj.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    if (r != null && r.enabled)
                    {
                        r.enabled = false;
                        hiddenPlayerRenderers.Add(r);
                    }
                }

                // コライダーの無効化（マウス判定横取りを防止）
                var colliders = playerObj.GetComponentsInChildren<Collider2D>(true);
                foreach (var c in colliders)
                {
                    if (c != null && c.enabled)
                    {
                        c.enabled = false;
                        hiddenPlayerColliders.Add(c);
                    }
                }

                // プレイヤーの頭上HPバー（PlayerHealth.gaugeCanvas）の非表示
                var pHealth = playerObj.GetComponent<PlayerHealth>();
                if (pHealth != null && pHealth.gaugeCanvas != null && pHealth.gaugeCanvas.activeSelf)
                {
                    pHealth.gaugeCanvas.SetActive(false);
                    hiddenCanvases.Add(pHealth.gaugeCanvas);
                }
            }

            // 2. シーン上の PlayerHPCanvas や CircleHPBar も確実に非表示
            var allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (var c in allCanvases)
            {
                if (c != null && c.gameObject.scene.isLoaded &&
                    (c.gameObject.name.Contains("PlayerHPCanvas") || c.gameObject.name.Contains("CircleHP") || c.gameObject.name.Contains("DamageCanvas")))
                {
                    if (c.gameObject.activeSelf && !hiddenCanvases.Contains(c.gameObject))
                    {
                        c.gameObject.SetActive(false);
                        hiddenCanvases.Add(c.gameObject);
                    }
                }
            }
        }

        private void RestorePlayerAndInGameUI()
        {
            foreach (var r in hiddenPlayerRenderers)
            {
                if (r != null) r.enabled = true;
            }
            hiddenPlayerRenderers.Clear();

            foreach (var c in hiddenPlayerColliders)
            {
                if (c != null) c.enabled = true;
            }
            hiddenPlayerColliders.Clear();

            foreach (var go in hiddenCanvases)
            {
                if (go != null) go.SetActive(true);
            }
            hiddenCanvases.Clear();
        }

        // ==========================
        // スキル販売ショップ生成
        // ==========================
        private void GenerateSkillShop()
        {
            foreach (var btn in spawnedSkillButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedSkillButtons.Clear();
            activeSkillEntries.Clear();

            var generator = WeaponGenerator_Alpha.Instance;
            if (generator == null || generator.globalBuffEffects == null || generator.globalBuffEffects.Count == 0 || skillButtonPrefab == null || skillButtonsContainer == null) return;

            // 重複なしでスキルを選出するための候補リストを作成
            var availablePool = new List<WeaponEffectSO_Alpha>();
            foreach (var eff in generator.globalBuffEffects)
            {
                if (eff != null && !availablePool.Contains(eff))
                {
                    availablePool.Add(eff);
                }
            }

            // シャッフル
            for (int i = 0; i < availablePool.Count; i++)
            {
                int r = Random.Range(i, availablePool.Count);
                var temp = availablePool[i];
                availablePool[i] = availablePool[r];
                availablePool[r] = temp;
            }

            int countToSpawn = Mathf.Min(Mathf.Max(1, skillShopCount), availablePool.Count);
            for (int i = 0; i < countToSpawn; i++)
            {
                var effectSO = availablePool[i];
                if (effectSO == null) continue;

                GameObject obj = Instantiate(skillButtonPrefab, skillButtonsContainer);
                obj.SetActive(true);
                spawnedSkillButtons.Add(obj);

                activeSkillEntries.Add(new SkillCardEntry
                {
                    effectSO = effectSO,
                    cardObj = obj
                });

                Button btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnSkillButtonClicked(effectSO, obj));
                    
                    RightClickDetector_Alpha.Attach(btn.gameObject, (eventData) =>
                    {
                        if (detailPopup != null)
                        {
                            detailPopup.SetupForSkill(effectSO, eventData.position);
                        }
                    });
                }
            }

            RefreshSkillCardsVisual();
        }

        // ==========================
        // パーツ販売ショップ生成
        // ==========================
        private void GeneratePartShop()
        {
            foreach (var btn in spawnedPartButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedPartButtons.Clear();
            activePartEntries.Clear();

            var generator = WeaponGenerator_Alpha.Instance;
            if (generator == null || generator.allSeriesPool == null || generator.allSeriesPool.Count == 0 || partButtonsContainer == null) return;

            // partButtonPrefab が未設定の場合は skillButtonPrefab を自動フォールバック
            GameObject prefabToUse = partButtonPrefab != null ? partButtonPrefab : skillButtonPrefab;
            if (prefabToUse == null) return;

            int countToSpawn = Mathf.Max(1, partShopCount);
            for (int i = 0; i < countToSpawn; i++)
            {
                var randomSeries = generator.allSeriesPool[Random.Range(0, generator.allSeriesPool.Count)];
                if (randomSeries == null) continue;
                
                int rarity = Random.Range(1, 5); // 1:普, 2:良, 3:優, 4:極
                int price = (rarity >= 1 && rarity <= partPrices.Length) ? partPrices[rarity - 1] : partPrices[0];

                var partInstance = generator.CreatePartInstance(randomSeries, rarity);
                if (partInstance == null) continue;

                GameObject obj = Instantiate(prefabToUse, partButtonsContainer);
                obj.SetActive(true);
                spawnedPartButtons.Add(obj);

                activePartEntries.Add(new PartCardEntry
                {
                    partInstance = partInstance,
                    price = price,
                    cardObj = obj
                });

                Button btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnPartButtonClicked(partInstance, price, obj));
                    
                    RightClickDetector_Alpha.Attach(btn.gameObject, (eventData) =>
                    {
                        if (detailPopup != null)
                        {
                            detailPopup.Setup(partInstance.series, partInstance.partType, partInstance.quality, partInstance.currentEffects, eventData.position, partInstance.setBonusEffect);
                        }
                    });
                }
            }

            RefreshPartCardsVisual();
        }

        // ==========================
        // カードの明暗・可否更新（装備できるものを明るく、できないものを暗く）
        // ==========================
        public void RefreshCardsAvailability()
        {
            RefreshSkillCardsVisual();
            RefreshPartCardsVisual();
        }

        private void RefreshSkillCardsVisual()
        {
            int currentEther = playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.currentExp : 0;
            
            // スキルを刻印できる空き付与枠を持つパーツがインベントリにあるか？
            bool hasAvailableSlotForSkill = false;
            var inv = InventoryManager_Alpha.Instance;
            if (inv != null && inv.equipInstance != null)
            {
                foreach (var item in inv.equipInstance)
                {
                    if (item.series != null)
                    {
                        int currentCount = item.currentEffects != null ? item.currentEffects.Count : 0;
                        if (currentCount < item.rarity)
                        {
                            hasAvailableSlotForSkill = true;
                            break;
                        }
                    }
                }
            }

            foreach (var entry in activeSkillEntries)
            {
                if (entry.cardObj == null || entry.effectSO == null) continue;

                bool canAfford = currentEther >= entry.effectSO.price;
                bool isAvailable = canAfford && hasAvailableSlotForSkill;
                string reason = !canAfford ? "エーテル不足" : (!hasAvailableSlotForSkill ? "付与枠上限" : "");

                string desc = entry.effectSO.description;
                try { desc = string.Format(entry.effectSO.description, entry.effectSO.GetValue(1)); } catch {}

                ApplyCardVisual(
                    cardObj: entry.cardObj,
                    isAvailable: isAvailable,
                    unavailableReason: reason,
                    quality: 2, // スキルは翡翠グリーンを基調
                    mainTitle: entry.effectSO.effectName,
                    subDetails: desc,
                    price: entry.effectSO.price,
                    iconSprite: null
                );
            }
        }

        private void RefreshPartCardsVisual()
        {
            int currentEther = playerStatusManager_Alpha.Instance != null ? playerStatusManager_Alpha.Instance.currentExp : 0;

            foreach (var entry in activePartEntries)
            {
                if (entry.cardObj == null || entry.partInstance == null) continue;

                // 装備（パーツ）はインベントリで管理されるため、スロット空き有無による購入制限は行わずエーテルのみで判定
                bool canAfford = currentEther >= entry.price;
                bool isAvailable = canAfford;
                string reason = !canAfford ? "エーテル不足" : "";

                Sprite targetSprite = null;
                if (entry.partInstance.series != null)
                {
                    if (entry.partInstance.partType == WeaponPartType_Alpha.Bullet) targetSprite = entry.partInstance.series.iconBullet;
                    else if (entry.partInstance.partType == WeaponPartType_Alpha.Casing) targetSprite = entry.partInstance.series.iconCasing;
                    else if (entry.partInstance.partType == WeaponPartType_Alpha.Primer) targetSprite = entry.partInstance.series.iconPrimer;
                    if (targetSprite == null) targetSprite = entry.partInstance.series.icon;
                }

                string seriesName = entry.partInstance.series != null ? entry.partInstance.series.seriesName : "靈核";
                string partTypeName = entry.partInstance.partType.ToString();

                ApplyCardVisual(
                    cardObj: entry.cardObj,
                    isAvailable: isAvailable,
                    unavailableReason: reason,
                    quality: entry.partInstance.quality,
                    mainTitle: $"{seriesName}",
                    subDetails: $"[{partTypeName}]",
                    price: entry.price,
                    iconSprite: targetSprite
                );
            }
        }

        private void ApplyCardVisual(GameObject cardObj, bool isAvailable, string unavailableReason, int quality, string mainTitle, string subDetails, int price, Sprite iconSprite)
        {
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(224f, 275f);
            }

            Color rarityColor = GetBotanicalRarityColor(quality);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            string rarityName = GetRarityName(quality);

            // 1. 背景色
            Image bgImg = cardObj.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.sprite = null;
                bgImg.color = isAvailable ? ColorCardBg : ColorCardDimBg;
                bgImg.raycastTarget = true;
            }

            // 2. 外枠（Outline）
            Outline outline = cardObj.GetComponent<Outline>();
            if (outline == null)
            {
                outline = cardObj.AddComponent<Outline>();
            }
            outline.effectDistance = new Vector2(2f, -2f);
            outline.effectColor = isAvailable ? rarityColor : new Color(0.20f, 0.26f, 0.22f, 0.35f);

            // 3. アイコン
            Image iconImg = null;
            foreach (var img in cardObj.GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject != cardObj && img.gameObject.name.Equals("Icon", System.StringComparison.OrdinalIgnoreCase))
                {
                    iconImg = img;
                    break;
                }
            }
            if (iconImg != null)
            {
                if (iconSprite != null)
                {
                    iconImg.gameObject.SetActive(true);
                    iconImg.sprite = iconSprite;
                    iconImg.color = isAvailable ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.40f);
                    iconImg.raycastTarget = false;
                }
                else
                {
                    iconImg.sprite = null;
                    iconImg.color = Color.clear;
                    iconImg.raycastTarget = false;
                }
            }

            // 4. テキスト
            var texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0)
            {
                var txt = texts[0];
                txt.raycastTarget = false;

                if (isAvailable)
                {
                    // 装備可能：明るく鮮やかな表示
                    string qualityStr = !string.IsNullOrEmpty(rarityName) ? $"<color=#{rarityHex}><size=80%>Quality: {quality} [{rarityName}]</size></color>\n" : "";
                    txt.text = $"<color=#e0f0e6><b>{mainTitle}</b></color>\n{qualityStr}<color=#98b8a6><size=78%>{subDetails}</size></color>\n\n<color=#ccad70><size=88%>Cost: {price} エーテル</size></color>";
                }
                else
                {
                    // 装備不可：暗く沈んだ表示＋不可理由ラベル
                    string qualityStr = !string.IsNullOrEmpty(rarityName) ? $"<color=#66756e><size=80%>Quality: {quality} [{rarityName}]</size></color>\n" : "";
                    txt.text = $"<color=#6f7e77><b>{mainTitle}</b></color>\n{qualityStr}<color=#505e57><size=78%>{subDetails}</size></color>\n\n<color=#604e38><size=88%>Cost: {price} エーテル</size></color>\n<color=#c44848><size=82%>[{unavailableReason}]</size></color>";
                }
            }

            // 5. ボタンの interactable（購入可能時のみ有効）
            Button btn = cardObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = isAvailable;
            }
        }

        public void ShowPopup(string message, float duration = 2f)
        {
            if (popupPanel == null || popupText == null) return;
            popupText.text = message;
            popupPanel.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HidePopupCoroutine(duration));
        }

        private IEnumerator HidePopupCoroutine(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            if (popupPanel != null) popupPanel.SetActive(false);
        }

        private GameObject confirmDimOverlay;

        private void SetupConfirmUIStyling(bool isSell = false)
        {
            if (confirmPanel == null) return;

            // 1. 全画面を完全に遮蔽する暗転オーバーレイ（背景の戦闘シーンが透けるのを100%防止）
            Transform parentCanvasTrans = confirmPanel.transform.parent;
            if (confirmDimOverlay == null && parentCanvasTrans != null)
            {
                confirmDimOverlay = new GameObject("ConfirmDimOverlay", typeof(RectTransform), typeof(Image));
                confirmDimOverlay.transform.SetParent(parentCanvasTrans, false);
                RectTransform dimRect = confirmDimOverlay.GetComponent<RectTransform>();
                dimRect.anchorMin = Vector2.zero;
                dimRect.anchorMax = Vector2.one;
                dimRect.sizeDelta = Vector2.zero;
                Image dimImg = confirmDimOverlay.GetComponent<Image>();
                dimImg.color = new Color(0.024f, 0.034f, 0.028f, 0.96f);
                dimImg.raycastTarget = true;
            }

            if (confirmDimOverlay != null)
            {
                confirmDimOverlay.transform.SetSiblingIndex(confirmPanel.transform.GetSiblingIndex());
                confirmDimOverlay.SetActive(true);
            }

            // 2. confirmPanel（ダイアログ本体）のスタイリング
            RectTransform pRect = confirmPanel.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.anchoredPosition = Vector2.zero;
            pRect.sizeDelta = new Vector2(760f, 540f);

            Image pImg = confirmPanel.GetComponent<Image>();
            if (pImg == null) pImg = confirmPanel.AddComponent<Image>();
            pImg.color = new Color(0.050f, 0.075f, 0.062f, 0.98f);
            pImg.raycastTarget = true;

            // 枠線・四隅アクセント
            AddCornerAccents(pRect, ColorBrassGold, 18f, 1.5f);

            // 3. テキストのスタイリング
            if (confirmExplainText != null)
            {
                RectTransform tRect = confirmExplainText.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(0.5f, 0.5f);
                tRect.anchorMax = new Vector2(0.5f, 0.5f);
                tRect.pivot = new Vector2(0.5f, 0.5f);
                tRect.anchoredPosition = new Vector2(0f, 40f);
                tRect.sizeDelta = new Vector2(680f, 380f);

                confirmExplainText.alignment = TextAlignmentOptions.TopLeft;
                confirmExplainText.fontSize = 17f;
                confirmExplainText.color = ColorPaleWhite;
                confirmExplainText.enableWordWrapping = true;
            }

            // 4. ボタンのスタイリング
            if (confirmYesButton != null)
            {
                RectTransform yRect = confirmYesButton.GetComponent<RectTransform>();
                yRect.anchorMin = new Vector2(0.5f, 0f);
                yRect.anchorMax = new Vector2(0.5f, 0f);
                yRect.pivot = new Vector2(0.5f, 0f);
                yRect.anchoredPosition = new Vector2(-150f, 30f);
                yRect.sizeDelta = new Vector2(250f, 54f);

                Image yImg = confirmYesButton.GetComponent<Image>();
                if (yImg != null)
                {
                    yImg.color = isSell ? new Color(0.55f, 0.22f, 0.18f, 0.98f) : new Color(0.16f, 0.38f, 0.25f, 0.98f);
                    yImg.raycastTarget = true;
                }

                TextMeshProUGUI yTmp = confirmYesButton.GetComponentInChildren<TextMeshProUGUI>();
                if (yTmp != null)
                {
                    yTmp.text = isSell ? "売却を実行する" : "決定";
                    yTmp.fontSize = 19f;
                    yTmp.fontStyle = FontStyles.Bold;
                    yTmp.color = ColorPaleWhite;
                }
            }

            if (confirmNoButton != null)
            {
                RectTransform nRect = confirmNoButton.GetComponent<RectTransform>();
                nRect.anchorMin = new Vector2(0.5f, 0f);
                nRect.anchorMax = new Vector2(0.5f, 0f);
                nRect.pivot = new Vector2(0.5f, 0f);
                nRect.anchoredPosition = new Vector2(150f, 30f);
                nRect.sizeDelta = new Vector2(250f, 54f);

                Image nImg = confirmNoButton.GetComponent<Image>();
                if (nImg != null)
                {
                    nImg.color = new Color(0.12f, 0.16f, 0.14f, 0.98f);
                    nImg.raycastTarget = true;
                }

                TextMeshProUGUI nTmp = confirmNoButton.GetComponentInChildren<TextMeshProUGUI>();
                if (nTmp != null)
                {
                    nTmp.text = "キャンセル";
                    nTmp.fontSize = 19f;
                    nTmp.color = ColorSubText;
                }
            }
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
            RectTransform crt = cornerGO.GetComponent<RectTransform>();
            crt.anchorMin = anchor;
            crt.anchorMax = anchor;
            crt.pivot = anchor;
            crt.sizeDelta = new Vector2(size, size);
            crt.anchoredPosition = Vector2.zero;

            // H line
            GameObject hLine = new GameObject("H", typeof(RectTransform), typeof(Image));
            hLine.transform.SetParent(cornerGO.transform, false);
            RectTransform hRect = hLine.GetComponent<RectTransform>();
            hRect.anchorMin = (anchor.y == 1f) ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
            hRect.anchorMax = (anchor.y == 1f) ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
            hRect.pivot = (anchor.y == 1f) ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
            hRect.sizeDelta = new Vector2(size, thickness);
            hRect.anchoredPosition = Vector2.zero;
            Image hImg = hLine.GetComponent<Image>();
            hImg.color = color;
            hImg.raycastTarget = false;

            // V line
            GameObject vLine = new GameObject("V", typeof(RectTransform), typeof(Image));
            vLine.transform.SetParent(cornerGO.transform, false);
            RectTransform vRect = vLine.GetComponent<RectTransform>();
            vRect.anchorMin = (anchor.x == 1f) ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            vRect.anchorMax = (anchor.x == 1f) ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            vRect.pivot = (anchor.x == 1f) ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            vRect.sizeDelta = new Vector2(thickness, size);
            vRect.anchoredPosition = Vector2.zero;
            Image vImg = vLine.GetComponent<Image>();
            vImg.color = color;
            vImg.raycastTarget = false;
        }

        private void ShowConfirmUI(string message, System.Action onConfirm, bool isSell = false)
        {
            if (confirmPanel == null || confirmExplainText == null) return;

            confirmExplainText.text = message;
            pendingConfirmAction = onConfirm;

            SetupConfirmUIStyling(isSell);

            confirmPanel.SetActive(true);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(true);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(true);
        }

        private void OnConfirmYes()
        {
            if (confirmDimOverlay != null) confirmDimOverlay.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
            if (panel != null) panel.SetActive(true);
        }

        private void OnConfirmNo()
        {
            if (confirmDimOverlay != null) confirmDimOverlay.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            pendingConfirmAction = null;
            if (panel != null) panel.SetActive(true);
        }

        // ==========================
        // パーツ購入モード
        // ==========================
        private void OnPartButtonClicked(WeaponPartInstance_Alpha partInstance, int price, GameObject buttonObj)
        {
            string details = "購入パーツ：" + partInstance.series.seriesName + " (" + partInstance.partType.ToString() + ")";
            string finalMessage = string.Format(partBuyConfirmFormat, details, price);

            ShowConfirmUI(finalMessage, () =>
            {
                if (playerStatusManager_Alpha.Instance.currentExp < price)
                {
                    ShowPopup("エーテルが不足しています！");
                    return;
                }

                playerStatusManager_Alpha.Instance.AddExp(-price);

                InventoryManager_Alpha.EquipInstance newPart = new InventoryManager_Alpha.EquipInstance();
                newPart.series = partInstance.series;
                newPart.partType = partInstance.partType;
                newPart.rarity = partInstance.quality;
                newPart.currentEffects = partInstance.currentEffects != null ? new List<WeaponEffectSO_Alpha>(partInstance.currentEffects) : new List<WeaponEffectSO_Alpha>();
                newPart.setBonusEffect = partInstance.setBonusEffect;
                InventoryManager_Alpha.Instance.AddItem(newPart);

                UpdateStatusDisplay();

                Button btn = buttonObj.GetComponent<Button>();
                if (btn != null) btn.interactable = false;

                ShowPopup("パーツを購入しました！");
                
                if (inventoryUI != null)
                {
                    inventoryUI.ShowForCheck(null);
                }
            });
        }

        // ==========================
        // スキル購入モード
        // ==========================
        private void OnSkillButtonClicked(WeaponEffectSO_Alpha effectSO, GameObject buttonObj)
        {
            if (playerStatusManager_Alpha.Instance.currentExp < effectSO.price)
            {
                ShowPopup("購入できません！（EXP不足）");
                return;
            }

            ShowPopup("付与する装備を選択してください。", 5f);
            if (panel != null) panel.SetActive(false);
            
            if (inventoryUI != null)
            {
                inventoryUI.ShowForSelection((slotIndex) =>
                {
                    TryApplySkillToItem(slotIndex, effectSO, buttonObj);
                }, () => {
                    if (panel != null) panel.SetActive(true);
                });
            }
        }

        private void TryApplySkillToItem(int slotIndex, WeaponEffectSO_Alpha effectSO, GameObject buttonObj)
        {
            if (inventoryUI != null) inventoryUI.currentMode = InventoryUI_Alpha.InventoryUIMode.Normal;
            if (inventoryUI != null) inventoryUI.Hide();

            var inv = InventoryManager_Alpha.Instance;
            if (inv == null || slotIndex < 0 || slotIndex >= inv.equipInstance.Count) 
            {
                if (panel != null) panel.SetActive(true);
                return;
            }

            var item = inv.equipInstance[slotIndex];
            if (item.series == null)
            {
                ShowPopup("装備がありません！");
                if (panel != null) panel.SetActive(true);
                return;
            }

            if (item.currentEffects == null) item.currentEffects = new List<WeaponEffectSO_Alpha>();

            if (item.currentEffects.Count >= item.rarity)
            {
                ShowPopup("購入できません！（付与枠上限：" + item.rarity + "）");
                if (panel != null) panel.SetActive(true);
                return;
            }

            string desc = effectSO.description;
            try { desc = string.Format(effectSO.description, effectSO.GetValue(1)); } catch {}
            string details = "付与スキル：" + effectSO.effectName + " - " + desc;

            string finalMessage = string.Format(skillBuyConfirmFormat, details, effectSO.price);

            ShowConfirmUI(finalMessage, () =>
            {
                if (playerStatusManager_Alpha.Instance.currentExp < effectSO.price)
                {
                    ShowPopup("エーテルが不足しています！");
                    return;
                }

                playerStatusManager_Alpha.Instance.AddExp(-effectSO.price);

                item.currentEffects.Add(effectSO);
                inv.equipInstance[slotIndex] = item;
                
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                Button btn = buttonObj.GetComponent<Button>();
                if (btn != null) btn.interactable = false;

                UpdateStatusDisplay();
                ShowPopup("スキルを付与しました！");
            });
        }

        // ==========================
        // 売却モード
        // ==========================
        private void EnterSellMode()
        {
            ShowPopup("売却する装備を選択してください。", 5f);
            if (panel != null) panel.SetActive(false);
            if (inventoryUI != null)
            {
                inventoryUI.ShowForSelection((slotIndex) =>
                {
                    TrySellItem(slotIndex);
                }, () => {
                    if (panel != null) panel.SetActive(true);
                });
            }
        }

        private void TrySellItem(int slotIndex)
        {
            if (inventoryUI != null) inventoryUI.currentMode = InventoryUI_Alpha.InventoryUIMode.Normal;
            if (inventoryUI != null) inventoryUI.Hide();

            var inv = InventoryManager_Alpha.Instance;
            if (inv == null || slotIndex < 0 || slotIndex >= inv.equipInstance.Count) 
            {
                if (panel != null) panel.SetActive(true);
                return;
            }

            var item = inv.equipInstance[slotIndex];
            if (item.series == null)
            {
                ShowPopup("未装備の枠です！");
                if (panel != null) panel.SetActive(true);
                return;
            }

            int totalSellValue = 0;
            int partSellPrice = (item.rarity >= 1 && item.rarity <= partPrices.Length) ? partPrices[item.rarity - 1] / 2 : partPrices[0] / 2;
            totalSellValue += partSellPrice;

            string rarityName = GetRarityName(item.rarity);
            string rarityHex = ColorUtility.ToHtmlStringRGB(GetBotanicalRarityColor(item.rarity));

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b><size=125%><color=#d4af37>【 装備パーツ売却の確認 】</color></size></b>\n");
            sb.AppendLine("<size=90%><color=#8fa89b>選択した装備パーツおよび付与スキルを売却します。</color></size>\n");

            sb.AppendLine($"<color=#c2d6cc>■ 売却パーツ：</color> <b><color=#{rarityHex}>{item.series.seriesName}</color></b> <color=#{rarityHex}>[{rarityName}]</color>  <color=#d4af37>+{partSellPrice} エーテル</color>");

            sb.AppendLine("<color=#c2d6cc>■ 付与スキル：</color>");
            bool hasSkills = false;
            if (item.currentEffects != null)
            {
                foreach (var effect in item.currentEffects)
                {
                    if (effect != null)
                    {
                        hasSkills = true;
                        totalSellValue += effect.sellPrice;
                        string desc = effect.description;
                        try { desc = string.Format(effect.description, effect.GetValue(1)); } catch {}
                        sb.AppendLine($"  • <b><color=#8ad4eb>{effect.effectName}</color></b>  <color=#d4af37>+{effect.sellPrice} エーテル</color>");
                        sb.AppendLine($"    <size=80%><color=#7d9e8c>{desc}</color></size>");
                    }
                }
            }
            if (!hasSkills)
            {
                sb.AppendLine("  <size=85%><color=#688072>• （付与スキルなし）</color></size>");
            }

            sb.AppendLine("\n--------------------------------------------------");
            sb.AppendLine($"<b>獲得エーテル合計： <size=130%><color=#f5c242>+{totalSellValue} エーテル</color></size></b>");

            string finalMessage = sb.ToString();

            ShowConfirmUI(finalMessage, () =>
            {
                if (item.currentEffects != null) item.currentEffects.Clear();
                item.setBonusEffect = null;
                item.series = null;
                item.defId = "";
                inv.equipInstance[slotIndex] = item;
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                playerStatusManager_Alpha.Instance.AddExp(totalSellValue);
                UpdateStatusDisplay();

                ShowPopup("パーツを売却し、" + totalSellValue + "エーテル獲得しました！");
            });
        }

        // ==========================
        // 品質アップモード
        // ==========================
        private void EnterUpgradeMode()
        {
            ShowPopup("強化する装備を選択してください。", 5f);
            if (panel != null) panel.SetActive(false);
            if (inventoryUI != null)
            {
                inventoryUI.ShowForSelection((slotIndex) =>
                {
                    TryUpgradeItem(slotIndex);
                }, () => {
                    if (panel != null) panel.SetActive(true);
                });
            }
        }

        private void TryUpgradeItem(int slotIndex)
        {
            if (inventoryUI != null) inventoryUI.currentMode = InventoryUI_Alpha.InventoryUIMode.Normal;
            if (inventoryUI != null) inventoryUI.Hide();

            var inv = InventoryManager_Alpha.Instance;
            if (inv == null || slotIndex < 0 || slotIndex >= inv.equipInstance.Count) 
            {
                if (panel != null) panel.SetActive(true);
                return;
            }

            var item = inv.equipInstance[slotIndex];
            if (item.series == null)
            {
                ShowPopup("装備がありません！");
                if (panel != null) panel.SetActive(true);
                return;
            }

            if (item.rarity >= 4)
            {
                ShowPopup("最高品質です！");
                if (panel != null) panel.SetActive(true);
                return;
            }

            int cost = item.rarity * upgradeCostMultiplier;
            if (playerStatusManager_Alpha.Instance.currentExp < cost)
            {
                ShowPopup("強化できません！（エーテル不足：" + cost + "必要）");
                if (panel != null) panel.SetActive(true);
                return;
            }

            string finalMessage = string.Format(upgradeConfirmFormat, cost);

            ShowConfirmUI(finalMessage, () =>
            {
                if (playerStatusManager_Alpha.Instance.currentExp < cost)
                {
                    ShowPopup("エーテルが不足しています！");
                    return;
                }

                playerStatusManager_Alpha.Instance.AddExp(-cost);
                item.rarity += 1;
                inv.equipInstance[slotIndex] = item;
                
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                UpdateStatusDisplay();
                ShowPopup("装備の品質が " + item.rarity + " に上がりました！");
            });
        }

        // ==========================
        // Modern Botanical UI Styling & Layout
        // ==========================
        private void ApplyModernBotanicalStyling()
        {
            if (panel == null) return;

            // 1. 画面フル統一（Fullscreen Stretch）
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.localScale = Vector3.one;
            }

            // 背景イメージを完全不透明な深森墨黒（ColorBaseInk）に設定
            Image panelImg = panel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.sprite = null;
                panelImg.color = ColorBaseInk;
                panelImg.raycastTarget = true;
            }

            // 最背面に完全不透明なフルスクリーン遮蔽板（Blocker）を配置
            Transform blockerTrans = panel.transform.Find("ShopFullscreenBlocker");
            if (blockerTrans == null)
            {
                GameObject blockerGO = new GameObject("ShopFullscreenBlocker", typeof(RectTransform), typeof(Image));
                blockerGO.transform.SetParent(panel.transform, false);
                blockerGO.transform.SetAsFirstSibling();
                RectTransform bRect = blockerGO.GetComponent<RectTransform>();
                bRect.anchorMin = Vector2.zero;
                bRect.anchorMax = Vector2.one;
                bRect.offsetMin = new Vector2(-200f, -200f);
                bRect.offsetMax = new Vector2(200f, 200f);
                Image bImg = blockerGO.GetComponent<Image>();
                bImg.sprite = null;
                bImg.color = ColorBaseInk;
                bImg.raycastTarget = true;
            }
            else
            {
                blockerTrans.SetAsFirstSibling();
                Image bImg = blockerTrans.GetComponent<Image>();
                if (bImg != null)
                {
                    bImg.sprite = null;
                    bImg.color = ColorBaseInk;
                }
            }

            if (isStylingApplied) return;
            isStylingApplied = true;

            // フォントアセットの探索
            TMP_FontAsset fontAsset = null;
            if (popupText != null) fontAsset = popupText.font;
            if (fontAsset == null && emptySlotText != null) fontAsset = emptySlotText.font;

            // 2. ヘッダーの生成・調整
            Transform headerTrans = panel.transform.Find("BotanicalShopHeader");
            if (headerTrans == null)
            {
                GameObject headerGO = new GameObject("BotanicalShopHeader", typeof(RectTransform));
                headerGO.transform.SetParent(panel.transform, false);
                RectTransform hRect = headerGO.GetComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0.5f, 1f);
                hRect.anchorMax = new Vector2(0.5f, 1f);
                hRect.pivot = new Vector2(0.5f, 1f);
                hRect.anchoredPosition = new Vector2(0f, -15f);
                hRect.sizeDelta = new Vector2(1600f, 85f);

                // タイトルテキスト
                GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
                titleGO.transform.SetParent(headerGO.transform, false);
                RectTransform tRect = titleGO.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(0.5f, 1f);
                tRect.anchorMax = new Vector2(0.5f, 1f);
                tRect.pivot = new Vector2(0.5f, 1f);
                tRect.anchoredPosition = new Vector2(0f, 0f);
                tRect.sizeDelta = new Vector2(1400f, 42f);
                TextMeshProUGUI tTmp = titleGO.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tTmp.font = fontAsset;
                tTmp.text = "休憩処・鍛冶工房  <size=65%><color=#90a89c>— Rest Sanctuary & Forge —</color></size>";
                tTmp.fontSize = 28f;
                tTmp.fontStyle = FontStyles.Bold;
                tTmp.alignment = TextAlignmentOptions.Center;
                tTmp.color = ColorBrassGold;
                tTmp.raycastTarget = false;

                // サブタイトル（ガイダンス）
                GameObject subGO = new GameObject("SubTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
                subGO.transform.SetParent(headerGO.transform, false);
                RectTransform sRect = subGO.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0.5f, 1f);
                sRect.anchorMax = new Vector2(0.5f, 1f);
                sRect.pivot = new Vector2(0.5f, 1f);
                sRect.anchoredPosition = new Vector2(0f, -42f);
                sRect.sizeDelta = new Vector2(1400f, 28f);
                TextMeshProUGUI sTmp = subGO.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) sTmp.font = fontAsset;
                sTmp.text = "聖域の湧水により体力が全回復しました。ボス戦に向けて装備の強化・購入を行ってください。";
                sTmp.fontSize = 16f;
                sTmp.alignment = TextAlignmentOptions.Center;
                sTmp.color = ColorSubText;
                sTmp.raycastTarget = false;

                // 境界金線
                GameObject lineGO = new GameObject("GoldDividerLine", typeof(RectTransform), typeof(Image));
                lineGO.transform.SetParent(headerGO.transform, false);
                RectTransform lRect = lineGO.GetComponent<RectTransform>();
                lRect.anchorMin = new Vector2(0.5f, 0f);
                lRect.anchorMax = new Vector2(0.5f, 0f);
                lRect.pivot = new Vector2(0.5f, 0f);
                lRect.anchoredPosition = new Vector2(0f, 0f);
                lRect.sizeDelta = new Vector2(1500f, 2f);
                Image lImg = lineGO.GetComponent<Image>();
                lImg.color = ColorBrassGoldDim;
                lImg.raycastTarget = false;
            }

            // 3. ステータスバー（所持エーテルのみをすっきり表示：空きスロット表示は削除）
            Transform statusTrans = panel.transform.Find("BotanicalShopStatusBar");
            if (statusTrans == null)
            {
                GameObject statusGO = new GameObject("BotanicalShopStatusBar", typeof(RectTransform));
                statusGO.transform.SetParent(panel.transform, false);
                RectTransform sbRect = statusGO.GetComponent<RectTransform>();
                sbRect.anchorMin = new Vector2(0.5f, 1f);
                sbRect.anchorMax = new Vector2(0.5f, 1f);
                sbRect.pivot = new Vector2(0.5f, 1f);
                sbRect.anchoredPosition = new Vector2(0f, -105f);
                sbRect.sizeDelta = new Vector2(1500f, 36f);

                // 所持エーテル表示（見やすく左中央に配置）
                GameObject etherGO = new GameObject("EtherStatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
                etherGO.transform.SetParent(statusGO.transform, false);
                RectTransform eRect = etherGO.GetComponent<RectTransform>();
                eRect.anchorMin = new Vector2(0f, 0.5f);
                eRect.anchorMax = new Vector2(0f, 0.5f);
                eRect.pivot = new Vector2(0f, 0.5f);
                eRect.anchoredPosition = new Vector2(60f, 0f);
                eRect.sizeDelta = new Vector2(500f, 36f);
                etherStatusText = etherGO.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) etherStatusText.font = fontAsset;
                etherStatusText.fontSize = 22f;
                etherStatusText.fontStyle = FontStyles.Bold;
                etherStatusText.color = ColorPaleWhite;
                etherStatusText.text = "所持エーテル: 0";
                etherStatusText.raycastTarget = false;
            }

            // 既存の emptySlotText は非表示・無効化
            if (emptySlotText != null)
            {
                emptySlotText.gameObject.SetActive(false);
            }

            // 4. ショップセクションの左右均等・整列レイアウト構築
            // --- 左側セクション：神格スキル付与 ---
            Transform skillPurchaser = panel.transform.Find("SkillPurchaser");
            if (skillPurchaser != null)
            {
                RectTransform spRect = skillPurchaser.GetComponent<RectTransform>();
                spRect.anchorMin = new Vector2(0.5f, 0.5f);
                spRect.anchorMax = new Vector2(0.5f, 0.5f);
                spRect.pivot = new Vector2(0.5f, 0.5f);
                spRect.anchoredPosition = new Vector2(-430f, -15f);
                spRect.sizeDelta = new Vector2(760f, 520f);

                // 見出しテキスト
                var headerTmp = skillPurchaser.GetComponentInChildren<TextMeshProUGUI>(true);
                if (headerTmp != null)
                {
                    headerTmp.text = "◆ 神格スキル付与 ◆";
                    headerTmp.color = ColorBrassGold;
                    headerTmp.fontSize = 22f;
                    headerTmp.fontStyle = FontStyles.Bold;
                    headerTmp.alignment = TextAlignmentOptions.Center;
                    headerTmp.raycastTarget = false;

                    RectTransform htRect = headerTmp.GetComponent<RectTransform>();
                    htRect.anchorMin = new Vector2(0.5f, 1f);
                    htRect.anchorMax = new Vector2(0.5f, 1f);
                    htRect.pivot = new Vector2(0.5f, 1f);
                    htRect.anchoredPosition = new Vector2(0f, -15f);
                    htRect.sizeDelta = new Vector2(700f, 36f);
                }

                // スキルコンテナ（SkillGroup）の整列
                if (skillButtonsContainer != null)
                {
                    RectTransform sbcRect = skillButtonsContainer.GetComponent<RectTransform>();
                    sbcRect.anchorMin = new Vector2(0.5f, 0.5f);
                    sbcRect.anchorMax = new Vector2(0.5f, 0.5f);
                    sbcRect.pivot = new Vector2(0.5f, 0.5f);
                    sbcRect.anchoredPosition = new Vector2(0f, -25f);
                    sbcRect.sizeDelta = new Vector2(740f, 320f);

                    var hlg = skillButtonsContainer.GetComponent<HorizontalLayoutGroup>();
                    if (hlg == null) hlg = skillButtonsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.spacing = 24f;
                    hlg.childAlignment = TextAnchor.MiddleCenter;
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = false;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = false;
                }
            }

            // --- 右側セクション：霊核パーツ販売 ---
            Transform partsPurchaser = panel.transform.Find("PartsPurchaser");
            if (partsPurchaser == null)
            {
                GameObject ppGO = new GameObject("PartsPurchaser", typeof(RectTransform));
                ppGO.transform.SetParent(panel.transform, false);
                partsPurchaser = ppGO.transform;
            }

            RectTransform ppRect = partsPurchaser.GetComponent<RectTransform>();
            ppRect.anchorMin = new Vector2(0.5f, 0.5f);
            ppRect.anchorMax = new Vector2(0.5f, 0.5f);
            ppRect.pivot = new Vector2(0.5f, 0.5f);
            ppRect.anchoredPosition = new Vector2(430f, -15f);
            ppRect.sizeDelta = new Vector2(760f, 520f);

            // パーツ見出しテキスト
            Transform pHeaderTrans = partsPurchaser.Find("PartsHeader");
            TextMeshProUGUI pHeaderTmp = pHeaderTrans != null ? pHeaderTrans.GetComponent<TextMeshProUGUI>() : null;
            if (pHeaderTmp == null)
            {
                GameObject phGO = new GameObject("PartsHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
                phGO.transform.SetParent(partsPurchaser, false);
                pHeaderTmp = phGO.GetComponent<TextMeshProUGUI>();
            }
            if (fontAsset != null) pHeaderTmp.font = fontAsset;
            pHeaderTmp.text = "◆ 霊核パーツ販売 ◆";
            pHeaderTmp.color = ColorBrassGold;
            pHeaderTmp.fontSize = 22f;
            pHeaderTmp.fontStyle = FontStyles.Bold;
            pHeaderTmp.alignment = TextAlignmentOptions.Center;
            pHeaderTmp.raycastTarget = false;

            RectTransform phtRect = pHeaderTmp.GetComponent<RectTransform>();
            phtRect.anchorMin = new Vector2(0.5f, 1f);
            phtRect.anchorMax = new Vector2(0.5f, 1f);
            phtRect.pivot = new Vector2(0.5f, 1f);
            phtRect.anchoredPosition = new Vector2(0f, -15f);
            phtRect.sizeDelta = new Vector2(700f, 36f);

            // パーツコンテナの整列（未設定の場合は自動生成してアサイン）
            if (partButtonsContainer == null || partButtonsContainer == skillButtonsContainer)
            {
                Transform pGroupTrans = partsPurchaser.Find("PartGroup");
                if (pGroupTrans == null)
                {
                    GameObject pgGO = new GameObject("PartGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                    pgGO.transform.SetParent(partsPurchaser, false);
                    partButtonsContainer = pgGO.transform;
                }
                else
                {
                    partButtonsContainer = pGroupTrans;
                }
            }

            if (partButtonsContainer != null)
            {
                partButtonsContainer.SetParent(partsPurchaser, false);
                RectTransform pbcRect = partButtonsContainer.GetComponent<RectTransform>();
                pbcRect.anchorMin = new Vector2(0.5f, 0.5f);
                pbcRect.anchorMax = new Vector2(0.5f, 0.5f);
                pbcRect.pivot = new Vector2(0.5f, 0.5f);
                pbcRect.anchoredPosition = new Vector2(0f, -25f);
                pbcRect.sizeDelta = new Vector2(740f, 320f);

                var hlg = partButtonsContainer.GetComponent<HorizontalLayoutGroup>();
                if (hlg == null) hlg = partButtonsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 24f;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
            }

            // 中央分割金線（左右のセクションの調和・境界線）
            Transform centerLineTrans = panel.transform.Find("CenterDividerLine");
            if (centerLineTrans == null)
            {
                GameObject clGO = new GameObject("CenterDividerLine", typeof(RectTransform), typeof(Image));
                clGO.transform.SetParent(panel.transform, false);
                RectTransform clRect = clGO.GetComponent<RectTransform>();
                clRect.anchorMin = new Vector2(0.5f, 0.5f);
                clRect.anchorMax = new Vector2(0.5f, 0.5f);
                clRect.pivot = new Vector2(0.5f, 0.5f);
                clRect.anchoredPosition = new Vector2(0f, -15f);
                clRect.sizeDelta = new Vector2(2f, 440f);
                Image clImg = clGO.GetComponent<Image>();
                clImg.color = ColorBrassGoldDim;
                clImg.raycastTarget = false;
            }

            // 5. 下部コントロールバー（ボタン群）のレイアウト補正 & 最前面化
            Transform shopGroup = panel.transform.Find("ShopGroup");
            if (shopGroup != null)
            {
                // 最前面に移動して Raycast 遮断を絶対に防止
                shopGroup.SetAsLastSibling();

                RectTransform sgRect = shopGroup.GetComponent<RectTransform>();
                sgRect.anchorMin = new Vector2(0.5f, 0f);
                sgRect.anchorMax = new Vector2(0.5f, 0f);
                sgRect.pivot = new Vector2(0.5f, 0f);
                sgRect.anchoredPosition = new Vector2(0f, 32f);
                sgRect.sizeDelta = new Vector2(1500f, 75f);
                sgRect.localScale = Vector3.one;

                // ボタンのスタイリング
                if (sellModeButton != null)
                {
                    RectTransform bRect = sellModeButton.GetComponent<RectTransform>();
                    bRect.anchorMin = new Vector2(0.5f, 0.5f);
                    bRect.anchorMax = new Vector2(0.5f, 0.5f);
                    bRect.pivot = new Vector2(0.5f, 0.5f);
                    bRect.anchoredPosition = new Vector2(-340f, 0f);
                    bRect.sizeDelta = new Vector2(240f, 58f);

                    Image bImg = sellModeButton.GetComponent<Image>();
                    if (bImg != null)
                    {
                        bImg.color = new Color(0.12f, 0.22f, 0.18f, 0.95f);
                        bImg.raycastTarget = true;
                    }

                    TextMeshProUGUI bTmp = sellModeButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (bTmp != null)
                    {
                        bTmp.text = "パーツ売却";
                        bTmp.color = ColorPaleWhite;
                        bTmp.fontSize = 20f;
                        bTmp.raycastTarget = false;
                    }
                }

                if (upgradeModeButton != null)
                {
                    RectTransform bRect = upgradeModeButton.GetComponent<RectTransform>();
                    bRect.anchorMin = new Vector2(0.5f, 0.5f);
                    bRect.anchorMax = new Vector2(0.5f, 0.5f);
                    bRect.pivot = new Vector2(0.5f, 0.5f);
                    bRect.anchoredPosition = new Vector2(-50f, 0f);
                    bRect.sizeDelta = new Vector2(240f, 58f);

                    Image bImg = upgradeModeButton.GetComponent<Image>();
                    if (bImg != null)
                    {
                        bImg.color = new Color(0.24f, 0.18f, 0.12f, 0.95f);
                        bImg.raycastTarget = true;
                    }

                    TextMeshProUGUI bTmp = upgradeModeButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (bTmp != null)
                    {
                        bTmp.text = "品質強化";
                        bTmp.color = ColorPaleWhite;
                        bTmp.fontSize = 20f;
                        bTmp.raycastTarget = false;
                    }
                }

                if (nextPhaseButton != null)
                {
                    nextPhaseButton.gameObject.SetActive(true);
                    nextPhaseButton.interactable = true;

                    // リスナーを確実にバインド
                    nextPhaseButton.onClick.RemoveAllListeners();
                    nextPhaseButton.onClick.AddListener(CloseBlacksmith);

                    RectTransform bRect = nextPhaseButton.GetComponent<RectTransform>();
                    bRect.anchorMin = new Vector2(0.5f, 0.5f);
                    bRect.anchorMax = new Vector2(0.5f, 0.5f);
                    bRect.pivot = new Vector2(0.5f, 0.5f);
                    bRect.anchoredPosition = new Vector2(300f, 0f);
                    bRect.sizeDelta = new Vector2(340f, 64f);

                    Image bImg = nextPhaseButton.GetComponent<Image>();
                    if (bImg != null)
                    {
                        bImg.color = new Color(0.62f, 0.48f, 0.20f, 1f); // 華麗な真鍮金
                        bImg.raycastTarget = true;
                    }

                    TextMeshProUGUI bTmp = nextPhaseButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (bTmp != null)
                    {
                        bTmp.text = "ボス戦へ進む ▶";
                        bTmp.color = Color.white;
                        bTmp.fontSize = 22f;
                        bTmp.fontStyle = FontStyles.Bold;
                        bTmp.raycastTarget = false;
                    }
                }
            }
        }
    }
}
