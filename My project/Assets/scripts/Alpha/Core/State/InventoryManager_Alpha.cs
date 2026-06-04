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
        public BASE_WeaponData_Alpha affix; 
        
        // 旧仕様の効果（既存コードとの互換性のため残す）
        public Alpha_Effect_Base effect1;
        public Alpha_Effect_Base effect2;
        public Alpha_Effect_Base effect3;

        // 新仕様（ScriptableObject）の効果
        public WeaponSeriesData_Alpha series;
        public WeaponPartType_Alpha partType;
        public List<WeaponEffectSO_Alpha> currentEffects;
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
    }

    public void BattleStartEffect()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                EquipInstance instance = Get(x, y);
                // スロットが未設定（defId が null または空）の場合はスキップ
                if (string.IsNullOrEmpty(instance.defId) && instance.series == null)
                    continue;

                instance.effect1?.StartEffect(instance.rarity);
                instance.effect2?.StartEffect(instance.rarity);
                instance.effect3?.StartEffect(instance.rarity);
            }
        }
    }

    // --- 新しいシステム用のメソッド群 ---

    public void AddItem(EquipInstance item)
    {
        // 空き枠（defIdがnullまたは空）があればそこに入れる
        for (int i = 0; i < equipInstance.Count; i++)
        {
            if (equipInstance[i].series == null)
            {
                equipInstance[i] = item;
                playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
                return;
            }
        }
        // 空きがなければ末尾に追加
        equipInstance.Add(item);
        playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
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

        if (equipInstance.Count > keepCount)
        {
            // 後ろから削除していく
            for (int i = equipInstance.Count - 1; i >= keepCount; i--)
            {
                var item = equipInstance[i];
                if (item.series != null)
                {
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
        }

        if (totalExpGained > 0)
        {
            Debug.Log($"[InventoryManager] Total EXP gained from selling temporary items: {totalExpGained}");
            // TODO: 実際のプレイヤーステータス等にEXPを加算する処理をここに繋げる
        }

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
    /// 特定のグループ（行）の3つのパーツがすべて同じシリーズで統一されているか
    /// </summary>
    public bool IsGroupSeriesAligned(int groupIndex)
    {
        int startIndex = groupIndex * 3;
        var seriesA = equipInstance[startIndex].series;
        var seriesB = equipInstance[startIndex + 1].series;
        var seriesC = equipInstance[startIndex + 2].series;

        if (seriesA == null || seriesB == null || seriesC == null) return false;
        if (seriesA.name == "InitialSeries" || seriesB.name == "InitialSeries" || seriesC.name == "InitialSeries") return false;
        if (seriesA != seriesB || seriesA != seriesC) return false;

        return true;
    }

    /// <summary>
    /// 特定のアイテムがBestSlot条件を満たしているか
    /// 条件1: そのグループのシリーズが完全に一致している
    /// 条件2: アイテムの装備部位（0:雷管, 1:薬莢, 2:弾頭）が、シリーズで定義されたBestSlotと一致している
    /// </summary>
    public bool IsBestSlotConditionMet(int itemIndex)
    {
        var item = equipInstance[itemIndex];
        if (item.series == null) return false;

        int groupIndex = itemIndex / 3;
        int equipPosition = itemIndex % 3; // 0=雷管(Primer), 1=薬莢(Casing), 2=弾頭(Bullet)

        // 1. シリーズが揃っているか
        if (!IsGroupSeriesAligned(groupIndex)) return false;

        // 2. 装備部位が最適部位と一致しているか
        Alpha.Data.WeaponPartType_Alpha currentPartType;
        switch (equipPosition)
        {
            case 0: currentPartType = Alpha.Data.WeaponPartType_Alpha.Primer; break;
            case 1: currentPartType = Alpha.Data.WeaponPartType_Alpha.Casing; break;
            case 2: currentPartType = Alpha.Data.WeaponPartType_Alpha.Bullet; break;
            default: return false;
        }

        return item.series.bestSlot == currentPartType;
    }

    public float GetTotalEffectValue(Alpha.Data.WeaponEffectType_Alpha effectType, int activeGroup = -1)
    {
        float totalValue = 0f;

        for (int i = 0; i < equipInstance.Count; i++)
        {
            var item = equipInstance[i];
            if (item.series == null) continue;

            int itemGroup = i / 3;
            bool isBestSlotMet = IsBestSlotConditionMet(i);

            // 1. 武器のベース(series)に紐づくパッシブ効果を加算
            if (item.series.passiveEffects != null)
            {
                totalValue += GetTotalEffectValueRecursive(item.series.passiveEffects, effectType, item.rarity, itemGroup, activeGroup, isBestSlotMet);
            }

            // 2. プレイヤーが直接付与した固有効果(currentEffects)を加算
            if (item.currentEffects != null)
            {
                totalValue += GetTotalEffectValueRecursive(item.currentEffects, effectType, item.rarity, itemGroup, activeGroup, isBestSlotMet);
            }
        }

        return totalValue;
    }

    private float GetTotalEffectValueRecursive(List<Alpha.Data.WeaponEffectSO_Alpha> effects, Alpha.Data.WeaponEffectType_Alpha targetType, int rarity, int itemGroup, int activeGroup, bool isBestSlotMet)
    {
        float value = 0f;
        foreach (var effectSO in effects)
        {
            if (effectSO == null) continue;

            // 複合スキルの場合は再帰的に中身を取り出す
            if (effectSO.effectType == Alpha.Data.WeaponEffectType_Alpha.Composite)
            {
                var comp = effectSO as Alpha.Data.CompositeWeaponEffectSO_Alpha;
                if (comp != null && comp.subEffects != null)
                {
                    value += GetTotalEffectValueRecursive(comp.subEffects, targetType, rarity, itemGroup, activeGroup, isBestSlotMet);
                }
            }
            else if (effectSO.effectType == targetType)
            {
                // BestSlot専用効果の場合、条件を満たしていなければスキップ
                if (effectSO.isBestSlotEffect && !isBestSlotMet) continue;

                // 常時発動ではなく、かつ現在構えている武器グループでない場合はスキップ
                if (!effectSO.isGlobalEffect && activeGroup != -1 && itemGroup != activeGroup) continue;

                value += effectSO.GetValue(rarity);
            }
        }
        return value;
    }
}