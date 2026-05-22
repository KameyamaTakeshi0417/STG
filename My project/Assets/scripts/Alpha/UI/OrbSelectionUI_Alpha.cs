using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Alpha.Data;
using Alpha.Flow;
using TMPro;

namespace Alpha.UI
{
    public class OrbSelectionUI_Alpha : MonoBehaviour
    {
        public static OrbSelectionUI_Alpha Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject selectionPanel;

        [Header("Choice Buttons")]
        public Button[] choiceButtons = new Button[3];
        public TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[3];

        private Queue<OrbData_Alpha> currentQueue;
        private List<WeaponPartInstance_Alpha> currentChoices;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (selectionPanel != null)
                selectionPanel.SetActive(false);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i;
                if (choiceButtons[index] != null)
                {
                    choiceButtons[index].onClick.AddListener(() => OnChoiceSelected(index));
                }
            }
        }

        public void StartOpeningOrbs(Queue<OrbData_Alpha> orbQueue)
        {
            if (orbQueue == null || orbQueue.Count == 0) return;

            currentQueue = orbQueue;
            if (selectionPanel != null)
                selectionPanel.SetActive(true);

            // タイムスケールを止めてゲームを一時停止する
            Time.timeScale = 0f;

            ShowNextOrb();
        }

        private void ShowNextOrb()
        {
            if (currentQueue.Count == 0)
            {
                CloseUI();
                return;
            }

            OrbData_Alpha orb = currentQueue.Dequeue();
            
            // 武器ジェネレーターから3つの選択肢をもらう
            if (WeaponGenerator_Alpha.Instance != null)
            {
                currentChoices = WeaponGenerator_Alpha.Instance.GenerateChoices(orb);
                UpdateUI(currentChoices);
            }
            else
            {
                Debug.LogError("[OrbSelectionUI] WeaponGenerator instance not found!");
                CloseUI();
            }
        }

        private void UpdateUI(List<WeaponPartInstance_Alpha> choices)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i < choices.Count && choices[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    var choice = choices[i];
                    string effectText = "";
                    foreach (var eff in choice.currentEffects)
                    {
                        effectText += $"\n- {eff.effectName}";
                    }

                    choiceTexts[i].text = $"Series: {(choice.series != null ? choice.series.seriesName : "Unknown")}\n" +
                                          $"Part: {choice.partType}\n" +
                                          $"Quality: {choice.quality}{effectText}";
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnChoiceSelected(int index)
        {
            if (currentChoices != null && index < currentChoices.Count)
            {
                var selectedWeapon = currentChoices[index];
                Debug.Log($"[OrbSelectionUI] Player Selected: {selectedWeapon.series.seriesName} ({selectedWeapon.partType})");

                // インベントリに追加
                if (InventoryManager_Alpha.Instance != null)
                {
                    InventoryManager_Alpha.EquipInstance newEquip = new InventoryManager_Alpha.EquipInstance();
                    newEquip.series = selectedWeapon.series;
                    newEquip.partType = selectedWeapon.partType;
                    newEquip.rarity = selectedWeapon.quality;
                    newEquip.currentEffects = selectedWeapon.currentEffects;
                    // defId等は必要に応じて設定
                    newEquip.defId = selectedWeapon.series.seriesName;
                    
                    InventoryManager_Alpha.Instance.AddItem(newEquip);
                }
            }

            ShowNextOrb(); // 次のオーブへ
        }

        private void CloseUI()
        {
            if (selectionPanel != null)
                selectionPanel.SetActive(false);
            
            // ゲーム再開
            Time.timeScale = 1f;

            Debug.Log("[OrbSelectionUI] All orbs opened.");
        }
    }
}
