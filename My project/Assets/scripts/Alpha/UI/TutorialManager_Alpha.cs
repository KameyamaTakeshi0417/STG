using UnityEngine;
using System.Collections.Generic;

namespace Alpha.UI
{
    public class TutorialManager_Alpha : MonoBehaviour
    {
        public static TutorialManager_Alpha Instance { get; private set; }

        [Tooltip("チュートリアル用の親Canvas。最前面に出るようにSort Orderを高く設定してください。")]
        public GameObject tutorialCanvas;
        [Tooltip("半透明の背景パネル")]
        public GameObject backgroundPanel;

        private GameObject currentTutorialObject;
        private bool isShowing = false;
        public bool IsShowing => isShowing;
        
        public bool IsPausingTimeline { get; private set; }

        private float previousTimeScale = 1f;
        private struct TutorialRequest
        {
            public string tutorialId;
            public bool useFadeMode;
            public float displayDuration;
            public bool pauseTimeline;
        }
        private Queue<TutorialRequest> tutorialQueue = new Queue<TutorialRequest>();
        private static HashSet<string> seenInSession = new HashSet<string>();

        private Coroutine activeFadeCoroutine;
        private CanvasGroup fadeCanvasGroup;
        private bool isFadingTutorial = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(false);
                fadeCanvasGroup = tutorialCanvas.GetComponent<CanvasGroup>();
                if (fadeCanvasGroup == null)
                {
                    fadeCanvasGroup = tutorialCanvas.AddComponent<CanvasGroup>();
                }
            }
        }

        public void ShowTutorial(string tutorialId, bool useFadeMode = false, float displayDuration = 3f, bool pauseTimeline = true)
        {
            if (Application.isEditor)
            {
                // エディターの場合は、1回のプレイにつき各チュートリアル1回のみ表示する
                if (seenInSession.Contains(tutorialId)) return;
                seenInSession.Add(tutorialId);
            }
            else
            {
                // ビルド時は永続的に1度だけ表示
                bool hasSeen = PlayerPrefs.GetInt("Tutorial_Seen_" + tutorialId, 0) == 1;
                if (hasSeen) return;
            }

            if (isShowing)
            {
                // フェード表示中にポーズ型のチュートリアルが来た場合はフェードを中断する
                if (isFadingTutorial && !useFadeMode)
                {
                    if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
                    CloseTutorialInstantly();
                }
                else
                {
                    bool contains = false;
                    foreach (var t in tutorialQueue) if (t.tutorialId == tutorialId) contains = true;
                    if (!contains && (currentTutorialObject == null || currentTutorialObject.name != tutorialId))
                    {
                        tutorialQueue.Enqueue(new TutorialRequest { tutorialId = tutorialId, useFadeMode = useFadeMode, displayDuration = displayDuration, pauseTimeline = pauseTimeline });
                    }
                    return;
                }
            }

            isShowing = true;
            isFadingTutorial = useFadeMode;
            IsPausingTimeline = pauseTimeline;

            if (useFadeMode)
            {
                activeFadeCoroutine = StartCoroutine(FadeTutorialCoroutine(tutorialId, displayDuration));
            }
            else
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                ShowTutorialInternal(tutorialId, true);
            }
        }

        private System.Collections.IEnumerator FadeTutorialCoroutine(string tutorialId, float duration)
        {
            ShowTutorialInternal(tutorialId, false);
            fadeCanvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.5f);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(duration);

            elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.5f);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;

            CloseTutorialInstantly();
        }

        private void CloseTutorialInstantly()
        {
            if (currentTutorialObject != null)
            {
                currentTutorialObject.SetActive(false);
                currentTutorialObject = null;
            }

            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(false);
            }
            isShowing = false;
            isFadingTutorial = false;
            IsPausingTimeline = false;
            if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;

            CheckQueue();
        }

        private void ShowTutorialInternal(string tutorialId, bool showBackground)
        {
            if (tutorialCanvas == null)
            {
                Debug.LogWarning("[TutorialManager] TutorialCanvas is not assigned!");
                CloseTutorial();
                return;
            }

            // チュートリアルオブジェクトを再帰的に検索
            Transform targetTransform = FindDeepChild(tutorialCanvas.transform, tutorialId);
            if (targetTransform == null)
            {
                Debug.LogWarning($"[TutorialManager] Tutorial Object '{tutorialId}' not found in TutorialCanvas!");
                CloseTutorial();
                return;
            }

            currentTutorialObject = targetTransform.gameObject;

            // UIの表示
            tutorialCanvas.SetActive(true);
            if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;
            if (backgroundPanel != null) backgroundPanel.SetActive(showBackground);
            
            // 背景パネル以外の直下のオブジェクトを一旦すべて非表示にする
            foreach (Transform child in tutorialCanvas.transform)
            {
                if (backgroundPanel != null && child.gameObject == backgroundPanel) continue;
                child.gameObject.SetActive(false);
            }

            // 対象オブジェクトからCanvasに至るまでの親階層をすべて表示する
            Transform curr = targetTransform;
            while (curr != null && curr != tutorialCanvas.transform)
            {
                curr.gameObject.SetActive(true);
                curr = curr.parent;
            }

            // 見たことを記録
            PlayerPrefs.SetInt("Tutorial_Seen_" + tutorialId, 1);
            PlayerPrefs.Save();
        }

        public void OverridePreviousTimeScale(float newTimeScale)
        {
            previousTimeScale = newTimeScale;
        }

        private void Update()
        {
            // 時間停止中の他の入力を防ぎつつ、左・右クリックでスキップを受け付ける
            if (isShowing)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    CloseTutorial();
                }
            }
        }

        public void CloseTutorial()
        {
            if (isFadingTutorial)
            {
                if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
                CloseTutorialInstantly();
                return;
            }

            if (currentTutorialObject != null)
            {
                currentTutorialObject.SetActive(false);
                currentTutorialObject = null;
            }

            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(false);
            }
            // TimeScaleを元に戻す
            Time.timeScale = previousTimeScale;
            isShowing = false;
            IsPausingTimeline = false;

            CheckQueue();
        }

        private void CheckQueue()
        {
            if (tutorialQueue.Count > 0)
            {
                var req = tutorialQueue.Dequeue();
                ShowTutorial(req.tutorialId, req.useFadeMode, req.displayDuration, req.pauseTimeline);
            }
        }

        private Transform FindDeepChild(Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = FindDeepChild(child, aName);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
