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

        public void Setup(WeaponSeriesData_Alpha series, WeaponPartType_Alpha partType, int quality, List<WeaponEffectSO_Alpha> effects, Vector2 clickPos, bool isBestSlotMet = false)
        {
            // 1. バフ一覧と基本情報の生成
            string effectStr = "";
            bool isAllEquipable = false;

            if (effects != null)
            {
                effectStr = BuildEffectString(effects, ref isAllEquipable, "", quality, isBestSlotMet);
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

        private string BuildEffectString(List<WeaponEffectSO_Alpha> effects, ref bool isAllEquipable, string indent, int quality, bool isBestSlotMet)
        {
            string result = "";
            foreach (var eff in effects)
            {
                if (eff == null) continue;
                
                // BestSlot専用効果なのに条件を満たしていない場合
                bool isInactive = eff.isBestSlotEffect && !isBestSlotMet;
                string inactiveTag = isInactive ? " <color=#888888>(未発動)</color>" : "";
                string colorStart = isInactive ? "<color=#888888>" : "";
                string colorEnd = isInactive ? "</color>" : "";

                if (eff.effectType == WeaponEffectType_Alpha.Composite)
                {
                    var comp = eff as CompositeWeaponEffectSO_Alpha;
                    if (comp != null)
                    {
                        // 複合スキル自体の名前を表示
                        string prefix = string.IsNullOrEmpty(indent) ? "-" : "└";
                        string nameColor = isInactive ? "#888888" : "#FFDDDD";
                        result += $"\n{indent}{prefix} <color={nameColor}>{eff.effectName}</color>{inactiveTag}";
                        
                        // 中身を再帰的に展開し、インデントを1段下げる
                        if (comp.subEffects != null && comp.subEffects.Count > 0)
                        {
                            string newIndent = string.IsNullOrEmpty(indent) ? "  " : indent + "  ";
                            result += BuildEffectString(comp.subEffects, ref isAllEquipable, newIndent, quality, isBestSlotMet);
                        }
                    }
                }
                else
                {
                    string prefix = string.IsNullOrEmpty(indent) ? "-" : "└";
                    
                    string descStr = "";
                    if (!string.IsNullOrEmpty(eff.description))
                    {
                        try 
                        { 
                            descStr = string.Format(eff.description, eff.GetValue(quality)); 
                        }
                        catch 
                        { 
                            descStr = eff.description; 
                        }
                    }

                    if (!string.IsNullOrEmpty(descStr))
                    {
                        result += $"\n{indent}{prefix} {colorStart}{eff.effectName}{inactiveTag}\n{indent}  <size=80%>{descStr}</size>{colorEnd}";
                    }
                    else
                    {
                        result += $"\n{indent}{prefix} {colorStart}{eff.effectName}{inactiveTag}{colorEnd}";
                    }

                    if (eff.effectType == WeaponEffectType_Alpha.AllEquipable && !isInactive)
                    {
                        isAllEquipable = true;
                    }
                }
            }
            return result;
        }
    }
}
