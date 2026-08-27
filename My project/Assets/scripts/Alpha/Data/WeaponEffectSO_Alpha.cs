using UnityEngine;

namespace Alpha.Data
{
    [System.Serializable]
    public class ActiveWeaponEffect_Alpha
    {
        public WeaponEffectSO_Alpha effectSO;
        public int rarity;
        public float genericTimer;

        public ActiveWeaponEffect_Alpha(WeaponEffectSO_Alpha so, int rarity)
        {
            this.effectSO = so;
            this.rarity = rarity;
            this.genericTimer = 0f;
        }
    }

    public enum WeaponEffectType_Alpha
    {
        MaxHP,
        StaminaRecoverySpeed,
        AttackFlatPlus,
        AttackFlatMinus,
        AttackMultiplierPlus,
        AttackMultiplierMinus,
        BulletLife,
        BulletLifeDebuff,
        BulletSpeed,
        BulletSpeedDebuff,
        DefenseFlat,
        DefenseFlatMinus,
        DefenseMultiplier,
        DefenseMultiplierMinus,
        PierceCountPlus,
        ShotCountPlus,
        SpawnPattern_Straight,
        SpawnPattern_Reverse,
        SpawnPattern_Barrage,
        SpawnPattern_Radial,
        AddActiveEffect_Volt,
        AddActiveEffect_Explosion,
        IgnorePierceDecay,
        MakeBarrier,
        BurstFire,
        Homing,
        Composite,
        AllEquipable,
        SpecialMove_Focus,
        SpecialMove_Warp,
        SpecialMove_Dash,
        HPGaugePlus,
        BulletChange,
        ReloadSpeedPlus,
        ReloadSpeedMinus,
        Wildcard,
        StaminaExhaustionRecoveryBoost,
        CircularSubShotPlus,
        VoltTickReduce,
        SecondaryDamageUp,
        Unsellable,
        DivineExecutioner,
        AddActiveEffect_Sunflower
    }

    [CreateAssetMenu(fileName = "NewWeaponEffect", menuName = "Alpha/Weapon Effect")]
    public class WeaponEffectSO_Alpha : ScriptableObject
    {
        public WeaponEffectType_Alpha effectType;
        public string effectName;
        public Sprite effectIcon;
        
        [TextArea(3, 10)]
        public string description;
        
        public bool isGlobalEffect = false;
        
        [Range(1, 4)]
        public int minQuality = 1;
        
        public bool accumulateGlobally = true;
        
        public int price = 100;
        public int sellPrice = 50;
        
        public bool useStepMultiplier = false;
        public int[] stepThresholds = new int[] { 3, 10, 20 };
        public float[] qualityValues = new float[4];
        
        [TextArea(2, 5)]
        public string[] stepDescriptions = new string[4];
        
        [Header("Behavior Override")]
        public Alpha_BulletBehavior_Base overrideBehavior;

        public virtual void OnWeaponFire(GameObject shooter, Vector3 muzzlePos, Vector3 aimDir, float damage, int rarity) { }
        public virtual void OnFire(Bullet_Base bullet, int rarity) { }
        public virtual void OnFlight(Bullet_Base bullet, int rarity, ref float genericTimer, float deltaTime) { }
        public virtual void OnHit(Bullet_Base bullet, int rarity, Collider2D target) { }

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
            int index = Mathf.Clamp(quality - 1, 0, qualityValues.Length - 1);
            return qualityValues[index];
        }
    }
}

