using System.Collections.Generic;
using UnityEngine;
using Alpha.Data;
using System.Linq;

namespace Alpha.Core.State
{
    public static class RecommendationEngine_Alpha
    {
        public static int EvaluateReward(InventoryManager_Alpha.EquipInstance reward, List<InventoryManager_Alpha.EquipInstance> currentInventory)
        {
            if (reward.series == null) return 0;

            int score = 0;

            // --- 1. Analyze current inventory state ---
            
            // Group by series and track collected parts
            Dictionary<string, HashSet<WeaponPartType_Alpha>> seriesParts = new Dictionary<string, HashSet<WeaponPartType_Alpha>>();
            int totalEffectQuality = 0;

            foreach (var item in currentInventory)
            {
                if (item.series == null) continue;
                if (string.IsNullOrEmpty(item.defId) && item.series == null) continue; // Empty slot
                if (item.series.name == "InitialSeries") continue;

                // Track series parts
                if (!seriesParts.ContainsKey(item.series.name))
                {
                    seriesParts[item.series.name] = new HashSet<WeaponPartType_Alpha>();
                }
                seriesParts[item.series.name].Add(item.partType);

                // Calculate total effect quality
                if (item.currentEffects != null)
                {
                    foreach (var effect in item.currentEffects)
                    {
                        if (effect != null)
                        {
                            totalEffectQuality += item.rarity; 
                        }
                    }
                }
                if (item.series.passiveEffects != null)
                {
                    foreach (var pe in item.series.passiveEffects)
                    {
                        if (pe.effect != null)
                        {
                            totalEffectQuality += item.rarity;
                        }
                    }
                }
            }

            // --- 2. Calculate potential changes from reward ---
            
            string rSeries = reward.series.name;
            WeaponPartType_Alpha rPart = reward.partType;
            
            int currentPartsCount = 0;
            bool alreadyHasThisPart = false;
            
            if (seriesParts.ContainsKey(rSeries))
            {
                currentPartsCount = seriesParts[rSeries].Count;
                alreadyHasThisPart = seriesParts[rSeries].Contains(rPart);
            }

            // How many series are completely collected? (Assuming 3 is complete)
            int completedSeriesCount = seriesParts.Values.Count(parts => parts.Count >= 3);

            // Calculate reward's effect quality
            int rewardEffectQuality = 0;
            if (reward.currentEffects != null)
            {
                foreach (var eff in reward.currentEffects)
                {
                    if (eff != null) rewardEffectQuality += reward.rarity;
                }
            }
            if (reward.series.passiveEffects != null)
            {
                foreach (var pe in reward.series.passiveEffects)
                {
                    if (pe.effect != null) rewardEffectQuality += reward.rarity;
                }
            }

            // --- 3. Evaluate Conditions ---

            // Condition 1: Divine Bouquet Reach (10000 points)
            // Player has 2 completed series, and the reward completes a 3rd series.
            if (completedSeriesCount >= 2 && currentPartsCount == 2 && !alreadyHasThisPart)
            {
                score += 10000;
            }

            // Condition 2: Effect 20 Reach (5000 points)
            // Player is close to 20, and reward pushes them to 20 or above.
            if (totalEffectQuality < 20 && (totalEffectQuality + rewardEffectQuality) >= 20)
            {
                score += 5000;
            }

            // Condition 3: Series Completion Reach (1000 points)
            // Reward is the 3rd missing part of a series
            if (currentPartsCount == 2 && !alreadyHasThisPart)
            {
                score += 1000;
            }

            return score;
        }
    }
}
