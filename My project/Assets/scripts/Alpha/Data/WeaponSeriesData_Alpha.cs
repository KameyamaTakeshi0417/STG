using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        
        [Tooltip("旧仕様互換のため残していますが、基本は下の4つのアイコンを使用してください")]
        public Sprite icon;
        
        [Header("Icons (Per Part Type)")]
        public Sprite iconBullet;
        public Sprite iconCasing;
        public Sprite iconPrimer;
        public Sprite iconAllEquipable;
        
        [Tooltip("レア枠として扱うか（全30中7シリーズをtrueにする想定）")]
        public bool isRareSeries;
        
        [Range(1, 4)]
        [Tooltip("抽選下限品質")]
        public int minQuality = 1;
        
        [Tooltip("このシリーズの最適部位（個性が出る枠）")]
        public WeaponPartType_Alpha bestSlot;

        [Header("Bullet Settings")]
        [Tooltip("このシリーズが雷管（インデックス2）に装備された時に発射される弾のプレハブ")]
        public GameObject bulletPrefab;

        [Header("Effects (Series Common)")]
        [Tooltip("装備枠・フリー枠・テンポラリー枠のどこにあっても発動するパッシブステータス効果（全パーツ共通）")]
        public List<WeaponEffectSO_Alpha> passiveEffects = new List<WeaponEffectSO_Alpha>();
        
        [Header("Effects (Part Specific)")]
        [Tooltip("このシリーズの弾頭(Bullet)パーツが生成された時に付与される固有効果")]
        public List<WeaponEffectSO_Alpha> bulletSpecificEffects = new List<WeaponEffectSO_Alpha>();

        [Tooltip("このシリーズの薬莢(Casing)パーツが生成された時に付与される固有効果")]
        public List<WeaponEffectSO_Alpha> casingSpecificEffects = new List<WeaponEffectSO_Alpha>();

        [Tooltip("このシリーズの雷管(Primer)パーツが生成された時に付与される固有効果")]
        public List<WeaponEffectSO_Alpha> primerSpecificEffects = new List<WeaponEffectSO_Alpha>();

        [Header("Active Effect")]
        [Tooltip("発射・航行・着弾時に挙動を変化させる Alpha_Effect_Base 派生クラスの名前（例: Karamidan_Effect_Alpha）")]
        public string activeEffectClassName;
    }
}
