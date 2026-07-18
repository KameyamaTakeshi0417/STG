using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Alpha.UI
{
    public class EffectDetailPopupUI_Alpha : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI effectNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI stagesText;
        [SerializeField] private RectTransform popupRect;

        // 余白などの設定
        [SerializeField] private Vector2 offset = new Vector2(20, -20);
        
        public void Setup(Alpha.Data.WeaponEffectSO_Alpha effect, int currentCount, float currentFlatValue, Vector2 pointerPos)
        {
            if (effect == null) return;

            gameObject.SetActive(true);

            if (effectNameText != null)
            {
                effectNameText.text = $"{currentCount} \"{effect.effectName}\"";
            }

            if (descriptionText != null)
            {
                descriptionText.text = effect.description;
            }

            if (stagesText != null)
            {
                string stagesStr = "";

                if (effect.useStepMultiplier)
                {
                    // Calculate which stage is currently active
                    int activeStageIndex = 0;
                    
                    // Base stage requires at least 1
                    int[] requiredCounts = new int[4];
                    requiredCounts[0] = 1;
                    for (int i = 0; i < effect.stepThresholds.Length && i < 3; i++)
                    {
                        requiredCounts[i + 1] = effect.stepThresholds[i];
                    }

                    // Find highest stage met
                    for (int i = 0; i < requiredCounts.Length; i++)
                    {
                        if (currentCount >= requiredCounts[i])
                        {
                            activeStageIndex = i;
                        }
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        string stepDesc = "";
                        if (effect.stepDescriptions != null && i < effect.stepDescriptions.Length)
                        {
                            stepDesc = effect.stepDescriptions[i];
                        }
                        
                        if (string.IsNullOrEmpty(stepDesc)) continue;

                        string colorHex = (i == activeStageIndex) ? "#FFFFFF" : "#808080"; // White for active, Gray for others
                        string prefix = (i == activeStageIndex) ? "▶" : "・";

                        stagesStr += $"<color={colorHex}>{prefix} [必要: {requiredCounts[i]}]\n{stepDesc}</color>\n\n";
                    }
                }
                else
                {
                    // For effects without step multiplier, we format the first description string with the total flat value
                    string baseDesc = "";
                    if (effect.stepDescriptions != null && effect.stepDescriptions.Length > 0 && !string.IsNullOrEmpty(effect.stepDescriptions[0]))
                    {
                        baseDesc = effect.stepDescriptions[0];
                    }

                    if (!string.IsNullOrEmpty(baseDesc))
                    {
                        // Try formatting it (e.g. "Attack Flat +{0}")
                        try
                        {
                            stagesStr = $"<color=#FFFFFF>▶ {string.Format(baseDesc, currentFlatValue)}</color>";
                        }
                        catch (System.FormatException)
                        {
                            // Fallback if formatting fails (e.g., the user didn't put {0} or formatting is broken)
                            stagesStr = $"<color=#FFFFFF>▶ {baseDesc}\n(Total: {currentFlatValue})</color>";
                        }
                    }
                }

                stagesText.text = stagesStr.TrimEnd();
            }

            UpdatePosition(pointerPos);
        }

        private void UpdatePosition(Vector2 pointerPos)
        {
            if (popupRect == null) popupRect = GetComponent<RectTransform>();
            
            // Convert screen point to local point in parent canvas
            RectTransform parentCanvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvasRect, pointerPos, null, out localPoint);

            // Apply offset
            localPoint += offset;

            // Optional: Clamp to screen bounds to prevent popup from going off-screen
            Vector2 size = popupRect.rect.size;
            Vector2 maxPos = parentCanvasRect.rect.max - size;
            Vector2 minPos = parentCanvasRect.rect.min;

            // Pivot adjustment (assuming pivot is top-left 0,1)
            float pivotX = popupRect.pivot.x * size.x;
            float pivotY = (1f - popupRect.pivot.y) * size.y;

            localPoint.x = Mathf.Clamp(localPoint.x, minPos.x + pivotX, maxPos.x + pivotX);
            localPoint.y = Mathf.Clamp(localPoint.y, minPos.y - pivotY, maxPos.y - pivotY);

            popupRect.localPosition = localPoint;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
