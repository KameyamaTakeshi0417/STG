using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Alpha.Data;
using DG.Tweening;

namespace Alpha.UI
{
    public class SequenceBarUI_Alpha : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("進行率を表示するスライダー")]
        public Slider progressSlider;
        [Tooltip("マーカーを配置する親オブジェクト（スライダーの背景部分など）")]
        public RectTransform markerContainer;

        [Header("Prefabs")]
        [Tooltip("マーカー用プレハブ（通常用）")]
        public GameObject normalMarkerPrefab;
        [Tooltip("マーカー用プレハブ（エリート用）")]
        public GameObject eliteMarkerPrefab;
        [Tooltip("マーカー用プレハブ（ボス用）")]
        public GameObject bossMarkerPrefab;
        [Tooltip("ラッシュ期間表示用のプレハブ（未設定の場合は動的生成）")]
        public GameObject rushBlockPrefab;

        private List<GameObject> activeMarkers = new List<GameObject>();
        private List<EnemyRushUIBlock_Alpha> activeRushBlocks = new List<EnemyRushUIBlock_Alpha>();
        private float currentSequenceDuration = 1f;

        [Header("Reward Gauge UI")]
        [Tooltip("報酬ゲージの中身（Image TypeがFilledのもの）")]
        public Image rewardGaugeImage;
        [Tooltip("報酬ゲージのテキスト（0/100などを表示するもの）")]
        public TMPro.TextMeshProUGUI rewardGaugeText;

        [Header("Timeline Skip Animation Settings")]
        [Tooltip("スキップ移動中に発生する波紋の色（真鍮ゴールド）")]
        public Color rippleColor = new Color(0.85f, 0.72f, 0.40f, 0.85f);
        [Tooltip("波紋が発生する間隔（秒）")]
        public float rippleInterval = 0.08f;
        [Tooltip("波紋の最大拡大スケール")]
        public float rippleExpandScale = 2.4f;
        [Tooltip("波紋が消えるまでの時間（秒）")]
        public float rippleDuration = 0.45f;
        [Tooltip("スキップ完了時のグミグミ弾性振動の継続時間（秒）")]
        public float jellyDuration = 0.55f;
        [Tooltip("スキップ完了時のグミ弾性パンチベクトル（X正:横伸び, Y負:縦縮み）")]
        public Vector3 jellyPunchVector = new Vector3(0.48f, -0.38f, 0f);
        [Tooltip("グミ弾性振動の振動回数")]
        public int jellyVibrato = 10;
        [Tooltip("グミ弾性振動の減衰率（1で減衰なし）")]
        public float jellyElasticity = 0.75f;
        [Tooltip("スキップ完了時の着地音（未設定時は Resources/SE/SE_GetExp_02_Ting が使用されます）")]
        public AudioClip landingSEClip;
        [Range(0f, 1f)] public float landingSEVolume = 0.4f;

        private RectTransform handleRect;
        private Vector3 initialHandleScale = Vector3.one;
        private bool hasCachedHandle = false;
        private static Sprite cachedRippleSprite;

        public static SequenceBarUI_Alpha Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 木や葉っぱなどのオブジェクト(SortingLayer: object)より手前に確実に表示する
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = "SystemUI";
                canvas.sortingOrder = 500;
            }
        }

        /// <summary>
        /// シーケンス開始時にマーカーを配置してUIを再構成する
        /// </summary>
        /// <summary>
        /// タイムラインバー（スライダーおよびマーカー群）の表示・非表示を切り替える
        /// </summary>
        public void SetBarVisible(bool visible)
        {
            if (progressSlider != null) progressSlider.gameObject.SetActive(visible);
            if (markerContainer != null) markerContainer.gameObject.SetActive(visible);
        }

        public void Setup(StageSequenceData_Alpha sequence)
        {
            if (sequence == null) return;

            // 既存のマーカーをクリア
            foreach (var marker in activeMarkers)
            {
                Destroy(marker);
            }
            activeMarkers.Clear();
            activeRushBlocks.Clear();

            currentSequenceDuration = sequence.duration > 0f ? sequence.duration : 1f;

            // スライダーの最大値を1に強制する
            if (progressSlider != null)
            {
                progressSlider.maxValue = 1f;
                progressSlider.minValue = 0f;
                progressSlider.value = 0f;
            }

            // ラッシュ用ブロックの配置
            if (sequence.enemyRushes != null)
            {
                foreach (var rush in sequence.enemyRushes)
                {
                    GameObject blockObj = null;
                    if (rushBlockPrefab != null)
                    {
                        blockObj = Instantiate(rushBlockPrefab, markerContainer);
                    }
                    else
                    {
                        blockObj = new GameObject("RushBlock");
                        blockObj.transform.SetParent(markerContainer, false);
                        Image img = blockObj.AddComponent<Image>();
                        img.color = new Color(0.72f, 0.22f, 0.24f, 0.55f); // 椿の深紅（ラッシュ警告帯） // 半透明の紫
                    }
                    
                    RectTransform rt = blockObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        float startProgress = rush.startTime / currentSequenceDuration;
                        float endProgress = rush.endTime / currentSequenceDuration;
                        rt.anchorMin = new Vector2(startProgress, 0f);
                        rt.anchorMax = new Vector2(endProgress, 1f);
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                    }

                    EnemyRushUIBlock_Alpha rushUI = blockObj.GetComponent<EnemyRushUIBlock_Alpha>();
                    if (rushUI == null) rushUI = blockObj.AddComponent<EnemyRushUIBlock_Alpha>();
                    
                    rushUI.Setup(rush.spawnInterval, rush.startTime, rush.startTime, rush.endTime);
                    activeRushBlocks.Add(rushUI);
                    activeMarkers.Add(blockObj);
                }
            }

            // 新しいマーカーを配置
            foreach (var waveData in sequence.waves)
            {
                GameObject prefabToUse = GetPrefabForType(waveData.markerType);
                if (prefabToUse != null)
                {
                    GameObject markerObj = Instantiate(prefabToUse, markerContainer);
                    activeMarkers.Add(markerObj);

                    RectTransform rt = markerObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        float progress = waveData.time / sequence.duration;
                        rt.anchorMin = new Vector2(progress, 0.5f);
                        rt.anchorMax = new Vector2(progress, 0.5f);
                        rt.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        public void UpdateRewardGauge(int currentPoints, int targetPoints, int targetQuality)
        {
            if (rewardGaugeImage != null)
            {
                rewardGaugeImage.fillAmount = targetPoints > 0 ? (float)currentPoints / targetPoints : 0f;
                switch (targetQuality)
                {
                    case 1: rewardGaugeImage.color = Color.white; break;
                    case 2: rewardGaugeImage.color = Color.green; break;
                    case 3: rewardGaugeImage.color = Color.blue; break;
                    case 4: rewardGaugeImage.color = new Color(1f, 0.5f, 0f); break; // Orange for Epic
                    default: rewardGaugeImage.color = Color.white; break;
                }
            }
            if (rewardGaugeText != null)
            {
                rewardGaugeText.text = $"{currentPoints}/{targetPoints}";
            }
        }

        public void UpdateProgress(float normalizedProgress)
        {
            progressSlider.value = Mathf.Clamp01(normalizedProgress);
            
            float currentTime = normalizedProgress * currentSequenceDuration;
            foreach (var block in activeRushBlocks)
            {
                if (block != null)
                {
                    block.UpdateGauge(currentTime);
                }
            }
        }

        public System.Collections.IEnumerator AnimateProgress(float startVal, float endVal, float duration)
        {
            CacheHandleIfNeeded();

            // スキップ開始時：進行方向（右）へ少し引き伸ばす（流線型ストレッチ）
            if (handleRect != null)
            {
                handleRect.DOKill();
                handleRect.DOScale(new Vector3(initialHandleScale.x * 1.25f, initialHandleScale.y * 0.82f, initialHandleScale.z), 0.12f)
                          .SetEase(Ease.OutQuad)
                          .SetUpdate(true);
            }

            float elapsed = 0f;
            float lastRippleTime = -1f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                UpdateProgress(Mathf.Lerp(startVal, endVal, t));

                // 周期的に波紋（Ripple）オブジェクトを発生
                if (elapsed - lastRippleTime >= rippleInterval)
                {
                    lastRippleTime = elapsed;
                    SpawnRipple(rippleColor, rippleExpandScale, rippleDuration);
                }

                yield return null;
            }

            UpdateProgress(endVal);

            // スキップ完了時：着地バウンス（グミグミ弾性）演出
            PlaySkipLandingAnimation();
        }

        private void CacheHandleIfNeeded()
        {
            if (hasCachedHandle && handleRect != null) return;

            if (progressSlider != null && progressSlider.handleRect != null)
            {
                handleRect = progressSlider.handleRect;
            }
            else if (progressSlider != null && progressSlider.targetGraphic != null)
            {
                handleRect = progressSlider.targetGraphic.rectTransform;
            }

            if (handleRect != null)
            {
                initialHandleScale = handleRect.localScale.sqrMagnitude > 0.001f ? handleRect.localScale : Vector3.one;
                hasCachedHandle = true;
            }
        }

        /// <summary>
        /// スキップ完了時の着地演出：グミグミ弾性振動＋着地バースト波紋＋光パルス
        /// </summary>
        private void PlaySkipLandingAnimation()
        {
            CacheHandleIfNeeded();
            if (handleRect == null) return;

            handleRect.DOKill();

            // 1. 着地時の慣性で一瞬グニュッと横に潰れ、直後にプルプルと減衰振動（体積一定のグミ振動）
            handleRect.localScale = initialHandleScale;
            handleRect.DOPunchScale(jellyPunchVector, jellyDuration, jellyVibrato, jellyElasticity)
                      .SetUpdate(true)
                      .OnComplete(() =>
                      {
                          if (handleRect != null) handleRect.localScale = initialHandleScale;
                      });

            // 2. 着地バースト波紋（Handleの中心から少し大きめの黄金波紋がフワッと外へ広がる）
            SpawnRipple(new Color(0.95f, 0.88f, 0.58f, 0.95f), 3.2f, 0.55f);

            // 3. 着地パルス（Handleの画像をほんの一瞬真鍮ゴールドに光らせて落ち着く）
            Image handleImg = handleRect.GetComponent<Image>() ?? handleRect.GetComponentInChildren<Image>();
            if (handleImg != null)
            {
                Color originalColor = handleImg.color;
                handleImg.DOKill();
                handleImg.DOColor(new Color(1f, 0.94f, 0.68f, 1f), 0.08f)
                         .SetUpdate(true)
                         .OnComplete(() =>
                         {
                             if (handleImg != null) handleImg.DOColor(originalColor, 0.35f).SetUpdate(true);
                         });
            }

            // 4. 着地アクセントSE
            PlayLandingSE();
        }

        /// <summary>
        /// スキップ中・着地時の波紋リングオブジェクトを生成・アニメーションさせる
        /// </summary>
        private void SpawnRipple(Color col, float targetScale, float dur)
        {
            CacheHandleIfNeeded();
            if (handleRect == null || handleRect.parent == null) return;

            GameObject rippleObj = new GameObject("TimelineSkipRipple", typeof(RectTransform));
            rippleObj.transform.SetParent(handleRect.parent, false);

            RectTransform rt = rippleObj.GetComponent<RectTransform>();
            rt.anchorMin = handleRect.anchorMin;
            rt.anchorMax = handleRect.anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = handleRect.anchoredPosition;
            rt.sizeDelta = new Vector2(40f, 40f);

            Image img = rippleObj.AddComponent<Image>();
            img.sprite = GetRippleSprite();
            img.color = col;
            img.raycastTarget = false;

            // アニメーション：スケール拡大＋フェードアウトで自動削除
            rt.localScale = Vector3.one * 0.5f;
            rt.DOScale(Vector3.one * targetScale, dur).SetEase(Ease.OutQuad).SetUpdate(true);
            img.DOFade(0f, dur).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
            {
                if (rippleObj != null) Destroy(rippleObj);
            });
        }

        /// <summary>
        /// 繊細なアンチエイリアスリングスプライト（実行時動的生成・キャッシュ）
        /// </summary>
        private static Sprite GetRippleSprite()
        {
            if (cachedRippleSprite != null) return cachedRippleSprite;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerRadius = size / 2f - 2f;
            float innerRadius = outerRadius - 4.5f; // 約4.5px幅の繊細なリング

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = 0f;
                    if (dist <= outerRadius + 1f && dist >= innerRadius - 1f)
                    {
                        float dOuter = outerRadius - dist;
                        float dInner = dist - innerRadius;
                        float edge = Mathf.Min(dOuter, dInner);
                        alpha = Mathf.Clamp01(edge + 0.5f);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            cachedRippleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return cachedRippleSprite;
        }

        private void PlayLandingSE()
        {
            if (landingSEClip == null)
            {
                landingSEClip = Resources.Load<AudioClip>("SE/SE_GetExp_02_Ting");
                if (landingSEClip == null) landingSEClip = Resources.Load<AudioClip>("SE/決定音_1");
            }

            if (landingSEClip != null && Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(landingSEClip, landingSEVolume);
            }
        }

        private GameObject GetPrefabForType(MarkerType_Alpha type)
        {
            switch (type)
            {
                case MarkerType_Alpha.Elite:
                    return eliteMarkerPrefab;
                case MarkerType_Alpha.MidBoss:
                case MarkerType_Alpha.Boss:
                    return bossMarkerPrefab;
                case MarkerType_Alpha.Normal:
                default:
                    return normalMarkerPrefab;
            }
        }
    }
}
