using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Alpha.Data;
using TMPro;
using DG.Tweening; // DOTween繧定ｿｽ蜉

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
                        // DontDestroyOnLoad(go); // 繧ｹ繝・・繧ｸ驕ｷ遘ｻ譎ゅ↓遐ｴ譽・＆繧後※繧よｧ九ｏ縺ｪ縺・ｴ蜷医・繧ｳ繝｡繝ｳ繝医い繧ｦ繝医∝ｿ・ｦ√↑繧芽ｧ｣髯､
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        public float textTypeSpeed = 0.05f; // 1譁・ｭ励≠縺溘ｊ縺ｮ陦ｨ遉ｺ譎る俣・育ｧ抵ｼ・

        [Header("Skip Button Settings")]
        [Tooltip("繧ｹ繧ｭ繝・・繝懊ち繝ｳ縺ｮ螟ｧ縺阪＆")]
        public Vector2 skipButtonSize = new Vector2(150, 60);
        [Tooltip("繧ｹ繧ｭ繝・・繝懊ち繝ｳ縺ｮ繝輔か繝ｳ繝医し繧､繧ｺ")]
        public float skipButtonFontSize = 32;

        [Header("Runtime/Debug")]
        public float animDuration = 0.5f; // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ譎る俣

        [Header("UI References (Assign in Editor)")]
        public Canvas advCanvas;
        public Image backgroundImage;
        public Image eventCGImage;
        public Image leftCharacterImage;
        public Image centerCharacterImage;
        public Image rightCharacterImage;
        public GameObject dialogBoxPanel;
        
        [Header("Font Asset")]
        [SerializeField] private TMP_FontAsset advFontAsset;

        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogText;
        public Button skipButton;

        private ADVData_Alpha currentADVData;
        private int currentPageIndex = 0;
        private Action onCompleteCallback;

        private bool isTyping = false;
        private bool isADVActive = false;
        private string currentFullText = "";
        private Coroutine typingCoroutine;
        
        // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ逕ｨ縺ｮ蝓ｺ貅門ｺｧ讓・
        private readonly Vector2 leftBasePos = new Vector2(-768, -440);
        private readonly Vector2 rightBasePos = new Vector2(656, -440);
        private readonly Vector2 centerBasePos = new Vector2(0, 100);
        
        private readonly float slideOffset = 1000f; // 逕ｻ髱｢螟悶∈繧ｹ繝ｩ繧､繝峨＆縺帙ｋ霍晞屬

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // UI縺後う繝ｳ繧ｹ繝壹け繧ｿ縺九ｉ繧｢繧ｵ繧､繝ｳ縺輔ｌ縺ｦ縺・↑縺代ｌ縺ｰ縲∝ｾ捺擂騾壹ｊ閾ｪ蜍慕函謌舌☆繧具ｼ井ｺ呈鋤諤ｧ・・
            if (advCanvas == null)
            {
                CreateUI();
            }
            else
            {
                // UI縺後い繧ｵ繧､繝ｳ縺輔ｌ縺ｦ縺・ｋ蝣ｴ蜷医√せ繧ｭ繝・・繝懊ち繝ｳ縺ｮ繧､繝吶Φ繝医ｒ逋ｻ骭ｲ
                if (skipButton != null)
                {
                    skipButton.onClick.RemoveAllListeners();
                    skipButton.onClick.AddListener(SkipADV);
                }
            }
            
            // 譛蛻昴・髱櫁｡ｨ遉ｺ
            if (advCanvas != null) advCanvas.gameObject.SetActive(false);
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
            advCanvas.sortingOrder = 1500; // FadeBoard(1200)繧医ｊ蜑阪↓陦ｨ遉ｺ

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 閭梧勹
            backgroundImage = CreateImage(canvasObj.transform, "Background", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            backgroundImage.color = Color.white;

            // 繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ・亥ｷｦ縲∽ｸｭ螟ｮ縲∝承・・
            leftCharacterImage = CreateImage(canvasObj.transform, "LeftChar", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            leftCharacterImage.rectTransform.anchoredPosition = new Vector2(-768, -440);
            leftCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            rightCharacterImage = CreateImage(canvasObj.transform, "RightChar", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rightCharacterImage.rectTransform.anchoredPosition = new Vector2(656, -440);
            rightCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            centerCharacterImage = CreateImage(canvasObj.transform, "CenterChar", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            centerCharacterImage.rectTransform.anchoredPosition = new Vector2(0, 100);
            centerCharacterImage.rectTransform.sizeDelta = new Vector2(500, 700);

            // 荳譫夂ｵｵ
            eventCGImage = CreateImage(canvasObj.transform, "EventCG", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            eventCGImage.color = Color.black; // 閭梧勹縺ｨ蜷後§縺丞・逕ｻ髱｢

            // 繝繧､繧｢繝ｭ繧ｰ繝懊ャ繧ｯ繧ｹ譫
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

            // 蜷榊燕繝・く繧ｹ繝・
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

            // 繧ｻ繝ｪ繝輔ユ繧ｭ繧ｹ繝・
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

            // 繧ｹ繧ｭ繝・・繝懊ち繝ｳ
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
            skipTxt.fontSize = skipButtonFontSize;
            skipTxt.alignment = TextAlignmentOptions.Center;
            skipTxt.color = Color.white;
            skipTextObj.GetComponent<RectTransform>().sizeDelta = skipButtonSize;

            RectTransform skipRect = skipObj.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1, 1);
            skipRect.anchorMax = new Vector2(1, 1);
            skipRect.pivot = new Vector2(1, 1);
            skipRect.anchoredPosition = new Vector2(-20, -20);
            skipRect.sizeDelta = skipButtonSize;
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

            // 譎る俣繧呈ｭ｢繧√ｋ・亥・蜉帙・unscaledTime縺ｧ逶｣隕悶√・繧ｦ繧ｹ繧ｯ繝ｪ繝・け繧ょ虚菴懊☆繧具ｼ・
            Time.timeScale = 0f;
            
            advCanvas.gameObject.SetActive(true);
            ShowPage(currentPageIndex);
        }

        private void ShowPage(int index)
        {
            var page = currentADVData.pages[index];

            // BGM繝ｻSE縺ｮ蜀咲函・郁ｨｭ螳壹＆繧後※縺・ｌ縺ｰ・・
            if (page.bgmClip != null)
            {
                if (Alpha.Audio.SoundManager_Alpha.Instance != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlayBGM(page.bgmClip, 0.5f);
                }
            }
            if (page.seClip != null)
            {
                if (Alpha.Audio.SoundManager_Alpha.Instance != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(page.seClip);
                }
            }

            // 閭梧勹縺ｨCG縺ｮ陦ｨ遉ｺ
            SetImageSprite(backgroundImage, page.backgroundImage);
            SetImageSprite(eventCGImage, page.eventCG);

            // 荳譫夂ｵｵ縺後≠繧句ｴ蜷医・繧ｭ繝｣繝ｩ繧帝國縺・
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

                Color inactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                if (leftCharacterImage.gameObject.activeSelf) leftCharacterImage.color = page.leftSpeaking ? Color.white : inactiveColor;
                if (centerCharacterImage.gameObject.activeSelf) centerCharacterImage.color = page.centerSpeaking ? Color.white : inactiveColor;
                if (rightCharacterImage.gameObject.activeSelf) rightCharacterImage.color = page.rightSpeaking ? Color.white : inactiveColor;
            }

            nameText.text = string.IsNullOrEmpty(page.characterName) ? "" : page.characterName;
            
            currentFullText = page.dialogueText;
            dialogText.text = ""; // 荳譌ｦ繧ｯ繝ｪ繧｢
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ螳溯｡・
            float maxAnimTime = PlayCharacterAnimations(page);

            if (page.waitForAnimationToFinish && maxAnimTime > 0f)
            {
                // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ螳御ｺ・ｒ蠕・▲縺ｦ縺九ｉ繝・く繧ｹ繝郁｡ｨ遉ｺ繧帝幕蟋九☆繧・
                typingCoroutine = StartCoroutine(WaitAndTypewriter(maxAnimTime));
            }
            else
            {
                // 蜊ｳ蠎ｧ縺ｫ繝・く繧ｹ繝郁｡ｨ遉ｺ繧帝幕蟋九☆繧・
                typingCoroutine = StartCoroutine(TypewriterEffect());
            }
        }

        private float PlayCharacterAnimations(ADVPage_Alpha page)
        {
            DOTween.Kill("ADVAnim");
            float longestDuration = 0f;

            if (page.eventCG != null) return 0f; // 荳譫夂ｵｵ縺ｮ蝣ｴ蜷医・繧ｭ繝｣繝ｩ繧｢繝九Γ繧ｹ繧ｭ繝・・

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

            // 蛻晄悄菴咲ｽｮ縺ｨ逶ｮ讓吩ｽ咲ｽｮ縺ｮ險ｭ螳・
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
            
            // SlideOut邉ｻ縺ｮ蝣ｴ蜷医・遘ｻ蜍募ｾ後↓髱櫁｡ｨ遉ｺ縺ｫ縺吶ｋ
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
            var page = currentADVData.pages[currentPageIndex];
            Color activeColor = Color.white;
            Color flashColor = new Color(0.8f, 0.8f, 0.8f, 1f);

            for (int i = 0; i < currentFullText.Length; i++)
            {
                dialogText.text += currentFullText[i];
                
                Color currentColor = (i % 2 == 0) ? activeColor : flashColor;
                if (leftCharacterImage.gameObject.activeSelf && page.leftSpeaking) leftCharacterImage.color = currentColor;
                if (centerCharacterImage.gameObject.activeSelf && page.centerSpeaking) centerCharacterImage.color = currentColor;
                if (rightCharacterImage.gameObject.activeSelf && page.rightSpeaking) rightCharacterImage.color = currentColor;

                yield return new WaitForSecondsRealtime(textTypeSpeed);
            }
            isTyping = false;

            if (leftCharacterImage.gameObject.activeSelf && page.leftSpeaking) leftCharacterImage.color = activeColor;
            if (centerCharacterImage.gameObject.activeSelf && page.centerSpeaking) centerCharacterImage.color = activeColor;
            if (rightCharacterImage.gameObject.activeSelf && page.rightSpeaking) rightCharacterImage.color = activeColor;
        }

        private void Update()
        {
            if (!isADVActive) return;

            // Enter繧ｭ繝ｼ縺ｾ縺溘・蟾ｦ繧ｯ繝ｪ繝・け縺ｫ繧医ｋ騾ｲ陦・
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
            {
                if (isTyping || isWaitingAnim)
                {
                    // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ荳ｭ縺ｪ繧牙ｮ御ｺ・＆縺帙ｋ
                    DOTween.Complete("ADVAnim");
                    
                    // 蠕・ｩ滉ｸｭ縺ｾ縺溘・繧ｿ繧､繝励Λ繧､繧ｿ繝ｼ荳ｭ縺ｪ繧牙・陦ｨ遉ｺ
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    dialogText.text = currentFullText;
                    
                    isWaitingAnim = false;
                    isTyping = false;
                }
                else
                {
                    // 谺｡縺ｮ繝壹・繧ｸ縺ｸ
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
            DOTween.Kill("ADVAnim"); // 邨ゆｺ・凾縺ｫ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繧堤ｴ譽・
            isADVActive = false;
            advCanvas.gameObject.SetActive(false);
            
            // 譎る俣繧貞・縺ｫ謌ｻ縺・
            Time.timeScale = 1f;

            var callback = onCompleteCallback;
            onCompleteCallback = null;
            callback?.Invoke();
        }
    }
}
