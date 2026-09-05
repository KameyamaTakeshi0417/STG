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
        public int rarity; // 1縲・ (quality縺ｨ縺励※蛻ｩ逕ｨ)
        public int originalRarity; // 繝懊せ謌ｦ遲峨〒縺ｮ荳譎ら噪縺ｪ蜩∬ｳｪ譖ｸ縺肴鋤縺亥燕縺ｮ菫晏ｭ倡畑
        public BASE_WeaponData_Alpha affix; 
        
        // 譁ｰ莉墓ｧ假ｼ・criptableObject・峨・蜉ｹ譫・
        public WeaponSeriesData_Alpha series;
        public WeaponPartType_Alpha partType;
        public List<WeaponEffectSO_Alpha> currentEffects;
        public WeaponEffectSO_Alpha setBonusEffect; // 逕滓・譎ゅ↓繝吶せ繝医せ繝ｭ繝・ヨ縺ｪ繧我ｻ倅ｸ弱＆繧後ｋ繧ｻ繝・ヨ繝懊・繝翫せ・医す繝ｪ繝ｼ繧ｺ邨ｱ荳譎ゅ・縺ｿ逋ｺ蜍包ｼ・

        public float GetMultiplier(WeaponPartType_Alpha statType)
        {
            if (series == null) return 0f;
            float mult = 0f;

            // 1. 閾ｪ霄ｫ縺ｮ驛ｨ菴・self)縺ｪ繧・+1.0
            if (statType == partType) mult += 1.0f;

            // 2. 繝ｬ繧｢繝ｪ繝・縺斐→縺ｮ繝廢繝翫せ蜉邂・
            if (rarity == 2) // Uncommon
            {
            }

            return mult;
        }

        public float GetPowerBonus() => series != null ? series.basePowerBonus[Mathf.Clamp(rarity - 1, 0, 3)] * GetMultiplier(WeaponPartType_Alpha.Bullet) : 0f;
        public float GetReloadBonus() => series != null ? series.reloadSpeedBonus[Mathf.Clamp(rarity - 1, 0, 3)] * GetMultiplier(WeaponPartType_Alpha.Casing) : 0f;
        public float GetSpeedBonus() => series != null ? series.bulletSpeedBonus[Mathf.Clamp(rarity - 1, 0, 3)] * GetMultiplier(WeaponPartType_Alpha.Primer) : 0f;
    }

    [Header("Inventory Management")]
    
    public List<EquipInstance> equipInstance = new List<EquipInstance>();

    
    public int freeSlotCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 蛻晄悄迥ｶ諷九〒9譫縺ｯ譛菴朱剞遒ｺ菫昴＠縺ｦ縺翫￥
        while (equipInstance.Count < BASIC_SLOT_COUNT)
        {
            equipInstance.Add(new EquipInstance());
        }
    }

    // --- 譌｢蟄倥さ繝ｼ繝・(Player_Shooter_Alpha) 縺九ｉ蜻ｼ縺ｰ繧後ｋ繝｡繧ｽ繝・ラ鄒､ ---

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
        Alpha_BulletPrototypeBuilder.ClearPrototypes();
        UpdateSlotCount(true);
    }

    public void BattleStartEffect()
    {
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                EquipInstance instance = Get(x, y);
                // 繧ｹ繝ｭ繝・ヨ縺梧悴險ｭ螳夲ｼ・efId 縺・null 縺ｾ縺溘・遨ｺ・峨・蝣ｴ蜷医・繧ｹ繧ｭ繝・・
                if (instance.series == null) continue;
            }
        }
    }

    // --- 譁ｰ縺励＞繧ｷ繧ｹ繝・Β逕ｨ縺ｮ繝｡繧ｽ繝・ラ鄒､ ---

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

        // 遨ｺ縺肴棧・・efId縺系ull縺ｾ縺溘・遨ｺ・峨′縺ゅｌ縺ｰ縺昴％縺ｫ蜈･繧後ｋ
        bool added = false;
        for (int i = 0; i < equipInstance.Count; i++)
        {
            if (equipInstance[i].series == null)
            {
                equipInstance[i] = item;
                added = true;
                playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
        Alpha_BulletPrototypeBuilder.ClearPrototypes();
                break;
            }
        }
        
        // 遨ｺ縺阪′縺ｪ縺代ｌ縺ｰ譛ｫ蟆ｾ縺ｫ霑ｽ蜉
        if (!added)
        {
            equipInstance.Add(item);
            playerStatusManager_Alpha.Instance?.UpdateEquipmentBuffs();
        Alpha_BulletPrototypeBuilder.ClearPrototypes();
        }

        // 霑ｽ蜉HP蝗槫ｾｩ蜃ｦ逅・
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
            // 蠕後ｍ縺九ｉ蜑企勁縺励※縺・￥
            for (int i = equipInstance.Count - 1; i >= keepCount; i--)
            {
                var item = equipInstance[i];
                if (item.series != null)
                {
                    // 雋ｩ螢ｲ荳崎・繧ｨ繝輔ぉ繧ｯ繝医・繝√ぉ繝・け
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
                        // 螢ｲ蜊ｴ荳榊庄繧｢繧､繝・Β縺ｯ繝ｪ繧ｹ繝医↓騾驕ｿ縺励※縺翫″縲∝ｾ後〒謌ｻ縺・
                        itemsToKeep.Add(item);
                        equipInstance.RemoveAt(i);
                        Debug.Log($"[InventoryManager] Item '{item.series.name}' is unsellable and was kept.");
                        continue;
                    }

                    int exp = 0;
                    if (item.series.name != "InitialSeries")
                    {
                        // 螢ｲ蜊ｴ蜃ｦ逅・ rarity * 2 縺ｮ EXP
                        exp = item.rarity * 2;
                        if (exp <= 0) exp = 2; // 譛菴惹ｿ晁ｨｼ
                    }
                    
                    totalExpGained += exp;
                    Debug.Log($"[InventoryManager] Sold temporary item for {exp} EXP.");
                }
                equipInstance.RemoveAt(i);
            }

            // 螢ｲ蜊ｴ荳榊庄縺縺｣縺溘い繧､繝・Β繧貞・縺ｮ鬆・ｺ上〒譛ｫ蟆ｾ縺ｫ謌ｻ縺・
            itemsToKeep.Reverse();
            foreach (var item in itemsToKeep)
            {
                equipInstance.Add(item);
            }
        }

        if (totalExpGained > 0)
        {
            Debug.Log($"[InventoryManager] Total EXP gained from selling temporary items: {totalExpGained}");
            // TODO: 螳滄圀縺ｮ繝励Ξ繧､繝､繝ｼ繧ｹ繝・・繧ｿ繧ｹ遲峨↓EXP繧貞刈邂励☆繧句・逅・ｒ縺薙％縺ｫ郢九￡繧・
        }

        UpdateSlotCount(false);
        return totalExpGained;
    }

    /// <summary>
    /// 荳譎ゅせ繝ｭ繝・ヨ縺ｫ縺ゅｋ蛻晄悄陬・ｙ(InitialSeries)繧堤峩縺｡縺ｫ蜑企勁縺励∪縺・
    /// 蝣ｱ驟ｬ迯ｲ蠕礼判髱｢遲峨〒陬・ｙ謨ｴ逅・′邨ゅｏ縺｣縺滄圀縺ｫ蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺・
    /// </summary>
    public void CleanUpInitialSeriesInEXSlots()
    {
        int keepCount = BASIC_SLOT_COUNT + freeSlotCount;

        if (equipInstance.Count > keepCount)
        {
            // 蠕後ｍ縺九ｉ隱ｿ縺ｹ縺ｦ蜑企勁
            for (int i = equipInstance.Count - 1; i >= keepCount; i--)
            {
                var item = equipInstance[i];
                if (item.series != null && item.series.name == "InitialSeries")
                {
                    // 蛻晄悄陬・ｙ縺ｪ繧牙叉蜑企勁
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
            // 3縺ｮ蛟肴焚縺ｫ謠・∴繧句・逅・ｒ蜑企勁縺励∽ｽ吝・縺ｪ繧ｹ繝ｭ繝・ヨ縺・縺､縺縺代↓縺ｪ繧九ｈ縺・↓縺吶ｋ
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
    /// 繝悶・繧ｱ迥ｶ諷九・蛻､螳・
    /// 蜈ｨ縺ｦ縺ｮ繧ｻ繝・ヨ(陦・縺後√◎繧後◇繧悟酔縺倥す繝ｪ繝ｼ繧ｺ縺ｧ邨ｱ荳縺輔ｌ縺ｦ縺・ｋ縺具ｼ亥・9譫縺悟沂縺ｾ縺｣縺ｦ縺・ｋ蠢・ｦ√′縺ゅｋ・・
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
    /// 繧｢繧､繝・Β縺ｫWildcard繧ｨ繝輔ぉ繧ｯ繝医′莉倅ｸ弱＆繧後※縺・ｋ縺九メ繧ｧ繝・け縺吶ｋ
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
    /// 迚ｹ螳壹・繧ｰ繝ｫ繝ｼ繝暦ｼ郁｡鯉ｼ峨・3縺､縺ｮ繝代・繝・☆縺ｹ縺ｦ蜷後§繧ｷ繝ｪ繝ｼ繧ｺ縺ｧ邨ｱ荳縺輔ｌ縺ｦ縺・ｋ縺具ｼ・ildcard繧定・・・・
    /// </summary>
    public bool IsGroupSeriesAligned(int groupIndex)
    {
        int startIndex = groupIndex * 3;
        
        // 霑ｽ蜉: 繝ｪ繧ｹ繝医・遽・峇螟悶い繧ｯ繧ｻ繧ｹ繧帝亟縺・
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

        // 繝吶・繧ｹ縺ｨ縺ｪ繧九す繝ｪ繝ｼ繧ｺ繧呈爾縺呻ｼ・ildcard莉･螟悶・譛蛻昴・繧ｷ繝ｪ繝ｼ繧ｺ・・
        Alpha.Data.WeaponSeriesData_Alpha baseSeries = null;
        if (!jokerA) baseSeries = seriesA;
        else if (!jokerB) baseSeries = seriesB;
        else if (!jokerC) baseSeries = seriesC;

        // 3縺､縺ｨ繧８ildcard縺ｮ蝣ｴ蜷医・邨ｱ荳縺輔ｌ縺ｦ縺・ｋ縺ｨ縺ｿ縺ｪ縺・
        if (baseSeries == null) return true; 

        // Wildcard莉･螟悶・繝代・繝・′繝吶・繧ｹ繧ｷ繝ｪ繝ｼ繧ｺ縺ｨ荳閾ｴ縺吶ｋ縺九メ繧ｧ繝・け
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
        
        // activeGroup縺・1・医げ繝ｫ繝ｼ繝玲欠螳壹↑縺暦ｼ峨・蝣ｴ蜷医・辟｡譚｡莉ｶ縺ｧ繧｢繧ｯ繝・ぅ繝悶→縺吶ｋ
        bool isActive = (activeGroup == -1);

        for (int i = 0; i < equipInstance.Count; i++)
        {
            var item = equipInstance[i];
            if (item.series == null) continue;

            int itemGroup = i / 3;

            // 1. 豁ｦ蝎ｨ縺ｮ繝吶・繧ｹ(series)縺ｫ邏舌▼縺上ヱ繝・す繝門柑譫懊ｒ蜉邂・
            if (item.series.passiveEffects != null)
            {
                foreach (var spe in item.series.passiveEffects)
                {
                    if (spe.effect == null) continue;
                    int appliedRarity = spe.fixedQualityOverride > 0 ? spe.fixedQualityOverride : item.rarity;
                    AccumulateSingleEffect(spe.effect, effectType, appliedRarity, itemGroup, activeGroup, ref totalFlatValue, ref totalQuality, ref stepEffectRef, ref isActive);
                }
            }

            // 2. 繝励Ξ繧､繝､繝ｼ縺檎峩謗･莉倅ｸ弱＠縺溷崋譛牙柑譫・currentEffects)繧貞刈邂・
            if (item.currentEffects != null)
            {
                foreach (var effectSO in item.currentEffects)
                {
                    if (effectSO == null) continue;
                    AccumulateSingleEffect(effectSO, effectType, item.rarity, itemGroup, activeGroup, ref totalFlatValue, ref totalQuality, ref stepEffectRef, ref isActive);
                }
            }

            // 3. 繧ｻ繝・ヨ繝懊・繝翫せ繧ｨ繝輔ぉ繧ｯ繝医・蜉邂暦ｼ医す繝ｪ繝ｼ繧ｺ邨ｱ荳譎ゅ・縺ｿ逋ｺ蜍包ｼ・
            if (item.setBonusEffect != null && IsGroupSeriesAligned(itemGroup))
            {
                AccumulateSingleEffect(item.setBonusEffect, effectType, item.rarity, itemGroup, activeGroup, ref totalFlatValue, ref totalQuality, ref stepEffectRef, ref isActive);
            }
        }

        // 迴ｾ蝨ｨ縺ｮ繧ｰ繝ｫ繝ｼ繝励〒逋ｺ蜍墓擅莉ｶ繧呈ｺ縺溘＠縺ｦ縺・↑縺代ｌ縺ｰ蜉ｹ譫憺㍼縺ｯ0
        if (!isActive) return 0f;

        if (stepEffectRef != null && stepEffectRef.useStepMultiplier)
        {
            return CalculateStepValue(stepEffectRef, totalQuality);
        }

        return totalFlatValue;
    }

    private void AccumulateSingleEffect(Alpha.Data.WeaponEffectSO_Alpha effectSO, Alpha.Data.WeaponEffectType_Alpha targetType, int rarity, int itemGroup, int activeGroup, ref float flatValue, ref int totalQuality, ref Alpha.Data.WeaponEffectSO_Alpha stepEffectRef, ref bool isActive)
    {
        // 隍・粋繧ｹ繧ｭ繝ｫ縺ｮ蝣ｴ蜷医・蜀榊ｸｰ逧・↓荳ｭ霄ｫ繧貞叙繧雁・縺・
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

                // 逋ｺ蜍墓擅莉ｶ縺ｮ繝√ぉ繝・け・育樟蝨ｨ縺ｮ繧ｰ繝ｫ繝ｼ繝励′蟇ｾ雎｡縺九√∪縺溘・繧ｰ繝ｭ繝ｼ繝舌Ν蜉ｹ譫懊°・・
                if (effectSO.isGlobalEffect || itemGroup == activeGroup)
                {
                    isActive = true;
                }

            // 蜉邂玲擅莉ｶ縺ｮ繝√ぉ繝・け
            // accumulateGlobally縺掲alse縺ｮ蝣ｴ蜷医∫樟蝨ｨ讒九∴縺ｦ縺・ｋ繧ｰ繝ｫ繝ｼ繝励〒縺ｯ縺ｪ縺・↑繧牙刈邂励＠縺ｪ縺・
            // ・医げ繝ｭ繝ｼ繝舌Ν蜉ｹ譫懊・蝣ｴ蜷医・isGlobalEffect縺ｧ辟｡譚｡莉ｶ縺ｫ蜉邂励＆繧後ｋ險ｭ險医〒縺ｯ縺ｪ縺上√≠縺上∪縺ｧ縺薙・繧ｰ繝ｫ繝ｼ繝励′髢｢菫ゅ＠縺ｦ縺・ｋ縺九←縺・°縺ｮ蛻ｶ蠕｡・・
            if (!effectSO.accumulateGlobally && !effectSO.isGlobalEffect && activeGroup != -1 && itemGroup != activeGroup)
            {
                return;
            }

            // 蛟､縺ｮ蜉邂励・縲檎匱蜍墓擅莉ｶ繧呈ｺ縺溘＠縺ｦ縺・ｋ縺九阪↓髢｢繧上ｉ縺壹・荳願ｨ俶擅莉ｶ繧偵ヱ繧ｹ縺励※縺・ｌ縺ｰ)蜈ｨ繧ｹ繝ｭ繝・ヨ縺九ｉ繝代ヶ繝ｪ繝・け縺ｫ陦後≧
            if (effectSO.useStepMultiplier)
            {
                totalQuality += rarity; // 繧ｷ繝ｪ繝ｼ繧ｺ蛛ｴ縺ｧ謖・ｮ壹＆繧後◆蝗ｺ螳壼刀雉ｪ縲√∪縺溘・騾壼ｸｸ蜩∬ｳｪ繧貞刈邂・
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
            // 谿ｵ髫弱・蛻､螳・
            int stepIndex = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (totalQuality >= thresholds[i])
                {
                    stepIndex = i + 1;
                }
                else
                {
                    break; // 髢ｾ蛟､縺ｫ貅縺溘↑縺・ｴ蜷医・縺昴ｌ莉･荳翫・谿ｵ髫弱↓縺ｯ騾ｲ縺ｾ縺ｪ縺・
                }
            }

            // qualityValues 縺九ｉ蟇ｾ蠢懊☆繧倶ｹ玲焚繧貞叙蠕・
            if (effectSO.qualityValues != null && effectSO.qualityValues.Length > 0)
            {
                int valIndex = Mathf.Clamp(stepIndex, 0, effectSO.qualityValues.Length - 1);
                multiplier = effectSO.qualityValues[valIndex];
            }
        }

        // 譛邨ら噪縺ｪ險育ｮ暦ｼ壼粋險亥刀雉ｪ * 蟇ｾ蠢懊☆繧倶ｹ玲焚
        return totalQuality * multiplier;
    }
}

