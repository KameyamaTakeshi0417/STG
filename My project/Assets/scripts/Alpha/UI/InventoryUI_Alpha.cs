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
        
        [Header("Player Status Display")]
        public TextMeshProUGUI statusText;

        [Header("Active Effects UI")]
        public Transform activeEffectsContainer;
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
        
        // 驕ｸ謚樔ｸｭ縺ｮ繧ｹ繝ｭ繝・ヨ繧､繝ｳ繝・ャ繧ｯ繧ｹ・・1縺ｯ譛ｪ驕ｸ謚橸ｼ・
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
                // 謌ｻ繧九・繧ｿ繝ｳ縺ｮ謖吝虚縺ｯConfirm縺ｨ蜷後§・・I繧帝哩縺倥※繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ繧貞ｮ溯｡鯉ｼ・
                backButtonForCheck.onClick.AddListener(OnConfirmClicked);
            }
            if (backToForgeButton != null)
            {
                backToForgeButton.onClick.AddListener(OnConfirmClicked);
            }
            
            // 繧ｹ繝ｭ繝・ヨ繧ｯ繝ｪ繝・け譎ゅ・莉ｮ螳溯｣・
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

        public void OpenAsTab()
        {
            openedByEscape = true;
            isReadOnly = true;
            
            if (panel != null) panel.SetActive(true);
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            if (confirmButton != null) confirmButton.gameObject.SetActive(true);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);

            RefreshUI();
            TryShowEquipTutorial();
        }

        public void CloseAsTab()
        {
            openedByEscape = false;
            
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
            if (effectDetailPopup != null) effectDetailPopup.Hide();
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            if (backButtonForCheck != null) backButtonForCheck.gameObject.SetActive(false);
            if (backToForgeButton != null) backToForgeButton.gameObject.SetActive(false);
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
            
            // HP
            string hpStr = $"HP: {Mathf.FloorToInt(stats.currentHP)} / {Mathf.FloorToInt(stats.HP)}";
            if (stats.HPGauge > 1) hpStr += $" (x{stats.HPGauge} Gauges)";

            // ATK
            float baseAtk = stats.pow;
            float totalAtk = stats.pow + stats.DamageAdd;
            if (totalAtk < 0) totalAtk = 1;
            if (stats.DamageMag > 0) totalAtk *= (stats.DamageMag / 100f);
            float atkDiff = totalAtk - baseAtk;
            string atkStr = $"ATK: {totalAtk:F1}";
            if (Mathf.Abs(atkDiff) > 0.1f)
            {
                string color = atkDiff > 0 ? "#00FF00" : "#FF0000";
                string sign = atkDiff > 0 ? "+" : "";
                atkStr += $" <color={color}>({sign}{atkDiff:F1})</color>";
            }

            // Stamina
            float baseStamRec = 10f; // Default base stamina recovery
            float totalStamRec = stats.staminaRecoveryRate;
            float stamRecDiff = totalStamRec - baseStamRec;
            string stamStr = $"Stamina: {Mathf.FloorToInt(stats.maxStamina)} (Rec: {totalStamRec:F1}/s)";
            if (Mathf.Abs(stamRecDiff) > 0.1f)
            {
                string color = stamRecDiff > 0 ? "#00FF00" : "#FF0000";
                string sign = stamRecDiff > 0 ? "+" : "";
                stamStr += $" <color={color}>({sign}{stamRecDiff:F1}/s)</color>";
            }

            // Fire Rate (Span Mag)
            float baseSpan = stats.BaseBulletSpanMag; 
            float totalSpan = stats.BulletSpanMag;
            float spanDiff = totalSpan - baseSpan;
            // Lower span is faster (better), so inverted color logic
            string spanStr = $"Fire Rate: {totalSpan:F2}x";
            if (Mathf.Abs(spanDiff) > 0.05f)
            {
                string color = spanDiff < 0 ? "#00FF00" : "#FF0000";
                string sign = spanDiff > 0 ? "+" : "";
                spanStr += $" <color={color}>({sign}{spanDiff:F2})</color>";
            }

            // Bullet Speed
            float baseBulSpd = stats.BaseBulletSpeedMag;
            float totalBulSpd = stats.bulletSpeedMag;
            float bulSpdDiff = totalBulSpd - baseBulSpd;
            string bulSpdStr = $"Bullet Spd: {totalBulSpd:F2}x";
            if (Mathf.Abs(bulSpdDiff) > 0.05f)
            {
                string color = bulSpdDiff > 0 ? "#00FF00" : "#FF0000";
                string sign = bulSpdDiff > 0 ? "+" : "";
                bulSpdStr += $" <color={color}>({sign}{bulSpdDiff:F2})</color>";
            }

            // Move Speed (Not affected by equipment, so diff is always 0 for this UI)
            float totalMovSpd = stats.moveSpeedMag;
            string movSpdStr = $"Move Spd: {totalMovSpd:F2}x";

            statusText.text = $"<size=110%><b>Player Status</b></size>\n\n" +
                              $"{hpStr}\n" +
                              $"{atkStr}\n" +
                              $"{stamStr}\n" +
                              $"{spanStr}\n" +
                              $"{bulSpdStr}\n" +
                              $"{movSpdStr}";
        }

        private void UpdateActiveEffectsDisplay()
        {
            if (activeEffectsContainer == null || activeEffectPrefab == null || InventoryManager_Alpha.Instance == null) return;

            foreach (var go in spawnedActiveEffects)
            {
                Destroy(go);
            }
            spawnedActiveEffects.Clear();

            int activeGroup = -1;
            Player_Shooter_Alpha shooter = Object.FindAnyObjectByType<Player_Shooter_Alpha>();
            if (shooter != null) activeGroup = shooter.currentWeaponGroup;
            int groupToPass = (InventoryManager_Alpha.Instance.IsBouquetActive() || (playerStatusManager_Alpha.Instance != null && playerStatusManager_Alpha.Instance.isOmniBouquetOverride)) ? -1 : activeGroup;

            var activeEffects = InventoryManager_Alpha.Instance.GetAllActiveEffectQualities(groupToPass);
            
            Debug.Log($"[InventoryUI] UpdateActiveEffectsDisplay called. Found {activeEffects.Count} active effects.");
            
            foreach (var kvp in activeEffects)
            {
                var effect = kvp.Key;
                int count = kvp.Value.count;
                float flatValue = kvp.Value.flatValue;
                
                Debug.Log($"[InventoryUI] Effect: {(effect != null ? effect.effectName : "null")}, Count: {count}, FlatValue: {flatValue}");
                
                if (count <= 0 || effect == null) continue;

                GameObject go = Instantiate(activeEffectPrefab, activeEffectsContainer, false);
                go.SetActive(true);
                spawnedActiveEffects.Add(go);
                
                // ScrollView (LayoutGroup) 縺ｫ繧医▲縺ｦWidth/Height縺・縺ｫ貎ｰ縺輔ｌ繧九・繧帝亟縺舌◆繧√・
                // 繝励Ξ繝上ヶ譛ｬ譚･縺ｮ繧ｵ繧､繧ｺ繧貞叙蠕励＠縺ｦLayoutElement縺ｧ蠑ｷ蛻ｶ縺吶ｋ
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
                    string colorTag = "";
                    string colorEnd = "";
                    
                    if (effect.effectType == Alpha.Data.WeaponEffectType_Alpha.DivineExecutioner)
                    {
                        bool isBoss = Alpha.Flow.StageManager_Alpha.Instance != null && Alpha.Flow.StageManager_Alpha.Instance.IsBossBattleActive;
                        if (isBoss)
                        {
                            colorTag = "<color=#FF4444>"; // Boss battle: Bright Red
                            colorEnd = "</color>";
                        }
                        else
                        {
                            colorTag = "<color=#808080>"; // Normal: Gray
                            colorEnd = "</color>";
                        }
                    }

                    text.text = $"{colorTag}{count} \"{effect.effectName}\"{colorEnd}";
                }

                // 繧｢繧､繧ｳ繝ｳ繧定ｨｭ螳夲ｼ・Icon" 縺ｨ縺・≧蜷榊燕縺ｮ蟄占ｦ∫ｴ�繧呈爾縺吶°縲∝ｭ占ｦ∫ｴ�縺ｮ荳ｭ縺ｮ譛蛻昴・Image繧呈爾縺呻ｼ・
                Image iconImg = null;
                foreach (Transform child in go.transform)
                {
                    if (child.name == "Icon")
                    {
                        iconImg = child.GetComponent<Image>();
                        break;
                    }
                }
                
                // "Icon"縺ｨ縺・≧蜷榊燕縺後↑縺九▲縺溘ｉ縲√Ν繝ｼ繝医ｒ驕ｿ縺代※縲悟ｭ占ｦ∫ｴ�縲阪°繧迂mage繧呈爾縺・
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

                var rcd = go.AddComponent<RightClickDetector_Alpha>();
                rcd.onRightClick = (eventData) => 
                {
                    if (effectDetailPopup != null)
                    {
                        effectDetailPopup.Setup(effect, count, flatValue, eventData.position);
                    }
                };
            }
        }

        private void SetSlotVisual(Button btn, InventoryManager_Alpha.EquipInstance item, bool isSelected = false, bool isTempSlot = false, int slotIndex = -1)
        {
            if (btn == null) return;

            // 閭梧勹譫�縺ｮ蜿門ｾ励→濶ｲ險ｭ螳・
            Image bg = btn.targetGraphic as Image;
            
            // 縲蝉ｸ榊・蜷亥ｯｾ遲悶繕nity繧､繝ｳ繧ｹ繝壹け繧ｿ荳翫〒繝懊ち繝ｳ閾ｪ菴薙・濶ｲ縺瑚幻濶ｲ縺ｫ險ｭ螳壹＆繧後※縺・ｋ縺ｨ縲・
            // 繧ｹ繧ｯ繝ｪ繝励ヨ縺ｧ菴戊牡繧呈欠螳壹＠縺ｦ繧り幻濶ｲ縺ｫ荳雁｡励ｊ・井ｹ礼ｮ暦ｼ峨＆繧後※縺励∪縺・◆繧√∝ｼｷ蛻ｶ逧・↓繝懊ち繝ｳ閾ｪ菴薙ｒ逵溘▲逋ｽ縺ｫ繝ｪ繧ｻ繝・ヨ縺励∪縺吶・
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = Color.yellow; // 繧､繝ｳ繝吶Φ繝医Μ縺ｮ驕ｸ謚櫁牡縺ｯ鮟・牡縺ｫ縺吶ｋ
            btn.colors = cb;

            if (bg != null)
            {
                if (isSelected)
                {
                    bg.color = Color.yellow; // 驕ｸ謚樔ｸｭ縺ｯ譛蜆ｪ蜈・
                }
                else if (slotIndex >= 9 && !isTempSlot)
                {
                    // EX・・ree・峨せ繝ｭ繝・ヨ縺ｯ蟶ｸ縺ｫ邏ｫ濶ｲ
                    bg.color = new Color32(180, 100, 255, 255);
                }
                else if (item.series != null)
                {
                    // 荳ｭ霄ｫ縺後≠繧句�ｴ蜷医・繝ｬ繧｢繝ｪ繝・ぅ縺ｫ蠢懊§縺溯牡繧定ｨｭ螳・
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
                    // 遨ｺ譫�縺ｮ蝣ｴ蜷・
                    bg.color = isTempSlot ? new Color(0.8f, 0.9f, 1f, 1f) : Color.white;
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
            if (openedByEscape)
            {
                if (PauseMenuManager_Alpha.Instance != null) PauseMenuManager_Alpha.Instance.CloseMenu();
                return;
            }

            if (currentMode == InventoryUIMode.SelectForBlacksmith)
            {
                // 驕ｸ謚槭く繝｣繝ｳ繧ｻ繝ｫ縺ｨ縺励※謇ｱ縺・
                currentMode = InventoryUIMode.Normal;
                Hide();
                onCancelCallbackForBlacksmith?.Invoke();
                return;
            }

            // 譁ｰ隕丞叙蠕励い繧､繝・Β縺ｮ閾ｪ蜍戊ｿｽ蜉�縺ｯRewardSequenceManager蛛ｴ縺ｧ陦後≧縺溘ａ縲√％縺薙〒縺ｯ菴輔ｂ縺励↑縺・
            selectedIndex = -1;
            Hide(); // UI閾ｪ霄ｫ繧帝國縺・
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
    }
}
