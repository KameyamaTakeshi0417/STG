using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Core
{
    /// <summary>
    /// 毒（Volt）弾着弾時・毒ダメージ発生時にプロシージャル生成される藤色（ウィステリア / パープル）の飛沫エフェクト。
    /// 外部画像アセットに一切依存せず、C#コードによるメモリ上テクスチャ生成と動的アニメーションで描画。
    /// 自立型ランナーにより、外部マネージャーなしで確実に動作・破棄される。
    /// </summary>
    public static class ProceduralPoisonSplash_Alpha
    {
        private static Sprite cachedDropletSprite;
        private static Material defaultSpriteMaterial;

        // 藤色（Wisteria / Lavender-Violet）のカラーパレット
        private static readonly Color[] WisteriaPalette = new Color[]
        {
            new Color(0.72f, 0.58f, 0.98f, 0.95f), // #B894FA: 明るい藤色（ハイライト）
            new Color(0.61f, 0.35f, 0.71f, 0.92f), // #9B59B6: 上品な藤紫
            new Color(0.76f, 0.61f, 0.83f, 0.90f), // #C39BD3: 薄藤・ラベンダー
            new Color(0.51f, 0.22f, 0.68f, 0.95f), // #8238AD: 濃い藤紫（深み）
            new Color(0.88f, 0.78f, 0.96f, 0.98f)  // #E0C7F5: 液滴コアの白藤光
        };

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

            if (cachedDropletSprite == null)
            {
                // 32x32 の滑らかな水滴・液滴（ドロップレット）テクスチャをメモリ上に生成
                const int sz = 32;
                Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                Color[] colors = new Color[sz * sz];
                Vector2 center = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.5f);
                float maxR = sz * 0.46f;

                for (int y = 0; y < sz; y++)
                {
                    for (int x = 0; x < sz; x++)
                    {
                        float nx = (x - center.x) / maxR;
                        // y軸は少し涙型（先端が尖り、後部が丸い）に歪ませる
                        float ny = (y - center.y) / maxR;
                        
                        // 涙型シェイプ計算: 上(y > 0)に向かって徐々に細くなる
                        float shapeX = nx / (1.0f - ny * 0.35f);
                        float distSq = shapeX * shapeX + ny * ny;

                        if (distSq <= 1.0f)
                        {
                            float dist = Mathf.Sqrt(distSq);
                            // 縁に向かって滑らかに減衰、中心と先端寄りにハイライト
                            float alpha = Mathf.Clamp01(1f - dist);
                            alpha = Mathf.Pow(alpha, 0.7f); // 豊かな不透明コア

                            colors[y * sz + x] = new Color(1f, 1f, 1f, alpha);
                        }
                        else
                        {
                            colors[y * sz + x] = Color.clear;
                        }
                    }
                }
                tex.SetPixels(colors);
                tex.Apply();
                cachedDropletSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 32);
            }
        }

        /// <summary>
        /// 藤色の飛沫（液滴）を着弾地点から放射状に生成
        /// </summary>
        public static void SpawnSplash(Vector3 hitPos, Vector3 bulletDir, int count = 7, float scaleMultiplier = 1.0f)
        {
            EnsureAssets();

            // 弾丸の進行方向の逆向き（跳ね返りベクトル）を中心に飛沫を飛ばす
            Vector3 bounceDir = -bulletDir.normalized;
            if (bounceDir.sqrMagnitude < 0.001f) bounceDir = Vector3.up;

            float baseAngle = Mathf.Atan2(bounceDir.y, bounceDir.x) * Mathf.Rad2Deg;

            // Z座標を手前に出して確実に最前面に描画
            Vector3 spawnPos = new Vector3(hitPos.x, hitPos.y, -0.5f);

            for (int i = 0; i < count; i++)
            {
                GameObject dropObj = new GameObject("ProceduralPoisonDroplet");
                dropObj.transform.position = spawnPos;

                // 跳ね返り方向を中心に ±65度で扇状に散布
                float spreadAngle = (baseAngle + Random.Range(-65f, 65f)) * Mathf.Deg2Rad;
                Vector3 velDir = new Vector3(Mathf.Cos(spreadAngle), Mathf.Sin(spreadAngle), 0f);
                float speed = Random.Range(3.5f, 7.0f);

                // 進行方向を向くように回転（しずくの先端が飛ぶ方向を向く）
                // 涙型テクスチャは上(90度)向きなので -90度オフセット
                float rotAngle = Mathf.Atan2(velDir.y, velDir.x) * Mathf.Rad2Deg - 90f;
                dropObj.transform.localRotation = Quaternion.Euler(0, 0, rotAngle);

                // しずくサイズ（縦長 0.5〜0.9、横幅 0.25〜0.45）
                Vector3 initialScale = new Vector3(
                    Random.Range(0.25f, 0.45f) * scaleMultiplier,
                    Random.Range(0.55f, 0.95f) * scaleMultiplier,
                    1f
                );
                dropObj.transform.localScale = initialScale;

                SpriteRenderer sr = dropObj.AddComponent<SpriteRenderer>();
                sr.sprite = cachedDropletSprite;
                if (defaultSpriteMaterial != null) sr.material = defaultSpriteMaterial;

                // ワールド空間の最前面（object レイヤー、order 250）
                sr.sortingLayerName = "object";
                sr.sortingOrder = 250;

                // 藤色パレットからランダム選択
                Color dropColor = WisteriaPalette[Random.Range(0, WisteriaPalette.Length)];
                sr.color = dropColor;

                // 自立型ランナーコンポーネントをアタッチ
                ProceduralPoisonSplashRunner runner = dropObj.AddComponent<ProceduralPoisonSplashRunner>();
                runner.Initialize(velDir * speed, initialScale, dropColor, Random.Range(0.22f, 0.35f));
            }
        }
    }

    /// <summary>
    /// 藤色飛沫の自立駆動コンポーネント
    /// </summary>
    public class ProceduralPoisonSplashRunner : MonoBehaviour
    {
        private Vector3 velocity;
        private Vector3 initialScale;
        private Color baseColor;
        private float duration;
        private float elapsed = 0f;
        private SpriteRenderer sr;
        private const float Gravity = -2.2f; // 下方向への微弱な液滴重力感

        public void Initialize(Vector3 startVelocity, Vector3 startScale, Color color, float lifeDuration)
        {
            velocity = startVelocity;
            initialScale = startScale;
            baseColor = color;
            duration = Mathf.Max(0.1f, lifeDuration);
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

            // 空気抵抗で減速
            float drag = Mathf.Clamp01(1f - t * 1.6f);
            velocity.y += Gravity * Time.deltaTime; // ほんのり垂れ落ちる放物線
            transform.position += velocity * (Time.deltaTime * drag);

            // 速度ベクトルの向きに合わせて回転を更新
            if (velocity.sqrMagnitude > 0.05f)
            {
                float rotAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, rotAngle);
            }

            // スケール縮小（最後は水滴が小さく丸くなって消える）
            float fade = 1f - t;
            transform.localScale = initialScale * (fade * 0.85f + 0.15f);

            if (sr != null)
            {
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * fade);
            }
        }
    }
}
