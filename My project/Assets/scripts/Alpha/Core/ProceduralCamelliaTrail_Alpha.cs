using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Core
{
    /// <summary>
    /// 零落弾（椿弾）の飛行軌道に真っ赤な椿の花弁に見えるチリをプロシージャル生成するトレイルシステム。
    /// 外部画像アセットに一切依存せず、C#コードによるメモリ上テクスチャ生成と動的アニメーションで描画。
    /// 優雅に舞い散る深紅の花弁チリを演出する。
    /// </summary>
    public static class ProceduralCamelliaTrail_Alpha
    {
        private static Sprite cachedPetalSprite;
        private static Material defaultSpriteMaterial;

        // 真っ赤な椿（Camellia）のカラーパレット
        private static readonly Color[] CamelliaPalette = new Color[]
        {
            new Color(0.85f, 0.08f, 0.20f, 0.95f), // #D91433: 鮮やかな深紅
            new Color(0.70f, 0.02f, 0.12f, 0.92f), // #B3051F: 濃厚な真紅（椿色）
            new Color(0.92f, 0.18f, 0.28f, 0.96f), // #EB2E47: 朱紅ハイライト
            new Color(0.55f, 0.01f, 0.08f, 0.90f), // #8C0314: 陰影の深紅
            new Color(0.98f, 0.35f, 0.45f, 0.98f)  // #FA5973: 散り際の花弁の照り返し
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

            if (cachedPetalSprite == null)
            {
                // 32x32 の椿の花弁（先端が少し丸く、ハート形〜楕円形）テクスチャをメモリ上に生成
                const int sz = 32;
                Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                Color[] colors = new Color[sz * sz];
                Vector2 center = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.45f);
                float maxR = sz * 0.45f;

                for (int y = 0; y < sz; y++)
                {
                    for (int x = 0; x < sz; x++)
                    {
                        float nx = (x - center.x) / maxR;
                        float ny = (y - center.y) / maxR;

                        // 椿の花弁シェイプ: 先端(y > 0)のくびれと優美な丸み
                        float r = Mathf.Sqrt(nx * nx + ny * ny);
                        float angle = Mathf.Atan2(nx, ny); // 縦基準
                        // 椿特有のゆるやかなハート花弁輪郭
                        float petalRadius = 0.90f - 0.15f * Mathf.Cos(angle * 2.0f) + 0.12f * Mathf.Cos(angle);

                        if (r <= petalRadius)
                        {
                            float edgeDist = Mathf.Clamp01(1f - (r / petalRadius));
                            float alpha = Mathf.Pow(edgeDist, 0.6f); // 豊かな不透明コア
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
                cachedPetalSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 32);
            }
        }

        /// <summary>
        /// 椿の花弁チリを1個生成して舞わせる
        /// </summary>
        public static void SpawnPetalDust(Vector3 spawnPos, Vector3 bulletDir, float scaleMultiplier = 1.0f)
        {
            EnsureAssets();

            GameObject petalObj = new GameObject("CamelliaPetalDust");
            // Z座標を手前に出して描画順を保証
            petalObj.transform.position = new Vector3(spawnPos.x, spawnPos.y, -0.5f);

            // 弾丸の進行方向と逆向き（後方）に少しふわりと流れる初速
            Vector3 backward = -bulletDir.normalized;
            if (backward.sqrMagnitude < 0.01f) backward = -Vector3.up;

            // 後方ベクトルの左右に少し散る
            float spreadAngle = Random.Range(-40f, 40f) * Mathf.Deg2Rad;
            Vector3 floatDir = Quaternion.Euler(0, 0, spreadAngle * Mathf.Rad2Deg) * backward;
            float speed = Random.Range(0.6f, 1.8f);

            // 花弁のランダムな初期角度とスケール
            float initialAngle = Random.Range(0f, 360f);
            petalObj.transform.localRotation = Quaternion.Euler(0, 0, initialAngle);

            // 花弁サイズ（幅 0.45〜0.8、厚み 0.35〜0.6）
            Vector3 initialScale = new Vector3(
                Random.Range(0.40f, 0.75f) * scaleMultiplier,
                Random.Range(0.35f, 0.65f) * scaleMultiplier,
                1f
            );
            petalObj.transform.localScale = initialScale;

            SpriteRenderer sr = petalObj.AddComponent<SpriteRenderer>();
            sr.sprite = cachedPetalSprite;
            if (defaultSpriteMaterial != null) sr.material = defaultSpriteMaterial;

            // 最前面描画
            sr.sortingLayerName = "object";
            sr.sortingOrder = 240;

            // 真紅パレットから選択
            Color petalColor = CamelliaPalette[Random.Range(0, CamelliaPalette.Length)];
            sr.color = petalColor;

            // 自立ランナー
            CamelliaPetalRunner runner = petalObj.AddComponent<CamelliaPetalRunner>();
            float life = Random.Range(0.42f, 0.65f);
            float rotSpeed = Random.Range(-120f, 120f);
            runner.Initialize(floatDir * speed, initialScale, petalColor, life, rotSpeed);
        }
    }

    /// <summary>
    /// 舞い散る椿の花弁チリの自立駆動コンポーネント
    /// </summary>
    public class CamelliaPetalRunner : MonoBehaviour
    {
        private Vector3 velocity;
        private Vector3 initialScale;
        private Color baseColor;
        private float duration;
        private float rotSpeed;
        private float elapsed = 0f;
        private float sinOffset;
        private SpriteRenderer sr;

        public void Initialize(Vector3 startVel, Vector3 startScale, Color color, float lifeDuration, float rotationSpeed)
        {
            velocity = startVel;
            initialScale = startScale;
            baseColor = color;
            duration = Mathf.Max(0.15f, lifeDuration);
            rotSpeed = rotationSpeed;
            sinOffset = Random.Range(0f, Mathf.PI * 2f);
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

            // 風に舞うような横揺れ（Sway）と空気抵抗
            float sway = Mathf.Sin(elapsed * 7.0f + sinOffset) * 0.45f;
            Vector3 driftVel = velocity * Mathf.Clamp01(1f - t * 1.2f);
            driftVel.x += sway * Time.deltaTime;

            transform.position += driftVel * Time.deltaTime;

            // ひらひらと回転
            transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

            // スケールとアルファ減衰
            float fade = 1f - t;
            // 舞い散る途中でわずかに縮む
            transform.localScale = initialScale * (fade * 0.8f + 0.2f);

            if (sr != null)
            {
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * fade);
            }
        }
    }

    /// <summary>
    /// 零落弾（TsubakiObject / TsubakiBullet）にアタッチして飛行中に花弁チリを自動放出するエミッター
    /// </summary>
    public class ProceduralCamelliaTrailEmitter : MonoBehaviour
    {
        private Bullet_Base bullet;
        private float emitTimer = 0f;
        private const float EmitInterval = 0.045f; // 約22回/秒で優雅にチリが舞う

        private void Awake()
        {
            bullet = GetComponent<Bullet_Base>();
        }

        private void Update()
        {
            // 非アクティブまたは滞留待機中（速度ゼロ）のときは放出頻度を抑える
            if (bullet != null && bullet.Speed <= 0.01f)
            {
                emitTimer += Time.deltaTime;
                if (emitTimer >= EmitInterval * 3.5f)
                {
                    emitTimer = 0f;
                    Vector3 bulletDir = bullet.transform.right;
                    ProceduralCamelliaTrail_Alpha.SpawnPetalDust(transform.position, bulletDir, 0.65f);
                }
                return;
            }

            emitTimer += Time.deltaTime;
            if (emitTimer >= EmitInterval)
            {
                emitTimer = 0f;

                Vector3 bulletDir = Vector3.right;
                if (bullet != null)
                {
                    var rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null && rb.velocity.sqrMagnitude > 0.01f)
                    {
                        bulletDir = rb.velocity.normalized;
                    }
                    else
                    {
                        bulletDir = bullet.transform.right;
                    }
                }

                // 弾丸の中心より少し後方からチリを放出
                Vector3 spawnPos = transform.position - bulletDir * 0.15f;
                ProceduralCamelliaTrail_Alpha.SpawnPetalDust(spawnPos, bulletDir, 1.0f);
            }
        }
    }
}
