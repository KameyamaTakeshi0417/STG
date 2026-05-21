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
        // その他イベント用なども適宜追加

        private List<GameObject> activeMarkers = new List<GameObject>();

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

            progressSlider.value = 0f;

            // 新しいマーカーを配置
            foreach (var markerData in sequence.markers)
            {
                GameObject prefabToUse = GetPrefabForType(markerData.markerType);
                if (prefabToUse != null)
                {
                    GameObject markerObj = Instantiate(prefabToUse, markerContainer);
                    activeMarkers.Add(markerObj);

                    RectTransform rt = markerObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        // AnchorをLeftに設定し、進行率(0〜1)に応じてX座標をずらす想定
                        // またはアンカー(min.x, max.x)自体をいじる方法もある。
                        // 今回は anchorMin = anchorMax = (progress, 0.5f) とする方法を採用
                        float progress = markerData.time / sequence.duration;
                        rt.anchorMin = new Vector2(progress, 0.5f);
                        rt.anchorMax = new Vector2(progress, 0.5f);
                        rt.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        public void UpdateProgress(float normalizedProgress)
        {
            progressSlider.value = Mathf.Clamp01(normalizedProgress);
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
