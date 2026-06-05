using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Alpha.Data;

namespace Alpha.UI.ADV
{
    public class ADVManager_Alpha : MonoBehaviour
    {
        private static ADVManager_Alpha _instance;
        public static ADVManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ADVManager_Alpha>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ADVManager_Alpha");
                        _instance = go.AddComponent<ADVManager_Alpha>();
                        // DontDestroyOnLoad(go); // ステージ遷移時に破棄されても構わない場合はコメントアウト、必要なら解除
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        public float textTypeSpeed = 0.05f; // 1文字あたりの表示時間（秒）

        private Canvas advCanvas;
        private Image backgroundImage;
        private Image eventCGImage;
        private Image leftCharacterImage;
        private Image centerCharacterImage;
        private Image rightCharacterImage;
        private GameObject dialogBoxPanel;
        private Text nameText;
        private Text dialogText;
        private Button skipButton;

        private ADVData_Alpha currentADVData;
        private int currentPageIndex = 0;
        private Action onCompleteCallback;

        private bool isTyping = false;
        private bool isADVActive = false;
        private string currentFullText = "";
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // UIの自動生成
            CreateUI();
            
            // 最初は非表示
            advCanvas.gameObject.SetActive(false);
        }

        private void CreateUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("ADVCanvas");
            canvasObj.transform.SetParent(transform);
            advCanvas = canvasObj.AddComponent<Canvas>();
            advCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            advCanvas.sortingOrder = 100; // 前面に表示
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 背景
            backgroundImage = CreateImage(canvasObj.transform, "Background", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            backgroundImage.color = Color.black;

            // キャラクター（左、中央、右）
            leftCharacterImage = CreateImage(canvasObj.transform, "LeftChar", new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            leftCharacterImage.rectTransform.anchoredPosition = new Vector2(200, 100);
            leftCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            rightCharacterImage = CreateImage(canvasObj.transform, "RightChar", new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
            rightCharacterImage.rectTransform.anchoredPosition = new Vector2(-200, 100);
            rightCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            centerCharacterImage = CreateImage(canvasObj.transform, "CenterChar", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            centerCharacterImage.rectTransform.anchoredPosition = new Vector2(0, 100);
            centerCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            // 一枚絵
            eventCGImage = CreateImage(canvasObj.transform, "EventCG", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            eventCGImage.color = Color.black; // 背景と同じく全画面

            // ダイアログボックス枠
            dialogBoxPanel = new GameObject("DialogBox");
            dialogBoxPanel.transform.SetParent(canvasObj.transform);
            Image dialogImage = dialogBoxPanel.AddComponent<Image>();
            dialogImage.color = new Color(0, 0, 0, 0.8f);
            RectTransform dialogRect = dialogBoxPanel.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.1f, 0.05f);
            dialogRect.anchorMax = new Vector2(0.9f, 0.3f);
            dialogRect.pivot = new Vector2(0.5f, 0);
            dialogRect.offsetMin = Vector2.zero;
            dialogRect.offsetMax = Vector2.zero;

            // 名前テキスト
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(dialogBoxPanel.transform);
            nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameText.fontSize = 32;
            nameText.color = Color.yellow;
            nameText.fontStyle = FontStyle.Bold;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.02f, 0.8f);
            nameRect.anchorMax = new Vector2(0.5f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // セリフテキスト
            GameObject textObj = new GameObject("DialogText");
            textObj.transform.SetParent(dialogBoxPanel.transform);
            dialogText = textObj.AddComponent<Text>();
            dialogText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            dialogText.fontSize = 28;
            dialogText.color = Color.white;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.1f);
            textRect.anchorMax = new Vector2(0.95f, 0.75f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // スキップボタン
            GameObject skipObj = new GameObject("SkipButton");
            skipObj.transform.SetParent(canvasObj.transform);
            Image skipBg = skipObj.AddComponent<Image>();
            skipBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            skipButton = skipObj.AddComponent<Button>();
            skipButton.onClick.AddListener(SkipADV);
            
            GameObject skipTextObj = new GameObject("SkipText");
            skipTextObj.transform.SetParent(skipObj.transform);
            Text skipTxt = skipTextObj.AddComponent<Text>();
            skipTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            skipTxt.text = "Skip";
            skipTxt.fontSize = 24;
            skipTxt.alignment = TextAnchor.MiddleCenter;
            skipTxt.color = Color.white;
            skipTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);

            RectTransform skipRect = skipObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1, 1);
            skipRect.anchorMax = new Vector2(1, 1);
            skipRect.pivot = new Vector2(1, 1);
            skipRect.anchoredPosition = new Vector2(-20, -20);
            skipRect.sizeDelta = new Vector2(100, 40);
        }

        private Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Image img = obj.AddComponent<Image>();
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return img;
        }

        public void StartADV(ADVData_Alpha advData, Action onComplete)
        {
            if (advData == null || advData.pages.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            currentADVData = advData;
            currentPageIndex = 0;
            onCompleteCallback = onComplete;
            isADVActive = true;

            // 時間を止める（入力はunscaledTimeで監視、マウスクリックも動作する）
            Time.timeScale = 0f;
            
            advCanvas.gameObject.SetActive(true);
            ShowPage(currentPageIndex);
        }

        private void ShowPage(int index)
        {
            var page = currentADVData.pages[index];

            // 背景とCGの表示
            SetImageSprite(backgroundImage, page.backgroundImage);
            SetImageSprite(eventCGImage, page.eventCG);

            // 一枚絵がある場合はキャラを隠す
            if (page.eventCG != null)
            {
                leftCharacterImage.gameObject.SetActive(false);
                centerCharacterImage.gameObject.SetActive(false);
                rightCharacterImage.gameObject.SetActive(false);
            }
            else
            {
                SetImageSprite(leftCharacterImage, page.leftCharacter);
                SetImageSprite(centerCharacterImage, page.centerCharacter);
                SetImageSprite(rightCharacterImage, page.rightCharacter);
            }

            nameText.text = string.IsNullOrEmpty(page.characterName) ? "" : page.characterName;
            
            currentFullText = page.dialogueText;
            dialogText.text = ""; // 一旦クリア
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterEffect());
        }

        private void SetImageSprite(Image img, Sprite sprite)
        {
            if (sprite != null)
            {
                img.sprite = sprite;
                img.gameObject.SetActive(true);
            }
            else
            {
                img.sprite = null;
                img.gameObject.SetActive(false);
            }
        }

        private IEnumerator TypewriterEffect()
        {
            isTyping = true;
            dialogText.text = "";
            for (int i = 0; i < currentFullText.Length; i++)
            {
                dialogText.text += currentFullText[i];
                yield return new WaitForSecondsRealtime(textTypeSpeed);
            }
            isTyping = false;
        }

        private void Update()
        {
            if (!isADVActive) return;

            // Enterキーによる進行
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (isTyping)
                {
                    // タイプライター中なら全表示
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    dialogText.text = currentFullText;
                    isTyping = false;
                }
                else
                {
                    // 次のページへ
                    NextPage();
                }
            }
        }

        private void NextPage()
        {
            currentPageIndex++;
            if (currentPageIndex < currentADVData.pages.Count)
            {
                ShowPage(currentPageIndex);
            }
            else
            {
                EndADV();
            }
        }

        public void SkipADV()
        {
            if (!isADVActive) return;
            EndADV();
        }

        private void EndADV()
        {
            isADVActive = false;
            advCanvas.gameObject.SetActive(false);
            
            // 時間を元に戻す
            Time.timeScale = 1f;

            var callback = onCompleteCallback;
            onCompleteCallback = null;
            callback?.Invoke();
        }
    }
}
