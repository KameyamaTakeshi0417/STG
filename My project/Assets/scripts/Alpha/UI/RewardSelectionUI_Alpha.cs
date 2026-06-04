using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alpha.Data;
using Alpha.Flow;

namespace Alpha.UI
{
    public class RewardSelectionUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        
        [Header("Check Equipment")]
        public Button checkEquipButton;
        public InventoryUI_Alpha inventoryUI;
        
        [Header("Skip Reward")]
        public Button skipButton;
        
        [Header("Detail Popup")]
        public EquipDetailPopupUI_Alpha detailPopup;
        
        [Header("Choice Elements")]
        public Button[] choiceButtons = new Button[3];
        public TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[3];

        private System.Action<WeaponPartInstance_Alpha> onChoiceSelected;
        private List<WeaponPartInstance_Alpha> currentChoices;

        private void Awake()
        {
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            
            if (panel != null) panel.SetActive(false);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i;
                if (choiceButtons[index] != null)
                {
                    choiceButtons[index].onClick.AddListener(() => OnButtonClicked(index));
                    
                    var rcd = choiceButtons[index].gameObject.AddComponent<RightClickDetector_Alpha>();
                    rcd.onRightClick = (eventData) => OnChoiceRightClicked(index, eventData);
                }
            }

            if (checkEquipButton != null)
            {
                checkEquipButton.onClick.AddListener(OnCheckEquipClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        private void OnSkipClicked()
        {
            if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            onChoiceSelected?.Invoke(null);
        }

        public void ShowChoices(OrbData_Alpha orb, System.Action<WeaponPartInstance_Alpha> callback)
        {
            onChoiceSelected = callback;
            
            if (WeaponGenerator_Alpha.Instance != null)
            {
                currentChoices = WeaponGenerator_Alpha.Instance.GenerateChoices(orb);
                UpdateUI(currentChoices);
                
                if (panel != null) panel.SetActive(true);
                if (detailPopup != null) detailPopup.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("[RewardSelectionUI] WeaponGenerator instance not found!");
                onChoiceSelected?.Invoke(null);
            }
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void UpdateUI(List<WeaponPartInstance_Alpha> choices)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < choices.Count && choices[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    var choice = choices[i];
                    
                    // レアリティごとの枠色設定
                    Image bg = choiceButtons[i].targetGraphic as Image;
                    
                    // 【不具合対策】Unityインスペクタ上のボタン自体の色が茶色に設定されていると、
                    // スクリプトで何色を指定しても茶色に上塗り（乗算）されてしまうため、強制的にボタン自体を真っ白にリセットします。
                    ColorBlock cb = choiceButtons[i].colors;
                    cb.normalColor = Color.white;
                    cb.selectedColor = Color.white;
                    choiceButtons[i].colors = cb;

                    if (bg != null)
                    {
                        switch (choice.quality)
                        {
                            case 1: bg.color = new Color32(128, 38, 5, 255); break;
                            case 2: bg.color = new Color32(136, 136, 136, 255); break;
                            case 3: bg.color = new Color32(238, 156, 23, 255); break;
                            case 4: bg.color = new Color32(1, 240, 91, 255); break;
                            default: bg.color = Color.white; break;
                        }
                    }
                    
                    string effectText = "";
                    bool isAllEquipable = false;
                    if (choice.currentEffects != null)
                    {
                        foreach (var eff in choice.currentEffects)
                        {
                            if (eff != null)
                            {
                                effectText += $"\n- {eff.effectName}";
                                if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.AllEquipable)
                                {
                                    isAllEquipable = true;
                                }
                            }
                        }
                    }

                    if (choiceTexts[i] != null)
                    {
                        string partStr = "";
                        switch (choice.partType)
                        {
                            case Alpha.Data.WeaponPartType_Alpha.Bullet: partStr = "弾頭 (Bullet)"; break;
                            case Alpha.Data.WeaponPartType_Alpha.Casing: partStr = "薬莢 (Casing)"; break;
                            case Alpha.Data.WeaponPartType_Alpha.Primer: partStr = "雷管 (Primer)"; break;
                        }
                        if (isAllEquipable) partStr += " (どこでも装備可能)";

                        choiceTexts[i].text = $"Series: {(choice.series != null ? choice.series.seriesName : "Unknown")}\n" +
                                              $"部位: {partStr}\n" +
                                              $"Quality: {choice.quality}{effectText}";
                    }
                    
                    // アイコンの表示処理（ボタンの子オブジェクト「Icon」を探す）
                    Image iconImg = null;
                    foreach (Transform child in choiceButtons[i].transform)
                    {
                        if (child.name == "Icon")
                        {
                            iconImg = child.GetComponent<Image>();
                            break;
                        }
                    }

                    if (iconImg != null)
                    {
                        if (choice.series != null)
                        {
                            Sprite targetSprite = null;
                            if (isAllEquipable && choice.series.iconAllEquipable != null) targetSprite = choice.series.iconAllEquipable;
                            else if (choice.partType == Alpha.Data.WeaponPartType_Alpha.Bullet && choice.series.iconBullet != null) targetSprite = choice.series.iconBullet;
                            else if (choice.partType == Alpha.Data.WeaponPartType_Alpha.Casing && choice.series.iconCasing != null) targetSprite = choice.series.iconCasing;
                            else if (choice.partType == Alpha.Data.WeaponPartType_Alpha.Primer && choice.series.iconPrimer != null) targetSprite = choice.series.iconPrimer;

                            // 堅牢なフォールバック
                            if (targetSprite == null) targetSprite = choice.series.icon;
                            if (targetSprite == null) targetSprite = choice.series.iconBullet;
                            if (targetSprite == null) targetSprite = choice.series.iconCasing;
                            if (targetSprite == null) targetSprite = choice.series.iconPrimer;
                            if (targetSprite == null) targetSprite = choice.series.iconAllEquipable;

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
                            iconImg.color = Color.clear; // アイコンが無い場合は透明にする
                        }
                    }
                }
                else
                {
                    if (choiceButtons[i] != null) choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnButtonClicked(int index)
        {
            if (currentChoices != null && index < currentChoices.Count)
            {
                var selectedWeapon = currentChoices[index];
                Debug.Log($"[RewardSelectionUI] Selected: {selectedWeapon.series.seriesName} ({selectedWeapon.partType})");
                
                // ポップアップが開いたままになっていたら消す
                if (detailPopup != null) detailPopup.gameObject.SetActive(false);
                
                // コールバックで選択されたものを返す
                onChoiceSelected?.Invoke(selectedWeapon);
            }
            else
            {
                onChoiceSelected?.Invoke(null);
            }
        }

        private void OnChoiceRightClicked(int index, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (currentChoices == null || index < 0 || index >= currentChoices.Count) return;
            var choice = currentChoices[index];
            if (choice == null || choice.series == null) return;

            if (detailPopup != null)
            {
                detailPopup.Setup(choice.series, choice.partType, choice.quality, choice.currentEffects, eventData.position);
            }
        }

        public void OnCheckEquipClicked()
        {
            Debug.Log($"[RewardSelectionUI] OnCheckEquipClicked called. inventoryUI is null? {inventoryUI == null}");
            if (inventoryUI != null)
            {
                // 報酬UIを一時的に隠す
                if (panel != null) panel.SetActive(false);
                
                // インベントリ画面を確認モードで開く
                Debug.Log("[RewardSelectionUI] Calling inventoryUI.ShowForCheck()");
                inventoryUI.ShowForCheck(OnCheckEquipFinished);
            }
        }

        private void OnCheckEquipFinished()
        {
            Debug.Log("[RewardSelectionUI] OnCheckEquipFinished called.");
            // インベントリ画面が閉じられたら、報酬UIを再表示する
            if (panel != null) panel.SetActive(true);
        }
    }
}
