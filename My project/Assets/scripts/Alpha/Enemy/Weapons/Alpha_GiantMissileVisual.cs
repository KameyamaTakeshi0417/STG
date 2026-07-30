using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Alpha.Enemy.Weapons
{
    public class Alpha_GiantMissileVisual : MonoBehaviour
    {
        public float implosionDuration = 0.5f;
        public float whiteoutDuration = 0.5f;
        public float holdDuration = 5f;
        public float shrinkDuration = 1f;

        private void Start()
        {
            StartCoroutine(ExplosionSequence());
        }

        private IEnumerator ExplosionSequence()
        {
            // 1. ホワイトアウト用キャンバス生成 (ScreenSpaceOverlayで画面全体を覆う)
            GameObject canvasObj = new GameObject("WhiteoutCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767; // 最前面に表示
            
            GameObject imageObj = new GameObject("WhiteImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            Image whiteImage = imageObj.AddComponent<Image>();
            whiteImage.color = new Color(1, 1, 1, 0);
            RectTransform rect = whiteImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // 2. 爆発ドーム（半球/円）の生成
            // 2D上で円形に見せるためSphereを利用する
            GameObject domeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            domeObj.name = "ExplosionDome";
            domeObj.transform.SetParent(transform);
            domeObj.transform.localPosition = Vector3.zero;
            domeObj.transform.localScale = Vector3.zero;
            
            // Sphereに自動付与されるColliderは不要なので削除（ダメージ判定は親が行う）
            Destroy(domeObj.GetComponent<Collider>());

            MeshRenderer renderer = domeObj.GetComponent<MeshRenderer>();
            // ライティングの影響を受けないSprites/Defaultシェーダーを利用
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0.3f, 0f, 0.8f); // 燃えるような赤黒いオレンジ
            renderer.material = mat;

            // --- フェーズ1: 爆縮 ＆ ホワイトアウト開始 ---
            float t = 0;
            while (t < implosionDuration)
            {
                t += Time.deltaTime;
                float ratio = t / implosionDuration;
                // 画面が真っ白に染まる
                whiteImage.color = new Color(1, 1, 1, ratio);
                yield return null;
            }
            whiteImage.color = new Color(1, 1, 1, 1);

            // --- フェーズ2: 大爆発展開 ＆ ホワイトアウト晴れる ---
            t = 0;
            Vector3 targetScale = new Vector3(120f, 120f, 1f); // 画面を完全に覆うほどの巨大サイズ
            while (t < whiteoutDuration)
            {
                t += Time.deltaTime;
                float ratio = t / whiteoutDuration;
                
                // ドームが急激に膨張 (EaseOut)
                float scaleRatio = 1f - Mathf.Pow(1f - ratio, 3f);
                domeObj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, scaleRatio);
                
                // ホワイトアウトが晴れていく
                whiteImage.color = new Color(1, 1, 1, 1f - ratio);
                
                yield return null;
            }
            whiteImage.color = new Color(1, 1, 1, 0);
            domeObj.transform.localScale = targetScale;

            // --- フェーズ3: 持続（明滅や鼓動でプレッシャーを与える） ---
            t = 0;
            while (t < holdDuration)
            {
                t += Time.deltaTime;
                // サイン波でサイズをわずかに鼓動させる
                float pulse = 1f + Mathf.Sin(t * 15f) * 0.015f;
                domeObj.transform.localScale = targetScale * pulse;
                
                // 色を少し明滅させて高エネルギー状態を表現
                float colorPulse = 0.8f + Mathf.Sin(t * 25f) * 0.2f;
                mat.color = new Color(1f, 0.3f * colorPulse, 0f, 0.8f);
                
                yield return null;
            }

            // --- フェーズ4: 収縮して消滅 ---
            t = 0;
            Vector3 startShrinkScale = domeObj.transform.localScale;
            while (t < shrinkDuration)
            {
                t += Time.deltaTime;
                float ratio = t / shrinkDuration;
                
                // 収縮 (EaseIn)
                float scaleRatio = ratio * ratio;
                domeObj.transform.localScale = Vector3.Lerp(startShrinkScale, Vector3.zero, scaleRatio);
                
                // 徐々に透明になって消える
                mat.color = new Color(1f, 0.3f, 0f, 0.8f * (1f - ratio));
                
                yield return null;
            }

            // クリーンアップ
            Destroy(canvasObj);
            Destroy(domeObj);
        }
    }
}
