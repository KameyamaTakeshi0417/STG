using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 爆発弾（ExplosionBullet）等の爆発エリア・プロシージャル爆風エフェクト制御。
/// 外部スプライトアセットに依存せず、C#コードによるプロシージャルテクスチャ生成と動的アニメーションで
/// 熱風・白熱火炎コア・衝撃波リング・火花スパークを描画・演出する。
/// 当たり判定コライダー範囲（ワールド半径 1.5）およびダメージ判定仕様を厳密に維持。
/// </summary>
public class Effect_Explosion : MonoBehaviour, IAlphaPoolable
{
    public GameObject sourcePrefab; // プール返却用の参照

    [Header("Damage Settings")]
    [SerializeField] private float dmg = 30f;
    public int explosionTime = 30; // 互換用パラメータ
    [SerializeField] private int damagedCount = 3;

    [Header("Procedural Visual Settings")]
    [SerializeField] private float animDuration = 0.38f;

    private bool isExploding = false;
    private float animTimer = 0f;

    // プロシージャル生成された共有スプライト（メモリ上で1度だけ動的生成・キャッシュ）
    private static Sprite cachedCoreSprite = null;
    private static Sprite cachedRingSprite = null;
    private static Sprite cachedSparkSprite = null;
    private static Material defaultSpriteMaterial = null;

    // 動的ビジュアル用コンポーネント
    private SpriteRenderer coreRenderer;
    private GameObject ringObj;
    private SpriteRenderer ringRenderer;
    private List<SparkParticle> sparks = new List<SparkParticle>();

    private struct SparkParticle
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public Vector3 velocity;
        public float baseScale;
    }

    private void Awake()
    {
        EnsureProceduralAssets();
        SetupComponents();
    }

    /// <summary>
    /// プロシージャルスプライト（白熱コア、衝撃波リング、火花）をC#コードからメモリ上で動的生成
    /// </summary>
    private static void EnsureProceduralAssets()
    {
        if (defaultSpriteMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) defaultSpriteMaterial = new Material(shader);
        }

        // 1. 白熱火炎コア (64x64): 中心が濃密で外周が滑らかに減衰するソフトラジアルグロー
        if (cachedCoreSprite == null)
        {
            const int sz = 64;
            Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] colors = new Color[sz * sz];
            Vector2 center = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.5f);
            float maxR = sz * 0.5f;

            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normDist = Mathf.Clamp01(dist / maxR);
                    // Smoothstep イージングによる滑らかなグラデーション
                    float alpha = Mathf.Clamp01(1f - normDist);
                    alpha = alpha * alpha * (3f - 2f * alpha);
                    colors[y * sz + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            cachedCoreSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
        }

        // 2. 鋭い衝撃波リング (128x128): 外周へ広がるアンチエイリアス円環
        if (cachedRingSprite == null)
        {
            const int sz = 128;
            Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] colors = new Color[sz * sz];
            Vector2 center = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.5f);
            float radius = sz * 0.44f;
            float thickness = 4.5f;

            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float diff = Mathf.Abs(dist - radius);
                    float alpha = Mathf.Clamp01(1f - diff / (thickness * 0.5f));
                    // わずかに内側にソフトグローを持たせる
                    if (dist < radius)
                    {
                        float innerGlow = Mathf.Clamp01(1f - (radius - dist) / (sz * 0.15f)) * 0.25f;
                        alpha = Mathf.Max(alpha, innerGlow);
                    }
                    colors[y * sz + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
        }

        // 3. 火花・破片 (32x32): シャープなダイヤ型光条
        if (cachedSparkSprite == null)
        {
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
                    float dx = Mathf.Abs(x - center.x) / maxR;
                    float dy = Mathf.Abs(y - center.y) / maxR;
                    float d = dx + dy;
                    float alpha = Mathf.Clamp01(1f - d);
                    colors[y * sz + x] = new Color(1f, 1f, 1f, alpha * alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            cachedSparkSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
        }
    }

    private void SetupComponents()
    {
        coreRenderer = GetComponent<SpriteRenderer>();
        if (coreRenderer == null) coreRenderer = gameObject.AddComponent<SpriteRenderer>();

        coreRenderer.sprite = cachedCoreSprite;
        if (defaultSpriteMaterial != null) coreRenderer.material = defaultSpriteMaterial;
        SetSorting(coreRenderer, 200);
        
        // startExplosion が呼ばれるまでは非表示（重要）
        coreRenderer.enabled = false;
    }

    private void SetSorting(SpriteRenderer sr, int order)
    {
        if (sr == null) return;
        int layerId = SortingLayer.NameToID("inGameUI");
        if (layerId != 0)
        {
            sr.sortingLayerName = "inGameUI";
        }
        else
        {
            sr.sortingLayerName = "object";
        }
        sr.sortingOrder = order;
    }

    // ★重要: Start() で勝手に爆発を開始しない！
    // ExplosionBullet.prefab にアタッチされている場合、弾丸が飛行中に勝手に消滅する事故を防ぐ
    void Start()
    {
    }

    public void OnRentFromPool()
    {
        damagedCount = 3;
        isExploding = false;
        animTimer = 0f;
        CleanupDynamicObjects();
        if (coreRenderer != null) coreRenderer.enabled = false;
    }

    public void OnReturnToPool()
    {
        isExploding = false;
        animTimer = 0f;
        CleanupDynamicObjects();
        if (coreRenderer != null) coreRenderer.enabled = false;
    }

    public void startExplosion(float setdmg, int setExplosionTime)
    {
        dmg = setdmg;
        explosionTime = setExplosionTime;
        damagedCount = 3;
        animTimer = 0f;
        isExploding = true;

        EnsureProceduralAssets();
        SetupComponents();
        StartProceduralExplosion();
    }

    private void StartProceduralExplosion()
    {
        CleanupDynamicObjects();

        // 1. コアの表示開始（初期状態: 白熱）
        if (coreRenderer != null)
        {
            coreRenderer.enabled = true;
            coreRenderer.color = Color.white;
        }

        // 2. 衝撃波リングを動的生成（コードから配置）
        ringObj = new GameObject("ProceduralShockwave");
        ringObj.transform.SetParent(this.transform, false);
        ringObj.transform.localPosition = Vector3.zero;
        ringObj.transform.localScale = Vector3.zero;
        ringRenderer = ringObj.AddComponent<SpriteRenderer>();
        ringRenderer.sprite = cachedRingSprite;
        if (defaultSpriteMaterial != null) ringRenderer.material = defaultSpriteMaterial;
        SetSorting(ringRenderer, 210);

        // 3. 放射火花（Sparks）をコードから動的生成（8本）
        sparks.Clear();
        int sparkCount = 8;
        for (int i = 0; i < sparkCount; i++)
        {
            GameObject sparkObj = new GameObject("ProceduralSpark_" + i);
            sparkObj.transform.SetParent(this.transform, false);
            sparkObj.transform.localPosition = Vector3.zero;

            float angle = (i * (360f / sparkCount) + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            float speed = Random.Range(1.8f, 3.2f);

            sparkObj.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            sparkObj.transform.localScale = new Vector3(Random.Range(0.6f, 1.2f), Random.Range(0.2f, 0.4f), 1f);

            SpriteRenderer sr = sparkObj.AddComponent<SpriteRenderer>();
            sr.sprite = cachedSparkSprite;
            if (defaultSpriteMaterial != null) sr.material = defaultSpriteMaterial;
            sr.color = new Color(1f, 0.92f, 0.6f, 1f);
            SetSorting(sr, 220);

            sparks.Add(new SparkParticle
            {
                obj = sparkObj,
                sr = sr,
                velocity = dir * speed,
                baseScale = Random.Range(0.5f, 1.0f)
            });
        }

        // 4. 画面揺れ連携（爽快感向上）
        if (Alpha.Core.JuiceManager_Alpha.Instance != null)
        {
            Alpha.Core.JuiceManager_Alpha.Instance.ScreenShake(0.08f, 0.18f);
        }

        // 初期フレームの更新（ゼロ状態のチラつき防止）
        UpdateProceduralAnimation(0.001f);
    }

    void Update()
    {
        if (!isExploding) return;

        animTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animTimer / animDuration);

        UpdateProceduralAnimation(progress);

        if (progress >= 1.0f)
        {
            FinishAndRecycle();
        }
    }

    /// <summary>
    /// プロシージャル演出のイージング更新
    /// 注意: Root Transform の localScale は当たり判定基準 (3, 3, 1) なので一切触らない！
    /// </summary>
    private void UpdateProceduralAnimation(float t)
    {
        // --- 1. 白熱・火炎コア ---
        if (coreRenderer != null)
        {
            // コアの色変化: 白熱(0.0) -> 黄金(0.2) -> 朱赤(0.5) -> 灰赤・煙(1.0)
            Color coreCol;
            if (t < 0.2f)
            {
                float subT = t / 0.2f;
                coreCol = Color.Lerp(Color.white, new Color(1f, 0.88f, 0.35f, 1f), subT);
            }
            else
            {
                float subT = (t - 0.2f) / 0.8f;
                Color midCol = new Color(0.95f, 0.38f, 0.12f, 1f);
                Color endCol = new Color(0.35f, 0.15f, 0.1f, 0f);
                coreCol = Color.Lerp(midCol, endCol, subT);
            }
            coreRenderer.color = coreCol;
        }

        // --- 2. 衝撃波リング ---
        if (ringObj != null && ringRenderer != null)
        {
            // 外周へ急速展開 (0 -> 1.15)
            float waveT = Mathf.Clamp01(t / 0.65f);
            float waveScale = Mathf.Sqrt(waveT) * 1.15f;
            ringObj.transform.localScale = Vector3.one * waveScale;

            float waveAlpha = Mathf.Lerp(0.95f, 0f, waveT * waveT);
            ringRenderer.color = new Color(1f, 0.95f, 0.8f, waveAlpha);
        }

        // --- 3. 飛び散る火花 ---
        for (int i = 0; i < sparks.Count; i++)
        {
            var p = sparks[i];
            if (p.obj != null && p.sr != null)
            {
                // 移動（外側へ初速がありつつ空気抵抗で減速）
                float drag = Mathf.Clamp01(1f - t * 1.5f);
                p.obj.transform.localPosition += p.velocity * (Time.deltaTime * drag);

                // スケールとアルファ減衰
                float sparkAlpha = Mathf.Clamp01(1f - t * 1.3f);
                p.sr.color = new Color(1f, 0.85f, 0.45f, sparkAlpha);
                p.obj.transform.localScale = Vector3.one * (p.baseScale * sparkAlpha);
            }
        }
    }

    private void CleanupDynamicObjects()
    {
        if (ringObj != null)
        {
            Destroy(ringObj);
            ringObj = null;
            ringRenderer = null;
        }

        for (int i = 0; i < sparks.Count; i++)
        {
            if (sparks[i].obj != null)
            {
                Destroy(sparks[i].obj);
            }
        }
        sparks.Clear();
    }

    private void FinishAndRecycle()
    {
        isExploding = false;
        CleanupDynamicObjects();

        if (coreRenderer != null) coreRenderer.enabled = false;

        if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
        {
            Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (damagedCount > 0)
            {
                _Health_Base health = collision.gameObject.GetComponent<_Health_Base>();
                if (health != null)
                {
                    health.ApplyDamage(dmg);
                }
                damagedCount -= 1;
            }
        }
    }
}
