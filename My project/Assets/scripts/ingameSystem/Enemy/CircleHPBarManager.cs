using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤー用プロシージャル・サークルHPゲージマネージャー。
/// シックでリッチなUIデザイン、深みのある濃密なグリーン系カラーリング（段階付け）、
/// 鋭く強烈な被弾時ホワイトスライバー・フラッシュ、ダメージ遅延追従（Ghost Bar）、
/// 先端発光ビード、低HP時の有機的鼓動パルス（Heartbeat Breath）を完全なC#コーディングで制御・演出する。
/// </summary>
public class CircleHPBarManager : MonoBehaviour
{
    [Header("Circle HP Bar References (Inner:0/Final to Outer:2/Third)")]
    public GameObject[] CircleHPBar;
    private int currentCircleBarCount = 0;

    [System.Serializable]
    public struct TierColorProfile
    {
        public string tierName;
        public Color mainColor;       // メインゲージ色（濃密でリッチな緑）
        public Color ghostColor;      // ダメージ遅延残像バー色
        public Color baseColor;       // 背景リング色（深緑黒の溝）
        public Color flashColor;      // 被弾時ホワイトフラッシュ色（純白）
        public Color beadColor;       // 先端ビード（スライバー発光点）色
    }

    [Header("Tier Color Profiles (Green-themed Chic Palette)")]
    public TierColorProfile[] tierProfiles = new TierColorProfile[]
    {
        // Tier 0 (Final: 最内側・最終層 - ディープ・フォレストグリーン)
        new TierColorProfile
        {
            tierName = "Deep Forest (Tier 1)",
            mainColor = new Color(0.00f, 0.58f, 0.35f, 1.00f),       // 濃密で引き締まったフォレスト
            ghostColor = new Color(0.48f, 0.94f, 0.74f, 0.85f),      // 淡いミント白の残像
            baseColor = new Color(0.01f, 0.11f, 0.07f, 0.65f),       // 深いフォレストブラックの溝
            flashColor = new Color(1.00f, 1.00f, 1.00f, 1.00f),      // 純白フラッシュ
            beadColor = new Color(0.40f, 1.00f, 0.75f, 1.00f)        // 鋭いエメラルドスライバー
        },
        // Tier 1 (Second: 中間・第2層 - ディープ・ジェイドグリーン)
        new TierColorProfile
        {
            tierName = "Deep Jade (Tier 2)",
            mainColor = new Color(0.04f, 0.66f, 0.48f, 1.00f),       // 深みと気品のあるジェイド
            ghostColor = new Color(0.52f, 0.96f, 0.80f, 0.85f),
            baseColor = new Color(0.01f, 0.12f, 0.09f, 0.65f),
            flashColor = new Color(1.00f, 1.00f, 1.00f, 1.00f),
            beadColor = new Color(0.45f, 1.00f, 0.80f, 1.00f)
        },
        // Tier 2 (Third: 最外側・第3層健全 - ディープ・エメラルドグリーン)
        new TierColorProfile
        {
            tierName = "Deep Emerald (Tier 3)",
            mainColor = new Color(0.00f, 0.76f, 0.42f, 1.00f),       // 濃く鮮烈なエメラルド
            ghostColor = new Color(0.58f, 0.98f, 0.82f, 0.85f),
            baseColor = new Color(0.02f, 0.14f, 0.08f, 0.65f),
            flashColor = new Color(1.00f, 1.00f, 1.00f, 1.00f),
            beadColor = new Color(0.55f, 1.00f, 0.85f, 1.00f)
        }
    };

    // 各リングの内部ランタイム制御データ
    private class RingController
    {
        public GameObject rootObj;
        public Image baseImage;
        public Image fillImage;
        public Image ghostImage;
        public RectTransform beadRect;
        public Image beadImage;

        public float targetFill = 1f;
        public float currentFill = 1f;
        public float ghostFill = 1f;
        public float ghostVelocity = 0f;
        public float ghostDelayTimer = 0f;

        public float flashTimer = 0f;
        public const float FlashDuration = 0.22f; // しっかり視認できるフラッシュ持続時間
        public TierColorProfile profile;
        public bool isClockwise = false;
        public int origin = 2; // 0:Bottom, 1:Right, 2:Top, 3:Left
        public float radius = 50f;
    }

    private RingController[] rings;
    private bool isInitialized = false;

    // 先端ビード・スライバー用のプロシージャルスプライト（メモリ上で1度だけ動的生成）
    private static Sprite cachedBeadSprite = null;

    private void Awake()
    {
        InitializeRings();
    }

    private static Sprite GetOrCreateBeadSprite()
    {
        if (cachedBeadSprite != null) return cachedBeadSprite;

        const int sz = 32;
        Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[sz * sz];
        Vector2 c = new Vector2((sz - 1) * 0.5f, (sz - 1) * 0.5f);
        float maxR = sz * 0.48f;

        for (int y = 0; y < sz; y++)
        {
            for (int x = 0; x < sz; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                float normD = Mathf.Clamp01(d / maxR);
                // 中心が高輝度白熱、外周がソフトグローの鋭いスライバードット
                float a = Mathf.Clamp01(1f - normD);
                a = Mathf.Pow(a, 1.6f);
                px[y * sz + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        cachedBeadSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz);
        return cachedBeadSprite;
    }

    private void InitializeRings()
    {
        if (isInitialized) return;
        if (CircleHPBar == null || CircleHPBar.Length == 0) return;

        rings = new RingController[CircleHPBar.Length];
        Sprite beadSprite = GetOrCreateBeadSprite();

        for (int i = 0; i < CircleHPBar.Length; i++)
        {
            GameObject barObj = CircleHPBar[i];
            if (barObj == null) continue;

            RingController rc = new RingController();
            rc.rootObj = barObj;
            rc.profile = (i < tierProfiles.Length) ? tierProfiles[i] : tierProfiles[0];

            // 1. Base (背景リング・溝)
            Transform baseT = barObj.transform.Find("base");
            if (baseT != null)
            {
                rc.baseImage = baseT.GetComponent<Image>();
                if (rc.baseImage != null)
                {
                    rc.baseImage.color = rc.profile.baseColor;
                }
            }

            // 2. Fill (メインゲージ)
            Transform fillT = barObj.transform.Find("fill");
            if (fillT != null)
            {
                rc.fillImage = fillT.GetComponent<Image>();
                if (rc.fillImage != null)
                {
                    // 濃くリッチに発色させるため不透明で鮮明なメインカラーを適用
                    rc.fillImage.color = rc.profile.mainColor;
                    rc.isClockwise = rc.fillImage.fillClockwise;
                    rc.origin = rc.fillImage.fillOrigin;

                    RectTransform fillRect = fillT.GetComponent<RectTransform>();
                    rc.radius = (fillRect != null) ? Mathf.Min(fillRect.rect.width, fillRect.rect.height) * 0.44f : 44f;

                    // 3. Ghost Fill（ダメージ遅延バー）をC#から動的生成（fillの直下に配置）
                    GameObject ghostObj = new GameObject("ghostFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    ghostObj.transform.SetParent(barObj.transform, false);
                    ghostObj.transform.SetSiblingIndex(fillT.GetSiblingIndex());

                    RectTransform ghostRect = ghostObj.GetComponent<RectTransform>();
                    if (fillRect != null)
                    {
                        ghostRect.anchorMin = fillRect.anchorMin;
                        ghostRect.anchorMax = fillRect.anchorMax;
                        ghostRect.anchoredPosition = fillRect.anchoredPosition;
                        ghostRect.sizeDelta = fillRect.sizeDelta;
                        ghostRect.localScale = Vector3.one;
                    }

                    rc.ghostImage = ghostObj.GetComponent<Image>();
                    rc.ghostImage.sprite = rc.fillImage.sprite;
                    rc.ghostImage.type = rc.fillImage.type;
                    rc.ghostImage.fillMethod = rc.fillImage.fillMethod;
                    rc.ghostImage.fillOrigin = rc.fillImage.fillOrigin;
                    rc.ghostImage.fillClockwise = rc.fillImage.fillClockwise;
                    rc.ghostImage.fillAmount = rc.fillImage.fillAmount;
                    rc.ghostImage.color = rc.profile.ghostColor;

                    // 4. 先端光彩ビード・スライバー（Leading Edge Sliver Bead）をC#から動的生成
                    GameObject beadObj = new GameObject("leadingBead", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    beadObj.transform.SetParent(barObj.transform, false);
                    rc.beadRect = beadObj.GetComponent<RectTransform>();
                    rc.beadRect.sizeDelta = new Vector2(18f, 18f);
                    rc.beadImage = beadObj.GetComponent<Image>();
                    rc.beadImage.sprite = beadSprite;
                    rc.beadImage.color = rc.profile.beadColor;
                }
            }

            rings[i] = rc;
        }

        isInitialized = true;
    }

    public void SetCircleBar(int num)
    {
        if (!isInitialized) InitializeRings();

        currentCircleBarCount = num;
        for (int i = 0; i < CircleHPBar.Length; i++)
        {
            if (CircleHPBar[i] != null)
            {
                CircleHPBar[i].SetActive(i < num);
            }
        }
    }

    public void UpdateCircleBar(int targetCircle, float ratio)
    {
        if (!isInitialized) InitializeRings();
        if (targetCircle - 1 < 0 || targetCircle - 1 >= CircleHPBar.Length) return;

        int activeIdx = targetCircle - 1;
        float clampedRatio = Mathf.Clamp01(ratio);

        if (rings != null && activeIdx < rings.Length && rings[activeIdx] != null)
        {
            RingController rc = rings[activeIdx];

            // ダメージを受けた場合の被弾フラッシュ＆ゴースト遅延ディレイ設定
            if (clampedRatio < rc.targetFill)
            {
                rc.flashTimer = RingController.FlashDuration; // 強力なホワイトスライバー・フラッシュ発動 (0.22s)
                rc.ghostDelayTimer = 0.28f;                   // 0.28秒間ゴーストバーが静止
            }

            rc.targetFill = clampedRatio;
        }

        // 跨いだ上のゲージ（外側などすでに削れた層）を空にする
        for (int i = targetCircle; i < currentCircleBarCount; i++)
        {
            if (rings != null && i < rings.Length && rings[i] != null)
            {
                rings[i].targetFill = 0f;
            }
        }

        // 跨いだ下のゲージ（まだ到達していない内側の満タン層）を満タンにする
        for (int i = 0; i < activeIdx; i++)
        {
            if (rings != null && i < rings.Length && rings[i] != null)
            {
                rings[i].targetFill = 1f;
            }
        }
    }

    void Update()
    {
        if (rings == null) return;

        float dt = Time.deltaTime;

        for (int i = 0; i < rings.Length; i++)
        {
            RingController rc = rings[i];
            if (rc == null || rc.rootObj == null || !rc.rootObj.activeInHierarchy) continue;

            // --- 1. メインゲージのしなやかな追従 ---
            rc.currentFill = Mathf.MoveTowards(rc.currentFill, rc.targetFill, dt * 3.5f);
            if (rc.fillImage != null)
            {
                rc.fillImage.fillAmount = rc.currentFill;
            }

            // --- 2. ゴーストバー（遅延残像）のイージング追従 ---
            if (rc.ghostDelayTimer > 0f)
            {
                rc.ghostDelayTimer -= dt;
            }
            else
            {
                // ゴーストバーがメインゲージへ滑らかに追いつく
                rc.ghostFill = Mathf.SmoothDamp(rc.ghostFill, rc.currentFill, ref rc.ghostVelocity, 0.22f);
                if (rc.ghostImage != null)
                {
                    rc.ghostImage.fillAmount = rc.ghostFill;
                }
            }

            // --- 3. 被弾ホワイトスライバー・フラッシュの計算（強化版） ---
            float flashIntensity = 0f;
            if (rc.flashTimer > 0f)
            {
                rc.flashTimer -= dt;
                // 最初の0.05秒は純白ピークを保持し、その後二次曲線でスーッと減衰
                float elapsed = RingController.FlashDuration - rc.flashTimer;
                if (elapsed <= 0.05f)
                {
                    flashIntensity = 1.0f;
                }
                else
                {
                    float fadeT = Mathf.Clamp01(rc.flashTimer / (RingController.FlashDuration - 0.05f));
                    flashIntensity = fadeT * fadeT; // 自然な指数減衰
                }
            }

            // --- 4. メインゲージの色制御 ---
            if (rc.fillImage != null)
            {
                Color baseCol = rc.profile.mainColor;

                // 瀕死時（最終層かつHP35%以下）の有機的鼓動パルス (Heartbeat Pulse)
                // 普段は統一感ある緑、極限ピンチ時のみ心拍に合わせて微かな紅が息づく
                if (i == 0 && rc.currentFill <= 0.35f && rc.currentFill > 0f)
                {
                    float pulseSpeed = Mathf.Lerp(8.0f, 4.5f, rc.currentFill / 0.35f);
                    float pulse = Mathf.Sin(Time.time * pulseSpeed);
                    float breath = 0.5f + 0.5f * (pulse * pulse); // 0〜1の心拍波形
                    baseCol = Color.Lerp(rc.profile.mainColor, new Color(0.95f, 0.22f, 0.35f, 1f), breath * 0.40f);
                }

                // ホワイトフラッシュのブレンド（ピーク時は鮮烈な純白！）
                if (flashIntensity > 0f)
                {
                    rc.fillImage.color = Color.Lerp(baseCol, Color.white, flashIntensity);
                }
                else
                {
                    rc.fillImage.color = baseCol;
                }
            }

            // --- 5. 先端光彩スライバー・ビードの追従とフラッシュ演出 ---
            if (rc.beadRect != null && rc.beadImage != null)
            {
                if (rc.currentFill <= 0.005f || rc.currentFill >= 0.995f)
                {
                    rc.beadImage.enabled = false;
                }
                else
                {
                    rc.beadImage.enabled = true;
                    // 円周角度の計算
                    float angle = CalculateFillAngle(rc.currentFill, rc.origin, rc.isClockwise);
                    float rad = angle * Mathf.Deg2Rad;
                    Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * rc.radius;
                    rc.beadRect.anchoredPosition = pos;

                    // 被弾フラッシュ時はスライバーがバッと大きく（最大2.5倍）鋭く光り輝く
                    float scaleMultiplier = 1.0f + flashIntensity * 1.5f;
                    rc.beadRect.localScale = Vector3.one * scaleMultiplier;

                    // 微細なオーガニック揺らぎ＋純白フラッシュ
                    float shimmer = 0.88f + 0.12f * Mathf.Sin(Time.time * 6f + i);
                    Color normalBeadCol = rc.profile.beadColor * shimmer;
                    rc.beadImage.color = Color.Lerp(normalBeadCol, Color.white, flashIntensity);
                }
            }
        }
    }

    /// <summary>
    /// Radial 360 Fill の fillAmount から先端の円周角度（度数法）を算出
    /// </summary>
    private float CalculateFillAngle(float fill, int origin, bool clockwise)
    {
        // Image.FillOrigin (0:Bottom, 1:Right, 2:Top, 3:Left)
        float startAngle = 90f; // default: Top
        switch (origin)
        {
            case 0: startAngle = 270f; break; // Bottom
            case 1: startAngle = 0f; break;   // Right
            case 2: startAngle = 90f; break;  // Top
            case 3: startAngle = 180f; break; // Left
        }

        float delta = fill * 360f;
        return clockwise ? (startAngle - delta) : (startAngle + delta);
    }
}
