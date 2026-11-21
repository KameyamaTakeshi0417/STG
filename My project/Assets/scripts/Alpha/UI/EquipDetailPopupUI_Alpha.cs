using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Alpha.Data;

namespace Alpha.UI
{
    public class EquipDetailPopupUI_Alpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI References")]
        public Image iconImage;
        public TextMeshProUGUI detailText;

        [Header("Font Settings")]
        public TMP_FontAsset customFontAsset;

        [Header("Settings")]
        [Tooltip("クリックした位置から画面中央方向へズラすための基準ポイント数")]
        public float offsetDistance = 100f;

        private bool hasPointerEntered = false;

        // Modern Botanical Palette
        private static readonly Color ColorBaseInk = new Color(0.040f, 0.055f, 0.048f, 0.96f);
        private static readonly Color ColorBrassGold = new Color(0.80f, 0.68f, 0.44f, 1.0f);
        private static readonly Color ColorBrassMuted = new Color(0.55f, 0.46f, 0.32f, 0.65f);
        private static readonly Color ColorIvoryWhite = new Color(0.93f, 0.91f, 0.86f, 1.0f);

        private void Awake()
        {
            ApplyBotanicalStyle();
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

        public void Setup(WeaponSeriesData_Alpha series, WeaponPartType_Alpha partType, int quality, List<WeaponEffectSO_Alpha> effects, Vector2 clickPos, WeaponEffectSO_Alpha setBonusEffect = null)
        {
            ApplyBotanicalStyle();

            // 1. バフ一覧と基本情報の生成
            string effectStr = "";
            bool isAllEquipable = false;

            if (effects != null)
            {
                effectStr = BuildEffectString(effects, ref isAllEquipable, "", quality, series, partType);
            }
            
            if (setBonusEffect != null)
            {
                string colorPrefix = "<color=#ccad70>"; // 真鍮ゴールド
                string colorSuffix = "</color>";
                float val = setBonusEffect.GetValue(quality);
                string desc = setBonusEffect.description;
                try { desc = desc.Contains("{0}") ? string.Format(desc, val) : desc.Replace("{0}", val.ToString()); } catch {}
                effectStr += $"\n{colorPrefix}<b>[神格連契]</b> {setBonusEffect.effectName}: {desc}{colorSuffix}";

                string stages = BuildStagesString(setBonusEffect, quality, "  ");
                if (!string.IsNullOrEmpty(stages))
                {
                    effectStr += $"\n{stages}";
                }
            }

            string partStr = "";
            switch (partType)
            {
                case WeaponPartType_Alpha.Bullet: partStr = "弾頭 (Bullet)"; break;
                case WeaponPartType_Alpha.Casing: partStr = "薬莢 (Casing)"; break;
                case WeaponPartType_Alpha.Primer: partStr = "雷管 (Primer)"; break;
            }
            if (isAllEquipable) partStr += " <color=#52b788>(全枠装着可能)</color>";

            string seriesName = series != null ? series.seriesName : "Unknown";
            
            if (detailText != null)
            {
                detailText.text = $"<size=75%><color=#8c7552>SPECIMEN // </color></size><size=115%><b>{seriesName}</b></size>\n" +
                                  $"<color=#adaba2>部位 (Slot)   :</color> <color=#ece8dc>{partStr}</color>\n" +
                                  $"<color=#adaba2>品質 (Quality):</color> <color=#ebbf52>{quality}</color>\n" +
                                  $"\n<color=#ccad70>【固有・付与条項】</color>{effectStr}";
                
                if (series != null && !string.IsNullOrEmpty(series.activeEffectClassName) && !string.IsNullOrEmpty(series.activeEffectDescription))
                {
                    detailText.text += $"\n\n<color=#52b788>【神格条項・マトリックス効果】</color>\n<color=#ece8dc>{series.activeEffectDescription}</color>";
                }
            }

            // 2. アイコンの設定
            if (iconImage != null)
            {
                if (series != null)
                {
                    Sprite targetSprite = series.icon;
                    if (isAllEquipable && series.iconAllEquipable != null) targetSprite = series.iconAllEquipable;
                    else if (partType == WeaponPartType_Alpha.Bullet && series.iconBullet != null) targetSprite = series.iconBullet;
                    else if (partType == WeaponPartType_Alpha.Casing && series.iconCasing != null) targetSprite = series.iconCasing;
                    else if (partType == WeaponPartType_Alpha.Primer && series.iconPrimer != null) targetSprite = series.iconPrimer;

                    iconImage.sprite = targetSprite;
                    iconImage.color = targetSprite != null ? Color.white : Color.clear;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                }
            }

            // 3. 表示位置の調整
            PositionPopup(clickPos);

            // アクティブにして最前面に表示する
            hasPointerEntered = false;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hasPointerEntered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 一度マウスが乗ってから外れたら消す
            if (hasPointerEntered)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // もう一度右クリックしたら消す
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                gameObject.SetActive(false);
            }
        }

        private string BuildEffectString(List<WeaponEffectSO_Alpha> effects, ref bool isAllEquipable, string indent, int quality, WeaponSeriesData_Alpha series, WeaponPartType_Alpha partType)
        {
            string result = "";
            int effectCount = 0;
            int randomEffectCount = 0;

            if (effects != null)
            {
                foreach (var eff in effects)
                {
                    if (eff == null) continue;
                    effectCount++;
                    
                    bool isSpecific = false;

                    if (series != null)
                    {
                        if (partType == WeaponPartType_Alpha.Bullet && series.bulletSpecificEffects != null && series.bulletSpecificEffects.Contains(eff)) isSpecific = true;
                        if (partType == WeaponPartType_Alpha.Casing && series.casingSpecificEffects != null && series.casingSpecificEffects.Contains(eff)) isSpecific = true;
                        if (partType == WeaponPartType_Alpha.Primer && series.primerSpecificEffects != null && series.primerSpecificEffects.Contains(eff)) isSpecific = true;
                    }

                    if (!isSpecific && string.IsNullOrEmpty(indent))
                    {
                        randomEffectCount++;
                    }

                    string prefixIcon = "<color=#52b788>◇</color>"; // 蓮翡翠（通常・ランダム効果）
                    if (isSpecific) prefixIcon = "<color=#ebbf52>◆</color>"; // 向日葵ゴールド（固有特効）

                    if (eff.effectType == WeaponEffectType_Alpha.Composite)
                    {
                        var comp = eff as CompositeWeaponEffectSO_Alpha;
                        if (comp != null)
                        {
                            // 複合スキル自体の名前を表示
                            string nameColor = "#e6d8ce"; // 落ち着いた桜白
                            result += $"\n{indent}{prefixIcon} <color={nameColor}>{eff.effectName}</color>";
                            
                            // 中身を再帰的に展開し、インデントを1段下げる
                            if (comp.subEffects != null && comp.subEffects.Count > 0)
                            {
                                string newIndent = string.IsNullOrEmpty(indent) ? "  " : indent + "  ";
                                result += BuildEffectString(comp.subEffects, ref isAllEquipable, newIndent, quality, series, partType);
                            }
                        }
                    }
                    else
                    {
                        string prefix = string.IsNullOrEmpty(indent) ? prefixIcon : "・";
                        
                        string descStr = "";
                        if (!string.IsNullOrEmpty(eff.description))
                        {
                            try 
                            { 
                                descStr = eff.description.Contains("{0}") ? string.Format(eff.description, eff.GetValue(quality)) : eff.description.Replace("{0}", eff.GetValue(quality).ToString()); 
                            }
                            catch 
                            { 
                                descStr = eff.description; 
                            }
                        }

                        if (!string.IsNullOrEmpty(descStr))
                        {
                            result += $"\n{indent}{prefix} <color=#ece8dc>{eff.effectName}</color>\n{indent}  <size=80%><color=#adaba2>{descStr}</color></size>";
                        }
                        else
                        {
                            result += $"\n{indent}{prefix} <color=#ece8dc>{eff.effectName}</color>";
                        }

                        string stagesStr = BuildStagesString(eff, quality, indent + "  ");
                        if (!string.IsNullOrEmpty(stagesStr))
                        {
                            result += "\n" + stagesStr;
                        }

                        if (eff.effectType == WeaponEffectType_Alpha.AllEquipable)
                        {
                            isAllEquipable = true;
                        }
                    }
                }
            }

            // ルート階層（インデントなし）の場合のみ、空き枠を表示
            if (string.IsNullOrEmpty(indent))
            {
                int emptySlots = quality - randomEffectCount;
                for (int i = 0; i < emptySlots; i++)
                {
                    result += $"\n{indent}<color=#464f4a>□</color> <color=#7a857f>[ 空白条項スロット ]</color>";
                }
            }

            return result;
        }

        private string BuildStagesString(WeaponEffectSO_Alpha effect, int currentCount, string indent)
        {
            string stagesStr = "";
            if (effect.useStepMultiplier)
            {
                int activeStageIndex = 0;
                int[] requiredCounts = new int[4];
                requiredCounts[0] = 1;
                if (effect.stepThresholds != null)
                {
                    for (int i = 0; i < effect.stepThresholds.Length && i < 3; i++)
                    {
                        requiredCounts[i + 1] = effect.stepThresholds[i];
                    }
                }

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
                    catch { }

                    string colorHex = (i == activeStageIndex) ? "#ece8dc" : "#6f7a73"; // 生成り白（活性） / 墨苔灰（休眠）
                    string prefix = (i == activeStageIndex) ? "◆" : "・";

                    stagesStr += $"{indent}<color={colorHex}>{prefix} [必要: {requiredCounts[i]}] {stepDesc}</color>\n";
                }
            }
            else
            {
                string baseDesc = "";
                if (effect.stepDescriptions != null && effect.stepDescriptions.Length > 0 && !string.IsNullOrEmpty(effect.stepDescriptions[0]))
                {
                    baseDesc = effect.stepDescriptions[0];
                    float val = effect.GetValue(currentCount);
                    try
                    {
                        baseDesc = baseDesc.Contains("{0}") ? string.Format(baseDesc, val) : baseDesc.Replace("{0}", val.ToString());
                    }
                    catch { }

                    stagesStr += $"{indent}<color=#ece8dc>◆ {baseDesc}</color>\n";
                }
            }

            return stagesStr.TrimEnd('\n');
        }

        public void SetupForSkill(WeaponEffectSO_Alpha effect, Vector2 clickPos)
        {
            ApplyBotanicalStyle();
            if (effect == null) return;
            
            string desc = effect.description;
            try { desc = desc.Contains("{0}") ? string.Format(desc, effect.GetValue(1)) : desc.Replace("{0}", effect.GetValue(1).ToString()); } catch { }
            
            if (detailText != null)
            {
                detailText.text = $"<size=75%><color=#8c7552>DIVINE CLAUSE // </color></size><size=115%><b>{effect.effectName}</b></size>\n" +
                                  $"<color=#adaba2>分類:</color> <color=#ece8dc>神格条項 (Skill)</color>\n" +
                                  $"\n<color=#ccad70>【条項効果】</color>\n<color=#ece8dc>{desc}</color>";
            }

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = Color.clear;
            }

            PositionPopup(clickPos);
            hasPointerEntered = false;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void PositionPopup(Vector2 clickPos)
        {
            RectTransform myRect = (RectTransform)transform;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(myRect);
            Vector2 size = myRect.rect.size;

            if (transform.parent is RectTransform parentRect)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (canvas != null ? canvas.worldCamera : null);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, 
                    clickPos, 
                    cam,
                    out Vector2 localPoint);
                
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect, 
                    screenCenter, 
                    cam, 
                    out Vector2 localCenter);
                
                Vector2 dirToCenter = (localCenter - localPoint).normalized;
                Vector2 targetPos = localPoint + dirToCenter * offsetDistance;

                // ---- 親の矩形領域内にはみ出さないよう Clamp ----
                Vector2 minPos = parentRect.rect.min;
                Vector2 maxPos = parentRect.rect.max;

                float minX = minPos.x + myRect.pivot.x * size.x;
                float maxX = maxPos.x - (1f - myRect.pivot.x) * size.x;
                float minY = minPos.y + myRect.pivot.y * size.y;
                float maxY = maxPos.y - (1f - myRect.pivot.y) * size.y;

                if (minX <= maxX) targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
                if (minY <= maxY) targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

                myRect.localPosition = new Vector3(targetPos.x, targetPos.y, 0f);
            }
            else
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 dirToCenter = (screenCenter - clickPos).normalized;
                transform.position = clickPos + dirToCenter * offsetDistance;
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
