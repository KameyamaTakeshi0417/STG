using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Core
{
    /// <summary>
    /// アンコモン以下（および多段ヒット弾・零落弾）の着弾時にプロシージャル生成される跳ね火花エフェクト。
    /// 外部画像アセットに一切依存せず、C#コードによるメモリ上テクスチャ生成と動的アニメーションで描画。
    /// 外部のシングルトンやコルーチンランナーに依存せず、生成されたオブジェクト自身が自立して更新・破棄される。
    /// </summary>
    public static class ProceduralHitSparks_Alpha
    {
        private static Sprite cachedSparkSprite;
        private static Material defaultSpriteMaterial;

        private static void EnsureAssets()
        {
            if (defaultSpriteMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    defaultSpriteMaterial = new Material(shader);
                }
            }

            if (cachedSparkSprite == null)
            {
                // 32x32 のシャープな星芒（スパーク）テクスチャをメモリ上に生成
                const int sz = 32;
                Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                Color[] colors = new Color[sz * sz];
                Vector2 center = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.5f);
                float maxR = sz * 0.48f;

                for (int y = 0; y < sz; y++)
                {
                    for (int x = 0; x < sz; x++)
                    {
                        float nx = Mathf.Abs(x - center.x) / maxR;
                        float ny = Mathf.Abs(y - center.y) / maxR;

                        // 十字方向の鋭い光芒 + 中心コアの白熱グロー
                        float cross = Mathf.Max(0f, 1f - (nx * 3.5f + ny * 0.8f)) +
                                      Mathf.Max(0f, 1f - (ny * 3.5f + nx * 0.8f));
                        float diag = Mathf.Max(0f, 1f - (nx + ny) * 1.4f);
                        float core = Mathf.Max(0f, 1f - Mathf.Sqrt(nx * nx + ny * ny) * 2.2f);

                        float intensity = Mathf.Clamp01(cross * 0.7f + diag * 0.4f + core * 1.2f);
                        colors[y * sz + x] = new Color(1f, 1f, 1f, intensity);
                    }
                }
                tex.SetPixels(colors);
                tex.Apply();
                cachedSparkSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 32);
            }
        }

        /// <summary>
        /// 着弾地点から弾丸の入射ベクトルと逆向きに跳ねる軽微な火花を動的生成
        /// </summary>
        public static void SpawnBounceSparks(Vector3 hitPos, Vector3 bulletDir, int count = 5, float spreadAngleMax = 55f, float speedScale = 1.0f)
        {
            EnsureAssets();

            // 弾丸の進行方向の逆向き（跳ね返りベクトル）
            Vector3 bounceDir = -bulletDir.normalized;
            if (bounceDir.sqrMagnitude < 0.001f) bounceDir = Vector3.up;

            float baseAngle = Mathf.Atan2(bounceDir.y, bounceDir.x) * Mathf.Rad2Deg;

            // Z座標を手前に出してスプライトの前後関係で埋もれないようにする
            Vector3 spawnPos = new Vector3(hitPos.x, hitPos.y, -0.5f);

            for (int i = 0; i < count; i++)
            {
                GameObject sparkObj = new GameObject("ProceduralHitSpark");
                sparkObj.transform.position = spawnPos;

                // 跳ね返り方向を中心にランダム散布
                float spreadAngle = (baseAngle + Random.Range(-spreadAngleMax, spreadAngleMax)) * Mathf.Deg2Rad;
                Vector3 velDir = new Vector3(Mathf.Cos(spreadAngle), Mathf.Sin(spreadAngle), 0f);
                float speed = Random.Range(4.5f, 8.5f) * speedScale;

                sparkObj.transform.localRotation = Quaternion.Euler(0, 0, spreadAngle * Mathf.Rad2Deg);

                // しっかり視認できるサイズ（幅 0.7〜1.2、厚み 0.2〜0.35）
                Vector3 initialScale = new Vector3(Random.Range(0.7f, 1.2f), Random.Range(0.2f, 0.35f), 1f);
                sparkObj.transform.localScale = initialScale;

                SpriteRenderer sr = sparkObj.AddComponent<SpriteRenderer>();
                sr.sprite = cachedSparkSprite;
                if (defaultSpriteMaterial != null) sr.material = defaultSpriteMaterial;

                // ワールド空間で確実に最前面に描画（object レイヤー、order 250）
                sr.sortingLayerName = "object";
                sr.sortingOrder = 250;

                // 白熱ゴールド〜温かい火花オレンジ
                Color sparkColor = Color.Lerp(
                    new Color(1f, 0.98f, 0.82f, 1f), // 白熱イエローホワイト
                    new Color(1f, 0.65f, 0.18f, 1f), // 温かい火花オレンジ
                    Random.value
                );
                sr.color = sparkColor;

                // 自立型ランナーコンポーネントをアタッチして駆動（シングルトン依存ゼロ）
                ProceduralSparkRunner runner = sparkObj.AddComponent<ProceduralSparkRunner>();
                runner.Initialize(velDir * speed, initialScale, sparkColor, Random.Range(0.20f, 0.30f));
            }
        }
    }

    /// <summary>
    /// 火花パーティクルの自立駆動コンポーネント
    /// </summary>
    public class ProceduralSparkRunner : MonoBehaviour
    {
        private Vector3 velocity;
        private Vector3 initialScale;
        private Color baseColor;
        private float duration;
        private float elapsed = 0f;
        private SpriteRenderer sr;

        public void Initialize(Vector3 startVelocity, Vector3 startScale, Color color, float lifeDuration)
        {
            velocity = startVelocity;
            initialScale = startScale;
            baseColor = color;
            duration = Mathf.Max(0.08f, lifeDuration);
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // 空気抵抗による減速（初速は速く、徐々に失速）
            float dragFactor = Mathf.Clamp01(1f - t * 1.5f);
            transform.position += velocity * (Time.deltaTime * dragFactor);

            // スケールとアルファの減衰
            float fade = 1f - t;
            transform.localScale = initialScale * (fade * 0.9f + 0.1f);

            if (sr != null)
            {
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (fade * fade));
            }
        }
    }
}
