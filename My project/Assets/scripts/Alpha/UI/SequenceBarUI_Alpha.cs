using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Alpha.Data;

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

        public static SequenceBarUI_Alpha Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// シーケンス開始時にマーカーを配置してUIを再構成する
        /// </summary>
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
                        img.color = new Color(0.5f, 0f, 0.5f, 0.5f); // 半透明の紫
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
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                UpdateProgress(Mathf.Lerp(startVal, endVal, elapsed / duration));
                yield return null;
            }
            UpdateProgress(endVal);
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
