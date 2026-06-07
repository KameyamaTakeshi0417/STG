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

        [Header("Action Confirm UI")]
        public GameObject confirmPanel;
        public TextMeshProUGUI confirmExplainText;
        public Button confirmYesButton;
        public Button confirmNoButton;

        [Header("Confirm Text Formats")]
        [TextArea(3, 5)]
        public string sellConfirmFormat = "装備のスキルを売却します。\n{0}\n合計{1}エーテル\nよろしかったですか？";
        [TextArea(3, 5)]
        public string upgradeConfirmFormat = "これをアップグレードします。\n消費エーテル: {0}\nよろしかったですか？";
        [TextArea(3, 5)]
        public string buyConfirmFormat = "このスキルを付与します。\n{0}\n消費エーテル: {1}\nよろしかったですか？";

        private System.Action pendingConfirmAction;

        [Header("Skill Shop Settings")]
        public Transform skillButtonsContainer;
        public GameObject skillButtonPrefab;
        private List<GameObject> spawnedSkillButtons = new List<GameObject>();

        [Header("General Buttons")]
        public Button sellModeButton;
        public Button upgradeModeButton;
        public Button nextPhaseButton;
        
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
        }

        public void OpenBlacksmith()
        {
            Debug.Log("[Blacksmith] Blacksmith Phase started.");
            
            // 1. 体力の全回復
            if (playerStatusManager_Alpha.Instance != null)
            {
                playerStatusManager_Alpha.Instance.Heal(playerStatusManager_Alpha.Instance.HP);
            }

            // 2. ショップの展開
            if (panel != null) panel.SetActive(true);

            // 3. スキルの抽選
            GenerateSkillShop();
        }

        public void CloseBlacksmith()
        {
            if (panel != null) panel.SetActive(false);
            
            // 次のフェーズへ進む
            if (StageManager_Alpha.Instance != null)
            {
                StageManager_Alpha.Instance.StartPreBossADVAndFight();
            }
        }

        private void GenerateSkillShop()
        {
            // 古いボタンを削除
            foreach (var btn in spawnedSkillButtons)
            {
                if (btn != null) Destroy(btn);
            }
            spawnedSkillButtons.Clear();

            var generator = WeaponGenerator_Alpha.Instance;
            if (generator == null || generator.globalBuffEffects == null || generator.globalBuffEffects.Count == 0 || skillButtonPrefab == null || skillButtonsContainer == null) return;

            // ランダムに5つ選出
            for (int i = 0; i < 5; i++)
            {
                var effectSO = generator.globalBuffEffects[Random.Range(0, generator.globalBuffEffects.Count)];
                if (effectSO == null) continue;

                GameObject obj = Instantiate(skillButtonPrefab, skillButtonsContainer);
                obj.SetActive(true);
                spawnedSkillButtons.Add(obj);

                // テキスト等の設定
                var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) texts[0].text = effectSO.effectName;
                if (texts.Length > 1) texts[1].text = $"Cost: {effectSO.price} EXP";

                Button btn = obj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnSkillButtonClicked(effectSO, obj));
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
            if (panel != null) panel.SetActive(true); // フォージパネルに戻る
        }

        private void OnConfirmNo()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(false);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(false);

            pendingConfirmAction = null;
            if (panel != null) panel.SetActive(true); // フォージパネルに戻る
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

            // 固有でないスキルの上限チェック（品質数まで）
            if (item.currentEffects.Count >= item.rarity)
            {
                ShowPopup($"購入できません！（付与枠上限：{item.rarity}）");
                if (panel != null) panel.SetActive(true);
                return;
            }

            // EXP消費（エーテル）確認
            string details = $"付与スキル：{effectSO.effectName} - {effectSO.description}";
            string finalMessage = string.Format(buyConfirmFormat, details, effectSO.price);

            ShowConfirmUI(finalMessage, () =>
            {
                if (playerStatusManager_Alpha.Instance.currentExp < effectSO.price)
                {
                    ShowPopup("エーテルが不足しています！");
                    return;
                }

                // EXP消費
                playerStatusManager_Alpha.Instance.AddExp(-effectSO.price);

                // スキル付与
                item.currentEffects.Add(effectSO);
                inv.equipInstance[slotIndex] = item;
                
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                // ボタンを無効化（1回のみ購入可能）
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
                ShowPopup("装備がありません！");
                if (panel != null) panel.SetActive(true);
                return;
            }

            if (item.currentEffects == null || item.currentEffects.Count == 0)
            {
                ShowPopup("売却できるスキルがありません！");
                if (panel != null) panel.SetActive(true);
                return;
            }

            // 売却額の計算とテキストの作成
            int totalSellValue = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var effect in item.currentEffects)
            {
                if (effect != null)
                {
                    totalSellValue += effect.sellPrice;
                    sb.AppendLine($"売却スキル：{effect.effectName} - {effect.description}");
                    sb.AppendLine($"売却額:{effect.sellPrice}エーテル");
                }
            }

            string details = sb.ToString().TrimEnd();
            string finalMessage = string.Format(sellConfirmFormat, details, totalSellValue);

            ShowConfirmUI(finalMessage, () =>
            {
                // 売却確定処理（スキルのみ削除）
                item.currentEffects.Clear();
                inv.equipInstance[slotIndex] = item;
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                // EXP付与
                playerStatusManager_Alpha.Instance.AddExp(totalSellValue);

                ShowPopup($"スキルを売却し、{totalSellValue}エーテルを獲得しました！");
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
                ShowPopup($"強化できません！（エーテル不足：{cost}必要）");
                if (panel != null) panel.SetActive(true);
                return;
            }

            string finalMessage = string.Format(upgradeConfirmFormat, cost);

            ShowConfirmUI(finalMessage, () =>
            {
                // エーテル再確認
                if (playerStatusManager_Alpha.Instance.currentExp < cost)
                {
                    ShowPopup("エーテルが不足しています！");
                    return;
                }

                // EXP消費と品質アップ
                playerStatusManager_Alpha.Instance.AddExp(-cost);
                item.rarity += 1;
                inv.equipInstance[slotIndex] = item;
                
                // 品質アップの際は上限（HPGauge等）のみを再計算
                playerStatusManager_Alpha.Instance.UpdateEquipmentBuffs();

                ShowPopup($"装備の品質が {item.rarity} に上がりました！");
            });
        }
    }
}
