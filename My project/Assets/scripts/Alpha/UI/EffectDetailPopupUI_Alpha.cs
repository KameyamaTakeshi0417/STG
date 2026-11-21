using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Alpha.UI
{
    public class EffectDetailPopupUI_Alpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private RectTransform popupRect;

        [Header("Font Settings")]
        public TMP_FontAsset customFontAsset;

        // Modern Botanical Palette
        private static readonly Color ColorBaseInk = new Color(0.040f, 0.055f, 0.048f, 0.96f);
        private static readonly Color ColorBrassGold = new Color(0.80f, 0.68f, 0.44f, 1.0f);
        private static readonly Color ColorIvoryWhite = new Color(0.93f, 0.91f, 0.86f, 1.0f);

        // 余白などの設定
        [SerializeField] private Vector2 offset = new Vector2(20, -20);
        
        private bool hasPointerEntered = false;

        private void Awake()
        {
            ApplyBotanicalStyle();
            gameObject.SetActive(false);
        }

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

            // 背景枠を深森墨黒すりガラス調に設定
            Image bg = GetComponent<Image>();
            if (bg != null)
            {
                bg.color = ColorBaseInk;
                bg.raycastTarget = true;
            }

            // 四隅に真鍮ゴールドのL字アクセントを付与
            AddCornerAccents(transform as RectTransform, ColorBrassGold, 18f, 1.5f);

            // フォント適用
            if (detailText != null && customFontAsset != null)
            {
                detailText.font = customFontAsset;
                detailText.color = ColorIvoryWhite;
            }
        }

        public void Setup(Alpha.Data.WeaponEffectSO_Alpha effect, int currentCount, float currentFlatValue, Vector2 pointerPos)
        {
            if (effect == null) return;

            ApplyBotanicalStyle();
            hasPointerEntered = false;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            string finalStr = $"<size=75%><color=#8c7552>EFFECT CLAUSE // </color></size><size=115%><b>{currentCount} \"{effect.effectName}\"</b></size>\n\n";
            
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
                finalStr += $"<color=#ece8dc>{desc}</color>\n\n";
            }

            string stagesStr = "";

            if (effect.useStepMultiplier)
            {
                // Calculate which stage is currently active
                int activeStageIndex = 0;
                
                // Base stage requires at least 1
                int[] requiredCounts = new int[4];
                requiredCounts[0] = 1;
                if (effect.stepThresholds != null)
                {
                    for (int i = 0; i < effect.stepThresholds.Length && i < 3; i++)
                    {
                        requiredCounts[i + 1] = effect.stepThresholds[i];
                    }
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

                    string colorHex = (i == activeStageIndex) ? "#ece8dc" : "#6f7a73"; // 生成り白（活性） / 墨苔灰（休眠）
                    string prefix = (i == activeStageIndex) ? "◆" : "・";

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
                        stagesStr = $"<color=#ece8dc>◆ { (baseDesc.Contains("{0}") ? string.Format(baseDesc, currentFlatValue) : baseDesc.Replace("{0}", currentFlatValue.ToString())) }</color>";
                    }
                    catch (System.Exception)
                    {
                        stagesStr = $"<color=#ece8dc>◆ {baseDesc}\n(Total: {currentFlatValue})</color>";
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

            // 親の RectTransform を取得
            RectTransform parentRect = transform.parent as RectTransform;
            if (parentRect == null) parentRect = canvasRect;

            Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            // マウス位置から画面中央に向かって少しオフセットしたスクリーン座標を計算
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 dirToCenter = (screenCenter - pointerPos).normalized;
            Vector2 targetScreenPos = pointerPos + dirToCenter * 60f;

            // スクリーン座標を parentRect のローカル座標に直接変換
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, 
                targetScreenPos, 
                cam, 
                out Vector2 localPoint);

            // ---- 親の矩形領域内にはみ出さないよう Clamp ----
            Vector2 minPos = parentRect.rect.min;
            Vector2 maxPos = parentRect.rect.max;

            // Pivotを考慮した限界座標を計算
            float minX = minPos.x + popupRect.pivot.x * size.x;
            float maxX = maxPos.x - (1f - popupRect.pivot.x) * size.x;
            
            float minY = minPos.y + popupRect.pivot.y * size.y;
            float maxY = maxPos.y - (1f - popupRect.pivot.y) * size.y;

            if (minX <= maxX)
                localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            if (minY <= maxY)
                localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

            // ワールド変換を介さず親ローカル座標を直接代入し、Zを確実に0に維持
            popupRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hasPointerEntered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hasPointerEntered)
            {
                Hide();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Hide();
            }
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
