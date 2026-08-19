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

    [System.Serializable]
    public struct SeriesPassiveEffect
    {
        public WeaponEffectSO_Alpha effect;
        [Tooltip("0ならパーツ品質に依存。1以上ならその値を固定品質として扱う（パーツの品質を無視します）")]
        public int fixedQualityOverride;
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

        [Header("Chimera UI Settings (Plant Motif)")]
        [Tooltip("画像の基準点(Pivot)。0～1の値。中央に根元があれば(0.5, 0.5)")]
        public Vector2 pivotPrimer = new Vector2(0.5f, 0.5f);
        public Vector2 pivotCasing = new Vector2(0.5f, 0.5f);
        public Vector2 pivotBullet = new Vector2(0.5f, 0.5f);

        [Tooltip("茎根(Primer)として装備された時、葉(Casing)が生える位置(0～1)")]
        public List<Vector2> leafAttachmentPoints = new List<Vector2>() { new Vector2(0.5f, 1.0f) };

        [Tooltip("茎根(Primer)として装備された時、花(Bullet)が咲く位置(0～1)")]
        public List<Vector2> flowerAttachmentPoints = new List<Vector2>() { new Vector2(0.5f, 1.0f) };

        [Tooltip("各パーツの表示スケール調整用")]
        public Vector2 scalePrimer = Vector2.one;
        public Vector2 scaleCasing = Vector2.one;
        public Vector2 scaleBullet = Vector2.one;
        
        [Tooltip("レア枠として扱うか（全30中7シリーズをtrueにする想定）")]
        public bool isRareSeries;
        
        [Range(1, 4)]
        [Tooltip("抽選下限品質")]
        public int minQuality = 1;
        
        [Tooltip("このシリーズの最適部位（個性が出る枠）")]
        public WeaponPartType_Alpha bestSlot;

        [Header("Synergy (Blessing) Settings")]
        [Tooltip("このシリーズが装備された際に加算される特性ポイント（祝福の発動に用いる）")]
        public int traitPoint = 1;

        [Header("Part Specific Buffs")]
        public float basePowerBonus = 1.0f;
        
        public float survivalTimeBonus = 0.5f;
        
        public float speedBonus = 100.0f;

        [Header("Bullet Settings")]
        [Tooltip("このシリーズが雷管（インデックス2）に装備された時に発射される弾のプレハブ")]
        public GameObject bulletPrefab;

        [Header("Legacy Effects (Will be removed in Phase 4)")]
        public WeaponEffectSO_Alpha seriesCompleteEffect;
        public List<SeriesPassiveEffect> passiveEffects = new List<SeriesPassiveEffect>();
        public List<WeaponEffectSO_Alpha> bulletSpecificEffects = new List<WeaponEffectSO_Alpha>();
        public List<WeaponEffectSO_Alpha> casingSpecificEffects = new List<WeaponEffectSO_Alpha>();
        public List<WeaponEffectSO_Alpha> primerSpecificEffects = new List<WeaponEffectSO_Alpha>();

        [Header("Active Effect")]
        [Tooltip("発射・航行・着弾時に挙動を変化させるAlpha_Effect_Base 派生クラスの名前（例: Karamidan_Effect_Alpha）")]
        public string activeEffectClassName;

        [Tooltip("上記のアクティブエフェクト（マトリックスエフェクト）のUI表示用説明文")]
        [TextArea(2, 5)]
        public string activeEffectDescription;
    }
}
