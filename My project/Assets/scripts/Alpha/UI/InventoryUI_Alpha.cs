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
        
        [Header("Detail Popup")]
        public EquipDetailPopupUI_Alpha detailPopup;
        
        [Header("Grid (3x3)")]
        [Tooltip("装備枠(3x3)のボタンスロット。インデックスは 0〜8 (y*3+x)")]
        public Button[] gridSlots = new Button[9];
        
        [Header("Extra Slots (Dynamic)")]
        public Transform extraSlotsContainer;
        public GameObject extraSlotPrefab;
        private List<Button> spawnedExtraSlots = new List<Button>();
        
        [Header("Confirm & Check Mode")]
        public Button confirmButton;
        [Tooltip("確認モード時のみ表示される戻るボタン")]
        public Button backButtonForCheck;
        [Tooltip("フォージフェーズから呼ばれた際に表示される戻るボタン")]
        public Button backToForgeButton;

        private System.Action onConfirmCallback;
        private bool openedByEscape = false;
        private bool isReadOnly = false;
        
        // 選択中のスロットインデックス（-1は未選択）
        private int selectedIndex = -1;

        public enum InventoryUIMode { Normal, SelectForBlacksmith }
        [HideInInspector]
        public InventoryUIMode currentMode = InventoryUIMode.Normal;
        private System.Action<int> onSlotSelectedCallback;
        private System.Action onCancelCallbackForBlacksmith;

        private void Awake()
        {
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            if (panel != null) panel.SetActive(false);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);
            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(false);

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
            if (backButtonForCheck != null)
            {
                // 戻るボタンの挙動はConfirmと同じ（UIを閉じてコールバックを実行）
                backButtonForCheck.onClick.AddListener(OnConfirmClicked);
            }
            if (backToForgeButton != null)
            {
                backToForgeButton.onClick.AddListener(OnConfirmClicked);
            }
            
            // スロットクリック時の仮実装
            for (int i = 0; i < gridSlots.Length; i++)
            {
                int index = i;
                if (gridSlots[index] != null)
                {
                    gridSlots[index].onClick.AddListener(() => OnGridSlotClicked(index));
                    
                    var rcd = gridSlots[index].gameObject.AddComponent<RightClickDetector_Alpha>();
                    rcd.onRightClick = (eventData) => OnSlotRightClicked(index, eventData);

                    var dragHandler = gridSlots[index].gameObject.AddComponent<InventorySlotDragHandler_Alpha>();
                    dragHandler.slotIndex = index;
                    dragHandler.onSlotDropped = HandleSlotDropped;
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
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            if (confirmButton != null) confirmButton.gameObject.SetActive(true);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);

            RefreshUI();
            TryShowEquipTutorial();
        }

        private void CloseEscapeInventory()
        {
            openedByEscape = false;
            
            if (TutorialManager_Alpha.Instance != null && TutorialManager_Alpha.Instance.IsShowing)
            {
                // チュートリアルがまだ開いている場合は、ゲームを再開させず
                // チュートリアルが閉じたタイミングでゲームが再開されるように仕込む
                TutorialManager_Alpha.Instance.OverridePreviousTimeScale(1f);
            }
            else
            {
                Time.timeScale = 1f;
            }
            
            if (panel != null) panel.SetActive(false);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
        }

        public void ShowForCheck(System.Action callback)
        {
            Debug.Log("[InventoryUI] ShowForCheck called.");
            openedByEscape = false;
            isReadOnly = false;
            onConfirmCallback = callback;
            selectedIndex = -1;

            if (panel != null)
            {
                panel.SetActive(true);
                Debug.Log($"[InventoryUI] panel.SetActive(true) executed. Is active in hierarchy? {panel.activeInHierarchy}");
            }
            else
            {
                Debug.LogError("[InventoryUI] panel is NULL in ShowForCheck!");
            }
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(true);
            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(false);

            RefreshUI();
            TryShowEquipTutorial();
        }

        public void Show(WeaponPartInstance_Alpha newItem, System.Action callback)
        {
            openedByEscape = false;
            isReadOnly = false;
            onConfirmCallback = callback;
            selectedIndex = -1;

            if (panel != null) panel.SetActive(true);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);

            if (confirmButton != null) confirmButton.gameObject.SetActive(true);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);
            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(false);

            RefreshUI();
            TryShowEquipTutorial();
        }

        public void ShowForSelection(System.Action<int> onSlotSelected, System.Action onCancel = null)
        {
            Debug.Log("[InventoryUI] ShowForSelection called.");
            openedByEscape = false;
            isReadOnly = false;
            currentMode = InventoryUIMode.SelectForBlacksmith;
            onSlotSelectedCallback = onSlotSelected;
            onCancelCallbackForBlacksmith = onCancel;
            selectedIndex = -1;

            if (panel != null) panel.SetActive(true);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            // 選択モードではConfirmボタン（画面を閉じるボタン）や通常の戻るボタンは非表示
            // 新たに追加したフォージ用戻るボタンを表示する
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);
            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(true);

            RefreshUI();
        }

        private void TryShowEquipTutorial()
        {
            if (TutorialManager_Alpha.Instance != null)
            {
                TutorialManager_Alpha.Instance.ShowTutorial("Tutorial_Equip", true, 3f);
            }
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
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
                    SetSlotVisual(gridSlots[i], equipList[i], i == selectedIndex);
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
                        
                        var rcd = btn.gameObject.AddComponent<RightClickDetector_Alpha>();
                        rcd.onRightClick = (eventData) => OnSlotRightClicked(slotIndex, eventData);

                        var dragHandler = btn.gameObject.AddComponent<InventorySlotDragHandler_Alpha>();
                        dragHandler.slotIndex = slotIndex;
                        dragHandler.onSlotDropped = HandleSlotDropped;
                        
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
                    bool isTempSlot = i >= freeSlotCount;
                    SetSlotVisual(btn, equipList[invIndex], invIndex == selectedIndex, isTempSlot);
                }
            }
        }

        private void SetSlotVisual(Button btn, InventoryManager_Alpha.EquipInstance item, bool isSelected = false, bool isTempSlot = false)
        {
            if (btn == null) return;

            // 背景枠の取得と色設定
            Image bg = btn.targetGraphic as Image;
            
            // 【不具合対策】Unityインスペクタ上のボタン自体の色が茶色に設定されていると、
            // スクリプトで何色を指定しても茶色に上塗り（乗算）されてしまうため、強制的にボタン自体を真っ白にリセットします。
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = Color.yellow; // インベントリの選択色は黄色にする
            btn.colors = cb;

            if (bg != null)
            {
                if (isSelected)
                {
                    bg.color = Color.yellow; // 選択中は最優先
                }
                else if (item.series != null)
                {
                    // 中身がある場合はレアリティに応じた色を設定
                    switch (item.rarity)
                    {
                        case 1: bg.color = new Color32(128, 38, 5, 255); break;
                        case 2: bg.color = new Color32(136, 136, 136, 255); break;
                        case 3: bg.color = new Color32(238, 156, 23, 255); break;
                        case 4: bg.color = new Color32(1, 240, 91, 255); break;
                        default: bg.color = Color.white; break;
                    }
                }
                else
                {
                    // 空枠の場合
                    bg.color = isTempSlot ? new Color(0.8f, 0.9f, 1f, 1f) : Color.white;
                }
            }

            // "Icon" という名前の子オブジェクトを探す
            Image iconImg = null;
            foreach (Transform child in btn.transform)
            {
                if (child.name == "Icon")
                {
                    iconImg = child.GetComponent<Image>();
                    break;
                }
            }

            if (iconImg != null)
            {
                if (item.series != null)
                {
                    bool isAllEq = HasAllEquipableEffect(item.currentEffects);
                    if (!isAllEq && item.series != null) isAllEq = HasAllEquipableEffect(item.series.passiveEffects);
                    
                    Sprite targetSprite = null;
                    if (isAllEq && item.series.iconAllEquipable != null) targetSprite = item.series.iconAllEquipable;
                    else if (item.partType == Alpha.Data.WeaponPartType_Alpha.Bullet && item.series.iconBullet != null) targetSprite = item.series.iconBullet;
                    else if (item.partType == Alpha.Data.WeaponPartType_Alpha.Casing && item.series.iconCasing != null) targetSprite = item.series.iconCasing;
                    else if (item.partType == Alpha.Data.WeaponPartType_Alpha.Primer && item.series.iconPrimer != null) targetSprite = item.series.iconPrimer;

                    // 堅牢なフォールバック
                    if (targetSprite == null) targetSprite = item.series.icon;
                    if (targetSprite == null) targetSprite = item.series.iconBullet;
                    if (targetSprite == null) targetSprite = item.series.iconCasing;
                    if (targetSprite == null) targetSprite = item.series.iconPrimer;
                    if (targetSprite == null) targetSprite = item.series.iconAllEquipable;

                    if (targetSprite != null)
                    {
                        iconImg.sprite = targetSprite;
                        iconImg.color = Color.white;
                    }
                    else
                    {
                        iconImg.sprite = null;
                        iconImg.color = Color.clear;
                    }
                }
                else
                {
                    iconImg.sprite = null;
                    iconImg.color = Color.clear; // 空枠またはアイコンがない場合は透明にする
                }
            }
            else
            {
                // 開発者への警告：Icon子要素がない場合
                if (item.series != null)
                {
                    Debug.LogWarning($"[InventoryUI] 装備スロット '{btn.gameObject.name}' に 'Icon' という名前の子要素(Image)がありません。枠画像を維持してアイコンを表示するために、子要素を追加してください。");
                }
            }
        }

        private void OnGridSlotClicked(int index)
        {
            if (currentMode == InventoryUIMode.SelectForBlacksmith)
            {
                // 選択モード時はアイテムの移動を行わず、コールバックを返す
                onSlotSelectedCallback?.Invoke(index);
                return;
            }

            if (isReadOnly)
            {
                Debug.Log("[InventoryUI] Read-only mode. Cannot modify items.");
                return;
            }

            if (InventoryManager_Alpha.Instance == null) return;

            if (selectedIndex == -1)
            {
                // まだ何も選択されていない場合、クリックしたスロットを選択状態にする
                selectedIndex = index;
                Debug.Log($"[InventoryUI] Selected slot {index}");
                RefreshUI();
            }
            else
            {
                // すでに選択されているスロットがある場合
                if (selectedIndex == index)
                {
                    // 同じスロットをクリックしたらキャンセル
                    selectedIndex = -1;
                    Debug.Log("[InventoryUI] Selection cancelled.");
                }
                else
                {
                    SwapItems(selectedIndex, index);
                    selectedIndex = -1;
                }
                RefreshUI();
            }
        }

        private void HandleSlotDropped(int fromIndex, int toIndex)
        {
            if (currentMode == InventoryUIMode.SelectForBlacksmith) return;
            if (isReadOnly) return;
            if (InventoryManager_Alpha.Instance == null) return;
            
            SwapItems(fromIndex, toIndex);
            
            // もし選択中だったものが移動したら選択解除するなどのケア
            selectedIndex = -1;
            RefreshUI();
        }

        private void SwapItems(int index1, int index2)
        {
            var list = InventoryManager_Alpha.Instance.equipInstance;
            if (index1 < list.Count && index2 < list.Count)
            {
                var item1 = list[index1];
                var item2 = list[index2];

                bool CheckEquipRestriction(int targetSlotIndex, InventoryManager_Alpha.EquipInstance item)
                {
                    if (item.series == null) return true; // Empty item can go anywhere
                    if (targetSlotIndex >= InventoryManager_Alpha.BASIC_SLOT_COUNT) return true; // Free/Temp slots have no restriction

                    int column = targetSlotIndex % 3;
                    Alpha.Data.WeaponPartType_Alpha expectedPart = Alpha.Data.WeaponPartType_Alpha.Bullet;
                    if (column == 0) expectedPart = Alpha.Data.WeaponPartType_Alpha.Primer;
                    else if (column == 1) expectedPart = Alpha.Data.WeaponPartType_Alpha.Casing;
                    else if (column == 2) expectedPart = Alpha.Data.WeaponPartType_Alpha.Bullet;

                    if (HasAllEquipableEffect(item.currentEffects)) return true;
                    if (item.series != null && HasAllEquipableEffect(item.series.passiveEffects)) return true;

                    return item.partType == expectedPart;
                }

                if (!CheckEquipRestriction(index2, item1))
                {
                    Debug.LogWarning("[InventoryUI] 装備先のスロットと部位が一致しないため、移動をキャンセルしました。");
                    return;
                }
                else if (!CheckEquipRestriction(index1, item2))
                {
                    // テンポラリー枠にあるアイテム(item2)と装備枠のアイテム(item1)を入れ替えようとしたが、
                    // テンポラリー枠のアイテムが装備枠の部位と一致しない場合は、装備を解除するのではなくキャンセルにする
                    if (index2 >= InventoryManager_Alpha.BASIC_SLOT_COUNT)
                    {
                        Debug.LogWarning("[InventoryUI] 入れ替え先のアイテムが元のスロットに装備できないため、移動をキャンセルしました。");
                        return;
                    }

                    int freeSlotIdx = -1;
                    for (int i = InventoryManager_Alpha.BASIC_SLOT_COUNT; i < list.Count; i++)
                    {
                        if (list[i].series == null)
                        {
                            freeSlotIdx = i;
                            break;
                        }
                    }

                    if (freeSlotIdx == -1)
                    {
                        freeSlotIdx = list.Count;
                    }

                    InventoryManager_Alpha.Instance.SetByIndex(freeSlotIdx, item2);
                    InventoryManager_Alpha.Instance.SetByIndex(index2, item1);
                    InventoryManager_Alpha.Instance.SetByIndex(index1, new InventoryManager_Alpha.EquipInstance());
                    Debug.Log($"[InventoryUI] 装備を入れ替え、元の装備は一時枠({freeSlotIdx})に退避しました。");
                }
                else
                {
                    InventoryManager_Alpha.Instance.SetByIndex(index1, item2);
                    InventoryManager_Alpha.Instance.SetByIndex(index2, item1);

                    int tempStartIndex = InventoryManager_Alpha.BASIC_SLOT_COUNT + InventoryManager_Alpha.Instance.freeSlotCount;
                    for (int i = list.Count - 1; i >= tempStartIndex; i--)
                    {
                        if (list[i].series == null)
                        {
                            list.RemoveAt(i);
                        }
                    }

                    Debug.Log($"[InventoryUI] Swapped slot {index1} with {index2}");
                }
            }
        }

        private void OnConfirmClicked()
        {
            if (openedByEscape)
            {
                CloseEscapeInventory();
                return;
            }

            if (currentMode == InventoryUIMode.SelectForBlacksmith)
            {
                // 選択キャンセルとして扱う
                currentMode = InventoryUIMode.Normal;
                Hide();
                onCancelCallbackForBlacksmith?.Invoke();
                return;
            }

            // 新規取得アイテムの自動追加はRewardSequenceManager側で行うため、ここでは何もしない
            selectedIndex = -1;
            Hide(); // UI自身を隠す
            onConfirmCallback?.Invoke();
        }

        private bool HasAllEquipableEffect(List<Alpha.Data.WeaponEffectSO_Alpha> effects)
        {
            if (effects == null) return false;
            foreach (var eff in effects)
            {
                if (eff == null) continue;
                if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.AllEquipable) return true;
                if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
                {
                    var comp = eff as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                    if (comp != null && HasAllEquipableEffect(comp.subEffects)) return true;
                }
            }
            return false;
        }

        private void OnSlotRightClicked(int index, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (InventoryManager_Alpha.Instance == null) return;
            var equipList = InventoryManager_Alpha.Instance.equipInstance;
            if (index < 0 || index >= equipList.Count) return;

            var item = equipList[index];
            if (item.series == null) return;

            bool isBestSlotMet = false;
            if (InventoryManager_Alpha.Instance != null)
            {
                isBestSlotMet = InventoryManager_Alpha.Instance.IsBestSlotConditionMet(index);
            }

            if (detailPopup != null)
            {
                detailPopup.Setup(item.series, item.partType, item.rarity, item.currentEffects, eventData.position, isBestSlotMet);
            }
        }
    }
}
