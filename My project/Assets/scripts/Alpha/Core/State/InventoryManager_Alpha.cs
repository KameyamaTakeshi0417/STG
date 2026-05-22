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
        while (equipInstance.Count <= index)
        {
            equipInstance.Add(new EquipInstance());
        }
        equipInstance[index] = v;
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
        // 空いている基本枠（null扱い）があればそこに入れる
        for (int i = 0; i < equipInstance.Count; i++)
        {
            if (string.IsNullOrEmpty(equipInstance[i].defId) && equipInstance[i].series == null)
            {
                equipInstance[i] = item;
                return;
            }
        }
        // 空きがなければ末尾に追加
        equipInstance.Add(item);
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
                if (!string.IsNullOrEmpty(item.defId) || item.series != null)
                {
                    // 売却処理: rarity * 2 の EXP
                    int exp = item.rarity * 2;
                    totalExpGained += exp;
                    Debug.Log($"[InventoryManager] Sold temporary item (Rarity {item.rarity}) for {exp} EXP.");
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

    public float GetTotalEffectValue(WeaponEffectType_Alpha effectType)
    {
        float totalValue = 0f;

        for (int i = 0; i < equipInstance.Count; i++)
        {
            var item = equipInstance[i];
            if (item.series == null || item.currentEffects == null) continue;

            bool isBasicSlot = (i < BASIC_SLOT_COUNT);

            foreach (var effectSO in item.currentEffects)
            {
                if (effectSO != null && effectSO.effectType == effectType)
                {
                    bool isAnySlot = item.series.anySlotEffects.Contains(effectSO);
                    bool isBestSlot = item.series.bestSlotEffects.Contains(effectSO);

                    if (isAnySlot || (isBestSlot && isBasicSlot))
                    {
                        totalValue += effectSO.GetValue(item.rarity);
                    }
                }
            }
        }

        return totalValue;
    }
}