using UnityEngine;

namespace Alpha.Data
{
    public enum WeaponEffectType_Alpha
    {
        // 旧仕様互換（必要に応じて削除可能）
        StatUp,
        BulletChange,
        Constraint,
        Other,

        // --- 今回追加する8種類のステータス系 ---
        MaxHP,                 // 最大HP上昇
        StaminaRecoverySpeed,  // スタミナ回復速度上昇
        AttackFlat,            // 火力上昇（固定値）
        AttackDebuff,          // 火力デバフ（固定値）
        AttackMultiplier,      // 火力倍率強化（パーセント）
        BulletLife,            // 弾の生存時間延長
        BulletLifeDebuff,      // 弾の生存時間短縮
        BulletSpeed,           // 弾速強化
        BulletSpeedDebuff,     // 弾速減衰
        DefenseFlat,           // 防御力上昇（固定値）
        DefenseMultiplier,     // 防御力倍率上昇（パーセント）
        PierceCountPlus,       // 貫通数増加
        ShotCountPlus,         // 発射数増加
        SpawnPattern_Straight, // 発射挙動：ストレート
        SpawnPattern_Reverse,  // 発射挙動：リバース
        SpawnPattern_Barrage,  // 発射挙動：バラージ
        SpawnPattern_Radial,   // 発射挙動：放射
        
        // --- 特殊効果 ---
        AddActiveEffect_Volt,      // 雷生成パッシブ効果
        AddActiveEffect_Explosion, // 爆発生成パッシブ効果
        IgnorePierceDecay,         // 貫通減衰無効化
        MakeBarrier,               // バリア付与
        BurstFire,                 // 指定回数のバースト発射化
        Homing,                    // ホーミング（旋回力0〜100）
        Composite,                 // 複合スキル
        // --- 装備制限解除 ---
        AllEquipable           // どこにでも装備可能
    }

    [CreateAssetMenu(fileName = "NewWeaponEffect", menuName = "Alpha/Weapon Effect")]
    public class WeaponEffectSO_Alpha : ScriptableObject
    {
        public WeaponEffectType_Alpha effectType;
        public string effectName;
        public string description;
        
        [Tooltip("trueの場合、どの装備枠にセットしていても常に発動します。falseの場合、現在構えている装備セットの時だけ発動します。")]
        public bool isGlobalEffect = false;

        [Tooltip("trueの場合、同シリーズが3部位揃っており、かつこのパーツが最適部位（BestSlot）に装備されている時のみ発動します。")]
        public bool isBestSlotEffect = false;

        [Tooltip("品質(1〜4)による効果量。インデックス0=品質1、インデックス3=品質4")]
        public float[] qualityValues = new float[4];

        public float GetValue(int quality)
        {
            if (qualityValues == null || qualityValues.Length == 0) return 0f;
            int index = Mathf.Clamp(quality - 1, 0, qualityValues.Length - 1);
            return qualityValues[index];
        }
    }
}
