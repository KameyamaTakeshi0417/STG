using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alpha.Data;
using DG.Tweening;

namespace Alpha.UI
{
    public class InventoryUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        
        [Header("Font Settings")]
        public TMP_FontAsset customFontAsset;

        [Header("Modern Botanical Palette")]
        public static readonly Color ColorBaseInk = new Color(0.040f, 0.055f, 0.048f, 0.96f);
        public static readonly Color ColorBasePlate = new Color(0.055f, 0.080f, 0.068f, 0.94f);
        public static readonly Color ColorBrassGold = new Color(0.80f, 0.68f, 0.44f, 1.0f);
        public static readonly Color ColorBrassMuted = new Color(0.55f, 0.46f, 0.32f, 0.65f);
        public static readonly Color ColorIvoryWhite = new Color(0.93f, 0.91f, 0.86f, 1.0f);
        public static readonly Color ColorIvoryMuted = new Color(0.68f, 0.68f, 0.64f, 0.85f);
        public static readonly Color ColorMossSlate = new Color(0.14f, 0.21f, 0.17f, 0.95f);
        public static readonly Color ColorDarkMoss = new Color(0.08f, 0.12f, 0.10f, 0.85f);
        public static readonly Color ColorLotusJade = new Color(0.32f, 0.72f, 0.53f, 1.0f);
        public static readonly Color ColorCamelliaCrimson = new Color(0.72f, 0.22f, 0.24f, 1.0f);
        public static readonly Color ColorWisteriaPurple = new Color(0.48f, 0.38f, 0.60f, 1.0f);
        public static readonly Color ColorSunflowerGold = new Color(0.92f, 0.75f, 0.32f, 1.0f);

        [Header("Detail Popup")]
        public EquipDetailPopupUI_Alpha detailPopup;
        
        [Header("Player Status Display")]
        public TextMeshProUGUI statusText;

            [Header("Active Effects UI")]
    public Transform globalEffectsContainer;
    public Transform group1EffectsContainer;
    public Transform group2EffectsContainer;
    public Transform group3EffectsContainer;
    public GameObject activeEffectPrefab; 
        public Alpha.UI.EffectDetailPopupUI_Alpha effectDetailPopup;
        private List<GameObject> spawnedActiveEffects = new List<GameObject>();
        
        [Header("Effect Icon")]
        public GameObject effectIconPrefab;
        
        [Header("Grid (3x3)")]
        [Tooltip("陬・ｙ譫�(3x3)縺ｮ繝懊ち繝ｳ繧ｹ繝ｭ繝・ヨ縲ゅう繝ｳ繝・ャ繧ｯ繧ｹ縺ｯ 0縲・ (y*3+x)")]
        public Button[] gridSlots = new Button[9];
        
        [Header("Extra Slots (Dynamic)")]
        public Transform extraSlotsContainer;
        public GameObject extraSlotPrefab;
        private List<Button> spawnedExtraSlots = new List<Button>();
        
        [Header("Confirm & Check Mode")]
        public Button confirmButton;
        [Tooltip("遒ｺ隱阪Δ繝ｼ繝画凾縺ｮ縺ｿ陦ｨ遉ｺ縺輔ｌ繧区綾繧九・繧ｿ繝ｳ")]
        public Button backButtonForCheck;
        [Tooltip("繝輔か繝ｼ繧ｸ繝輔ぉ繝ｼ繧ｺ縺九ｉ蜻ｼ縺ｰ繧後◆髫帙↓陦ｨ遉ｺ縺輔ｌ繧区綾繧九・繧ｿ繝ｳ")]
        public Button backToForgeButton;

        private System.Action onConfirmCallback;
        private bool openedByEscape = false;
        private bool isReadOnly = false;

        // Animation fields
        private CanvasGroup panelCanvasGroup;
        private Vector3 panelOriginalScale = Vector3.one;
        private bool isCloseAnimating = false;
        
        // 驕ｸ謚樔ｸｭ縺ｮ繧ｹ繝ｭ繝・ヨ繧､繝ｳ繝・ャ繧ｯ繧ｹ・・1縺ｯ譛ｪ驕ｸ謚橸ｼ・
        private int selectedIndex = -1;

        public enum InventoryUIMode { Normal, SelectForBlacksmith }
        [HideInInspector]
        public InventoryUIMode currentMode = InventoryUIMode.Normal;
        private System.Action<int> onSlotSelectedCallback;
        private System.Action onCancelCallbackForBlacksmith;

        private void Awake()
        {
            LoadFontAssetIfNeeded();
            ApplyBotanicalStyle();

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
                    
                    RightClickDetector_Alpha.Attach(gridSlots[index].gameObject, (eventData) => OnSlotRightClicked(index, eventData));

                    var dragHandler = gridSlots[index].gameObject.AddComponent<InventorySlotDragHandler_Alpha>();
                    dragHandler.slotIndex = index;
                    dragHandler.onSlotDropped = HandleSlotDropped;
                }
            }
        }

        public void OpenAsTab()
        {
            openedByEscape = true;
            isReadOnly = true;
            
            if (panel != null) panel.SetActive(true);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            if (confirmButton != null) confirmButton.gameObject.SetActive(true);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);

            RefreshUI();
            PlayOpenAnimation();
            TryShowEquipTutorial();
        }

        public void CloseAsTab()
        {
            openedByEscape = false;
            
            PlayCloseAnimation(null);
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
            PlayOpenAnimation();
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
            PlayOpenAnimation();
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
            
            // 驕ｸ謚槭Δ繝ｼ繝峨〒縺ｯConfirm繝懊ち繝ｳ・育判髱｢繧帝哩縺倥ｋ繝懊ち繝ｳ・峨ｄ騾壼ｸｸ縺ｮ謌ｻ繧九・繧ｿ繝ｳ縺ｯ髱櫁｡ｨ遉ｺ
            // 譁ｰ縺溘↓霑ｽ蜉�縺励◆繝輔か繝ｼ繧ｸ逕ｨ謌ｻ繧九・繧ｿ繝ｳ繧定｡ｨ遉ｺ縺吶ｋ
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            
            if (backToForgeButton != null)
            {
                if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);
                backToForgeButton.gameObject.SetActive(true);
            }
            else if (backButtonForCheck != null)
            {
                backButtonForCheck.gameObject.SetActive(true);
            }

            RefreshUI();
            PlayOpenAnimation();
        }

        private void TryShowEquipTutorial()
        {
            if (TutorialManager_Alpha.Instance != null)
            {
                TutorialManager_Alpha.Instance.ShowTutorial("Tutorial_Equip", true, 3f);
            }
        }

        public void Hide(System.Action onClosed = null)
        {
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            if (effectDetailPopup != null) effectDetailPopup.Hide();
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);
            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(false);

            // すでに非表示または閉じアニメーション中なら二重にアニメーション・ブロックしない
            if (panel == null || !panel.activeSelf || isCloseAnimating)
            {
                onClosed?.Invoke();
                return;
            }

            PlayCloseAnimation(onClosed);
        }

        private void RefreshUI()
        {
            if (InventoryManager_Alpha.Instance == null) return;

            UpdatePlayerStatusDisplay();
            UpdateActiveEffectsDisplay();

            var equipList = InventoryManager_Alpha.Instance.equipInstance;

            // 1. Grid (0-8)
            for (int i = 0; i < gridSlots.Length; i++)
            {
                if (gridSlots[i] != null && i < equipList.Count)
                {
                    SetSlotVisual(gridSlots[i], equipList[i], i == selectedIndex, false, i);
                }
            }

            // 2. Extra Slots (9 onwards)
            int extraCount = Mathf.Max(0, equipList.Count - 9);

            // 菴吝・縺ｪ繧ｹ繝ｭ繝・ヨ縺後≠繧後・遐ｴ譽・
            while (spawnedExtraSlots.Count > extraCount)
            {
                int lastIdx = spawnedExtraSlots.Count - 1;
                if (spawnedExtraSlots[lastIdx] != null) Destroy(spawnedExtraSlots[lastIdx].gameObject);
                spawnedExtraSlots.RemoveAt(lastIdx);
            }

            // 雜ｳ繧翫↑縺・せ繝ｭ繝・ヨ縺後≠繧後・逕滓・
            while (spawnedExtraSlots.Count < extraCount)
            {
                if (extraSlotPrefab != null && extraSlotsContainer != null)
                {
                    GameObject obj = Instantiate(extraSlotPrefab, extraSlotsContainer, false);
                    Button btn = obj.GetComponent<Button>();
                    if (btn != null)
                    {
                        int currentListCount = spawnedExtraSlots.Count; 
                        int slotIndex = 9 + currentListCount;
                        btn.onClick.AddListener(() => OnGridSlotClicked(slotIndex));
                        
                        RightClickDetector_Alpha.Attach(btn.gameObject, (eventData) => OnSlotRightClicked(slotIndex, eventData));

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

            // 繧｢繧､繧ｳ繝ｳ縺ｨ濶ｲ縺ｮ蜿肴丐
            int freeSlotCount = InventoryManager_Alpha.Instance.freeSlotCount;

            for (int i = 0; i < spawnedExtraSlots.Count; i++)
            {
                int invIndex = 9 + i;
                Button btn = spawnedExtraSlots[i];
                if (invIndex < equipList.Count)
                {
                    bool isTempSlot = i >= freeSlotCount;
                    SetSlotVisual(btn, equipList[invIndex], invIndex == selectedIndex, isTempSlot, invIndex);
                }
            }
        }

        private void UpdatePlayerStatusDisplay()
        {
            if (statusText == null || playerStatusManager_Alpha.Instance == null) return;

            var stats = playerStatusManager_Alpha.Instance;
            if (customFontAsset != null) statusText.font = customFontAsset;
            
            // HP (生命力)
            string hpStr = $"生命力 (Vitality) : <color=#ece8dc>{Mathf.FloorToInt(stats.currentHP)}</color> / <color=#ece8dc>{Mathf.FloorToInt(stats.HP)}</color>";
            if (stats.HPGauge > 1) hpStr += $" <color=#8c7552>(x{stats.HPGauge} 層)</color>";

            // ATK (攻撃威力)
            float baseAtk = stats.pow;
            float totalAtk = stats.pow + stats.DamageAdd;
            if (totalAtk < 0) totalAtk = 1;
            if (stats.DamageMag > 0) totalAtk *= (stats.DamageMag / 100f);
            float atkDiff = totalAtk - baseAtk;
            string atkStr = $"攻撃威力 (Power)   : <color=#ece8dc>{totalAtk:F1}</color>";
            if (Mathf.Abs(atkDiff) > 0.1f)
            {
                string color = atkDiff > 0 ? "#52b788" : "#b8383d";
                string sign = atkDiff > 0 ? "+" : "";
                atkStr += $" <color={color}>({sign}{atkDiff:F1})</color>";
            }

            // Stamina (霊力持久)
            float baseStamRec = 10f; // Default base stamina recovery
            float totalStamRec = stats.staminaRecoveryRate;
            float stamRecDiff = totalStamRec - baseStamRec;
            string stamStr = $"霊力持久 (Stamina) : <color=#ece8dc>{Mathf.FloorToInt(stats.maxStamina)}</color> <size=85%><color=#8c7552>(回復: {totalStamRec:F1}/s)</color></size>";
            if (Mathf.Abs(stamRecDiff) > 0.1f)
            {
                string color = stamRecDiff > 0 ? "#52b788" : "#b8383d";
                string sign = stamRecDiff > 0 ? "+" : "";
                stamStr += $" <color={color}>({sign}{stamRecDiff:F1}/s)</color>";
            }

            // Fire Rate (掃射間隔)
            float baseSpan = stats.BaseBulletSpanMag; 
            float totalSpan = stats.BulletSpanMag;
            float spanDiff = totalSpan - baseSpan;
            // Lower span is faster (better), so inverted color logic
            string spanStr = $"掃射間隔 (Fire Rate): <color=#ece8dc>{totalSpan:F2}x</color>";
            if (Mathf.Abs(spanDiff) > 0.05f)
            {
                string color = spanDiff < 0 ? "#52b788" : "#b8383d";
                string sign = spanDiff > 0 ? "+" : "";
                spanStr += $" <color={color}>({sign}{spanDiff:F2})</color>";
            }

            // Bullet Speed (弾道速度)
            float baseBulSpd = stats.BaseBulletSpeedMag;
            float totalBulSpd = stats.bulletSpeedMag;
            float bulSpdDiff = totalBulSpd - baseBulSpd;
            string bulSpdStr = $"弾道速度 (Bullet Spd): <color=#ece8dc>{totalBulSpd:F2}x</color>";
            if (Mathf.Abs(bulSpdDiff) > 0.05f)
            {
                string color = bulSpdDiff > 0 ? "#52b788" : "#b8383d";
                string sign = bulSpdDiff > 0 ? "+" : "";
                bulSpdStr += $" <color={color}>({sign}{bulSpdDiff:F2})</color>";
            }

            // Bullet Size (弾塊規模)
            float baseBulSize = stats.BaseBulletSizeMag;
            float totalBulSize = stats.bulletSizeMag;
            float bulSizeDiff = totalBulSize - baseBulSize;
            string bulSizeStr = $"弾塊規模 (Bullet Size): <color=#ece8dc>{totalBulSize:F2}x</color>";
            if (Mathf.Abs(bulSizeDiff) > 0.01f)
            {
                string color = bulSizeDiff > 0 ? "#52b788" : "#b8383d";
                string sign = bulSizeDiff > 0 ? "+" : "";
                bulSizeStr += $" <color={color}>({sign}{bulSizeDiff:F2})</color>";
            }

            // Move Speed (巡航速度)
            float totalMovSpd = stats.moveSpeedMag;
            string movSpdStr = $"巡航速度 (Move Spd)  : <color=#ece8dc>{totalMovSpd:F2}x</color>";

            statusText.text = $"<size=75%><color=#8c7552>SPECIMEN POTENTIAL // </color></size><size=105%><b>神格適性・機体諸元</b></size>\n\n" +
                              $"{hpStr}\n" +
                              $"{atkStr}\n" +
                              $"{stamStr}\n" +
                              $"{spanStr}\n" +
                              $"{bulSpdStr}\n" +
                              $"{bulSizeStr}\n" +
                              $"{movSpdStr}";
        }

        private void UpdateActiveEffectsDisplay()
        {
            if (activeEffectPrefab == null || InventoryManager_Alpha.Instance == null) return;

            foreach (var go in spawnedActiveEffects)
            {
                Destroy(go);
            }
            spawnedActiveEffects.Clear();

            var activeEffects = InventoryManager_Alpha.Instance.GetActiveEffectsForDisplay();

            foreach (var info in activeEffects)
            {
                var effect = info.effectSO;
                int count = info.count;
                float flatValue = info.flatValue;
                int groupIndex = info.groupIndex;
                
                if (count <= 0 || effect == null) continue;

                Transform targetContainer = null;
                if (groupIndex == -1) targetContainer = globalEffectsContainer;
                else if (groupIndex == 0) targetContainer = group1EffectsContainer;
                else if (groupIndex == 1) targetContainer = group2EffectsContainer;
                else if (groupIndex == 2) targetContainer = group3EffectsContainer;

                if (targetContainer == null) continue;

                GameObject go = Instantiate(activeEffectPrefab, targetContainer, false);
                go.SetActive(true);
                spawnedActiveEffects.Add(go);
                
                RectTransform prefabRt = activeEffectPrefab.transform as RectTransform;
                if (prefabRt != null)
                {
                    LayoutElement le = go.GetComponent<LayoutElement>();
                    if (le == null) le = go.AddComponent<LayoutElement>();
                    le.preferredWidth = prefabRt.sizeDelta.x;
                    le.preferredHeight = prefabRt.sizeDelta.y;
                }

                TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    if (customFontAsset != null) text.font = customFontAsset;
                    string colorTag = "<color=#ece8dc>";
                    string colorEnd = "</color>";
                    
                    if (effect.effectType == Alpha.Data.WeaponEffectType_Alpha.DivineExecutioner)
                    {
                        bool isBoss = Alpha.Flow.StageManager_Alpha.Instance != null && Alpha.Flow.StageManager_Alpha.Instance.IsBossBattleActive;
                        if (isBoss)
                        {
                            colorTag = "<color=#b8383d>"; 
                            colorEnd = "</color>";
                        }
                        else
                        {
                            colorTag = "<color=#6f7a73>"; 
                            colorEnd = "</color>";
                        }
                    }

                    text.text = $"<color=#ccad70>◆</color> {colorTag}{count} \"{effect.effectName}\"{colorEnd}";
                }

                Image iconImg = null;
                foreach (Transform child in go.transform)
                {
                    if (child.name == "Icon")
                    {
                        iconImg = child.GetComponent<Image>();
                        break;
                    }
                }
                
                if (iconImg == null)
                {
                    foreach (Transform child in go.transform)
                    {
                        iconImg = child.GetComponent<Image>();
                        if (iconImg != null) break;
                    }
                }

                if (iconImg != null)
                {
                    if (effect.effectIcon != null)
                    {
                        iconImg.sprite = effect.effectIcon;
                        iconImg.color = Color.white;
                    }
                }

                var rootImg = go.GetComponent<Image>();
                if (rootImg == null)
                {
                    rootImg = go.AddComponent<Image>();
                    rootImg.color = Color.clear;
                }
                rootImg.raycastTarget = true;

                RightClickDetector_Alpha.Attach(go, (eventData) => 
                {
                    if (effectDetailPopup != null)
                    {
                        effectDetailPopup.Setup(effect, count, flatValue, eventData.position);
                    }
                });
            }

            Canvas.ForceUpdateCanvases();
            if (globalEffectsContainer != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(globalEffectsContainer as RectTransform);
            if (group1EffectsContainer != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(group1EffectsContainer as RectTransform);
            if (group2EffectsContainer != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(group2EffectsContainer as RectTransform);
            if (group3EffectsContainer != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(group3EffectsContainer as RectTransform);
            if (globalEffectsContainer != null && globalEffectsContainer.parent != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(globalEffectsContainer.parent as RectTransform);
            }
        }

        private void SetSlotVisual(Button btn, InventoryManager_Alpha.EquipInstance item, bool isSelected = false, bool isTempSlot = false, int slotIndex = -1)
        {
            if (btn == null) return;

            // 閭梧勹譫�縺ｮ蜿門ｾ励→濶ｲ險ｭ螳・
            Image bg = btn.targetGraphic as Image;
            
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = ColorBrassGold;
            cb.highlightedColor = new Color(1.08f, 1.08f, 1.05f, 1f);
            btn.colors = cb;

            if (bg != null)
            {
                if (isSelected)
                {
                    bg.color = ColorBrassGold; // 選択スロット: 輝く真鍮ゴールド
                }
                else if (slotIndex >= 9 && !isTempSlot)
                {
                    // EX（Free）枠: 落ち着いた深藤紫
                    bg.color = new Color32(48, 38, 64, 235);
                }
                else if (item.series != null)
                {
                    // 装備中アイテム: 植物標本・プレパラート調の落ち着いた階層色
                    switch (item.rarity)
                    {
                        case 1: bg.color = new Color32(30, 44, 36, 245); break;  // 苔暗緑・深石色
                        case 2: bg.color = new Color32(42, 75, 58, 250); break;  // 蓮・翡翠
                        case 3: bg.color = new Color32(72, 52, 92, 250); break;  // 藤・藤紫
                        case 4: bg.color = new Color32(115, 88, 38, 255); break; // アンティーク真鍮黄金
                        default: bg.color = new Color32(40, 50, 45, 240); break;
                    }
                }
                else
                {
                    // 空枠
                    bg.color = isTempSlot ? new Color(0.08f, 0.12f, 0.10f, 0.80f) : new Color(0.05f, 0.08f, 0.07f, 0.85f);
                }
            }
            // "Icon" 縺ｨ縺・≧蜷榊燕縺ｮ蟄舌が繝悶ず繧ｧ繧ｯ繝医ｒ謗｢縺・
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
                    if (!isAllEq && item.series != null) isAllEq = HasSeriesAllEquipableEffect(item.series.passiveEffects);
                    
                    Sprite targetSprite = null;
                    if (isAllEq && item.series.iconAllEquipable != null) targetSprite = item.series.iconAllEquipable;
                    else if (item.partType == Alpha.Data.WeaponPartType_Alpha.Bullet && item.series.iconBullet != null) targetSprite = item.series.iconBullet;
                    else if (item.partType == Alpha.Data.WeaponPartType_Alpha.Casing && item.series.iconCasing != null) targetSprite = item.series.iconCasing;
                    else if (item.partType == Alpha.Data.WeaponPartType_Alpha.Primer && item.series.iconPrimer != null) targetSprite = item.series.iconPrimer;

                    // 蝣・欧縺ｪ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
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
                    iconImg.color = Color.clear; // 遨ｺ譫�縺ｾ縺溘・繧｢繧､繧ｳ繝ｳ縺後↑縺・�ｴ蜷医・騾乗・縺ｫ縺吶ｋ
                }
            }
            else
            {
                // 髢狗匱閠・∈縺ｮ隴ｦ蜻奇ｼ唔con蟄占ｦ∫ｴ�縺後↑縺・�ｴ蜷・
                if (item.series != null)
                {
                    Debug.LogWarning($"[InventoryUI] 装備スロットに Icon がありません");
                }
            }
        }

        private void OnGridSlotClicked(int index)
        {
            if (currentMode == InventoryUIMode.SelectForBlacksmith)
            {
                // 驕ｸ謚槭Δ繝ｼ繝画凾縺ｯ繧｢繧､繝・Β縺ｮ遘ｻ蜍輔ｒ陦後ｏ縺壹√さ繝ｼ繝ｫ繝舌ャ繧ｯ繧定ｿ斐☆
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
                // 縺ｾ縺�菴輔ｂ驕ｸ謚槭＆繧後※縺・↑縺・�ｴ蜷医√け繝ｪ繝・け縺励◆繧ｹ繝ｭ繝・ヨ繧帝∈謚樒憾諷九↓縺吶ｋ
                selectedIndex = index;
                Debug.Log($"[InventoryUI] Selected slot {index}");
                RefreshUI();
            }
            else
            {
                // 縺吶〒縺ｫ驕ｸ謚槭＆繧後※縺・ｋ繧ｹ繝ｭ繝・ヨ縺後≠繧句�ｴ蜷・
                if (selectedIndex == index)
                {
                    // 蜷後§繧ｹ繝ｭ繝・ヨ繧偵け繝ｪ繝・け縺励◆繧峨く繝｣繝ｳ繧ｻ繝ｫ
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
            
            // 繧ゅ＠驕ｸ謚樔ｸｭ縺�縺｣縺溘ｂ縺ｮ縺檎ｧｻ蜍輔＠縺溘ｉ驕ｸ謚櫁ｧ｣髯､縺吶ｋ縺ｪ縺ｩ縺ｮ繧ｱ繧｢
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
                    if (item.series != null && HasSeriesAllEquipableEffect(item.series.passiveEffects)) return true;

                    return item.partType == expectedPart;
                }

                if (!CheckEquipRestriction(index2, item1))
                {
                    Debug.LogWarning("[InventoryUI] 装備先と部位が一致しないためキャンセル");
                    return;
                }
                else if (!CheckEquipRestriction(index1, item2))
                {
                    // 繝・Φ繝昴Λ繝ｪ繝ｼ譫�縺ｫ縺ゅｋ繧｢繧､繝・Β(item2)縺ｨ陬・ｙ譫�縺ｮ繧｢繧､繝・Β(item1)繧貞・繧梧崛縺医ｈ縺・→縺励◆縺後・
                    // 繝・Φ繝昴Λ繝ｪ繝ｼ譫�縺ｮ繧｢繧､繝・Β縺瑚｣・ｙ譫�縺ｮ驛ｨ菴阪→荳閾ｴ縺励↑縺・�ｴ蜷医・縲∬｣・ｙ繧定ｧ｣髯､縺吶ｋ縺ｮ縺ｧ縺ｯ縺ｪ縺上く繝｣繝ｳ繧ｻ繝ｫ縺ｫ縺吶ｋ
                    if (index2 >= InventoryManager_Alpha.BASIC_SLOT_COUNT)
                    {
                        Debug.LogWarning("[InventoryUI] 入れ替え先のアイテムが元のスロットに装備できないためキャンセル");
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
                    Debug.Log($"[InventoryUI] 装備を入れ替え、元の装備は一時枠({freeSlotIdx})に退避しました");
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
            if (isCloseAnimating) return;

            if (openedByEscape)
            {
                if (PauseMenuManager_Alpha.Instance != null) PauseMenuManager_Alpha.Instance.CloseMenu();
                return;
            }

            if (currentMode == InventoryUIMode.SelectForBlacksmith)
            {
                // 驕ｸ謚槭く繝｣繝ｳ繧ｻ繝ｫ縺ｨ縺励※謇ｱ縺・
                currentMode = InventoryUIMode.Normal;
                var cancelCb = onCancelCallbackForBlacksmith;
                onCancelCallbackForBlacksmith = null;
                Hide(() => cancelCb?.Invoke());
                return;
            }

            // 譁ｰ隕丞叙蠕励い繧､繝・Β縺ｮ閾ｪ蜍戊ｿｽ蜉�縺ｯRewardSequenceManager蛛ｴ縺ｧ陦後≧縺溘ａ縲√％縺薙〒縺ｯ菴輔ｂ縺励↑縺・
            selectedIndex = -1;
            var confirmCb = onConfirmCallback;
            onConfirmCallback = null;
            Hide(() => confirmCb?.Invoke());
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

        private bool HasSeriesAllEquipableEffect(List<Alpha.Data.SeriesPassiveEffect> effects)
        {
            if (effects == null) return false;
            foreach (var eff in effects)
            {
                if (eff.effect == null) continue;
                if (eff.effect.effectType == Alpha.Data.WeaponEffectType_Alpha.AllEquipable) return true;
                if (eff.effect.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
                {
                    var comp = eff.effect as Alpha.Data.CompositeWeaponEffectSO_Alpha;
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

            if (detailPopup != null)
            {
                List<Alpha.Data.WeaponEffectSO_Alpha> displayEffects = new List<Alpha.Data.WeaponEffectSO_Alpha>();
                
                if (item.series != null)
                {
                    if (item.series.passiveEffects != null)
                    {
                        foreach (var pe in item.series.passiveEffects)
                        {
                            if (pe.effect != null) displayEffects.Add(pe.effect);
                        }
                    }
                    
                    // specific effects are already added to currentEffects during generation,
                    // so we don't need to add them here again manually.
                }
                
                if (item.currentEffects != null)
                {
                    displayEffects.AddRange(item.currentEffects);
                }

                detailPopup.Setup(item.series, item.partType, item.rarity, displayEffects, eventData.position, item.setBonusEffect);
            }
        }

        // --- Panel Animation Helpers ---
        private CanvasGroup EnsurePanelCanvasGroup()
        {
            if (panelCanvasGroup != null) return panelCanvasGroup;
            if (panel == null) return null;
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = panel.AddComponent<CanvasGroup>();
            panelOriginalScale = panel.transform.localScale;
            if (panelOriginalScale.sqrMagnitude < 0.01f) panelOriginalScale = Vector3.one;
            return panelCanvasGroup;
        }

        private void PlayOpenAnimation()
        {
            var cg = EnsurePanelCanvasGroup();
            if (cg == null || panel == null) return;

            cg.DOKill();
            panel.transform.DOKill();
            if (isCloseAnimating)
            {
                isCloseAnimating = false;
                UIInputBlocker_Alpha.Unblock();
            }

            cg.alpha = 0f;
            panel.transform.localScale = panelOriginalScale * 0.85f;
            panel.SetActive(true);

            cg.DOFade(1f, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
            panel.transform.DOScale(panelOriginalScale, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private void PlayCloseAnimation(System.Action onComplete)
        {
            var cg = EnsurePanelCanvasGroup();
            if (cg == null || panel == null)
            {
                if (panel != null) panel.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            if (isCloseAnimating)
            {
                onComplete?.Invoke();
                return;
            }

            cg.DOKill();
            panel.transform.DOKill();

            isCloseAnimating = true;
            UIInputBlocker_Alpha.Block();

            cg.DOFade(0f, 0.12f).SetEase(Ease.InCubic).SetUpdate(true);
            panel.transform.DOScale(panelOriginalScale * 0.85f, 0.12f).SetEase(Ease.InCubic).SetUpdate(true)
                .OnComplete(() =>
                {
                    panel.SetActive(false);
                    panel.transform.localScale = panelOriginalScale;
                    cg.alpha = 1f;
                    isCloseAnimating = false;
                    UIInputBlocker_Alpha.Unblock();
                    onComplete?.Invoke();
                });
        }

        // --- Modern Botanical UI Helpers ---
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
            if (customFontAsset == null)
            {
                customFontAsset = Resources.Load<TMP_FontAsset>("Fonts/SoukouMincho SDF");
            }
            if (customFontAsset == null)
            {
                customFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/SoukouMincho SDF");
            }
        }

        public void ApplyBotanicalStyle()
        {
            LoadFontAssetIfNeeded();

            // 1. パネル全体の背景を深森墨黒すりガラス調に設定
            if (panel != null)
            {
                var panelImg = panel.GetComponent<Image>();
                if (panelImg != null)
                {
                    panelImg.color = ColorBaseInk;
                }
                AddCornerAccents(panel.transform as RectTransform, ColorBrassGold, 28f, 2f);
            }

            // 2. ボタン類のボタニカル化
            SetupBotanicalButton(confirmButton, "条項承認 [ 確定 ]");
            SetupBotanicalButton(backButtonForCheck, "戻る [ 閉じる ]");
            SetupBotanicalButton(backToForgeButton, "鍛冶台へ帰還");

            // 3. テキスト全体のフォント適用
            if (panel != null && customFontAsset != null)
            {
                var texts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t != null) t.font = customFontAsset;
                }
            }
        }

        private void SetupBotanicalButton(Button btn, string defaultText = null)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = ColorMossSlate;
            }

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.1f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor = Color.white;
            btn.colors = cb;

            var txt = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt != null)
            {
                if (customFontAsset != null) txt.font = customFontAsset;
                txt.color = ColorIvoryWhite;
                if (!string.IsNullOrEmpty(defaultText) && string.IsNullOrEmpty(txt.text))
                {
                    txt.text = defaultText;
                }
            }

            AddCornerAccents(btn.transform as RectTransform, ColorBrassMuted, 8f, 1.5f);
        }

        private static void AddCornerAccents(RectTransform targetParent, Color color, float size = 16f, float thickness = 1.5f)
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