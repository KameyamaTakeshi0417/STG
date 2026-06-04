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

        private float previousTimeScale = 1f;
        private Queue<string> tutorialQueue = new Queue<string>();
        private static HashSet<string> seenInSession = new HashSet<string>();

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
            }
        }

        public void ShowTutorial(string tutorialId)
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
                if (!tutorialQueue.Contains(tutorialId) && (currentTutorialObject == null || currentTutorialObject.name != tutorialId))
                {
                    tutorialQueue.Enqueue(tutorialId);
                }
                return;
            }

            // 初回ポーズ時のTimeScaleを保存
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isShowing = true;

            ShowTutorialInternal(tutorialId);
        }

        private void ShowTutorialInternal(string tutorialId)
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
            if (backgroundPanel != null) backgroundPanel.SetActive(true);
            
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
            // 時間停止中の他の入力を防ぎつつ、右クリックだけ受け付ける
            if (isShowing)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    CloseTutorial();
                }
            }
        }

        public void CloseTutorial()
        {
            if (currentTutorialObject != null)
            {
                currentTutorialObject.SetActive(false);
                currentTutorialObject = null;
            }

            if (tutorialQueue.Count > 0)
            {
                string nextTutorial = tutorialQueue.Dequeue();
                ShowTutorialInternal(nextTutorial);
            }
            else
            {
                if (tutorialCanvas != null)
                {
                    tutorialCanvas.SetActive(false);
                }
                // TimeScaleを元に戻す
                Time.timeScale = previousTimeScale;
                isShowing = false;
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
