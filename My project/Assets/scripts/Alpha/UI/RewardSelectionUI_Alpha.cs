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
        
        [Header("Choice Elements")]
        public Button[] choiceButtons = new Button[3];
        public TextMeshProUGUI[] choiceTexts = new TextMeshProUGUI[3];

        private System.Action<WeaponPartInstance_Alpha> onChoiceSelected;
        private List<WeaponPartInstance_Alpha> currentChoices;

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i;
                if (choiceButtons[index] != null)
                {
                    choiceButtons[index].onClick.AddListener(() => OnButtonClicked(index));
                }
            }
        }

        public void ShowChoices(OrbData_Alpha orb, System.Action<WeaponPartInstance_Alpha> callback)
        {
            onChoiceSelected = callback;
            
            if (WeaponGenerator_Alpha.Instance != null)
            {
                currentChoices = WeaponGenerator_Alpha.Instance.GenerateChoices(orb);
                UpdateUI(currentChoices);
                
                if (panel != null) panel.SetActive(true);
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
                    
                    string effectText = "";
                    if (choice.currentEffects != null)
                    {
                        foreach (var eff in choice.currentEffects)
                        {
                            if (eff != null)
                            {
                                effectText += $"\n- {eff.effectName}";
                            }
                        }
                    }

                    if (choiceTexts[i] != null)
                    {
                        choiceTexts[i].text = $"Series: {(choice.series != null ? choice.series.seriesName : "Unknown")}\n" +
                                              $"Part: {choice.partType}\n" +
                                              $"Quality: {choice.quality}{effectText}";
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
                
                // コールバックで選択されたものを返す
                onChoiceSelected?.Invoke(selectedWeapon);
            }
            else
            {
                onChoiceSelected?.Invoke(null);
            }
        }
    }
}
