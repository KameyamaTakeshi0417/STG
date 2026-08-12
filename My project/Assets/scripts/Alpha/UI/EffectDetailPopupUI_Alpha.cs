using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Alpha.UI
{
    public class EffectDetailPopupUI_Alpha : MonoBehaviour, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private RectTransform popupRect;

        // 余白などの設定
        [SerializeField] private Vector2 offset = new Vector2(20, -20);
        
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Setup(Alpha.Data.WeaponEffectSO_Alpha effect, int currentCount, float currentFlatValue, Vector2 pointerPos)
        {
            if (effect == null) return;

            gameObject.SetActive(true);

            string finalStr = $"<size=120%><b>{currentCount} \"{effect.effectName}\"</b></size>\n\n";
            
            if (!string.IsNullOrEmpty(effect.description))
            {
                string desc = effect.description;
                try
                {
                    desc = desc.Contains("{0}") ? string.Format(desc, currentFlatValue) : desc.Replace("{0}", currentFlatValue.ToString());
                }
                catch (System.Exception)
                {
                    // フォールバック
                }
                finalStr += $"{desc}\n\n";
            }

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

                    try
                    {
                        float val = (effect.qualityValues != null && i < effect.qualityValues.Length) ? effect.qualityValues[i] : 0f;
                        stepDesc = stepDesc.Contains("{0}") ? string.Format(stepDesc, val) : stepDesc.Replace("{0}", val.ToString());
                    }
                    catch (System.Exception) {}

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
                    try
                    {
                        stagesStr = $"<color=#FFFFFF>▶ { (baseDesc.Contains("{0}") ? string.Format(baseDesc, currentFlatValue) : baseDesc.Replace("{0}", currentFlatValue.ToString())) }</color>";
                    }
                    catch (System.Exception)
                    {
                        stagesStr = $"<color=#FFFFFF>▶ {baseDesc}\n(Total: {currentFlatValue})</color>";
                    }
                }
            }

            finalStr += stagesStr.TrimEnd();

            if (detailText != null)
            {
                detailText.text = finalStr;
            }

            if (popupRect == null) popupRect = GetComponent<RectTransform>();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

            UpdatePosition(pointerPos);
        }

        private void UpdatePosition(Vector2 pointerPos)
        {
            if (popupRect == null) popupRect = GetComponent<RectTransform>();
            
            // テキスト変更直後の正しい横幅・高さを取得
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);
            Vector2 size = popupRect.rect.size;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            // マウスが画面の右半分にあるか左半分にあるか判定
            bool isMouseOnRight = pointerPos.x > Screen.width / 2f;

            // 大味に配置するためのターゲットスクリーン座標
            // Yは画面中央。Xは空いている方（マウスの逆側）の「中央やや寄り」にドンと置く
            float targetScreenX = isMouseOnRight ? (Screen.width * 0.35f) : (Screen.width * 0.65f);
            float targetScreenY = Screen.height / 2f;
            Vector2 targetScreenPos = new Vector2(targetScreenX, targetScreenY);

            // スクリーン座標をCanvasのローカル座標に変換
            Vector2 canvasLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                targetScreenPos, 
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, 
                out canvasLocalPoint);

            // ---- 画面外はみ出し防止 (Clamp) ----
            Vector2 minPos = canvasRect.rect.min;
            Vector2 maxPos = canvasRect.rect.max;

            // Pivotを考慮した限界座標を計算
            float minX = minPos.x + popupRect.pivot.x * size.x;
            float maxX = maxPos.x - (1f - popupRect.pivot.x) * size.x;
            
            float minY = minPos.y + popupRect.pivot.y * size.y;
            float maxY = maxPos.y - (1f - popupRect.pivot.y) * size.y;

            // Canvas内に完全に収まるようにClamp
            canvasLocalPoint.x = Mathf.Clamp(canvasLocalPoint.x, minX, maxX);
            canvasLocalPoint.y = Mathf.Clamp(canvasLocalPoint.y, minY, maxY);

            // ワールド座標に変換して直接代入
            Vector3 worldPos = canvas.transform.TransformPoint(canvasLocalPoint);
            popupRect.position = worldPos;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Hide();
            }
        }
    }
}
