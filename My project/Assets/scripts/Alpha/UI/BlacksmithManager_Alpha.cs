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
        public string buyConfirmFormat = "これを購入・付与します。\n{0}\n消費エーテル: {1}\nよろしかったですか？";

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
        private List<GameObject> spawnedSkillButtons = new List<GameObject>();

        [Header("General Buttons")]
        public Button sellModeButton;
        public Button upgradeModeButton;
        public Button nextPhaseButton;
        public TextMeshProUGUI emptySlotText;
        
        [Header("Costs")]
        [Tooltip("装備アップグレードのコスト倍率 (現在の品質 * n)")]
        public int upgradeCostMultiplier = 50;

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
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            if (sellModeButton != null) sellModeButton.onClick.AddListener(EnterSellMode);
            if (upgradeModeButton != null) upgradeModeButton.onClick.AddListener(EnterUpgradeMode);
            if (nextPhaseButton != null) nextPhaseButton.onClick.AddListener(CloseBlacksmith);
            
            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmYes);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(OnConfirmNo);
            
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
        }

        public void OpenBlacksmith()
        {
            Debug.Log("[Blacksmith] Blacksmith Phase started.");
            
            if (playerStatusManager_Alpha.Instance != null)
            {
                playerStatusManager_Alpha.Instance.Heal(playerStatusManager_Alpha.Instance.HP);
            }

            if (panel != null) panel.SetActive(true);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);

            GenerateSkillShop();
            GeneratePartShop();
            UpdateEmptySlotDisplay();
        }

        public void UpdateEmptySlotDisplay()
        {
            if (emptySlotText != null && InventoryManager_Alpha.Instance != null)
            {
                var inv = InventoryManager_Alpha.Instance;
                int maxSlots = InventoryManager_Alpha.BASIC_SLOT_COUNT + inv.freeSlotCount;
                int usedSlots = 0;
                for (int i = 0; i < maxSlots; i++)
                {
                    if (i < inv.equipInstance.Count && inv.equipInstance[i].series != null)
                    {
                        usedSlots++;
                    }
                }
                int emptySlots = maxSlots - usedSlots;
                emptySlotText.text = $"空きスロット: {emptySlots} / {maxSlots}";
            }
        }

        public void CloseBlacksmith()
        {
            if (panel != null) panel.SetActive(false);
            
            if (StageManager_Alpha.Instance != null)
            {
                StageManager_Alpha.Instance.StartPostBlacksmithADV();
            }
        }

        private void GenerateSkillShop()
        {
            foreach (var btn in spawnedSkillButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedSkillButtons.Clear();

            var generator = WeaponGenerator_Alpha.Instance;
            if (generator == null || generator.globalBuffEffects == null || generator.globalBuffEffects.Count == 0 || skillButtonPrefab == null || skillButtonsContainer == null) return;

            for (int i = 0; i < 5; i++)
            {
                var effectSO = generator.globalBuffEffects[Random.Range(0, generator.globalBuffEffects.Count)];
                if (effectSO == null) continue;

                GameObject obj = Instantiate(skillButtonPrefab, skillButtonsContainer);
                obj.SetActive(true);
                spawnedSkillButtons.Add(obj);

                var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) texts[0].text = effectSO.effectName;
                if (texts.Length > 1) texts[1].text = "Cost: " + effectSO.price + " EXP";

                Button btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnSkillButtonClicked(effectSO, obj));
                    
                    var rcd = btn.gameObject.AddComponent<RightClickDetector_Alpha>();
                    rcd.onRightClick = (eventData) =>
                    {
                        if (detailPopup != null)
                        {
                            detailPopup.SetupForSkill(effectSO, eventData.position);
                        }
                    };
                }
            }
        }

        private void GeneratePartShop()
        {
            foreach (var btn in spawnedPartButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedPartButtons.Clear();

            var generator = WeaponGenerator_Alpha.Instance;
            if (generator == null || generator.allSeriesPool == null || generator.allSeriesPool.Count == 0 || partButtonPrefab == null || partButtonsContainer == null) return;

            for (int i = 0; i < partShopCount; i++)
            {
                var randomSeries = generator.allSeriesPool[Random.Range(0, generator.allSeriesPool.Count)];
                if (randomSeries == null) continue;
                
                WeaponPartType_Alpha randomPartType = (WeaponPartType_Alpha)Random.Range(0, 3);
                int rarity = Random.Range(1, 5); // 1 to 4
                int price = (rarity >= 1 && rarity <= partPrices.Length) ? partPrices[rarity - 1] : partPrices[0];

                GameObject obj = Instantiate(partButtonPrefab, partButtonsContainer);
                obj.SetActive(true);
                spawnedPartButtons.Add(obj);

                var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) texts[0].text = randomSeries.seriesName + " (" + randomPartType.ToString() + ")\n<size=80%>Quality: " + rarity + "</size>";
                if (texts.Length > 1) texts[1].text = "Cost: " + price + " EXP";

                Image iconImg = null;
                foreach (var img in obj.GetComponentsInChildren<Image>(true))
                {
                    if (img.gameObject.name.Equals("Icon", System.StringComparison.OrdinalIgnoreCase))
                    {
                        iconImg = img;
                        break;
                    }
                }

                if (iconImg != null)
                {
                    Sprite targetSprite = null;
                    if (randomPartType == WeaponPartType_Alpha.Bullet) targetSprite = randomSeries.iconBullet;
                    else if (randomPartType == WeaponPartType_Alpha.Casing) targetSprite = randomSeries.iconCasing;
                    else if (randomPartType == WeaponPartType_Alpha.Primer) targetSprite = randomSeries.iconPrimer;
                    
                    if (targetSprite == null) targetSprite = randomSeries.icon;
                    
                    if (targetSprite != null) {
                        iconImg.sprite = targetSprite;
                        iconImg.color = Color.white;
                    } else {
                        iconImg.sprite = null;
                        iconImg.color = Color.clear;
                    }
                }

                Image bgImg = obj.GetComponent<Image>();
                if (bgImg != null)
                {
                    switch (rarity)
                    {
                        case 1: bgImg.color = new Color32(128, 38, 5, 255); break;
                        case 2: bgImg.color = new Color32(136, 136, 136, 255); break;
                        case 3: bgImg.color = new Color32(238, 156, 23, 255); break;
                        case 4: bgImg.color = new Color32(1, 240, 91, 255); break;
                    }
                }

                Button btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnPartButtonClicked(randomSeries, randomPartType, rarity, price, obj));
                    
                    var rcd = btn.gameObject.AddComponent<RightClickDetector_Alpha>();
                    rcd.onRightClick = (eventData) =>
                    {
                        if (detailPopup != null)
                        {
                            detailPopup.Setup(randomSeries, randomPartType, rarity, null, eventData.position);
                        }
                    };
                }
            }
        }

        private void ShowPopup(string message, float duration = 2f)
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

        private void ShowConfirmUI(string message, System.Action onConfirm)
        {
            if (confirmPanel == null || confirmExplainText == null) return;

            confirmExplainText.text = message;
            pendingConfirmAction = onConfirm;

            confirmPanel.SetActive(true);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(true);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(true);
        }

        private void OnConfirmYes()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
            if (panel != null) panel.SetActive(true);
        }

        private void OnConfirmNo()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            pendingConfirmAction = null;
            if (panel != null) panel.SetActive(true);
        }

        // ==========================
        // パーツ購入モード
        // ==========================
        private void OnPartButtonClicked(WeaponSeriesData_Alpha series, WeaponPartType_Alpha partType, int rarity, int price, GameObject buttonObj)
        {
            string details = "購入パーツ：" + series.seriesName + " (" + partType.ToString() + ")";
            string finalMessage = string.Format(buyConfirmFormat, details, price);

            ShowConfirmUI(finalMessage, () =>
            {
                if (playerStatusManager_Alpha.Instance.currentExp < price)
                {
                    ShowPopup("エーテルが不足しています！");
                    return;
                }

                playerStatusManager_Alpha.Instance.AddExp(-price);

                InventoryManager_Alpha.EquipInstance newPart = new InventoryManager_Alpha.EquipInstance();
                newPart.series = series;
                newPart.partType = partType;
                newPart.rarity = rarity;
                newPart.currentEffects = new List<WeaponEffectSO_Alpha>();
                InventoryManager_Alpha.Instance.AddItem(newPart);

                UpdateEmptySlotDisplay();

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
                ShowPopup("購入できません！（付与枠上限：" + item.rarity + "");
                if (panel != null) panel.SetActive(true);
                return;
            }

            string desc = effectSO.description;
            try { desc = string.Format(effectSO.description, effectSO.GetValue(1)); } catch {}
            string details = "付与スキル：" + effectSO.effectName + " - " + desc;

            string finalMessage = string.Format(buyConfirmFormat, details, effectSO.price);

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

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("売却パーツ：" + item.series.seriesName + " - " + partSellPrice + "エーテル");

            if (item.currentEffects != null)
            {
                foreach (var effect in item.currentEffects)
                {
                    if (effect != null)
                    {
                        totalSellValue += effect.sellPrice;
                        string desc = effect.description;
                        try { desc = string.Format(effect.description, effect.GetValue(1)); } catch {}
                        sb.AppendLine("売却スキル：" + effect.effectName + " - " + desc);
                        sb.AppendLine("売却額:" + effect.sellPrice + "エーテル");
                    }
                }
            }

            string details = sb.ToString().TrimEnd();
            string finalMessage = string.Format(sellConfirmFormat, details, totalSellValue);

            ShowConfirmUI(finalMessage, () =>
            {
                if (item.currentEffects != null) item.currentEffects.Clear();
                item.series = null;
                item.defId = "";
                inv.equipInstance[slotIndex] = item;
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                UpdateEmptySlotDisplay();

                playerStatusManager_Alpha.Instance.AddExp(totalSellValue);

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

                ShowPopup("装備の品質が " + item.rarity + " に上がりました！");
            });
        }
    }
}
