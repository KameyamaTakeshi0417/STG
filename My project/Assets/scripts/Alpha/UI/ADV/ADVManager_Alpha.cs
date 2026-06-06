using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Alpha.Data;
using TMPro;
using DG.Tweening; // DOTweenを追加

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
        public float animDuration = 0.5f; // アニメーション時間

        private Canvas advCanvas;
        private Image backgroundImage;
        private Image eventCGImage;
        private Image leftCharacterImage;
        private Image centerCharacterImage;
        private Image rightCharacterImage;
        private GameObject dialogBoxPanel;
        
        [Header("Font Asset")]
        [SerializeField] private TMP_FontAsset advFontAsset;

        private TextMeshProUGUI nameText;
        private TextMeshProUGUI dialogText;
        private Button skipButton;

        private ADVData_Alpha currentADVData;
        private int currentPageIndex = 0;
        private Action onCompleteCallback;

        private bool isTyping = false;
        private bool isADVActive = false;
        private string currentFullText = "";
        private Coroutine typingCoroutine;
        
        // アニメーション用の基準座標
        private readonly Vector2 leftBasePos = new Vector2(-768, -440);
        private readonly Vector2 rightBasePos = new Vector2(656, -440);
        private readonly Vector2 centerBasePos = new Vector2(0, 100);
        
        private readonly float slideOffset = 1000f; // 画面外へスライドさせる距離

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
#if UNITY_EDITOR
            if (advFontAsset == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("SoukouMincho SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    advFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                    if (advFontAsset != null) UnityEngine.Debug.Log("ADVManager: Auto-loaded font from " + path);
                }
            }
#endif

            // Canvas
            GameObject canvasObj = new GameObject("ADVCanvas");
            canvasObj.transform.SetParent(transform, false);
            advCanvas = canvasObj.AddComponent<Canvas>();
            advCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            advCanvas.worldCamera = Camera.main;
            advCanvas.planeDistance = 10f;
            advCanvas.sortingLayerName = "SystemUI";
            advCanvas.sortingOrder = 1500; // FadeBoard(1200)より前に表示

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 背景
            backgroundImage = CreateImage(canvasObj.transform, "Background", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            backgroundImage.color = Color.white;

            // キャラクター（左、中央、右）
            leftCharacterImage = CreateImage(canvasObj.transform, "LeftChar", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            leftCharacterImage.rectTransform.anchoredPosition = new Vector2(-768, -440);
            leftCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            rightCharacterImage = CreateImage(canvasObj.transform, "RightChar", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rightCharacterImage.rectTransform.anchoredPosition = new Vector2(656, -440);
            rightCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            centerCharacterImage = CreateImage(canvasObj.transform, "CenterChar", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            centerCharacterImage.rectTransform.anchoredPosition = new Vector2(0, 100);
            centerCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            // 一枚絵
            eventCGImage = CreateImage(canvasObj.transform, "EventCG", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            eventCGImage.color = Color.black; // 背景と同じく全画面

            // ダイアログボックス枠
            dialogBoxPanel = new GameObject("DialogBox");
            dialogBoxPanel.transform.SetParent(canvasObj.transform, false);
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
            nameObj.transform.SetParent(dialogBoxPanel.transform, false);
            nameText = nameObj.AddComponent<TextMeshProUGUI>();
            if (advFontAsset != null) nameText.font = advFontAsset;
            nameText.fontSize = 48;
            nameText.color = Color.yellow;
            nameText.fontStyle = FontStyles.Bold;
            nameText.overflowMode = TextOverflowModes.Overflow;
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.02f, 0.8f);
            nameRect.anchorMax = new Vector2(0.5f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // セリフテキスト
            GameObject textObj = new GameObject("DialogText");
            textObj.transform.SetParent(dialogBoxPanel.transform, false);
            dialogText = textObj.AddComponent<TextMeshProUGUI>();
            if (advFontAsset != null) dialogText.font = advFontAsset;
            dialogText.fontSize = 36;
            dialogText.color = Color.white;
            dialogText.overflowMode = TextOverflowModes.Overflow;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.1f);
            textRect.anchorMax = new Vector2(0.95f, 0.75f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // スキップボタン
            GameObject skipObj = new GameObject("SkipButton");
            skipObj.transform.SetParent(canvasObj.transform, false);
            Image skipBg = skipObj.AddComponent<Image>();
            skipBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            skipButton = skipObj.AddComponent<Button>();
            skipButton.onClick.AddListener(SkipADV);
            
            GameObject skipTextObj = new GameObject("SkipText");
            skipTextObj.transform.SetParent(skipObj.transform, false);
            TextMeshProUGUI skipTxt = skipTextObj.AddComponent<TextMeshProUGUI>();
            if (advFontAsset != null) skipTxt.font = advFontAsset;
            skipTxt.text = "Skip";
            skipTxt.fontSize = 24;
            skipTxt.alignment = TextAlignmentOptions.Center;
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
            obj.transform.SetParent(parent, false);
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
            
            // アニメーション実行
            float maxAnimTime = PlayCharacterAnimations(page);

            if (page.waitForAnimationToFinish && maxAnimTime > 0f)
            {
                // アニメーション完了を待ってからテキスト表示を開始する
                typingCoroutine = StartCoroutine(WaitAndTypewriter(maxAnimTime));
            }
            else
            {
                // 即座にテキスト表示を開始する
                typingCoroutine = StartCoroutine(TypewriterEffect());
            }
        }

        private float PlayCharacterAnimations(ADVPage_Alpha page)
        {
            DOTween.Kill("ADVAnim");
            float longestDuration = 0f;

            if (page.eventCG != null) return 0f; // 一枚絵の場合はキャラアニメスキップ

            if (page.leftCharacter != null)
                longestDuration = Mathf.Max(longestDuration, ApplyAnim(leftCharacterImage, page.leftCharacterAnim, leftBasePos));

            if (page.centerCharacter != null)
                longestDuration = Mathf.Max(longestDuration, ApplyAnim(centerCharacterImage, page.centerCharacterAnim, centerBasePos));

            if (page.rightCharacter != null)
                longestDuration = Mathf.Max(longestDuration, ApplyAnim(rightCharacterImage, page.rightCharacterAnim, rightBasePos));

            return longestDuration;
        }

        private float ApplyAnim(Image img, ADVCharacterAnim animType, Vector2 basePos)
        {
            RectTransform rt = img.rectTransform;
            if (animType == ADVCharacterAnim.None)
            {
                rt.anchoredPosition = basePos;
                return 0f;
            }

            // 初期位置と目標位置の設定
            Vector2 startPos = basePos;
            Vector2 endPos = basePos;

            switch (animType)
            {
                case ADVCharacterAnim.SlideInLeft:
                    startPos = basePos + new Vector2(-slideOffset, 0);
                    endPos = basePos;
                    break;
                case ADVCharacterAnim.SlideInRight:
                    startPos = basePos + new Vector2(slideOffset, 0);
                    endPos = basePos;
                    break;
                case ADVCharacterAnim.SlideInBottom:
                    startPos = basePos + new Vector2(0, -slideOffset);
                    endPos = basePos;
                    break;
                case ADVCharacterAnim.SlideOutLeft:
                    startPos = basePos;
                    endPos = basePos + new Vector2(-slideOffset, 0);
                    break;
                case ADVCharacterAnim.SlideOutRight:
                    startPos = basePos;
                    endPos = basePos + new Vector2(slideOffset, 0);
                    break;
                case ADVCharacterAnim.SlideOutBottom:
                    startPos = basePos;
                    endPos = basePos + new Vector2(0, -slideOffset);
                    break;
            }

            rt.anchoredPosition = startPos;
            
            // SlideOut系の場合は移動後に非表示にする
            bool isOut = (animType == ADVCharacterAnim.SlideOutLeft || animType == ADVCharacterAnim.SlideOutRight || animType == ADVCharacterAnim.SlideOutBottom);

            rt.DOAnchorPos(endPos, animDuration)
              .SetUpdate(true)
              .SetEase(Ease.OutCubic)
              .SetId("ADVAnim")
              .OnComplete(() =>
              {
                  if (isOut) img.gameObject.SetActive(false);
              });

            return animDuration;
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

        private bool isWaitingAnim = false;

        private IEnumerator WaitAndTypewriter(float delay)
        {
            isWaitingAnim = true;
            yield return new WaitForSecondsRealtime(delay);
            isWaitingAnim = false;
            yield return StartCoroutine(TypewriterEffect());
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

            // Enterキーまたは左クリックによる進行
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
            {
                if (isTyping || isWaitingAnim)
                {
                    // アニメーション中なら完了させる
                    DOTween.Complete("ADVAnim");
                    
                    // 待機中またはタイプライター中なら全表示
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    dialogText.text = currentFullText;
                    
                    isWaitingAnim = false;
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
            DOTween.Kill("ADVAnim"); // 終了時にアニメーションを破棄
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
