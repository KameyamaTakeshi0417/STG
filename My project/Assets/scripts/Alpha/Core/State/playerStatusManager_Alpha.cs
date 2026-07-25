using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStatusManager_Alpha : ObjectStatus_Alpha
{
    // 親クラス(ObjectStatus_Alpha)が HP, currentHP を持っているためここでの再定義は不要

    // HPBar更新用などのイベント（引数で現在HPと最大HPを渡す）
    public delegate void PlayerHPChangedHandler(float current, float max);
    public static event PlayerHPChangedHandler OnPlayerHPChanged;
    public static event System.Action OnGaugeLost;

    public static playerStatusManager_Alpha Instance { get; private set; }

    [Header("Base Status Caches (For Buff Recalculation)")]
    private float baseMaxHP;
    private float baseStaminaRecovery;
    private float baseDamageAdd;
    private float baseDamageMag;
    private float baseBlockDmg;
    private float baseBlockMag;
    public float BaseBulletSpeedMag => baseBulletSpeedMag;
    private float baseBulletSpeedMag;
    public float BaseBulletSpanMag => baseBulletSpanMag;
    private float baseBulletSpanMag;


    private float baseFocusStaminaCost;
    private float baseWarpStaminaCost;
    private float baseDashStaminaCost;
    private int baseHPGauge;
    private SpecialMoveType baseSpecialMove;

    [Header("New Status Fields")]
    public float bulletLifeMag = 1.0f;
    private float baseBulletLifeMag = 1.0f;
    
    public bool ignorePierceDecay = false;
    [HideInInspector] public float staminaExhaustionRecoveryBoost = 0f;
    
    public int extraPierceCount = 0;
    private int baseExtraPierceCount = 0;

    public int extraShotCount = 0;
    private int baseExtraShotCount = 0;

    [Header("Burst Status")]
    public int burstCount = 1;
    private int baseBurstCount = 1;

    [Header("Barrier Status")]
    public bool hasBarrierBuff = false;
    public float barrierEndurableDamage = 0f;
    public float barrierRespawnTime = 0f;

    [Header("EXP & Petal")]
    public int currentExp = 0;
    public int currentPetals = 0;
    public static event System.Action<int> OnExpAdded;
    public static event System.Action<int> OnPetalAdded;

    public enum SpawnPattern { Straight, Barrage, Radial, Reverse }
    public SpawnPattern currentSpawnPattern = SpawnPattern.Straight;

    [Header("Executioner Status")]
    public int executionerTier = 0;
    public bool isOmniBouquetOverride = false;

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
        baseBulletSpanMag = BulletSpanMag;
        baseBulletLifeMag = bulletLifeMag;
        baseExtraPierceCount = extraPierceCount;
        baseExtraShotCount = extraShotCount;

        baseFocusStaminaCost = focusStaminaCostPerSec;
        baseWarpStaminaCost = warpStaminaCost;
        baseDashStaminaCost = dashStaminaCost;
        baseHPGauge = HPGauge;
        baseSpecialMove = currentSpecialMove;
    }

    private void Start()
    {
        // 起動時に初期装備のバフを適用する
        UpdateEquipmentBuffs();
    }

    private void OnEnable()
    {
        if (Alpha.Flow.StageManager_Alpha.Instance != null)
        {
            Alpha.Flow.StageManager_Alpha.OnBossBattleStateChanged += HandleBossStateChanged;
        }
    }

    private void OnDisable()
    {
        Alpha.Flow.StageManager_Alpha.OnBossBattleStateChanged -= HandleBossStateChanged;
    }

    private void HandleBossStateChanged(bool isBoss)
    {
        UpdateEquipmentBuffs();
    }

    /// <summary>
    /// スキル等でプレイヤーにバリアを付与・設定するメソッド
    /// </summary>
    public void ApplyBarrierToPlayer(int quality, float respawnTime)
    {
        hasBarrierBuff = true;
        barrierEndurableDamage = Mathf.Max(1, quality) * 7f; // 最低でも品質1相当の耐久値を保証
        barrierRespawnTime = respawnTime;

        PlayerHealth[] pHealths = Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        if (pHealths.Length > 0)
        {
            foreach(var pHealth in pHealths)
            {
                Debug.Log($"[PlayerStatusManager] Applying barrier to PlayerHealth on GameObject: '{pHealth.gameObject.name}'. EndurableDamage: {barrierEndurableDamage}");
                pHealth.isBarrierActive = true;
                pHealth.barrierEndurableDamage = barrierEndurableDamage;
                pHealth.barrierBaseRespawnTime = barrierRespawnTime;
            }
        }
        else
        {
            Debug.LogWarning("[PlayerStatusManager] ApplyBarrierToPlayer called but no PlayerHealth was found in scene!");
        }
    }

    [ContextMenu("Recalculate Buffs")]
    public void UpdateEquipmentBuffs()
    {
        if (InventoryManager_Alpha.Instance == null) return;
        var inv = InventoryManager_Alpha.Instance;
        
        // --- アクティブな武器グループを取得 ---
        int activeGroup = 0;
        Player_Shooter_Alpha shooter = Object.FindAnyObjectByType<Player_Shooter_Alpha>();
        if (shooter != null) activeGroup = shooter.currentWeaponGroup;
        
        UpdateExecutionerState(inv, activeGroup);

        bool isBouquet = inv.IsBouquetActive() || isOmniBouquetOverride;
        int groupToPass = isBouquet ? -1 : activeGroup;

        // --- 1. 計算前の基礎値リセット ---
        int oldHPGaugeLimit = HPGauge; // 変更前の最大ゲージ数を保存
        HP = baseMaxHP;
        staminaRecoveryRate = baseStaminaRecovery;
        DamageAdd = baseDamageAdd;
        DamageMag = baseDamageMag;
        BlockDmg = baseBlockDmg;
        BlockMag = baseBlockMag;
        bulletSpeedMag = baseBulletSpeedMag;
        BulletSpanMag = baseBulletSpanMag;
        bulletLifeMag = baseBulletLifeMag;
        extraPierceCount = baseExtraPierceCount;
        extraShotCount = baseExtraShotCount;
        burstCount = baseBurstCount;
        currentSpawnPattern = SpawnPattern.Straight; // デフォルトはStraight

        focusStaminaCostPerSec = baseFocusStaminaCost;
        warpStaminaCost = baseWarpStaminaCost;
        dashStaminaCost = baseDashStaminaCost;
        HPGauge = baseHPGauge;

        // --- 2. 各ステータスバフを取得・加算 ---
        
        float hpBuff = inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.MaxHP, groupToPass);
        HP += hpBuff;
        
        staminaRecoveryRate += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.StaminaRecoverySpeed, groupToPass);
        DamageAdd += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackFlatPlus, groupToPass);
        DamageAdd -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackFlatMinus, groupToPass);
        DamageMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackMultiplierPlus, groupToPass);
        DamageMag -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.AttackMultiplierMinus, groupToPass);

        BlockDmg += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.DefenseFlat, groupToPass);
        BlockDmg -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.DefenseFlatMinus, groupToPass);
        // DefenseMultiplier(防御力上昇)は被ダメージ倍率を下げる、Minusは上げる
        BlockMag -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.DefenseMultiplier, groupToPass);
        BlockMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.DefenseMultiplierMinus, groupToPass);

        bulletSpeedMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletSpeed, groupToPass);
        bulletSpeedMag -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletSpeedDebuff, groupToPass);
        bulletSpeedMag = Mathf.Max(bulletSpeedMag, 0.2f); // 下限20%

        bulletLifeMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletLife, groupToPass);
        bulletLifeMag -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BulletLifeDebuff, groupToPass);
        bulletLifeMag = Mathf.Max(bulletLifeMag, 0.5f); // 下限50%

        bool isBoss = Alpha.Flow.StageManager_Alpha.Instance != null && Alpha.Flow.StageManager_Alpha.Instance.IsBossBattleActive;
        if (isBoss && executionerTier >= 1)
        {
            HP += 150f;
            DamageMag += 100f; // +1.0倍
            BlockDmg += 30f;
        }

        // バースト回数
        float burstVal = inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BurstFire, groupToPass);
        if (burstVal > 0)
        {
            burstCount = Mathf.Max(1, Mathf.FloorToInt(burstVal));
        }

        // 貫通減衰無効化フラグ
        ignorePierceDecay = inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.IgnorePierceDecay, groupToPass) > 0;
        staminaExhaustionRecoveryBoost = inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.StaminaExhaustionRecoveryBoost, groupToPass);

        // バリアの判定（品質とリスポーン時間を取得）
        int maxBarrierQuality = -1; // 0の装備も検知できるように-1からスタート
        float bestRespawnTime = 9999f;
        for (int i = 0; i < inv.equipInstance.Count; i++)
        {
            var item = inv.equipInstance[i];
            if (item.series == null) continue;
            int itemGroup = i / 3;

            System.Action<Alpha.Data.WeaponEffectSO_Alpha> checkBarrier = (effectSO) =>
            {
                if (effectSO != null && effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.MakeBarrier)
                {
                    if (!effectSO.isGlobalEffect && groupToPass != -1 && itemGroup != groupToPass) return;
                    if (item.rarity > maxBarrierQuality) maxBarrierQuality = item.rarity;
                    float rTime = effectSO.GetValue(item.rarity);
                    if (rTime < bestRespawnTime) bestRespawnTime = rTime; // 一番短い復活時間を採用
                }
            };

            if (item.series.passiveEffects != null)
            {
                foreach (var e in item.series.passiveEffects) checkBarrier(e.effect);
            }
            if (item.currentEffects != null)
            {
                foreach (var e in item.currentEffects) checkBarrier(e);
            }
        }

        if (maxBarrierQuality >= 0)
        {
            Debug.Log($"[PlayerStatusManager] Barrier Found! Quality: {maxBarrierQuality}, RespawnTime: {bestRespawnTime}");
            ApplyBarrierToPlayer(maxBarrierQuality, bestRespawnTime);
        }
        else
        {
            Debug.Log("[PlayerStatusManager] No Barrier Buff Found.");
            hasBarrierBuff = false;
            barrierEndurableDamage = 0f;
            barrierRespawnTime = 0f;

            // バリアがない場合、無効化する
            PlayerHealth[] pHealths = Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach(var pHealth in pHealths)
            {
                pHealth.isBarrierActive = false;
                pHealth.barrierEndurableDamage = 0f;
            }
        }

        // 貫通回数の加算（floatをintにキャストして適用）
        extraPierceCount += (int)inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.PierceCountPlus, groupToPass);

        // 発射弾数の加算
        extraShotCount += Mathf.FloorToInt(inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.ShotCountPlus, groupToPass));

        // 装填速度 (ReloadSpeed): Plusは早くなる（Spanが小さくなる）ので引く。Minusは遅くなるので足す。
        BulletSpanMag -= inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.ReloadSpeedPlus, groupToPass);
        BulletSpanMag += inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.ReloadSpeedMinus, groupToPass);

        // バースト発射と発射挙動
        burstCount += Mathf.FloorToInt(inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.BurstFire, groupToPass));
        
        if (isBouquet)
        {
            extraShotCount += 2; // ブーケ状態のボーナス発射数
        }

        // --- 3. 発射パターンの優先度決定 ---
        if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Reverse, groupToPass) > 0)
        {
            currentSpawnPattern = SpawnPattern.Reverse;
        }
        else if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Radial, groupToPass) > 0)
        {
            currentSpawnPattern = SpawnPattern.Radial;
        }
        else if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Barrage, groupToPass) > 0)
        {
            currentSpawnPattern = SpawnPattern.Barrage;
        }
        else if (inv.GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha.SpawnPattern_Straight, groupToPass) > 0)
        {
            currentSpawnPattern = SpawnPattern.Straight; // 明示的なStraight付与（なくてもデフォルトがStraight）
        }

        // --- 5. スペシャルムーブの優先度決定 ---
        int focusScore = 0;
        int warpScore = 0;
        int dashScore = 0;

        float bestFocusCost = 9999f;
        float bestWarpCost = 9999f;
        float bestDashCost = 9999f;

        for (int i = 0; i < inv.equipInstance.Count; i++)
        {
            var item = inv.equipInstance[i];
            if (item.series == null) continue;
            int itemGroup = i / 3;

            System.Action<Alpha.Data.WeaponEffectSO_Alpha> checkSpecialMove = (effectSO) =>
            {
                if (effectSO != null)
                {
                    if (!effectSO.isGlobalEffect && groupToPass != -1 && itemGroup != groupToPass) return;
                    
                    if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.SpecialMove_Focus)
                    {
                        focusScore += item.rarity;
                        float cost = effectSO.GetValue(item.rarity);
                        if (cost > 0 && cost < bestFocusCost) bestFocusCost = cost;
                    }
                    else if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.SpecialMove_Warp)
                    {
                        warpScore += item.rarity;
                        float cost = effectSO.GetValue(item.rarity);
                        if (cost > 0 && cost < bestWarpCost) bestWarpCost = cost;
                    }
                    else if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.SpecialMove_Dash)
                    {
                        dashScore += item.rarity;
                        float cost = effectSO.GetValue(item.rarity);
                        if (cost > 0 && cost < bestDashCost) bestDashCost = cost;
                    }
                }
            };

            if (item.series.passiveEffects != null)
            {
                foreach (var e in item.series.passiveEffects) checkSpecialMove(e.effect);
            }
            if (item.currentEffects != null)
            {
                foreach (var e in item.currentEffects) checkSpecialMove(e);
            }
        }

        // 優先度判定 (Focus > Warp > Dash)
        if (focusScore == 0 && warpScore == 0 && dashScore == 0)
        {
            currentSpecialMove = baseSpecialMove;
        }
        else
        {
            int maxScore = Mathf.Max(focusScore, warpScore, dashScore);
            if (focusScore == maxScore)
            {
                currentSpecialMove = SpecialMoveType.Focus;
            }
            else if (warpScore == maxScore)
            {
                currentSpecialMove = SpecialMoveType.Warp;
            }
            else if (dashScore == maxScore)
            {
                currentSpecialMove = SpecialMoveType.Dash;
            }
        }

        // 消費スタミナの上書き適用
        if (currentSpecialMove == SpecialMoveType.Focus && bestFocusCost != 9999f) focusStaminaCostPerSec = bestFocusCost;
        if (currentSpecialMove == SpecialMoveType.Warp && bestWarpCost != 9999f) warpStaminaCost = bestWarpCost;
        if (currentSpecialMove == SpecialMoveType.Dash && bestDashCost != 9999f) dashStaminaCost = bestDashCost;

        // --- 6. HPGaugeの増加処理 ---
        int hpGaugeBuffCount = 0;
        
        for (int i = 0; i < inv.equipInstance.Count; i++)
        {
            var item = inv.equipInstance[i];
            if (item.series == null) continue;
            int itemGroup = i / 3;

            System.Action<Alpha.Data.WeaponEffectSO_Alpha> checkHPGauge = (effectSO) =>
            {
                if (effectSO != null && effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.HPGaugePlus)
                {
                    if (!effectSO.isGlobalEffect && groupToPass != -1 && itemGroup != groupToPass) return;
                    hpGaugeBuffCount++;
                }
            };

            if (item.series.passiveEffects != null)
            {
                foreach (var e in item.series.passiveEffects) checkHPGauge(e.effect);
            }
            if (item.currentEffects != null)
            {
                foreach (var e in item.currentEffects) checkHPGauge(e);
            }
        }

        if (executionerTier >= 1)
        {
            hpGaugeBuffCount += 1;
        }

        HPGauge = Mathf.Min(4, baseHPGauge + hpGaugeBuffCount);

        // ゲージの最大値が増えた場合、増えたゲージ数分だけ現在HPを回復する
        if (HPGauge > oldHPGaugeLimit)
        {
            int gaugeDiff = HPGauge - oldHPGaugeLimit;
            // Heal関数はオーバーフロー時にゲージを増やす処理を含むため、
            // そのまま最大HP(1ゲージ分の体力) × 増えたゲージ数をHealに渡す
            Heal(HP * gaugeDiff);
            Debug.Log($"[PlayerStatusManager] HPGauge increased by {gaugeDiff}. Healing {HP * gaugeDiff} HP.");
        }

        // 外した時用に、現在のゲージ数が最大ゲージ数を超えていたら丸める処理
        if (nowHPGauge > HPGauge)
        {
            nowHPGauge = HPGauge;
            // 現在のHPも調整（必要であれば）
            if (currentHP > HP) currentHP = HP;
        }

        // --- 7. 最大HPが変化した場合の現在HP補正 ---
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

    private void UpdateExecutionerState(InventoryManager_Alpha inv, int activeGroup)
    {
        executionerTier = 0;
        isOmniBouquetOverride = false;
        
        System.Action<Alpha.Data.WeaponEffectSO_Alpha> checkExecutioner = (effectSO) =>
        {
            if (effectSO != null && effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.DivineExecutioner)
            {
                executionerTier++;
            }
        };

        for (int i = 0; i < inv.equipInstance.Count; i++)
        {
            var item = inv.equipInstance[i];
            if (item.series == null) continue;
            
            bool found = false;
            if (item.series.passiveEffects != null)
            {
                foreach(var eff in item.series.passiveEffects)
                {
                    if (eff.effect != null && eff.effect.effectType == Alpha.Data.WeaponEffectType_Alpha.DivineExecutioner)
                    {
                        executionerTier++;
                        found = true;
                        break;
                    }
                }
            }
            if (!found && item.currentEffects != null)
            {
                foreach(var eff in item.currentEffects)
                {
                    if (eff != null && eff.effectType == Alpha.Data.WeaponEffectType_Alpha.DivineExecutioner)
                    {
                        executionerTier++;
                        break;
                    }
                }
            }
        }
        
        bool isBoss = Alpha.Flow.StageManager_Alpha.Instance != null && Alpha.Flow.StageManager_Alpha.Instance.IsBossBattleActive;

        if (isBoss && executionerTier >= 3)
        {
            isOmniBouquetOverride = true;
        }

        for (int group = 0; group < 3; group++)
        {
            int bulletIndex = group * 3 + 2;
            if (bulletIndex < inv.equipInstance.Count)
            {
                var bulletItem = inv.equipInstance[bulletIndex];
                if (group == activeGroup && isBoss && executionerTier >= 2)
                {
                    if (bulletItem.originalRarity == 0 && bulletItem.rarity > 0)
                    {
                        bulletItem.originalRarity = bulletItem.rarity;
                    }
                    bulletItem.rarity = 4;
                }
                else
                {
                    if (bulletItem.originalRarity != 0)
                    {
                        bulletItem.rarity = bulletItem.originalRarity;
                        bulletItem.originalRarity = 0;
                    }
                }
                inv.equipInstance[bulletIndex] = bulletItem;
            }
        }
    }

    // --- Damage / Defense Calculation API ---

    public float GetFinalDamage(float baseWeaponDamage = 0f)
    {
        // 最終火力 = (基礎火力(pow) + 武器の基本ダメージ + 火力上昇(DamageAdd)) * 火力倍率(DamageMag / 100)
        float baseDmg = pow + baseWeaponDamage + DamageAdd;
        baseDmg = Mathf.Max(baseDmg, 10f); // AttackDebuffによるステータスの下限は10
        return baseDmg * (DamageMag / 100f);
    }

    public float GetTakenDamage(float enemyDamage)
    {
        // 被ダメージ = (敵の元々のダメージ - 防御力上昇(BlockDmg)) * 防御力倍率(BlockMag / 100)
        // ※BlockMag は 100 を基準とし、50なら50%カット、150なら50%増加とする（被ダメージ倍率）
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

    public enum SpecialMoveType { None, Dash, Warp, Focus }
    [Header("Special Move (Dash/Warp/Focus) Settings")]
    public SpecialMoveType currentSpecialMove = SpecialMoveType.Dash;

    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRecoveryRate = 10f; // 100 units over 10 seconds
    public float staminaRecoveryDelay = 0.5f;
    [HideInInspector] public float lastStaminaConsumeTime = -100f;
    [HideInInspector] public bool isStaminaExhausted = false;

    public float dashDuration = 0.1f;
    public float dashStaminaCost = 30f;
    public float dashDistance = 4f; // 調整可能

    public float warpDuration = 0.2f;  // DBZ瞬間移動のタメ時間
    public float warpStaminaCost = 45f;
    public float warpDistance = 8f; // 調整可能

    public float focusStaminaCostPerSec = 20f; // フォーカスモード中の秒間スタミナ消費量

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
                float recoveryMod = 1.0f;
                if (isStaminaExhausted)
                {
                    if (staminaExhaustionRecoveryBoost > 0f)
                    {
                        recoveryMod = staminaExhaustionRecoveryBoost; // エフェクトがあればその倍率
                    }
                    else
                    {
                        recoveryMod = 0.5f; // デフォルトは半減ペナルティ
                    }
                }
                
                currentStamina += staminaRecoveryRate * recoveryMod * Time.deltaTime;
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    isStaminaExhausted = false; // 満タンで枯渇解除
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
        
        // --- Juice Additions ---
        if (finalDamage > 0)
        {
            var playerObj = GameObject.Find("Player");
            if (playerObj == null) playerObj = this.gameObject;
            
            var juice = playerObj.GetComponent<Alpha.Core.EntityJuice_Alpha>();
            if (juice == null)
            {
                juice = playerObj.AddComponent<Alpha.Core.EntityJuice_Alpha>();
            }

            // HitFlash: Bluish-Red for player
            juice.FlashColor(new Color(0.8f, 0.2f, 0.4f), 0.1f);

            // ScreenShake if damage >= 30% of max HP
            if (finalDamage >= HP * 0.3f)
            {
                if (Alpha.Core.JuiceManager_Alpha.Instance != null)
                {
                    Alpha.Core.JuiceManager_Alpha.Instance.ScreenShake(0.3f, 0.5f);
                }
            }
        }
        // --- End Juice Additions ---

        Debug.Log($"[PlayerStatusManager] ApplyDamage: Original {amount} -> Final {finalDamage} | Current HP: {currentHP}");

        // UI更新などのためにイベント発火（受け手が未実装でも安全）
        OnPlayerHPChanged?.Invoke(currentHP, HP);

        if (currentHP <= 0)
        {
            nowHPGauge--;
            
            if (nowHPGauge < 1) {
                Die();
            } else {
                OnGaugeLost?.Invoke();
            }           
            
            currentHP = HP;
            OnPlayerHPChanged?.Invoke(currentHP, HP);
        }
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        
        // オーバーフロー処理：現在のHPが最大値を超え、かつ次のゲージが存在する場合
        while (currentHP > HP && nowHPGauge < HPGauge)
        {
            currentHP -= HP;
            nowHPGauge++;
        }
        
        // 最終的に最大HPを超えないようにクリップ
        if (currentHP > HP) 
        {
            currentHP = HP;
        }
        
        Debug.Log($"[PlayerStatusManager] Heal: {amount} | Current HP: {currentHP} | Current Gauge: {nowHPGauge}");

        OnPlayerHPChanged?.Invoke(currentHP, HP);
    }

    private void Die()
    {
        Debug.Log("[PlayerStatusManager] Player HP reached 0. Triggering GameOver.");
        if (Alpha.UI.GameOverManager_Alpha.Instance != null)
        {
            Alpha.UI.GameOverManager_Alpha.Instance.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager_Alpha instance not found in the scene! Cannot show Game Over screen.");
        }
    }

    // --- EXP & Petal Management API ---
    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"[PlayerStatusManager] Gained {amount} EXP. Total: {currentExp}");
        OnExpAdded?.Invoke(amount);
    }

    public void AddPetal(int amount = 1)
    {
        currentPetals += amount;
        Debug.Log($"[PlayerStatusManager] Gained {amount} Petal(s). Total: {currentPetals}");
        
        // 10個目以降はEXP 300に変換
        if (currentPetals >= 10)
        {
            Debug.Log("[PlayerStatusManager] 10th Petal collected! Converting to 300 EXP.");
            AddExp(300);
        }
        else
        {
            OnPetalAdded?.Invoke(amount);
        }
    }
}
