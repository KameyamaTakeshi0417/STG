using UnityEngine;

namespace Alpha.Data
{
    public enum WeaponEffectType_Alpha
    {
        // --- 今回追加する8種類のステータス系 ---
        MaxHP,                 // 最大HP上昇
        StaminaRecoverySpeed,  // スタミナ回復速度上昇
        AttackFlatPlus,        // 基礎攻撃力上昇（固定値）
        AttackFlatMinus,       // 基礎攻撃力デバフ（固定値）
        AttackMultiplierPlus,  // 基礎攻撃倍率上昇（パーセント）
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
        AllEquipable,          // どこにでも装備可能

        // --- スペシャルムーブ切り替え ---
        SpecialMove_Focus,
        SpecialMove_Warp,
        SpecialMove_Dash,
        
        // --- HPゲージ関連 ---
        HPGaugePlus,
        
        // --- 追加ステータスデバフ ---
        AttackMultiplierMinus,  // 基礎攻撃倍率デバフ（パーセント）

        // --- 弾変更 ---
        BulletChange,

        // --- 防御系デバフ ---
        DefenseFlatMinus,       // 防御力低下（固定値）
        DefenseMultiplierMinus, // 防御力低下（パーセント）

        // --- 装填速度（インターバル） ---
        ReloadSpeedPlus,        // 装填速度上昇（インターバル短縮）
        ReloadSpeedMinus,       // 装填速度低下（インターバル延長）

        // --- 特殊枠 ---
        Wildcard,               // 全シリーズ対応（ジョーカー）
        StaminaExhaustionRecoveryBoost, // スタミナ枯渇時の回復速度倍率
        CircularSubShotPlus,    // 輝照弾（サーキュラー）のサブバレット数増加
        VoltTickReduce,         // 毒絡弾（Volt）のダメージTick短縮
        SecondaryDamageUp,      // 派生ダメージ（Secondary Damage）の威力上昇
        Unsellable,             // 販売不能（売却不可）
        
        // --- 固有スキル ---
        DivineExecutioner       // 神裁者（ジャッジメント/エクスキューショナー固有）
    }

    [CreateAssetMenu(fileName = "NewWeaponEffect", menuName = "Alpha/Weapon Effect")]
    public class WeaponEffectSO_Alpha : ScriptableObject
    {
        public WeaponEffectType_Alpha effectType;
        public string effectName;
        public Sprite effectIcon;
        
        [TextArea(3, 10)]
        public string description;
        
        [Tooltip("trueの場合、どの装備枠にセットしていても常に発動します。falseの場合、現在構えている装備セットの時だけ発動します。")]
        public bool isGlobalEffect = false;

        [Header("Drop Settings")]
        [Tooltip("ドロップ時にこのエフェクトが付与されるためのレアリティ（1: Common, 2: Uncommon, 3: Rare, 4: Divine）。")]
        [Range(1, 4)]
        public int minQuality = 1;

        [Header("Accumulation Settings")]
        [Tooltip("trueの場合、ローカル効果であっても他のウェポンセットに装備されている同効果の値を合算（パブリックカウント）します。デバフなどで他のセットの値を合算させたくない場合はfalseにしてください。")]
        public bool accumulateGlobally = true;

        [Header("Shop Settings")]
        public int price = 100;
        public int sellPrice = 50;

        [Header("Value Calculation")]
        [Tooltip("合計品質に基づく段階的乗算を使用するか")]
        public bool useStepMultiplier = false;

        [Tooltip("段階が変化する合計品質の閾値（例: 3, 10, 20）。配列のサイズは基本的に3にしてください。")]
        public int[] stepThresholds = new int[] { 3, 10, 20 };

        [Tooltip("品質(1〜4)による効果量、または合計品質時の段階別乗数（インデックス0=第1段階, インデックス3=第4段階）")]
        public float[] qualityValues = new float[4];

        [TextArea(2, 5)]
        [Tooltip("インスペクタで段階ごと（1～4段階）の強化内容を説明するテキスト")]
        public string[] stepDescriptions = new string[4];

        public bool IsDebuff()
        {
            return effectType == WeaponEffectType_Alpha.AttackFlatMinus ||
                   effectType == WeaponEffectType_Alpha.AttackMultiplierMinus ||
                   effectType == WeaponEffectType_Alpha.DefenseFlatMinus ||
                   effectType == WeaponEffectType_Alpha.DefenseMultiplierMinus ||
                   effectType == WeaponEffectType_Alpha.BulletSpeedDebuff ||
                   effectType == WeaponEffectType_Alpha.BulletLifeDebuff ||
                   effectType == WeaponEffectType_Alpha.ReloadSpeedMinus;
        }

        public float GetValue(int quality)
        {
          

            if (qualityValues == null || qualityValues.Length == 0) return 0f;
            // 品質(1〜4想定)をインデックス(0〜3)に変換。配列外アクセスを防ぐ
            int index = Mathf.Clamp(quality - 1, 0, qualityValues.Length - 1);
            return qualityValues[index];
        }
    }
}
