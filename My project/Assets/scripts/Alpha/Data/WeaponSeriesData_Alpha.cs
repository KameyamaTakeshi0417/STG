using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Data
{
    public enum WeaponPartType_Alpha
    {
        Bullet, // 弾頭
        Casing, // 薬莢
        Primer  // 雷管
    }

    [CreateAssetMenu(fileName = "NewWeaponSeries", menuName = "Alpha/Weapon Series Data")]
    public class WeaponSeriesData_Alpha : ScriptableObject
    {
        public string seriesName;
        
        [Tooltip("UI用アイコン")]
        public Sprite icon;
        
        [Tooltip("レア枠として扱うか（全30中7シリーズをtrueにする想定）")]
        public bool isRareSeries;
        
        [Range(1, 4)]
        [Tooltip("抽選下限品質")]
        public int minQuality = 1;
        
        [Tooltip("このシリーズの最適部位（個性が出る枠）")]
        public WeaponPartType_Alpha bestSlot;

        [Header("Effects")]
        [Tooltip("どこに装備しても発動する汎用効果プール")]
        public List<WeaponEffectSO_Alpha> anySlotEffects = new List<WeaponEffectSO_Alpha>();
        
        [Tooltip("最適部位に装備した時の強力な効果プール")]
        public List<WeaponEffectSO_Alpha> bestSlotEffects = new List<WeaponEffectSO_Alpha>();
    }
}
