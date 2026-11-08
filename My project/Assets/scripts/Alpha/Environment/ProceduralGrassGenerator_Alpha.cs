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

        [Header("Withered Grass (Stage Half Start)")]
        [Tooltip("ステージ前半・後半の開始時に枯れ草を少量生やすかどうか（StageManager_Alphaから呼ばれる）")]
        public bool spawnWitheredOnHalfStart = true;
        [Tooltip("枯れ草の本数（通常の草より少なめ推奨）")]
        public int witheredGrassCount = 150;
        [Tooltip("枯れ草のノイズ閾値。高いほどまばら・小さな塊になる")]
        public float witheredSpawnThreshold = 0.55f;
        public Color witheredColorA = new Color(0.55f, 0.42f, 0.25f, 1f);
        public Color witheredColorB = new Color(0.40f, 0.30f, 0.18f, 1f);

        [Header("Layer Settings")]
        public string sortingLayerName = "Default";
        public int baseSortingOrder = 0;
        [Tooltip("If true, automatically adjusts sorting order based on Y position for depth.")]
        public bool autoSortByY = true;

        [ContextMenu("Clear Grass")]
        public void ClearGrass()
        {
            // すでに生成されている草があれば削除（インデックスのズレを防ぐため逆順でループ）
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child.gameObject);
                }
                else
                {
                    Destroy(child.gameObject);
                }
#else
                Destroy(child.gameObject);
#endif
            }
        }

        [ContextMenu("Generate Grass")]
        public void GenerateGrassEditor()
        {
            GenerateGrass(0f);
        }

        [ContextMenu("Generate Withered Grass")]
        public void GenerateWitheredGrassEditor()
        {
            GenerateWitheredGrass(0f);
        }

        /// <summary>通常（緑）の草を生成する。既存の草（枯れ草含む）は消去される。</summary>
        public void GenerateGrass(float duration = 0.2f)
        {
            StopAllCoroutines(); // ここで既存のコルーチンを停止
            StartCoroutine(GenerateGrassRoutine(duration, grassCount, spawnThreshold, colorA, colorB));
        }

        /// <summary>枯れ草（茶色）を少量生成する。既存の草は消去される。</summary>
        public void GenerateWitheredGrass(float duration = 0f)
        {
            StopAllCoroutines();
            StartCoroutine(GenerateGrassRoutine(duration, witheredGrassCount, witheredSpawnThreshold, witheredColorA, witheredColorB));
        }

        private System.Collections.IEnumerator GenerateGrassRoutine(float duration, int count, float threshold, Color cA, Color cB)
        {
            ClearGrass();

            if (grassPrefab == null)
            {
                Debug.LogWarning("Grass Prefab is not set!");
                yield break;
            }

            // ランダムなノイズのオフセット（生成ごとに形を変えるため）
            float noiseOffsetX = Random.Range(-10000f, 10000f);
            float noiseOffsetY = Random.Range(-10000f, 10000f);

            int spawned = 0;
            int maxAttempts = count * 10; // 閾値に引っかからなかった場合のループ上限

            float startTime = Time.unscaledTime; // timeScaleに依存しないように変更

            for (int i = 0; i < maxAttempts; i++)
            {
                if (spawned >= count) break;

                // エリア内のランダムな座標
                float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
                float randomY = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
                
                Vector2 localPos = new Vector2(randomX, randomY) + areaOffset;
                Vector3 worldPos = transform.position + (Vector3)localPos;

                // パーリンノイズを取得
                // noiseScale が 0 以下だとノイズ値が常に一定(約0.465)になり、閾値次第で一本も生えなくなるため、
                // その場合はクラスタリング無し（常に生成可）として扱う
                float noiseVal = (noiseScale <= 0f) ? 1f : Mathf.PerlinNoise(
                    (worldPos.x + noiseOffsetX) * noiseScale, 
                    (worldPos.y + noiseOffsetY) * noiseScale
                );

                // ノイズが閾値を超えていれば生成
                if (noiseVal > threshold)
                {
                    GameObject grassObj = Instantiate(grassPrefab, worldPos, Quaternion.identity, transform);
                    
                    // 色の調整
                    SpriteRenderer sr = grassObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        // ノイズの値を使って色をブレンドするか、完全にランダムにする
                        float colorBlend = Random.value;
                        sr.color = Color.Lerp(cA, cB, colorBlend);

                        // 枯れ草かどうかを判定し、シェーダーの白黒化を適用
                        // (緑の草と枯れ草を区別するため、閾値かカラーで判定。
                        // 今回は「countがwitheredGrassCountと同じなら枯れ草」として判定)
                        bool isWithered = (count == witheredGrassCount);
                        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                        sr.GetPropertyBlock(mpb);
                        mpb.SetFloat("_Desaturate", isWithered ? 1f : 0f);
                        sr.SetPropertyBlock(mpb);

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

                    // durationが0より大きい場合、時間経過に合わせて待機
                    if (duration > 0f)
                    {
                        float elapsed = Time.unscaledTime - startTime;
                        float progress = Mathf.Clamp01(elapsed / duration);
                        int targetSpawned = Mathf.RoundToInt(count * progress);

                        if (spawned > targetSpawned)
                        {
                            yield return null;
                        }
                    }
                }
            }
            
            Debug.Log($"Generated {spawned} grass instances over {Time.unscaledTime - startTime} seconds.");
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
