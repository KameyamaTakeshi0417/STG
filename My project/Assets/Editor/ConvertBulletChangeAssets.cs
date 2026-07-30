using UnityEngine;
using UnityEditor;
using System.IO;
using Alpha.Data;

public class ConvertBulletChangeAssets : EditorWindow
{
    [MenuItem("Tools/Alpha/Convert BulletChange Effects")]
    public static void ShowWindow()
    {
        GetWindow<ConvertBulletChangeAssets>("Convert BulletChange Effects");
    }

    private void OnGUI()
    {
        GUILayout.Label("既存のBulletChange（WeaponEffectSO_Alpha）を", EditorStyles.boldLabel);
        GUILayout.Label("BulletChangeWeaponEffectSO_Alpha型に変換します。", EditorStyles.boldLabel);

        if (GUILayout.Button("変換開始"))
        {
            ConvertAssets();
        }
    }

    private void ConvertAssets()
    {
        // プロジェクト内のすべてのWeaponEffectSO_Alphaを検索
        string[] guids = AssetDatabase.FindAssets("t:WeaponEffectSO_Alpha");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponEffectSO_Alpha oldAsset = AssetDatabase.LoadAssetAtPath<WeaponEffectSO_Alpha>(path);

            if (oldAsset != null && oldAsset.effectType == WeaponEffectType_Alpha.BulletChange)
            {
                // すでに変換済みならスキップ
                if (oldAsset is BulletChangeWeaponEffectSO_Alpha)
                    continue;

                // 新しいアセットを作成
                BulletChangeWeaponEffectSO_Alpha newAsset = ScriptableObject.CreateInstance<BulletChangeWeaponEffectSO_Alpha>();

                // 古いアセットから基本パラメータをコピー
                newAsset.effectType = oldAsset.effectType;
                newAsset.effectName = oldAsset.effectName;
                newAsset.description = oldAsset.description;
                newAsset.qualityValues = oldAsset.qualityValues;
                newAsset.useStepMultiplier = oldAsset.useStepMultiplier;
                newAsset.isGlobalEffect = oldAsset.isGlobalEffect;
                // 他のフィールドがあればここに追加

                // 対応するシリーズデータを探してBulletPrefabを移植する（名前などで推定）
                // 完璧な推定は難しいため、ユーザーに手動設定をお願いするか、
                // 今回はWeaponSeriesData_Alphaを全検索して、passiveEffectsにこの旧アセットを含んでいるものを探す
                WeaponSeriesData_Alpha sourceSeries = FindSeriesWithEffect(oldAsset);
                if (sourceSeries != null)
                {
                    newAsset.bulletPrefab = sourceSeries.bulletPrefab;
                    
                    // シリーズのレアリティ(isRareSeriesなど)からTierを推定する
                    if (sourceSeries.isRareSeries)
                        newAsset.seriesTier = BulletChangeTier.Rare;
                    else
                        newAsset.seriesTier = BulletChangeTier.Common; // 詳細な判別は手動で調整をお願いする
                }

                // 古いアセットを上書き
                string oldPath = path;
                AssetDatabase.DeleteAsset(oldPath);
                AssetDatabase.CreateAsset(newAsset, oldPath);

                // シリーズデータの参照を更新
                if (sourceSeries != null)
                {
                    for (int i = 0; i < sourceSeries.passiveEffects.Count; i++)
                    {
                        if (sourceSeries.passiveEffects[i].effect == null) // DeleteAssetによりnullになる
                        {
                            var effect = sourceSeries.passiveEffects[i];
                            effect.effect = newAsset;
                            sourceSeries.passiveEffects[i] = effect;
                        }
                    }
                    EditorUtility.SetDirty(sourceSeries);
                }

                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ConvertBulletChangeAssets] {count} 個のアセットを変換しました。");
        EditorUtility.DisplayDialog("完了", $"{count} 個のアセットを BulletChangeWeaponEffectSO_Alpha に変換し、プレハブを移植しました。\n※SeriesTierや未移植のシリーズがある場合はインスペクターで手動設定してください。", "OK");
    }

    private WeaponSeriesData_Alpha FindSeriesWithEffect(WeaponEffectSO_Alpha targetEffect)
    {
        string[] guids = AssetDatabase.FindAssets("t:WeaponSeriesData_Alpha");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponSeriesData_Alpha series = AssetDatabase.LoadAssetAtPath<WeaponSeriesData_Alpha>(path);
            if (series != null && series.passiveEffects != null)
            {
                foreach (var pe in series.passiveEffects)
                {
                    if (pe.effect == targetEffect) return series;
                }
            }
        }
        return null;
    }
}
