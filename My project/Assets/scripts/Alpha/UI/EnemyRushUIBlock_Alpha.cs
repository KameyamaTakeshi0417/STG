using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    public class EnemyRushUIBlock_Alpha : MonoBehaviour
    {
        public Image innerGauge;
        private float nextSpawnTime;
        private float spawnInterval;

        public void Setup(float interval, float initialNextTime)
        {
            spawnInterval = interval;
            nextSpawnTime = initialNextTime;
            
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
        }

        public void UpdateGauge(float currentTime)
        {
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
