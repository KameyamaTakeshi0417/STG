using UnityEngine;
using UnityEngine.UI;

namespace Alpha.UI
{
    /// <summary>
    /// アニメーション中の誤入力を防止するフルスクリーンRaycastブロッカー。
    /// 最前面のCanvasに透明なImageを被せることで、すべてのUI操作をブロックする。
    /// </summary>
    public class UIInputBlocker_Alpha : MonoBehaviour
    {
        private static UIInputBlocker_Alpha instance;
        private static GameObject blockerObject;
        private static int blockCount = 0; // ネストされたBlock呼び出しに対応

        /// <summary>
        /// UI入力をブロックする。ネスト対応（Block2回→Unblock2回で解除）。
        /// </summary>
        public static void Block()
        {
            blockCount++;
            if (blockCount == 1)
            {
                EnsureBlocker();
                if (blockerObject != null) blockerObject.SetActive(true);
            }
        }

        /// <summary>
        /// UI入力のブロックを解除する。
        /// </summary>
        public static void Unblock()
        {
            blockCount--;
            if (blockCount <= 0)
            {
                blockCount = 0;
                if (blockerObject != null) blockerObject.SetActive(false);
            }
        }

        /// <summary>
        /// 強制的にブロックを解除する（安全弁）。
        /// </summary>
        public static void ForceUnblock()
        {
            blockCount = 0;
            if (blockerObject != null) blockerObject.SetActive(false);
        }

        private static void EnsureBlocker()
        {
            if (blockerObject != null) return;

            // 最前面Canvas を生成
            GameObject canvasObj = new GameObject("UIInputBlocker_Canvas");
            DontDestroyOnLoad(canvasObj);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000; // 最前面
            canvasObj.AddComponent<GraphicRaycaster>();

            // 透明なフルスクリーンImage
            blockerObject = new GameObject("Blocker");
            blockerObject.transform.SetParent(canvasObj.transform, false);
            Image img = blockerObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // 完全透明
            img.raycastTarget = true;

            RectTransform rt = blockerObject.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            blockerObject.SetActive(false);

            instance = canvasObj.AddComponent<UIInputBlocker_Alpha>();
        }
    }
}
