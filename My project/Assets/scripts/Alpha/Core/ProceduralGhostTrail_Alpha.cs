using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Core
{
    public class ProceduralGhostTrail_Alpha : MonoBehaviour
    {
        [Header("Trail Settings")]
        public float spawnInterval = 0.05f; // 何秒ごとに残像を出すか
        public float ghostDuration = 0.3f; // 残像が消えるまでの時間
        public Color ghostColor = new Color(0.5f, 0.5f, 1f, 0.5f); // 残像の色

        private SpriteRenderer[] targetRenderers;
        private bool isTrailing = false;
        private float lastSpawnTime = 0f;

        private void Awake()
        {
            // 子オブジェクト含めすべてのSpriteRendererを取得
            targetRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        public void EnableTrail(bool enable)
        {
            isTrailing = enable;
            if (enable) lastSpawnTime = Time.time;
        }

        private void Update()
        {
            if (isTrailing && Time.time - lastSpawnTime >= spawnInterval)
            {
                SpawnGhost();
                lastSpawnTime = Time.time;
            }
        }

        private void SpawnGhost()
        {
            foreach (var sr in targetRenderers)
            {
                if (sr == null || sr.sprite == null || !sr.enabled) continue;

                GameObject ghostObj = new GameObject("GhostTrail");
                ghostObj.transform.position = sr.transform.position;
                ghostObj.transform.rotation = sr.transform.rotation;
                ghostObj.transform.localScale = sr.transform.lossyScale;

                SpriteRenderer ghostSr = ghostObj.AddComponent<SpriteRenderer>();
                ghostSr.sprite = sr.sprite;
                ghostSr.color = ghostColor;
                ghostSr.sortingLayerID = sr.sortingLayerID;
                ghostSr.sortingOrder = sr.sortingOrder - 1; // 本体の少し奥に表示

                StartCoroutine(FadeOutAndDestroy(ghostSr, ghostDuration));
            }
        }

        private IEnumerator FadeOutAndDestroy(SpriteRenderer sr, float duration)
        {
            float elapsed = 0f;
            Color startColor = sr.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 徐々に透明にする
                sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
                yield return null;
            }

            Destroy(sr.gameObject);
        }
    }
}
