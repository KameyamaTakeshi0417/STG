using UnityEngine;
using UnityEditor;
using Alpha.Data;
using System.Collections.Generic;

public class SetupDrillAssets
{
    [MenuItem("Tools/Setup Drill Assets")]
    public static void Setup()
    {
        // 1. Create BurstFire Effect SO
        string effectPath = "Assets/scripts/Alpha/Data/Effects/Effect_BurstFire.asset";
        WeaponEffectSO_Alpha burstEffect = AssetDatabase.LoadAssetAtPath<WeaponEffectSO_Alpha>(effectPath);
        if (burstEffect == null)
        {
            burstEffect = ScriptableObject.CreateInstance<WeaponEffectSO_Alpha>();
            burstEffect.effectType = WeaponEffectType_Alpha.BurstFire;
            burstEffect.isGlobalEffect = false;
            // qualityValues: quality 1=2, 2=4, 3=6, 4=8, 5=10
            burstEffect.qualityValues = new float[] { 2f, 4f, 6f, 8f, 10f, 12f, 14f, 16f, 18f, 20f };
            
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(effectPath));
            AssetDatabase.CreateAsset(burstEffect, effectPath);
        }
        else
        {
            burstEffect.qualityValues = new float[] { 2f, 4f, 6f, 8f, 10f, 12f, 14f, 16f, 18f, 20f };
            EditorUtility.SetDirty(burstEffect);
        }

        // 2. Create Drill Series SO
        string seriesPath = "Assets/scripts/Alpha/Battle/Bullet/series/DrillSeries_Alpha.asset";
        WeaponSeriesData_Alpha drillSeries = AssetDatabase.LoadAssetAtPath<WeaponSeriesData_Alpha>(seriesPath);
        if (drillSeries == null)
        {
            drillSeries = ScriptableObject.CreateInstance<WeaponSeriesData_Alpha>();
            drillSeries.seriesName = "ドリル弾";
            
            drillSeries.passiveEffects = new List<WeaponEffectSO_Alpha>();
            drillSeries.passiveEffects.Add(burstEffect);
            
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(seriesPath));
            AssetDatabase.CreateAsset(drillSeries, seriesPath);
        }
        else
        {
            if (drillSeries.passiveEffects == null) drillSeries.passiveEffects = new List<WeaponEffectSO_Alpha>();
            if (!drillSeries.passiveEffects.Contains(burstEffect))
            {
                drillSeries.passiveEffects.Add(burstEffect);
            }
            EditorUtility.SetDirty(drillSeries);
        }

        // 3. Update DrillBullet Prefab
        string prefabPath = "Assets/resources/Objects/Bullet/DrillBullet.prefab";
        GameObject drillPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (drillPrefab != null)
        {
            var bulletScript = drillPrefab.GetComponent<Bullet_Base>();
            if (bulletScript != null)
            {
                // ドリル弾は生存時間がとても短い
                bulletScript.DestroyTime = 0.15f; 
                // ドリル弾は弾速を早めにして射程を稼ぐか、短いままでいくか。近接攻撃なので弾速は速くしすぎない。
                bulletScript.Speed = 15f; 
                
                EditorUtility.SetDirty(drillPrefab);
                PrefabUtility.SavePrefabAsset(drillPrefab);

                // Assign prefab to series
                if (drillSeries != null)
                {
                    drillSeries.bulletPrefab = drillPrefab;
                    EditorUtility.SetDirty(drillSeries);
                }
            }
            else
            {
                Debug.LogWarning("DrillBullet prefab is missing Bullet_Base component!");
            }
        }
        else
        {
            Debug.LogError("DrillBullet prefab not found at: " + prefabPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Drill Assets setup completed!");
    }
}
