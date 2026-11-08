#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Alpha.Enemy.Weapons;

namespace Alpha.EditorTools
{
    public static class SunflowerPrefabGenerator
    {
        [MenuItem("Alpha/弾幕作成/向日葵弾幕アセットを自動生成 (Setup Sunflower Assets)")]
        public static void GenerateSunflowerAssets()
        {
            // 1. スプライト取得
            string spritePath = "Assets/resources/Texture/item/himawari.png";
            Sprite himawariSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (himawariSprite == null)
            {
                Debug.LogError($"[SunflowerGenerator] スプライトが見つかりません: {spritePath}");
                return;
            }

            // 2. 弾プレハブの検索（花びら弾または敵弾）
            GameObject circleBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/resources/Objects/Bullet/enemyBullet/testEnemyBullet_Barrage.prefab");
            if (circleBulletPrefab == null)
            {
                circleBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/resources/Objects/Bullet/enemyBullet/testEnemyBullet_small.prefab");
            }

            GameObject scatterBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/resources/Objects/Bullet/PetalBullet.prefab");
            if (scatterBulletPrefab == null)
            {
                scatterBulletPrefab = circleBulletPrefab;
            }

            // 3. GameObjectの構築
            GameObject go = new GameObject("SunflowerBarrageUnit");
            
            // SpriteRenderer
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = himawariSprite;
            sr.sortingOrder = 5; // 弾や背景の前、見やすい位置

            // SunflowerBarrageUnit_Alpha
            SunflowerBarrageUnit_Alpha unit = go.AddComponent<SunflowerBarrageUnit_Alpha>();
            unit.riseSpeed = 2.5f;
            unit.rotationSpeed = 120f;
            unit.swayAmount = 0.6f;
            unit.swaySpeed = 2.0f;
            unit.destroyY = 12.0f;

            // 円形弾
            unit.enableCircleBarrage = true;
            unit.circleInterval = 1.5f;
            unit.circleBulletCount = 16;
            unit.circleBulletSpeed = 3.5f;
            unit.circleBulletPrefab = circleBulletPrefab;

            // ばらまき弾
            unit.enableScatterBarrage = true;
            unit.scatterInterval = 0.2f;
            unit.scatterWays = 3; // 3wayスパイラル
            unit.scatterBulletSpeed = 2.5f;
            unit.scatterRandomSpread = 12f;
            unit.scatterBulletPrefab = scatterBulletPrefab;

            // 4. Prefabの保存
            string dirPath = "Assets/resources/Objects/Enemy";
            if (!AssetDatabase.IsValidFolder(dirPath))
            {
                AssetDatabase.CreateFolder("Assets/resources/Objects", "Enemy");
            }

            string prefabPath = $"{dirPath}/SunflowerBarrageUnit.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"[SunflowerGenerator] 向日葵プレハブを作成しました: {prefabPath}");

            // 5. Behavior ScriptableObject の作成
            string dataDir = "Assets/resources/Data/EnemyBehaviors";
            if (!AssetDatabase.IsValidFolder("Assets/resources/Data"))
            {
                AssetDatabase.CreateFolder("Assets/resources", "Data");
            }
            if (!AssetDatabase.IsValidFolder(dataDir))
            {
                AssetDatabase.CreateFolder("Assets/resources/Data", "EnemyBehaviors");
            }

            string behaviorPath = $"{dataDir}/Behavior_SunflowerBloom.asset";
            Behavior_SunflowerAscent behavior = AssetDatabase.LoadAssetAtPath<Behavior_SunflowerAscent>(behaviorPath);
            if (behavior == null)
            {
                behavior = ScriptableObject.CreateInstance<Behavior_SunflowerAscent>();
                behavior.attackName = "Sunflower Bloom";
                behavior.sunflowerPrefab = savedPrefab;
                behavior.behaviorDuration = 12f;
                behavior.spawnIntervalMin = 1.2f;
                behavior.spawnIntervalMax = 2.5f;
                behavior.spawnXMin = -7.0f;
                behavior.spawnXMax = 7.0f;
                behavior.spawnY = -10.0f;
                behavior.destroyY = 12.0f;

                AssetDatabase.CreateAsset(behavior, behaviorPath);
                Debug.Log($"[SunflowerGenerator] Behaviorアセットを作成しました: {behaviorPath}");
            }
            else
            {
                behavior.sunflowerPrefab = savedPrefab;
                EditorUtility.SetDirty(behavior);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 作成したアセットを選択
            Selection.activeObject = behavior;
            EditorGUIUtility.PingObject(behavior);

            EditorUtility.DisplayDialog("向日葵弾幕セットアップ完了", 
                "向日葵プレハブと Behavior アセットを自動作成しました！\n\n" +
                $"・プレハブ: {prefabPath}\n" +
                $"・Behavior: {behaviorPath}\n\n" +
                "ボスの行動リストにこの Behavior を追加してテストできます。", "OK");
        }
    }
}
#endif
