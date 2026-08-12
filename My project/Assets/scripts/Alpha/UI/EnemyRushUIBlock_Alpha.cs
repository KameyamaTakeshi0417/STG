using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    public class EnemyRushUIBlock_Alpha : MonoBehaviour
    {
        public Image innerGauge;
        public Text timeText;
        private float nextSpawnTime;
        private float spawnInterval;
        private float rushStartTime;
        private float rushEndTime;

        public void Setup(float interval, float initialNextTime, float start, float end)
        {
            spawnInterval = interval;
            nextSpawnTime = initialNextTime;
            rushStartTime = start;
            rushEndTime = end;
            
            // 背景（紫色のブロック）のImageがある想定。ゲージ用のImageを作成
            if (innerGauge == null)
            {
                GameObject innerObj = new GameObject("InnerGauge");
                innerObj.transform.SetParent(this.transform, false);
                
                RectTransform rt = innerObj.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                
                innerGauge = innerObj.AddComponent<Image>();
                innerGauge.color = new Color(1f, 1f, 1f, 0.5f); // 半透明の白
                innerGauge.type = Image.Type.Filled;
                innerGauge.fillMethod = Image.FillMethod.Horizontal;
                innerGauge.fillOrigin = (int)Image.OriginHorizontal.Left;
                innerGauge.fillAmount = 0f;
            }

            if (timeText == null)
            {
                GameObject textObj = new GameObject("TimeText");
                textObj.transform.SetParent(this.transform, false);
                
                RectTransform rt = textObj.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                
                timeText = textObj.AddComponent<Text>();
                timeText.alignment = TextAnchor.MiddleCenter;
                timeText.color = Color.white;
                timeText.fontSize = 20;
                timeText.horizontalOverflow = HorizontalWrapMode.Overflow;
                timeText.verticalOverflow = VerticalWrapMode.Overflow;
                
                Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (legacyFont == null)
                {
                    legacyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                timeText.font = legacyFont;
                
                // 初期の全体時間を表示
                timeText.text = (rushEndTime - rushStartTime).ToString("F1") + "s";
            }
        }

        public void UpdateGauge(float currentTime)
        {
            // 時間テキストの更新
            if (timeText != null)
            {
                if (currentTime < rushStartTime)
                {
                    timeText.text = (rushEndTime - rushStartTime).ToString("F1") + "s";
                }
                else if (currentTime >= rushStartTime && currentTime <= rushEndTime)
                {
                    float remaining = rushEndTime - currentTime;
                    timeText.text = remaining.ToString("F1") + "s";
                }
                else
                {
                    timeText.text = "0.0s";
                }
            }

            if (spawnInterval <= 0) return;
            
            // currentTime が nextSpawnTime を超えたら次のタイミングへ
            while (currentTime >= nextSpawnTime)
            {
                nextSpawnTime += spawnInterval;
            }

            // 0 -> 1 への進行度
            float timeSinceLastSpawn = spawnInterval - (nextSpawnTime - currentTime);
            float fillAmount = Mathf.Clamp01(timeSinceLastSpawn / spawnInterval);

            if (innerGauge != null)
            {
                innerGauge.fillAmount = fillAmount;
            }
        }
    }
}
