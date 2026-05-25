using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStatusManager_Alpha : ObjectStatus_Alpha
{
    // 親クラス(ObjectStatus_Alpha)が HP, currentHP を持っているためここでの再定義は不要

    // HPBar更新用などのイベント（引数で現在HPと最大HPを渡す）
    public delegate void PlayerHPChangedHandler(float current, float max);
    public static event PlayerHPChangedHandler OnPlayerHPChanged;

    public static playerStatusManager_Alpha Instance { get; private set; }

    [Header("Base Status Caches (For Buff Recalculation)")]
    private float baseMaxHP;
    private float baseStaminaRecovery;
    private float baseDamageAdd;
    private float baseDamageMag;
    private float baseBlockDmg;
    private float baseBlockMag;
    private float baseBulletSpeedMag;

    [Header("New Status Fields")]
    public float bulletLifeMag = 1.0f;
    private float baseBulletLifeMag = 1.0f;
    
    public int extraPierceCount = 0;
    private int baseExtraPierceCount = 0;

    public int extraShotCount = 0;
    private int baseExtraShotCount = 0;

    public enum SpawnPattern { Straight, Barrage, Radial, Reverse }
    public SpawnPattern currentSpawnPattern = SpawnPattern.Straight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 初期値をキャッシュ
        baseMaxHP = HP;
        baseStaminaRecovery = staminaRecoveryRate;
        baseDamageAdd = DamageAdd;
        baseDamageMag = DamageMag;
        baseBlockDmg = BlockDmg;
        baseBlockMag = BlockMag;
        baseBulletSpeedMag = bulletSpeedMag;
        baseBulletLifeMag = bulletLifeMag;
        baseExtraPierceCount = extraPierceCount;
        baseExtraShotCount = extraShotCount;
    }

    [ContextMenu("Recalculate Buffs")]
    public void UpdateEquipmentBuffs()
    {
        if (InventoryManager_Alpha.Instance == null) return;
        
        // --- 1. 計算前の基礎値リセット ---
        HP = baseMaxHP;
        staminaRecoveryRate = baseStaminaRecovery;
        DamageAdd = baseDamageAdd;
        DamageMag = baseDamageMag;
        BlockDmg = baseBlockDmg;
        BlockMag = baseBlockMag;
        bulletSpeedMag = baseBulletSpeedMag;
        bulletLifeMag = baseBulletLifeMag;
        extraPierceCount = baseExtraPierceCount;
        extraShotCount = baseExtraShotCount;
        currentSpawnPattern = SpawnPattern.Straight; // デフォルトはStraight

        // --- 2. 各ステータスバフを取得・加算 ---
        var inv = InventoryManager_Alpha.Instance;
        
        float hpBuff = inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.MaxHP);
        HP += hpBuff;
        
        staminaRecoveryRate += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.StaminaRecoverySpeed);
        DamageAdd += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackFlat);
        DamageAdd -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackDebuff);
        DamageMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackMultiplier);
        BlockDmg += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.DefenseFlat);
        BlockMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.DefenseMultiplier);
        bulletSpeedMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletSpeed);
        bulletSpeedMag -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletSpeedDebuff);
        bulletLifeMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletLife);

        // 貫通回数の加算（floatをintにキャストして適用）
        extraPierceCount += (int)inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.PierceCountPlus);

        // 発射弾数の加算
        extraShotCount += (int)inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.ShotCountPlus);

        // --- 3. 発射パターンの優先度決定 ---
        if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Reverse) > 0)
        {
            currentSpawnPattern = SpawnPattern.Reverse;
        }
        else if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Radial) > 0)
        {
            currentSpawnPattern = SpawnPattern.Radial;
        }
        else if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Barrage) > 0)
        {
            currentSpawnPattern = SpawnPattern.Barrage;
        }
        else if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Straight) > 0)
        {
            currentSpawnPattern = SpawnPattern.Straight; // 明示的なStraight付与（なくてもデフォルトがStraight）
        }

        // --- 4. 最大HPが変化した場合の現在HP補正 ---
        if (currentHP > HP) 
        {
            currentHP = HP;
        }
        else if (hpBuff > 0)
        {
            // MaxHPが増えた分だけ現在HPも回復させる
            // ※シンプル化のため、基礎値からの超過分を回復分として加算していますが、
            //   実際は装備変更による差分を計算するか、HP割合を維持する方が安全です。
            //   ここでは割合維持方式を採用します。
            float hpRatio = currentHP / baseMaxHP; // 旧HPからの割合
            currentHP = HP * hpRatio; 
        }

        OnPlayerHPChanged?.Invoke(currentHP, HP);
        Debug.Log($"[PlayerStatusManager] Equipment Buffs Updated. New MaxHP: {HP}, DmgMag: {DamageMag}");
    }

    // --- Damage / Defense Calculation API ---

    public float GetFinalDamage(float baseWeaponDamage = 0f)
    {
        // 最終火力 = (基礎火力(pow) + 武器の基本ダメージ + 火力上昇(DamageAdd)) * 火力倍率(DamageMag / 100)
        return (pow + baseWeaponDamage + DamageAdd) * (DamageMag / 100f);
    }

    public float GetTakenDamage(float enemyDamage)
    {
        // 被ダメージ = (敵の元々のダメージ - 防御力上昇(BlockDmg)) * 防御力倍率(BlockMag / 100)
        // ※BlockMag は 100 を基準とし、90なら10%カット、110なら10%増加とする（被ダメージ倍率）
        float dmg = (enemyDamage - BlockDmg) * (BlockMag / 100f);
        return Mathf.Max(1f, dmg); // 最低1ダメージは受ける仕様にするか、0を許容するか。ここでは最低1とする。
    }

    [Header("Initial Stun Settings")]
    public float initialStunResistance = 0f;
    public float initialBaseStunResistance = 0.5f;

    [Header("Movement Settings")]
    public float moveSpeed;
    public float moveSpeedMag = 1f;
    public float moveSpeedMag_CONST = 1.0f;
    public float bulletSpeed;
    public float bulletSpeedMag = 1.0f;
    public float BulletSpan; // フレーム
    public float BulletSpanMag = 1.0f;

    public enum SpecialMoveType { None, Dash, Warp }
    [Header("Special Move (Dash/Warp) Settings")]
    public SpecialMoveType currentSpecialMove = SpecialMoveType.Dash;

    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRecoveryRate = 10f; // 100 units over 10 seconds
    public float staminaRecoveryDelay = 0.5f;
    [HideInInspector] public float lastStaminaConsumeTime = -100f;

    public float dashDuration = 0.1f;
    public float dashStaminaCost = 30f;
    public float dashDistance = 4f; // 調整可能

    public float warpDuration = 0.2f;  // DBZ瞬間移動のタメ時間
    public float warpStaminaCost = 45f;
    public float warpDistance = 8f; // 調整可能

    [Header("Weapon Synthesis Settings")]
    [Tooltip("特定条件下で1〜3行目のすべての武器効果を使用可能にするフラグ")]
    public bool canUseAllEffects = false;

    [Header("Pierce Settings")]
    [Tooltip("貫通時のダメージ減衰率（デフォルト25%減）。アイテム等で0.10fなどに変動")]
    public float pierceDamageReductionRate = 0.25f;

    void Update()
    {
        if (Time.time > lastStaminaConsumeTime + staminaRecoveryDelay)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
        }
    }

    // --- Player Health Management API ---
    
    public void ApplyDamage(float amount)
    {
        float finalDamage = GetTakenDamage(amount);
        currentHP -= finalDamage;
        if (currentHP < 0) currentHP = 0;
        
        Debug.Log($"[PlayerStatusManager] ApplyDamage: Original {amount} -> Final {finalDamage} | Current HP: {currentHP}");

        // UI更新などのためにイベント発火（受け手が未実装でも安全）
        OnPlayerHPChanged?.Invoke(currentHP, HP);

        if (currentHP <= 0)
        {
            nowHPGauge--;
            if (nowHPGauge < 1) {
                Die();
            }           
            
            currentHP = HP;
            OnPlayerHPChanged?.Invoke(currentHP, HP);
        }
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > HP) currentHP = HP;
        
        Debug.Log($"[PlayerStatusManager] Heal: {amount} | Current HP: {currentHP}");

        OnPlayerHPChanged?.Invoke(currentHP, HP);
    }

    private void Die()
    {
        Debug.Log("[PlayerStatusManager] Player HP reached 0. Triggering GameOver.");
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null)
        {
            GameManager manager = gmObj.GetComponent<GameManager>();
            if (manager != null)
            {
                manager.GameOver();
            }
            else
            {
                Debug.LogError("GameManager component not found on GameManager object!");
            }
        }
        else
        {
            Debug.LogError("GameManager object not found in the scene!");
        }
    }
}
