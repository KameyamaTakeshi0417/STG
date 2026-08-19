using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Alpha.Effect
{
    public class Alpha_MidBossAppearEffect : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("演出にかける時間（秒）")]
        public float duration = 5f;
        
        [Tooltip("演出開始時のスケール（X, Y）")]
        public float startScale = 15f;
        
        [Header("Screen Shake Settings")]
        [Tooltip("画面揺れの持続時間")]
        public float shakeDuration = 0.5f;
        
        [Tooltip("画面揺れの強さ")]
        public float shakeIntensity = 0.3f;

        private Material effectMat;
        private RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            // ImageやRawImageなど、CanvasRendererを持つGraphicからマテリアルを取得
            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                // materialForRendering でインスタンス化されたマテリアルを取得（他のUIに影響を与えないため）
                // ただしマテリアルのプロパティを直接いじるには .material を取得するか、
                // Graphic.material にインスタンスをセットして使うのが安全。
                // ここではマテリアルをインスタンス化してGraphicにセットし直す。
                effectMat = Instantiate(graphic.material);
                graphic.material = effectMat;
            }

            // エディタ上で配置・確認しやすくするため、初動で非アクティブにしておく
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 演出を開始します。終了時に onComplete コールバックが呼ばれます。
        /// </summary>
        /// <param name="onComplete">演出終了時の処理</param>
        public void PlayEffect(System.Action onComplete)
        {
            gameObject.SetActive(true);
            StartCoroutine(EffectRoutine(onComplete));
        }

        private IEnumerator EffectRoutine(System.Action onComplete)
        {
            float elapsed = 0f;
            
            // 初期スケール設定
            if (rectTransform != null)
            {
                rectTransform.localScale = new Vector3(startScale, startScale, 1f);
            }
            else
            {
                transform.localScale = new Vector3(startScale, startScale, 1f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // スケールを startScale から 0 へ収束
                float currentScale = Mathf.Lerp(startScale, 0f, t);
                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
                }
                else
                {
                    transform.localScale = new Vector3(currentScale, currentScale, 1f);
                }

                // マテリアルのパラメータを変化
                if (effectMat != null)
                {
                    if (effectMat.HasProperty("_VortexSpeed"))
                    {
                        float vortex = Mathf.Lerp(-2f, -20f, t); // 回転を加速
                        effectMat.SetFloat("_VortexSpeed", vortex);
                    }
                    if (effectMat.HasProperty("_TwistAmount"))
                    {
                        float twist = Mathf.Lerp(3f, 15f, t); // ねじれを強く
                        effectMat.SetFloat("_TwistAmount", twist);
                    }
                }

                yield return null;
            }

            // 画面揺れを実行
            if (Camera.main != null)
            {
                StartCoroutine(ShakeRoutine(Camera.main.transform, shakeDuration, shakeIntensity));
            }

            // 非表示にする
            gameObject.SetActive(false);

            // 完了コールバック呼び出し
            onComplete?.Invoke();
        }

        private IEnumerator ShakeRoutine(Transform camTransform, float time, float intensity)
        {
            Vector3 initialPos = camTransform.localPosition;
            float shakeElapsed = 0f;
            
            while (shakeElapsed < time)
            {
                shakeElapsed += Time.deltaTime;
                camTransform.localPosition = initialPos + (Vector3)Random.insideUnitCircle * intensity;
                yield return null;
            }
            
            camTransform.localPosition = initialPos;
        }
    }
}
