using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Alpha.Audio;

namespace Alpha.UI
{
    /// <summary>
    /// ライブラリに項目が追加された際に画面右端にスライドインするトースト通知マネージャー
    /// </summary>
    public class LibraryNotificationManager_Alpha : MonoBehaviour
    {
        private static LibraryNotificationManager_Alpha _instance;
        public static LibraryNotificationManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LibraryNotificationManager_Alpha>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LibraryNotificationManager_Alpha");
                        _instance = go.AddComponent<LibraryNotificationManager_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public class NotificationRequest
        {
            public string category;
            public string itemName;
            public string customMessage;
        }

        [Header("Animation Settings")]
        [Tooltip("画面外からスライドイン・スライドアウトする時間（秒）")]
        public float slideDuration = 0.3f;

        [Tooltip("次の通知がない（スタックなし）場合の待機時間（秒）")]
        public float normalWaitDuration = 2.0f;

        [Tooltip("次の通知が待機している（スタックあり）場合の待機時間（秒）")]
        public float stackedWaitDuration = 1.0f;

        [Tooltip("画面右端の目標X座標（画面内位置）")]
        public float targetX = -25f;

        [Tooltip("画面外の待機X座標")]
        public float hiddenX = 480f;

        [Tooltip("表示Y座標")]
        public float targetY = 220f;

        [Header("Audio Settings")]
        public AudioClip notificationSEClip;
        [Range(0f, 1f)] public float seVolume = 0.85f;

        [Header("UI References")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RectTransform popupPanelRect;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup popupCanvasGroup;

        [Header("Font Settings")]
        public TMP_FontAsset customFontAsset;

        private Queue<NotificationRequest> notificationQueue = new Queue<NotificationRequest>();
        private bool isProcessingQueue = false;
        private Coroutine activeQueueCoroutine;
        private Tween activeMoveTween;
        private Tween activeFadeTween;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadFontAssetIfNeeded();
            LoadDefaultSE();
            EnsureUIHierarchy();
        }

        private void LoadFontAssetIfNeeded()
        {
#if UNITY_EDITOR
            if (customFontAsset == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("SoukouMincho SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    string p = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    customFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
                }
            }
#endif
            if (customFontAsset == null)
            {
                customFontAsset = Resources.Load<TMP_FontAsset>("Fonts/SoukouMincho SDF");
            }
        }

        private void LoadDefaultSE()
        {
            if (notificationSEClip == null)
            {
                // 明るく澄んだ通知音・獲得音を採用
                notificationSEClip = Resources.Load<AudioClip>("SE/SE_GetExp_02_Ting");
                if (notificationSEClip == null)
                {
                    notificationSEClip = Resources.Load<AudioClip>("SE/バフ");
                }
            }
        }

        /// <summary>
        /// 画面右端の通知UI階層を動的構築（存在しない場合）
        /// </summary>
        private void EnsureUIHierarchy()
        {
            if (popupPanelRect != null) return;

            // 1. 専用Canvasの生成（最上位ルートとして配置）
            GameObject canvasGO = new GameObject("LibraryNotificationCanvas", typeof(RectTransform));
            canvasGO.transform.SetParent(null);
            DontDestroyOnLoad(canvasGO);

            targetCanvas = canvasGO.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingLayerName = "SystemUI";
            targetCanvas.sortingOrder = 1050; // 最前面

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // 2. ポップアップパネルの生成（画面右端に配置）
            GameObject panelGO = new GameObject("NotificationPopupPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            popupPanelRect = panelGO.GetComponent<RectTransform>();
            popupPanelRect.anchorMin = new Vector2(1f, 0.5f);
            popupPanelRect.anchorMax = new Vector2(1f, 0.5f);
            popupPanelRect.pivot = new Vector2(1f, 0.5f);
            popupPanelRect.sizeDelta = new Vector2(450f, 76f);
            popupPanelRect.anchoredPosition = new Vector2(hiddenX, targetY);

            popupCanvasGroup = panelGO.AddComponent<CanvasGroup>();
            popupCanvasGroup.alpha = 1f;

            // 背景イメージ（ダークサイバー／半透明）
            Image bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.94f);

            // 左端アクセントカラー帯（エメラルド/シアン）
            GameObject barGO = new GameObject("AccentBar", typeof(RectTransform));
            barGO.transform.SetParent(panelGO.transform, false);
            RectTransform barRT = barGO.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(0f, 1f);
            barRT.pivot = new Vector2(0f, 0.5f);
            barRT.sizeDelta = new Vector2(5f, 0f);
            barRT.anchoredPosition = Vector2.zero;
            Image barImg = barGO.AddComponent<Image>();
            barImg.color = new Color(0.2f, 0.85f, 0.8f, 1f);

            // 上下レイアウトコンテナ
            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(panelGO.transform, false);
            RectTransform contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = new Vector2(16f, 6f);
            contentRT.offsetMax = new Vector2(-12f, -6f);

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 3f;

            // カテゴリテキスト（上段）
            GameObject catGO = new GameObject("CategoryText", typeof(RectTransform));
            catGO.transform.SetParent(contentGO.transform, false);
            categoryText = catGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) categoryText.font = customFontAsset;
            categoryText.fontSize = 13f;
            categoryText.fontStyle = FontStyles.Bold;
            categoryText.color = new Color(0.35f, 0.9f, 0.85f, 0.95f);
            categoryText.alignment = TextAlignmentOptions.Left;
            categoryText.text = "◆ LIBRARY UPDATED ◆";

            // メインメッセージテキスト（下段: xxxにyyyが追加されました！）
            GameObject msgGO = new GameObject("MessageText", typeof(RectTransform));
            msgGO.transform.SetParent(contentGO.transform, false);
            messageText = msgGO.AddComponent<TextMeshProUGUI>();
            if (customFontAsset != null) messageText.font = customFontAsset;
            messageText.fontSize = 17f;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Left;
            messageText.enableWordWrapping = false;
            messageText.overflowMode = TextOverflowModes.Ellipsis;
            messageText.text = "ライブラリに新しい項目が追加されました！";

            panelGO.SetActive(false);
        }

        /// <summary>
        /// ライブラリ追加通知をキューに登録して表示
        /// </summary>
        /// <param name="category">カテゴリ名（例: チュートリアル, 敵, 弾, エフェクト）</param>
        /// <param name="itemName">項目名（例: エネミーラッシュ）</param>
        public void EnqueueNotification(string category, string itemName)
        {
            EnqueueNotificationInternal(new NotificationRequest
            {
                category = category,
                itemName = itemName,
                customMessage = $"{category} に「{itemName}」が追加されました！"
            });
        }

        /// <summary>
        /// 自由文での通知をキューに登録
        /// </summary>
        public void EnqueueCustomNotification(string customMessage, string header = "LIBRARY UPDATED")
        {
            EnqueueNotificationInternal(new NotificationRequest
            {
                category = header,
                customMessage = customMessage
            });
        }

        private void EnqueueNotificationInternal(NotificationRequest request)
        {
            EnsureUIHierarchy();
            notificationQueue.Enqueue(request);

            if (!isProcessingQueue)
            {
                activeQueueCoroutine = StartCoroutine(ProcessQueueCoroutine());
            }
        }

        /// <summary>
        /// キューの順次処理コルーチン
        /// </summary>
        private IEnumerator ProcessQueueCoroutine()
        {
            isProcessingQueue = true;

            while (notificationQueue.Count > 0)
            {
                NotificationRequest req = notificationQueue.Dequeue();

                // テキスト更新
                if (!string.IsNullOrEmpty(req.category))
                {
                    categoryText.text = $"◆ LIBRARY [{req.category}] ◆";
                }
                else
                {
                    categoryText.text = "◆ LIBRARY UPDATED ◆";
                }

                messageText.text = req.customMessage;

                popupPanelRect.gameObject.SetActive(true);
                popupPanelRect.anchoredPosition = new Vector2(hiddenX, targetY);
                popupCanvasGroup.alpha = 1f;

                // SE再生
                PlayNotificationSE();

                // スライドイン（画面右外 -> 画面内定位置）
                activeMoveTween?.Kill();
                activeMoveTween = popupPanelRect.DOAnchorPosX(targetX, slideDuration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);

                yield return new WaitForSecondsRealtime(slideDuration);

                // 待機時間（スタック中なら短縮）
                // 待機中に次の通知が追加された場合も即座に対応できるようループで監視
                float timer = 0f;
                while (true)
                {
                    timer += Time.unscaledDeltaTime;

                    bool hasStack = notificationQueue.Count > 0;
                    float requiredDuration = hasStack ? stackedWaitDuration : normalWaitDuration;

                    if (timer >= requiredDuration)
                    {
                        break;
                    }
                    yield return null;
                }

                // スライドアウト（画面内定位置 -> 画面右外）
                activeMoveTween?.Kill();
                activeMoveTween = popupPanelRect.DOAnchorPosX(hiddenX, slideDuration)
                    .SetEase(Ease.InCubic)
                    .SetUpdate(true);

                yield return new WaitForSecondsRealtime(slideDuration);
            }

            popupPanelRect.gameObject.SetActive(false);
            isProcessingQueue = false;
        }

        private void PlayNotificationSE()
        {
            if (notificationSEClip == null) LoadDefaultSE();

            if (SoundManager_Alpha.Instance != null && notificationSEClip != null)
            {
                SoundManager_Alpha.Instance.PlaySE(notificationSEClip, seVolume);
            }
            else if (notificationSEClip != null)
            {
                AudioSource.PlayClipAtPoint(notificationSEClip, Camera.main ? Camera.main.transform.position : Vector3.zero, seVolume);
            }
        }

        private void OnDestroy()
        {
            activeMoveTween?.Kill();
            activeFadeTween?.Kill();
        }

#if UNITY_EDITOR
        [ContextMenu("Test Library Notification")]
        public void TestNotification()
        {
            EnqueueNotification("チュートリアル", "エネミーラッシュ");
        }
#endif
    }
}
