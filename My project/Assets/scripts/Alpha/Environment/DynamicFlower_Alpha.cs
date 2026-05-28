using System.Collections;
using UnityEngine;

namespace Alpha.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DynamicFlower_Alpha : MonoBehaviour
    {
        private SpriteRenderer sr;
        private DynamicFlowerManager_Alpha.FlowerData data;
        private float lifespan;
        private float fps;

        public void Initialize(DynamicFlowerManager_Alpha.FlowerData flowerData, float flowerLifespan)
        {
            sr = GetComponent<SpriteRenderer>();
            data = flowerData;
            lifespan = flowerLifespan;
            fps = data.animationFPS > 0 ? data.animationFPS : 12f;

            // 初期スプライトを設定（空の場合はエラー回避）
            if (data.spawnSprites != null && data.spawnSprites.Length > 0)
            {
                sr.sprite = data.spawnSprites[0];
            }
            else if (data.idleSprite != null)
            {
                sr.sprite = data.idleSprite;
            }

            // Y座標ベースのソート
            sr.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);

            StartCoroutine(FlowerLifeCycleCoroutine());
        }

        private IEnumerator FlowerLifeCycleCoroutine()
        {
            float delay = 1f / fps;

            // 1. Spawn（出現アニメーション）
            if (data.spawnSprites != null && data.spawnSprites.Length > 0)
            {
                for (int i = 0; i < data.spawnSprites.Length; i++)
                {
                    sr.sprite = data.spawnSprites[i];
                    yield return new WaitForSeconds(delay);
                }
            }

            // 2. Idle（待機）
            if (data.idleSprite != null)
            {
                sr.sprite = data.idleSprite;
            }

            // 寿命まで待機
            // 出現アニメーションにかかった時間を差し引いて待機する
            float timeSpent = (data.spawnSprites != null ? data.spawnSprites.Length : 0) * delay;
            float waitTime = Mathf.Max(0f, lifespan - timeSpent);
            
            yield return new WaitForSeconds(waitTime);

            // 3. Despawn（消滅アニメーション）
            if (data.despawnSprites != null && data.despawnSprites.Length > 0)
            {
                for (int i = 0; i < data.despawnSprites.Length; i++)
                {
                    sr.sprite = data.despawnSprites[i];
                    yield return new WaitForSeconds(delay);
                }
            }

            // アニメーション終了後に破棄
            Destroy(gameObject);
        }
    }
}
