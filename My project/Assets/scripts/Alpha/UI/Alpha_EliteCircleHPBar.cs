using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Linq;

namespace Alpha.UI
{
    /// <summary>
    /// エリート敵用円形HPバー管理クラス（最大4層）
    /// 各リングは Image の FillAmount で制御し、0 になるとカットイン演出を再生し当たり判定を無効化します。
    /// </summary>
    public class Alpha_EliteCircleHPBar : MonoBehaviour
    {
        [Header("リングオブジェクト（外側→内側）")]
        [Tooltip("最大4枚のリング GameObject。子に Image コンポーネントが入っている想定")]
        public GameObject[] ringObjects;

        // 内部で Image コンポーネントを保持
        public Image[] ringImages;
        // エリート全体のHP比率（0~1）
        [Header("Current HP Ratio (0~1)")]
        public float hpRatio;

        // 有効なリング数（1〜4）
        private int activeLayers = 0;

        // 敵AI への参照（無敵化・再開）
        private Alpha_EnemyAI linkedAI;

        // カットイン演出用プレハブ（立ち絵＋テキスト）
        [Tooltip("演出用プレハブ（Screen Space Overlay）")]
        public GameObject cutInPrefab;

        /// <summary>
        /// 初期化。フェーズ数に応じて表示するリング数を決め、AI への参照を保持します。
        /// </summary>
        public void Initialise(int layerCount, Alpha_EnemyAI ai)
        {
            activeLayers = Mathf.Clamp(layerCount, 1, 4);
            linkedAI = ai;

            // Auto-detect ring objects if not assigned
            if (ringObjects == null || ringObjects.Length == 0 || System.Array.TrueForAll(ringObjects, o => o == null))
            {
                var allObjs = GetComponentsInChildren<Transform>(true);
                var ringList = new System.Collections.Generic.List<GameObject>();
                foreach (var t in allObjs)
                {
                    if (t.gameObject.name.Contains("Ring") || t.gameObject.name.Contains("CircleHPBar"))
                        ringList.Add(t.gameObject);
                }
                // Fallback: use all children under this canvas
                if (ringList.Count == 0)
                {
                    foreach (var t in allObjs)
                        ringList.Add(t.gameObject);
                }
                ringList.Sort((a, b) => a.name.CompareTo(b.name));
                ringObjects = ringList.ToArray();
            }

            // Prepare ringImages array based on activeLayers
            ringImages = new Image[activeLayers];

            for (int i = 0; i < activeLayers; i++)
            {
                if (ringObjects != null && i < ringObjects.Length && ringObjects[i] != null)
                {
                    // Find child named "fill" and get its Image component
                    var fillObj = ringObjects[i].transform.Find("fill");
                    Image img = null;
                    if (fillObj != null)
                        img = fillObj.GetComponent<Image>();
                    else
                        img = ringObjects[i].GetComponentInChildren<Image>(true);
                    ringImages[i] = img;
                    if (img != null) {
                        img.fillAmount = 1f;
                    }
                }
            }

            // Set overall HP ratio to full at start
            hpRatio = 1f;

            // Deactivate unused ring objects
            for (int i = activeLayers; i < ringObjects.Length; i++)
            {
                if (ringObjects[i] != null) ringObjects[i].SetActive(false);
            }
        }

        /// <summary>
        /// 更新対象リング (外側から順に減少) とその HP 比率を受け取り UI を更新します。
        /// </summary>
        public void SetRingFill(int ringIndex, float ratio)
        {
            // 全体比率はデバッグ用に保持
            hpRatio = Mathf.Clamp01(ratio);

            // データが無い場合は何もしない
            if (ringImages == null) return;

            // 論理フェーズインデックス (0 = 最初) をビジュアルインデックスに変換
            // ビジュアル上は 0 が内側、activeLayers-1 が外側
            int visualIndex = activeLayers - 1 - ringIndex;

            // 各リングの fillAmount を設定
            //   i > visualIndex : すでに外側が削除済み (0)
            //   i == visualIndex : 現在フェーズ (ratio)
            //   i < visualIndex : まだ未到達 (1)
            for (int i = 0; i < activeLayers; i++)
            {
                if (ringImages[i] == null) continue;
                if (i > visualIndex)
                    ringImages[i].fillAmount = 0f;               // 外側は空
                else if (i == visualIndex)
                    ringImages[i].fillAmount = hpRatio;         // 現在フェーズ
                else
                    ringImages[i].fillAmount = 1f;               // 内側は満タン
            }

            // 現在リングが 0 になったらカットイン演出
            if (Mathf.Approximately(hpRatio, 0f))
                StartCoroutine(HandleRingBreak(ringIndex));
        }

        // カットイン演出と無敵化を行うコルーチン
        private IEnumerator HandleRingBreak(int brokenRingIndex)
        {
            // 無敵化（当たり判定・攻撃ビヘイビア停止）
            if (linkedAI != null)
                linkedAI.SetInvulnerable(true);

            // カットイン UI を生成
            GameObject cutIn = null;
            if (cutInPrefab != null)
                cutIn = Instantiate(cutInPrefab, transform.parent);

            // 演出時間はプレハブ側の Animator に合わせて調整可能です。
            float duration = 1.5f; // デフォルト
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (cutIn != null) Destroy(cutIn);

            // 無敵解除
            if (linkedAI != null)
                linkedAI.SetInvulnerable(false);
        }
    }
}
