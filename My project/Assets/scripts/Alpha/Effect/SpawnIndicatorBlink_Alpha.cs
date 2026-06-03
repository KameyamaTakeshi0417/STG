using UnityEngine;

namespace Alpha.Effect
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpawnIndicatorBlink_Alpha : MonoBehaviour
    {
        [Tooltip("出現するまでの合計時間（SpawnManagerの待機時間に合わせる）")]
        public float duration = 2.0f;

        [Tooltip("最初の明滅間隔（秒）。0.5なら0.5秒に1回切り替わる")]
        public float startInterval = 0.5f;

        [Tooltip("最終的な明滅間隔（秒）。0.1ならかなり高速に点滅する")]
        public float endInterval = 0.1f;

        private SpriteRenderer sr;
        private float timeAlive = 0f;
        private float blinkTimer = 0f;
        private bool isVisible = true;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            
            // 少しずつ速くするため、最初から間隔を持たせる
            blinkTimer = 0f;
        }

        private void Update()
        {
            timeAlive += Time.deltaTime;
            
            // 進行度 (0.0 ～ 1.0)
            float t = Mathf.Clamp01(timeAlive / duration);
            
            // 現在の明滅間隔を計算 (徐々に短くなる)
            float currentInterval = Mathf.Lerp(startInterval, endInterval, t);

            blinkTimer += Time.deltaTime;
            
            // 指定間隔に達したら表示・非表示を切り替える
            if (blinkTimer >= currentInterval)
            {
                blinkTimer = 0f;
                isVisible = !isVisible;
                sr.enabled = isVisible;
            }
        }
    }
}
