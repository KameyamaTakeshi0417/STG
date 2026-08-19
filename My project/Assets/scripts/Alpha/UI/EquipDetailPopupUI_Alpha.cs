using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Alpha.Data;

namespace Alpha.UI
{
    public class EquipDetailPopupUI_Alpha : MonoBehaviour, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI References")]
        public Image iconImage;
        public TextMeshProUGUI detailText;

        [Header("Settings")]
        [Tooltip("クリックした位置から画面中央方向へズラすための基準ポイント数")]
        public float offsetDistance = 100f;

        public void Setup(WeaponSeriesData_Alpha series, WeaponPartType_Alpha partType, int quality, List<WeaponEffectSO_Alpha> effects, Vector2 clickPos, WeaponEffectSO_Alpha setBonusEffect = null)
        {
            // 1. バフ一覧と基本情報の生成
            string effectStr = "";
            bool isAllEquipable = false;

            if (effects != null)
            {
                effectStr = BuildEffectString(effects, ref isAllEquipable, "", quality, series, partType);
            }
            
            if (setBonusEffect != null)
            {
                string colorPrefix = "<color=#FFFF00>"; // 黄色等で目立たせる
                string colorSuffix = "</color>";
                float val = setBonusEffect.GetValue(quality);
                string desc = setBonusEffect.description;
                try { desc = desc.Contains("{0}") ? string.Format(desc, val) : desc.Replace("{0}", val.ToString()); } catch {}
                effectStr += $"\n{colorPrefix}<b>[セットボーナス]</b> {setBonusEffect.effectName}: {desc}{colorSuffix}";

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
            if (isAllEquipable) partStr += " (どこでも装備可能)";

            string seriesName = series != null ? series.seriesName : "Unknown";
            
            if (detailText != null)
            {
                detailText.text = $"<size=120%><b>{seriesName}</b></size>\n" +
                                  $"部位: {partStr}\n" +
                                  $"Quality: {quality}\n" +
                                  $"\n<color=#FFFF00>【効果】</color>{effectStr}";
                
                if (series != null && !string.IsNullOrEmpty(series.activeEffectClassName) && !string.IsNullOrEmpty(series.activeEffectDescription))
                {
                    detailText.text += $"\n\n<color=#00FFFF>【マトリックス効果】</color>\n<color=#CCCCCC>{series.activeEffectDescription}</color>";
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
                
                RectTransform myRect = (RectTransform)transform;
                myRect.localPosition = localPoint + dirToCenter * offsetDistance;
            }
            else
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 dirToCenter = (screenCenter - clickPos).normalized;
                transform.position = clickPos + dirToCenter * offsetDistance;
            }

            // アクティブにして最前面に表示する
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // マウスカーソルが外れたら消す
            gameObject.SetActive(false);
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

                    string prefixIcon = "<color=#44FF44>■</color>"; // Green for random
                    if (isSpecific) prefixIcon = "<color=#FFA500>■</color>"; // Orange for specific

                    if (eff.effectType == WeaponEffectType_Alpha.Composite)
                    {
                        var comp = eff as CompositeWeaponEffectSO_Alpha;
                        if (comp != null)
                        {
                            // 複合スキル自体の名前を表示
                            string nameColor = "#FFDDDD";
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
                            result += $"\n{indent}{prefix} {eff.effectName}\n{indent}  <size=80%>{descStr}</size>";
                        }
                        else
                        {
                            result += $"\n{indent}{prefix} {eff.effectName}";
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
                    result += $"\n{indent}<color=#555555>□</color> <color=#888888>[ 空きスロット ]</color>";
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

                    string colorHex = (i == activeStageIndex) ? "#FFFFFF" : "#808080";
                    string prefix = (i == activeStageIndex) ? "▶" : "・";

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
                    stagesStr += $"{indent}<color=#FFFFFF>▶ {baseDesc}</color>\n";
                }
            }

            return stagesStr.TrimEnd('\n');
        }

        public void SetupForSkill(WeaponEffectSO_Alpha effect, Vector2 clickPos)
        {
            if (effect == null) return;
            
            string desc = effect.description;
            try { desc = desc.Contains("{0}") ? string.Format(desc, effect.GetValue(1)) : desc.Replace("{0}", effect.GetValue(1).ToString()); } catch { }
            
            if (detailText != null)
            {
                detailText.text = $"<size=120%><b>{effect.effectName}</b></size>\n" +
                                  $"Type: Skill\n" +
                                  $"\n<color=#FFFF00>【効果】</color>\n<color=#CCCCCC>{desc}</color>";
            }

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = Color.clear;
            }

            PositionPopup(clickPos);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void PositionPopup(Vector2 clickPos)
        {
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
                
                RectTransform myRect = (RectTransform)transform;
                myRect.localPosition = localPoint + dirToCenter * offsetDistance;
            }
            else
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 dirToCenter = (screenCenter - clickPos).normalized;
                transform.position = clickPos + dirToCenter * offsetDistance;
            }
        }
    }
}
