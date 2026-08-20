using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core.Utils;

public static class Alpha_BulletPrototypeBuilder
{
    // Keyed by weapon group index (0, 1, 2) and -1 for Bouquet
    private static Dictionary<int, GameObject> prebakedPrototypes = new Dictionary<int, GameObject>();
    private static Transform prototypeContainer;

    public static void ClearPrototypes()
    {
        foreach (var kvp in prebakedPrototypes)
        {
            if (kvp.Value != null) GameObject.Destroy(kvp.Value);
        }
        prebakedPrototypes.Clear();
    }

    public static GameObject GetOrBuildPrototype(int weaponGroup, InventoryManager_Alpha inventory)
    {
        if (prebakedPrototypes.ContainsKey(weaponGroup) && prebakedPrototypes[weaponGroup] != null)
        {
            return prebakedPrototypes[weaponGroup];
        }

        GameObject prototype = BuildPrototype(weaponGroup, inventory);
        if (prototype != null)
        {
            prebakedPrototypes[weaponGroup] = prototype;
        }
        return prototype;
    }

    private static GameObject BuildPrototype(int weaponGroup, InventoryManager_Alpha inventory)
    {
        if (inventory == null) return null;

        GameObject basePrefab = null;
        List<Alpha.Data.WeaponSeriesData_Alpha> seriesList = new List<Alpha.Data.WeaponSeriesData_Alpha>();
        List<int> rarities = new List<int>();

        if (weaponGroup == -1) // Bouquet
        {
            for (int i = 0; i < 3; i++)
            {
                var inst = inventory.Get(0, i);
                if (inst.series != null)
                {
                    seriesList.Add(inst.series);
                    rarities.Add(inst.rarity);
                    if (basePrefab == null && inst.series.bulletPrefab != null)
                    {
                        basePrefab = inst.series.bulletPrefab;
                    }
                }
            }
        }
        else
        {
            for (int i = 2; i >= 0; i--)
            {
                var inst = inventory.Get(i, weaponGroup);
                if (inst.series != null)
                {
                    seriesList.Add(inst.series);
                    rarities.Add(inst.rarity);
                    if (basePrefab == null && inst.series.bulletPrefab != null)
                    {
                        basePrefab = inst.series.bulletPrefab;
                    }
                }
            }
            seriesList.Reverse();
            rarities.Reverse();
        }

        if (basePrefab == null)
        {
            basePrefab = Resources.Load<GameObject>("Objects/Bullet/NormalBullet");
            if (basePrefab == null) return null;
        }

        GameObject prototype = GameObject.Instantiate(basePrefab);
        prototype.name = "PrebakedBulletPrototype_Group_" + weaponGroup;
        prototype.SetActive(false);
        
        if (prototypeContainer == null)
        {
            prototypeContainer = new GameObject("Alpha_BulletPrototypes").transform;
            GameObject.DontDestroyOnLoad(prototypeContainer.gameObject);
        }
        prototype.transform.SetParent(prototypeContainer);

        Bullet_Base bulletScript = prototype.GetComponent<Bullet_Base>();
        if (bulletScript == null) return prototype;

        for (int i = 0; i < seriesList.Count; i++)
        {
            var series = seriesList[i];
            int rarity = rarities[i];
            
            List<Alpha.Data.WeaponEffectSO_Alpha> effectsToApply = new List<Alpha.Data.WeaponEffectSO_Alpha>();
            if (series.bulletSpecificEffects != null) effectsToApply.AddRange(series.bulletSpecificEffects);
            if (series.casingSpecificEffects != null) effectsToApply.AddRange(series.casingSpecificEffects);
            if (series.primerSpecificEffects != null) effectsToApply.AddRange(series.primerSpecificEffects);
            if (series.passiveEffects != null)
            {
                foreach(var pe in series.passiveEffects)
                {
                    if (pe.effect != null) effectsToApply.Add(pe.effect);
                }
            }

            foreach (var eff in effectsToApply)
            {
                if (eff == null) continue;
                
                if (eff.effectType == Alpha.Data.WeaponEffectType_Alpha.Homing)
                {
                    var homing = prototype.AddComponent<Behavior_Homing_Alpha>();
                    homing.homingStrength = eff.GetValue(rarity) * 2f; 
                    homing.Initialize(bulletScript, rarity);
                }
                // Tsubaki commented out
            }
        }
        
        bulletScript.behaviors = prototype.GetComponents<Alpha_BulletBehavior_Base>();

        return prototype;
    }
}

