using System;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Data;

public class InventoryManager_Alpha : MonoBehaviour
{
    public static InventoryManager_Alpha Instance { get; private set; }

    public const int W = 3;
    public const int H = 3;
    public const int BASIC_SLOT_COUNT = W * H;

    [Serializable]
    public struct EquipInstance
    {
        public string defId;
        public int rarity; // 1〜4 (qualityとして利用)
        public int originalRarity; // ボス戦等での一時的な品質書き換え前の保存用
        public BASE_WeaponData_Alpha affix; 
        
        // 新仕様（ScriptableObject）の効果
        public WeaponSeriesData_Alpha series;
        public WeaponPartType_Alpha partType;
        public List<WeaponEffectSO_Alpha> currentEffects;
        public WeaponEffectSO_Alpha setBonusEffect; // 生成時にベストスロットなら付与されるセットボーナス（シリーズ統一時のみ発動）

        public float GetMultiplier(WeaponPartType_Alpha statType)
        {
            if (series == null) return 0f;
            float mult = 0f;

            // 1. 自身の部佁Eself)なめE+1.0
            if (statType == partType) mult += 1.0f;

            // 2. レアリチEごとのボEナス加箁E
            if (rarity == 2) // Uncommon
            {
            }

            return mult;
        }

        public float GetPowerBonus() => series != null ? series.basePowerBonus * GetMultiplier(WeaponPartType_Alpha.Bullet) : 0f;
        public float GetSurvivalBonus() => series != null ? series.survivalTimeBonus * GetMultiplier(WeaponPartType_Alpha.Casing) : 0f;
        public float GetSpeedBonus() => series != null ? series.speedBonus * GetMultiplier(WeaponPartType_Alpha.Primer) : 0f;
    }

    [Header("Inventory Management")]
    [Tooltip("インデックス0〜8: 基本枠\nインデックス9〜(8+freeSlotCount): フリー枠\nそれ以降: テンポラリー枠")]
    public List<EquipInstance> equipInstance = new List<EquipInstance>();

    [Tooltip("インデックス9からいくつ分をフリースロットとするかを管理")]
    public int freeSlotCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 初期状態で9枠は最低限確保しておく
        while (equipInstance.Count < BASIC_SLOT_COUNT)
        {
            equipInstance.Add(new EquipInstance());
        }
    }

    // --- 既存コード (Player_Shooter_Alpha) から呼ばれるメソッド群 ---

    public EquipInstance Get(int x, int y)
    {
        int index = y * W + x;
        if (index < equipInstance.Count)
        {
            return equipInstance[index];
        }
        return new EquipInstance();
    }

    public void Set(int x, int y, EquipInstance v)
    {
        int index = y * W + x;
        SetByIndex(index, v);
    }

    public void SetByIndex(int index, EquipInstance v)
    {
        while (equipInstance.Count <= index)
        {
            equipInstance.Add(new EquipInstance());
        }
        equipInstance[index] = v;
        
        playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
        UpdateSlotCount(true);
    }

    public void BattleStartEffect()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                EquipInstance instance = Get(x, y);
                // スロットが未設定（defId が null または空）の場合はスキップ
                if (instance.series == null) continue;
            }
        }
    }

    // --- 新しいシステム用のメソッド群 ---

    public void AddItem(EquipInstance item)
    {
        bool hasHPGaugePlus = false;
        float healPercent = 0f;

        if (item.series != null)
        {
            System.Action<Alpha.Data.WeaponEffectSO_Alpha> checkHeal = (effectSO) =>
            {
                if (effectSO != null && effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.HPGaugePlus)
                {
                    hasHPGaugePlus = true;
                    float percent = effectSO.GetValue(item.rarity);
                    if (percent > healPercent) healPercent = percent;
                }
            };

            if (item.series.passiveEffects != null)
            {
                foreach (var e in item.series.passiveEffects) checkHeal(e.effect);
            }
            if (item.currentEffects != null)
            {
                foreach (var e in item.currentEffects) checkHeal(e);
            }
        }

        // 空き枠（defIdがnullまたは空）があればそこに入れる
        bool added = false;
        for (int i = 0; i < equipInstance.Count; i++)
        {
            if (equipInstance[i].series == null)
            {
                equipInstance[i] = item;
                added = true;
                playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
                break;
            }
        }
        
        // 空きがなければ末尾に追加
        if (!added)
        {
            equipInstance.Add(item);
            playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
        }

        // 追加HP回復処理
        if (hasHPGaugePlus && playerStatusManager_Alpha.Instance != null && healPercent > 0)
        {
            float healAmount = playerStatusManager_Alpha.Instance.HP * (healPercent / 100f);
            playerStatusManager_Alpha.Instance.Heal(healAmount);
            Debug.Log($"[InventoryManager] HPGaugePlus acquired! Healing {healPercent}% of Max HP ({healAmount}).");
        }
        
        UpdateSlotCount(true);
    }

    public void AddFreeSlot()
    {
        freeSlotCount++;
        Debug.Log($"[InventoryManager] Free slots expanded. Current count: {freeSlotCount}");
    }

    public int SellTemporaryItems()
    {
        int totalExpGained = 0;
        int keepCount = BASIC_SLOT_COUNT + freeSlotCount;
        List<EquipInstance> itemsToKeep = new List<EquipInstance>();

        if (equipInstance.Count > keepCount)
        {
            // 後ろから削除していく
            for (int i = equipInstance.Count - 1; i >= keepCount; i--)
            {
                var item = equipInstance[i];
                if (item.series != null)
                {
                    // 販売不能エフェクトのチェック
                    bool isUnsellable = false;
                    if (item.currentEffects != null)
                    {
                        foreach (var effect in item.currentEffects)
                        {
                            if (effect != null && effect.effectType == Alpha.Data.WeaponEffectType_Alpha.Unsellable)
                            {
                                isUnsellable = true;
                                break;
                            }
                        }
                    }
                    if (!isUnsellable && item.series.passiveEffects != null)
                    {
                        foreach (var pe in item.series.passiveEffects)
                        {
                            if (pe.effect != null && pe.effect.effectType == Alpha.Data.WeaponEffectType_Alpha.Unsellable)
                            {
                                isUnsellable = true;
                                break;
                            }
                        }
                    }

                    if (isUnsellable)
                    {
                        // 売却不可アイテムはリストに退避しておき、後で戻す
                        itemsToKeep.Add(item);
                        equipInstance.RemoveAt(i);
                        Debug.Log($"[InventoryManager] Item '{item.series.name}' is unsellable and was kept.");
                        continue;
                    }

                    int exp = 0;
                    if (item.series.name != "InitialSeries")
                    {
                        // 売却処理: rarity * 2 の EXP
                        exp = item.rarity * 2;
                        if (exp <= 0) exp = 2; // 最低保証
                    }
                    
                    totalExpGained += exp;
                    Debug.Log($"[InventoryManager] Sold temporary item for {exp} EXP.");
                }
                equipInstance.RemoveAt(i);
            }

            // 売却不可だったアイテムを元の順序で末尾に戻す
            itemsToKeep.Reverse();
            foreach (var item in itemsToKeep)
            {
                equipInstance.Add(item);
            }
        }

        if (totalExpGained > 0)
        {
            Debug.Log($"[InventoryManager] Total EXP gained from selling temporary items: {totalExpGained}");
            // TODO: 実際のプレイヤーステータス等にEXPを加算する処理をここに繋げる
        }

        UpdateSlotCount(false);
        return totalExpGained;
    }

    /// <summary>
    /// 一時スロットにある初期装備(InitialSeries)を直ちに削除します
    /// 報酬獲得画面等で装備整理が終わった際に呼び出されます
    /// </summary>
    public void CleanUpInitialSeriesInEXSlots()
    {
        int keepCount = BASIC_SLOT_COUNT + freeSlotCount;

        if (equipInstance.Count > keepCount)
        {
            // 後ろから調べて削除
            for (int i = equipInstance.Count - 1; i >= keepCount; i--)
            {
                var item = equipInstance[i];
                if (item.series != null && item.series.name == "InitialSeries")
                {
                    // 初期装備なら即削除
                    equipInstance.RemoveAt(i);
                    Debug.Log("[InventoryManager] Cleaned up InitialSeries from EX slot.");
                }
            }
        }
        UpdateSlotCount(false);
    }

    public void UpdateSlotCount(bool padding = true)
    {
        int lastFilled = -1;
        for (int i = 0; i < equipInstance.Count; i++)
        {
            if (equipInstance[i].series != null) lastFilled = i;
        }

        int keepCount = BASIC_SLOT_COUNT + freeSlotCount;
        int requiredSlots = Mathf.Max(keepCount, lastFilled + 1);

        if (padding)
        {
            if (requiredSlots < lastFilled + 1 + 1)
            {
                requiredSlots = lastFilled + 1 + 1;
            }
            // 3の倍数に揃える処理を削除し、余分なスロットが1つだけになるようにする
        }

        while (equipInstance.Count < requiredSlots)
        {
            equipInstance.Add(new EquipInstance());
        }
        
        while (equipInstance.Count > requiredSlots)
        {
            equipInstance.RemoveAt(equipInstance.Count - 1);
        }
    }

    /// <summary>
    /// ブーケ状態の判定
    /// 全てのセット(行)が、それぞれ同じシリーズで統一されているか（全9枠が埋まっている必要がある）
    /// </summary>
    public bool IsBouquetActive()
    {
        if (equipInstance == null || equipInstance.Count < BASIC_SLOT_COUNT) return false;

        for (int row = 0; row < 3; row++)
        {
            if (!IsGroupSeriesAligned(row)) return false;
        }

        return true;
    }

    /// <summary>
    /// アイテムにWildcardエフェクトが付与されているかチェックする
    /// </summary>
    private bool HasWildcardEffect(int itemIndex)
    {
        var item = equipInstance[itemIndex];
        if (item.currentEffects == null) return false;
        foreach (var eff in item.currentEffects)
        {
            if (eff != null && eff.effectType == Alpha.Data.WeaponEffectType_Alpha.Wildcard) return true;
        }
        return false;
    }

    /// <summary>
    /// 特定のグループ（行）の3つのパーツすべて同じシリーズで統一されているか（Wildcardを考慮）
    /// </summary>
    public bool IsGroupSeriesAligned(int groupIndex)
    {
        int startIndex = groupIndex * 3;
        
        // 追加: リストの範囲外アクセスを防ぐ
        if (startIndex + 2 >= equipInstance.Count) return false;

        var instA = equipInstance[startIndex];
        var instB = equipInstance[startIndex + 1];
        var instC = equipInstance[startIndex + 2];

        var seriesA = instA.series;
        var seriesB = instB.series;
        var seriesC = instC.series;

        if (seriesA == null || seriesB == null || seriesC == null) return false;
        if (seriesA.name == "InitialSeries" || seriesB.name == "InitialSeries" || seriesC.name == "InitialSeries") return false;

        bool jokerA = HasWildcardEffect(startIndex);
        bool jokerB = HasWildcardEffect(startIndex + 1);
        bool jokerC = HasWildcardEffect(startIndex + 2);

        // ベースとなるシリーズを探す（Wildcard以外の最初のシリーズ）
        Alpha.Data.WeaponSeriesData_Alpha baseSeries = null;
        if (!jokerA) baseSeries = seriesA;
        else if (!jokerB) baseSeries = seriesB;
        else if (!jokerC) baseSeries = seriesC;

        // 3つともWildcardの場合は統一されているとみなす
        if (baseSeries == null) return true; 

        // Wildcard以外のパーツがベースシリーズと一致するかチェック
        if (!jokerA && seriesA != baseSeries) return false;
        if (!jokerB && seriesB != baseSeries) return false;
        if (!jokerC && seriesC != baseSeries) return false;

        return true;
    }


    public struct ActiveEffectInfo
    {
        public int count;
        public float flatValue;
    }

    public struct ActiveEffectDisplayInfo
    {
        public Alpha.Data.WeaponEffectSO_Alpha effectSO;
        public int groupIndex; // -1 for global, 0, 1, 2 for local weapon group
        public int count;
        public float flatValue;
    }

    public List<ActiveEffectDisplayInfo> GetActiveEffectsForDisplay()
    {
        var displayList = new List<ActiveEffectDisplayInfo>();
        var trackingDict = new Dictionary<Alpha.Data.WeaponEffectSO_Alpha, Dictionary<int, ActiveEffectInfo>>();

        for (int i = 0; i < equipInstance.Count; i++)
        {
            var item = equipInstance[i];
            if (item.series == null) continue;

            int itemGroup = i / 3;

            System.Action<Alpha.Data.WeaponEffectSO_Alpha, int> processEffect = null;
            processEffect = (effectSO, rarity) =>
            {
                if (effectSO == null) return;
                
                if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
                {
                    var comp = effectSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                    if (comp != null && comp.subEffects != null)
                    {
                        foreach (var sub in comp.subEffects) processEffect(sub, rarity);
                    }
                    return;
                }

                int targetGroup = (effectSO.isGlobalEffect) ? -1 : itemGroup;

                if (!trackingDict.ContainsKey(effectSO))
                {
                    trackingDict[effectSO] = new Dictionary<int, ActiveEffectInfo>();
                }
                
                if (!trackingDict[effectSO].ContainsKey(targetGroup))
                {
                    trackingDict[effectSO][targetGroup] = new ActiveEffectInfo();
                }

                var info = trackingDict[effectSO][targetGroup];
                info.count += rarity;
                if (!effectSO.useStepMultiplier)
                {
                    info.flatValue += effectSO.GetValue(rarity);
                }
                trackingDict[effectSO][targetGroup] = info;
            };

            if (item.series.passiveEffects != null)
            {
                foreach (var spe in item.series.passiveEffects)
                {
                    if (spe.effect == null) continue;
                    int appliedRarity = spe.fixedQualityOverride > 0 ? spe.fixedQualityOverride : item.rarity;
                    processEffect(spe.effect, appliedRarity);
                }
            }

            if (item.currentEffects != null)
            {
                foreach (var effectSO in item.currentEffects)
                {
                    if (effectSO == null) continue;
                    processEffect(effectSO, item.rarity);
                }
            }

            if (item.setBonusEffect != null && IsGroupSeriesAligned(itemGroup))
            {
                processEffect(item.setBonusEffect, item.rarity);
            }
        }

        foreach (var effectKvp in trackingDict)
        {
            var effectSO = effectKvp.Key;
            foreach (var groupKvp in effectKvp.Value)
            {
                displayList.Add(new ActiveEffectDisplayInfo
                {
                    effectSO = effectSO,
                    groupIndex = groupKvp.Key,
                    count = groupKvp.Value.count,
                    flatValue = groupKvp.Value.flatValue
                });
            }
        }

        displayList.Sort((a, b) => 
        {
            if (a.groupIndex != b.groupIndex) return a.groupIndex.CompareTo(b.groupIndex);
            
            bool aIsDebuff = a.effectSO != null && a.effectSO.IsDebuff();
            bool bIsDebuff = b.effectSO != null && b.effectSO.IsDebuff();
            if (aIsDebuff != bIsDebuff) return aIsDebuff ? 1 : -1;

            return string.Compare(a.effectSO != null ? a.effectSO.effectName : "", b.effectSO != null ? b.effectSO.effectName : "");
        });

        return displayList;
    }

    public Dictionary<Alpha.Data.WeaponEffectSO_Alpha, ActiveEffectInfo> GetAllActiveEffectQualities(int activeGroup = -1)
    {
        var result = new Dictionary<Alpha.Data.WeaponEffectSO_Alpha, ActiveEffectInfo>();
        var activeFlags = new Dictionary<Alpha.Data.WeaponEffectSO_Alpha, bool>();
        
        bool isGlobalActive = (activeGroup == -1);

        for (int i = 0; i < equipInstance.Count; i++)
        {
            var item = equipInstance[i];
            if (item.series == null) continue;

            int itemGroup = i / 3;

            System.Action<Alpha.Data.WeaponEffectSO_Alpha, int> processEffect = null;
            processEffect = (effectSO, rarity) =>
            {
                if (effectSO == null) return;
                
                if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
                {
                    var comp = effectSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                    if (comp != null && comp.subEffects != null)
                    {
                        foreach (var sub in comp.subEffects) processEffect(sub, rarity);
                    }
                    return;
                }

                if (!activeFlags.ContainsKey(effectSO)) activeFlags[effectSO] = isGlobalActive;
                if (effectSO.isGlobalEffect || itemGroup == activeGroup)
                {
                    activeFlags[effectSO] = true;
                }

                if (!effectSO.accumulateGlobally && !effectSO.isGlobalEffect && activeGroup != -1 && itemGroup != activeGroup)
                {
                    return;
                }

                if (!result.ContainsKey(effectSO)) result[effectSO] = new ActiveEffectInfo();
                
                var info = result[effectSO];
                info.count += rarity;
                if (!effectSO.useStepMultiplier)
                {
                    info.flatValue += effectSO.GetValue(rarity);
                }
                result[effectSO] = info;
            };

            if (item.series.passiveEffects != null)
            {
                foreach (var spe in item.series.passiveEffects)
                {
                    if (spe.effect == null) continue;
                    int appliedRarity = spe.fixedQualityOverride > 0 ? spe.fixedQualityOverride : item.rarity;
                    processEffect(spe.effect, appliedRarity);
                }
            }

            if (item.currentEffects != null)
            {
                foreach (var effectSO in item.currentEffects)
                {
                    if (effectSO == null) continue;
                    processEffect(effectSO, item.rarity);
                }
            }

            if (item.setBonusEffect != null && IsGroupSeriesAligned(itemGroup))
            {
                processEffect(item.setBonusEffect, item.rarity);
            }
        }

        // Filter out effects that are accumulated but not active in the current group
        var finalResult = new Dictionary<Alpha.Data.WeaponEffectSO_Alpha, ActiveEffectInfo>();
        foreach (var kvp in result)
        {
            if (activeFlags.ContainsKey(kvp.Key) && activeFlags[kvp.Key])
            {
                finalResult[kvp.Key] = kvp.Value;
            }
        }

        return finalResult;
    }

    public float GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha effectType, int activeGroup = -1)
    {
        float totalFlatValue = 0f;
        int totalQuality = 0;
        Alpha.Data.WeaponEffectSO_Alpha stepEffectRef = null;
        
        // activeGroupが-1（グループ指定なし）の場合は無条件でアクティブとする
        bool isActive = (activeGroup == -1);

        for (int i = 0; i < equipInstance.Count; i++)
        {
            var item = equipInstance[i];
            if (item.series == null) continue;

            int itemGroup = i / 3;

            // 1. 武器のベース(series)に紐づくパッシブ効果を加算
            if (item.series.passiveEffects != null)
            {
                foreach (var spe in item.series.passiveEffects)
                {
                    if (spe.effect == null) continue;
                    int appliedRarity = spe.fixedQualityOverride > 0 ? spe.fixedQualityOverride : item.rarity;
                    AccumulateSingleEffect(spe.effect, effectType, appliedRarity, itemGroup, activeGroup, ref totalFlatValue, ref totalQuality, ref stepEffectRef, ref isActive);
                }
            }

            // 2. プレイヤーが直接付与した固有効果(currentEffects)を加算
            if (item.currentEffects != null)
            {
                foreach (var effectSO in item.currentEffects)
                {
                    if (effectSO == null) continue;
                    AccumulateSingleEffect(effectSO, effectType, item.rarity, itemGroup, activeGroup, ref totalFlatValue, ref totalQuality, ref stepEffectRef, ref isActive);
                }
            }

            // 3. セットボーナスエフェクトの加算（シリーズ統一時のみ発動）
            if (item.setBonusEffect != null && IsGroupSeriesAligned(itemGroup))
            {
                AccumulateSingleEffect(item.setBonusEffect, effectType, item.rarity, itemGroup, activeGroup, ref totalFlatValue, ref totalQuality, ref stepEffectRef, ref isActive);
            }
        }

        // 現在のグループで発動条件を満たしていなければ効果量は0
        if (!isActive) return 0f;

        if (stepEffectRef != null && stepEffectRef.useStepMultiplier)
        {
            return CalculateStepValue(stepEffectRef, totalQuality);
        }

        return totalFlatValue;
    }

    private void AccumulateSingleEffect(Alpha.Data.WeaponEffectSO_Alpha effectSO, Alpha.Data.WeaponEffectType_Alpha targetType, int rarity, int itemGroup, int activeGroup, ref float flatValue, ref int totalQuality, ref Alpha.Data.WeaponEffectSO_Alpha stepEffectRef, ref bool isActive)
    {
        // 複合スキルの場合は再帰的に中身を取り出す
        if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
        {
            var comp = effectSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
            if (comp != null && comp.subEffects != null)
            {
                foreach (var sub in comp.subEffects)
                {
                    if (sub == null) continue;
                    AccumulateSingleEffect(sub, targetType, rarity, itemGroup, activeGroup, ref flatValue, ref totalQuality, ref stepEffectRef, ref isActive);
                }
            }
        }
        else if (effectSO.effectType == targetType)
        {

                // 発動条件のチェック（現在のグループが対象か、またはグローバル効果か）
                if (effectSO.isGlobalEffect || itemGroup == activeGroup)
                {
                    isActive = true;
                }

            // 加算条件のチェック
            // accumulateGloballyがfalseの場合、現在構えているグループではないなら加算しない
            // （グローバル効果の場合はisGlobalEffectで無条件に加算される設計ではなく、あくまでこのグループが関係しているかどうかの制御）
            if (!effectSO.accumulateGlobally && !effectSO.isGlobalEffect && activeGroup != -1 && itemGroup != activeGroup)
            {
                return;
            }

            // 値の加算は「発動条件を満たしているか」に関わらず、(上記条件をパスしていれば)全スロットからパブリックに行う
            if (effectSO.useStepMultiplier)
            {
                totalQuality += rarity; // シリーズ側で指定された固定品質、または通常品質を加算
                if (stepEffectRef == null) stepEffectRef = effectSO;
            }
            else
            {
                flatValue += effectSO.GetValue(rarity);
            }
        }
    }

    private float CalculateStepValue(Alpha.Data.WeaponEffectSO_Alpha effectSO, int totalQuality)
    {
        if (totalQuality == 0) return 0f;

        float multiplier = 0f;
        int[] thresholds = effectSO.stepThresholds;
        
        if (thresholds == null || thresholds.Length == 0)
        {
            multiplier = effectSO.qualityValues != null && effectSO.qualityValues.Length > 0 ? effectSO.qualityValues[0] : 0f;
        }
        else
        {
            // 段階の判定
            int stepIndex = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (totalQuality >= thresholds[i])
                {
                    stepIndex = i + 1;
                }
                else
                {
                    break; // 閾値に満たない場合はそれ以上の段階には進まない
                }
            }

            // qualityValues から対応する乗数を取得
            if (effectSO.qualityValues != null && effectSO.qualityValues.Length > 0)
            {
                int valIndex = Mathf.Clamp(stepIndex, 0, effectSO.qualityValues.Length - 1);
                multiplier = effectSO.qualityValues[valIndex];
            }
        }

        // 最終的な計算：合計品質 * 対応する乗数
        return totalQuality * multiplier;
    }
}
