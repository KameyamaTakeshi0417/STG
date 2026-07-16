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
        private GameObject ringObj;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            
            // 少しずつ速くするため、最初から間隔を持たせる
            blinkTimer = 0f;

            // 回転する警告リングをプロシージャルに生成
            ringObj = new GameObject("WarningRing");
            ringObj.transform.SetParent(transform);
            ringObj.transform.localPosition = Vector3.zero;

            var ringSr = ringObj.AddComponent<SpriteRenderer>();
            ringSr.sprite = sr.sprite;
            ringSr.color = new Color(1f, 0.2f, 0.2f, 0f);
            ringSr.sortingLayerID = sr.sortingLayerID;
            ringSr.sortingOrder = sr.sortingOrder - 1;

            // 初期スケールを大きくしておく
            ringObj.transform.localScale = new Vector3(3f, 3f, 1f);
        }

        private void Update()
        {
            timeAlive += Time.deltaTime;
            
            // 進行度 (0.0 ～ 1.0)
            float t = Mathf.Clamp01(timeAlive / duration);
            
            // リングのアニメーション
            if (ringObj != null)
            {
                // スケールが徐々に 3 -> 1 へ縮小（ターゲットオン感）
                float scale = Mathf.Lerp(3f, 0.8f, t);
                ringObj.transform.localScale = new Vector3(scale, scale, 1f);
                
                // 回転（だんだん速くなる）
                float rotSpeed = Mathf.Lerp(90f, 720f, t);
                ringObj.transform.Rotate(0, 0, rotSpeed * Time.deltaTime);

                // 透明度のフェードイン
                var ringSr = ringObj.GetComponent<SpriteRenderer>();
                ringSr.color = new Color(1f, 0.2f, 0.2f, Mathf.Lerp(0f, 0.8f, t));
            }

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
