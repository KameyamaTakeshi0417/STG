using UnityEngine;

namespace Alpha.Environment
{
    public class ProceduralGrassGenerator_Alpha : MonoBehaviour
    {
        [Header("Grass Settings")]
        public GameObject grassPrefab;
        [Tooltip("Number of grass instances to generate")]
        public int grassCount = 1000;
        
        [Header("Generation Area")]
        public Vector2 areaSize = new Vector2(20f, 20f);
        public Vector2 areaOffset = Vector2.zero;

        [Header("Perlin Noise Settings (Clustering)")]
        [Tooltip("Scale of the noise. Lower = larger clusters")]
        public float noiseScale = 0.1f;
        [Tooltip("Threshold above which grass will spawn (0 to 1)")]
        public float spawnThreshold = 0.4f;

        [Header("Color Variation")]
        public Color colorA = new Color(0.3f, 0.8f, 0.3f, 1f);
        public Color colorB = new Color(0.2f, 0.6f, 0.2f, 1f);

        [Header("Layer Settings")]
        public string sortingLayerName = "Default";
        public int baseSortingOrder = 0;
        [Tooltip("If true, automatically adjusts sorting order based on Y position for depth.")]
        public bool autoSortByY = true;

        [ContextMenu("Generate Grass")]
        public void GenerateGrass()
        {
            // すでに生成されている草があれば削除
            foreach (Transform child in transform)
            {
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }

            if (grassPrefab == null)
            {
                Debug.LogWarning("Grass Prefab is not set!");
                return;
            }

            // ランダムなノイズのオフセット（生成ごとに形を変えるため）
            float noiseOffsetX = Random.Range(-10000f, 10000f);
            float noiseOffsetY = Random.Range(-10000f, 10000f);

            int spawned = 0;
            int maxAttempts = grassCount * 10; // 閾値に引っかからなかった場合のループ上限

            for (int i = 0; i < maxAttempts; i++)
            {
                if (spawned >= grassCount) break;

                // エリア内のランダムな座標
                float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
                float randomY = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
                
                Vector2 localPos = new Vector2(randomX, randomY) + areaOffset;
                Vector3 worldPos = transform.position + (Vector3)localPos;

                // パーリンノイズを取得
                float noiseVal = Mathf.PerlinNoise(
                    (worldPos.x + noiseOffsetX) * noiseScale, 
                    (worldPos.y + noiseOffsetY) * noiseScale
                );

                // ノイズが閾値を超えていれば生成
                if (noiseVal > spawnThreshold)
                {
                    GameObject grassObj = Instantiate(grassPrefab, worldPos, Quaternion.identity, transform);
                    
                    // 色の調整
                    SpriteRenderer sr = grassObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        // ノイズの値を使って色をブレンドするか、完全にランダムにする
                        float colorBlend = Random.value;
                        sr.color = Color.Lerp(colorA, colorB, colorBlend);

                        // レイヤー設定
                        sr.sortingLayerName = sortingLayerName;
                        if (autoSortByY)
                        {
                            // Y座標が下にあるほど手前に描画されるようにする（Unityの2D基本ルール）
                            sr.sortingOrder = baseSortingOrder - Mathf.RoundToInt(worldPos.y * 100f);
                        }
                        else
                        {
                            sr.sortingOrder = baseSortingOrder;
                        }
                    }

                    // サイズや向きのわずかなブレ
                    float scaleVariation = Random.Range(0.8f, 1.2f);
                    grassObj.transform.localScale = new Vector3(scaleVariation, scaleVariation, 1f);
                    
                    // X軸の反転（ランダム感アップ）
                    if (Random.value > 0.5f)
                    {
                        Vector3 ls = grassObj.transform.localScale;
                        ls.x *= -1f;
                        grassObj.transform.localScale = ls;
                    }

                    spawned++;
                }
            }
            
            Debug.Log($"Generated {spawned} grass instances.");
        }

        // Sceneビューに生成範囲の枠を表示
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position + (Vector3)areaOffset;
            Gizmos.DrawWireCube(center, new Vector3(areaSize.x, areaSize.y, 0f));
        }
    }
}
