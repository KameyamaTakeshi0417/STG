using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alpha.Data;

namespace Alpha.UI
{
    public class InventoryUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        
        [Header("Grid (3x3)")]
        [Tooltip("装備枠(3x3)のボタンスロット。インデックスは 0〜8 (y*3+x)")]
        public Button[] gridSlots = new Button[9];
        
        [Header("Extra Slots (Dynamic)")]
        public Transform extraSlotsContainer;
        public GameObject extraSlotPrefab;
        private List<Button> spawnedExtraSlots = new List<Button>();
        
        [Header("Confirm")]
        public Button confirmButton;

        private System.Action onConfirmCallback;
        private WeaponPartInstance_Alpha currentNewItem;
        
        private bool isReadOnly = false;
        private bool openedByEscape = false;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
            
            // スロットクリック時の仮実装
            for (int i = 0; i < gridSlots.Length; i++)
            {
                int index = i;
                if (gridSlots[index] != null)
                {
                    gridSlots[index].onClick.AddListener(() => OnGridSlotClicked(index));
                }
            }
        }

        public void ToggleEscapeInventory()
        {
            // すでにEscで開いた状態なら閉じる
            if (panel != null && panel.activeSelf && openedByEscape)
            {
                CloseEscapeInventory();
            }
            // 非表示状態かつ、現在ゲームが動いている（報酬フェーズ中などではない）場合にEscで開く
            else if (panel != null && !panel.activeSelf && Time.timeScale > 0f)
            {
                OpenEscapeInventory();
            }
        }

        private void OpenEscapeInventory()
        {
            openedByEscape = true;
            isReadOnly = true;
            Time.timeScale = 0f;
            
            if (panel != null) panel.SetActive(true);
            RefreshUI();
        }

        private void CloseEscapeInventory()
        {
            openedByEscape = false;
            Time.timeScale = 1f;
            if (panel != null) panel.SetActive(false);
        }

        public void Show(WeaponPartInstance_Alpha newItem, System.Action callback)
        {
            openedByEscape = false;
            isReadOnly = false;
            onConfirmCallback = callback;
            currentNewItem = newItem;

            if (panel != null) panel.SetActive(true);

            RefreshUI();
            
            // TODO: newItem を「現在カーソルに持っているアイテム」として表示する、
            // もしくは一時インベントリに追加して表示する処理
            Debug.Log($"[InventoryUI] Please place: {newItem.series.seriesName}");
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void RefreshUI()
        {
            if (InventoryManager_Alpha.Instance == null) return;

            var equipList = InventoryManager_Alpha.Instance.equipInstance;

            // 1. Grid (0-8)
            for (int i = 0; i < gridSlots.Length; i++)
            {
                if (gridSlots[i] != null && i < equipList.Count)
                {
                    SetSlotVisual(gridSlots[i], equipList[i]);
                }
            }

            // 2. Extra Slots (9 onwards)
            int extraCount = Mathf.Max(0, equipList.Count - 9);

            // 余分なスロットがあれば破棄
            while (spawnedExtraSlots.Count > extraCount)
            {
                int lastIdx = spawnedExtraSlots.Count - 1;
                if (spawnedExtraSlots[lastIdx] != null) Destroy(spawnedExtraSlots[lastIdx].gameObject);
                spawnedExtraSlots.RemoveAt(lastIdx);
            }

            // 足りないスロットがあれば生成
            while (spawnedExtraSlots.Count < extraCount)
            {
                if (extraSlotPrefab != null && extraSlotsContainer != null)
                {
                    GameObject obj = Instantiate(extraSlotPrefab, extraSlotsContainer);
                    Button btn = obj.GetComponent<Button>();
                    if (btn != null)
                    {
                        int currentListCount = spawnedExtraSlots.Count; 
                        int slotIndex = 9 + currentListCount;
                        btn.onClick.AddListener(() => OnGridSlotClicked(slotIndex));
                        spawnedExtraSlots.Add(btn);
                    }
                }
                else
                {
                    break;
                }
            }

            // アイコンと色の反映
            int freeSlotCount = InventoryManager_Alpha.Instance.freeSlotCount;

            for (int i = 0; i < spawnedExtraSlots.Count; i++)
            {
                int invIndex = 9 + i;
                Button btn = spawnedExtraSlots[i];
                if (invIndex < equipList.Count)
                {
                    SetSlotVisual(btn, equipList[invIndex]);

                    // 背景枠はButtonのTargetGraphic（通常は背景Image）として取得する
                    Image bg = btn.targetGraphic as Image;
                    if (bg != null)
                    {
                        if (i < freeSlotCount)
                        {
                            bg.color = Color.white; // フリー枠
                        }
                        else
                        {
                            bg.color = new Color(0.8f, 0.9f, 1f, 1f); // テンポラリ枠
                        }
                    }
                }
            }
        }

        private void SetSlotVisual(Button btn, InventoryManager_Alpha.EquipInstance item)
        {
            if (btn == null) return;

            // "Icon" という名前の子オブジェクトを探す
            Transform iconTransform = btn.transform.Find("Icon");
            Image iconImage = null;

            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }

            if (iconImage != null)
            {
                // 中身がある場合
                if (!string.IsNullOrEmpty(item.defId) || item.series != null)
                {
                    if (item.series != null && item.series.icon != null)
                    {
                        iconImage.sprite = item.series.icon;
                    }
                    iconImage.color = Color.white; // 不透明にする
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = new Color(1, 1, 1, 0); // 空枠は透明にする
                }
            }
            else
            {
                // 開発者への警告：Icon子要素がない場合
                if (!string.IsNullOrEmpty(item.defId) || item.series != null)
                {
                    Debug.LogWarning($"[InventoryUI] 装備スロット '{btn.gameObject.name}' に 'Icon' という名前の子要素(Image)がありません！枠画像を維持してアイコンを表示するために、子要素を追加してください。");
                }
            }
        }

        private void OnGridSlotClicked(int index)
        {
            if (isReadOnly)
            {
                Debug.Log("[InventoryUI] Read-only mode. Cannot equip items.");
                return;
            }

            // TODO: ドロップ＆ドロップの代わりに、クリックでアイテムを配置する仮処理
            // x = index % 3, y = index / 3
            if (currentNewItem != null)
            {
                InventoryManager_Alpha.EquipInstance newEquip = new InventoryManager_Alpha.EquipInstance();
                newEquip.series = currentNewItem.series;
                newEquip.partType = currentNewItem.partType;
                newEquip.rarity = currentNewItem.quality;
                newEquip.currentEffects = currentNewItem.currentEffects;
                newEquip.defId = currentNewItem.series.seriesName;

                if (InventoryManager_Alpha.Instance != null)
                {
                    InventoryManager_Alpha.Instance.SetByIndex(index, newEquip);
                    Debug.Log($"[InventoryUI] Equipped to slot index {index}");
                }

                currentNewItem = null;
                RefreshUI();
            }
        }

        private void OnConfirmClicked()
        {
            if (openedByEscape)
            {
                CloseEscapeInventory();
                return;
            }

            // アイテムが未配置のまま決定されたら、自動的にインベントリの空き枠・または末尾に追加する
            if (currentNewItem != null && InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.EquipInstance newEquip = new InventoryManager_Alpha.EquipInstance();
                newEquip.series = currentNewItem.series;
                newEquip.partType = currentNewItem.partType;
                newEquip.rarity = currentNewItem.quality;
                newEquip.currentEffects = currentNewItem.currentEffects;
                newEquip.defId = currentNewItem.series.seriesName;

                InventoryManager_Alpha.Instance.AddItem(newEquip);
                Debug.Log($"[InventoryUI] Auto-added {newEquip.series.seriesName} to extra slots.");
                currentNewItem = null;
            }

            onConfirmCallback?.Invoke();
        }
    }
}
