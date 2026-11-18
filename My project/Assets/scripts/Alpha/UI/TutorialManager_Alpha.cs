using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

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

        // ---------- 動的テキスト（操作系切り替え）定義 ----------
        private struct TutorialTextPair
        {
            public string keyboardText;
            public string gamepadText;

            public TutorialTextPair(string kb, string gp)
            {
                keyboardText = kb;
                gamepadText = gp;
            }
        }

        private static readonly Dictionary<string, TutorialTextPair> TutorialTexts = new Dictionary<string, TutorialTextPair>
        {
            {
                "001", new TutorialTextPair(
                    "WASDで移動\n右クリックで特殊アクション（低速移動＆コア表示）！",
                    "左スティックで移動\nL3/R3（スティック押し込み）で特殊アクション（低速移動＆コア表示）！"
                )
            },
            {
                "002", new TutorialTextPair(
                    "マウスで敵にターゲット！\n左クリックで弾を発射出来ます。",
                    "右スティックで敵にターゲット！\nLB または RBで弾を発射出来ます。"
                )
            },
            {
                "003", new TutorialTextPair(
                    "敵が来なくて退屈ですか？\nShiftキーを入力すると、次ウェーブのエネミーが出てきます！\nウェーブ進行が早まるうえに報酬も増えますが、\n敵の増やし過ぎにはご注意。",
                    "敵が来なくて退屈ですか？\nShift / Spaceキーで次ウェーブのエネミーが出現！\nウェーブ進行が早まるうえに報酬も増えますが、\n敵の増やし過ぎにはご注意。"
                )
            },
            {
                "004", new TutorialTextPair(
                    "報酬ゲージが溜まるとオーブを獲得！\n敵を倒したり、進行ショートカットを使うことでゲージが溜まります。",
                    "報酬ゲージが溜まるとオーブを獲得！\n敵を倒したり、進行ショートカットを使うことでゲージが溜まります。"
                )
            },
            {
                "005", new TutorialTextPair(
                    "エリート戦お疲れ様でした！\n休む間もなく訪れたこのウェーブを抜けたらボス戦です。\nボス戦前には体力回復報酬フェーズがあるので頑張って！",
                    "エリート戦お疲れ様でした！\n休む間もなく訪れたこのウェーブを抜けたらボス戦です。\nボス戦前には体力回復報酬フェーズがあるので頑張って！"
                )
            },
            {
                "00-", new TutorialTextPair(
                    "1,3キーで現在の装備を換装できます！\nパーツをたくさん見つけて装備していきましょう！",
                    "LT / RT（トリガー）で現在の装備を換装できます！\nパーツをたくさん見つけて装備していきましょう！"
                )
            },
            {
                "Tutorial_Orb", new TutorialTextPair(
                    "オーブを拾いました！こうして偶に敵からドロップします。\n中身はウェーブ終了時に獲得できます。\n生き残れ！",
                    "オーブを拾いました！こうして偶に敵からドロップします。\n中身はウェーブ終了時に獲得できます。\n生き残れ！"
                )
            },
            {
                "Tutorial_Orb_AutoEquip", new TutorialTextPair(
                    "オーブからはパーツが手に入ります！\n発射時効果が変わる雷管\n航行時効果が変わる薬莢\n着弾時効果が変わる弾頭\n最初に手に入れる3つのオーブは自動で開封、装備されます！",
                    "オーブからはパーツが手に入ります！\n発射時効果が変わる雷管\n航行時効果が変わる薬莢\n着弾時効果が変わる弾頭\n最初に手に入れる3つのオーブは自動で開封、装備されます！"
                )
            },
            {
                "Tutorial_Elite", new TutorialTextPair(
                    "強敵には体力バーが複数本あります。\nゲージを削ると攻撃が強くなるので注意！\nしかも時間経過で逃げてしまいます！\n倒して豪華報酬をゲット！",
                    "強敵には体力バーが複数本あります。\nゲージを削ると攻撃が強くなるので注意！\nしかも時間経過で逃げてしまいます！\n倒して豪華報酬をゲット！"
                )
            },
            {
                "Tutorial_Reward", new TutorialTextPair(
                    "待ちに待った報酬フェーズです！\nオーブを開封して\n報酬装備をゲットしていきましょう！",
                    "待ちに待った報酬フェーズです！\nオーブを開封して\n報酬装備をゲットしていきましょう！"
                )
            },
            {
                "Tutorial_Equip", new TutorialTextPair(
                    "待ちに待った装備画面です。ポーズ画面でもあります\n装備変更は報酬獲得後しか行えないので注意してください。\n縦スロットに同じ紋章を揃えるとセットボーナスが発動します！",
                    "待ちに待った装備画面です。ポーズ画面でもあります\n装備変更は報酬獲得後しか行えないので注意してください。\n縦スロットに同じ紋章を揃えるとセットボーナスが発動します！"
                )
            },
            {
                "Tutorial_Rush", new TutorialTextPair(
                    "エネミーラッシュはスキップできません。\n大量に出てくる敵を沢山倒して沢山報酬を獲得しましょう！",
                    "エネミーラッシュはスキップできません。\n大量に出てくる敵を沢山倒して沢山報酬を獲得しましょう！"
                )
            }
        };

        private string currentTutorialId = "";
        private TextMeshProUGUI currentTMPText = null;
        private UnityEngine.UI.Text currentLegacyText = null;
        private bool lastUsingGamepad = false;

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
            // ライブラリ解放とポップアップ通知（チュートリアル発生時に即座に発火）
            if (Alpha.Data.LibraryManager_Alpha.Instance != null)
            {
                Alpha.Data.LibraryManager_Alpha.Instance.UnlockTutorial(tutorialId);
            }

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
            }

            ResetCurrentTutorialState();

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
                // 既存のチュートリアルオブジェクトをテンプレートにして自動複製（フォールバック）
                Transform template = FindDeepChild(tutorialCanvas.transform, "004") 
                                  ?? FindDeepChild(tutorialCanvas.transform, "001") 
                                  ?? FindDeepChild(tutorialCanvas.transform, "002")
                                  ?? FindDeepChild(tutorialCanvas.transform, "Tutorial_Orb");

                // それでも無ければ backgroundPanel 以外の最初の子要素をテンプレートとして利用
                if (template == null)
                {
                    foreach (Transform c in tutorialCanvas.transform)
                    {
                        if (backgroundPanel != null && c.gameObject == backgroundPanel) continue;
                        template = c;
                        break;
                    }
                }

                if (template != null)
                {
                    GameObject clone = Instantiate(template.gameObject, tutorialCanvas.transform);
                    clone.name = tutorialId;
                    targetTransform = clone.transform;
                }
                else
                {
                    Debug.LogWarning($"[TutorialManager] Tutorial Object '{tutorialId}' not found in TutorialCanvas!");
                    CloseTutorial();
                    return;
                }
            }

            currentTutorialObject = targetTransform.gameObject;
            currentTutorialId = tutorialId;
            currentTMPText = currentTutorialObject.GetComponentInChildren<TextMeshProUGUI>(true);
            currentLegacyText = currentTutorialObject.GetComponentInChildren<UnityEngine.UI.Text>(true);
            lastUsingGamepad = IsCurrentUsingGamepad();
            ApplyTutorialText();

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

            // ライブラリへの追加 & 画面右端ポップアップ通知
            if (Alpha.Data.LibraryManager_Alpha.Instance != null)
            {
                Alpha.Data.LibraryManager_Alpha.Instance.UnlockTutorial(tutorialId);
            }
        }

        private void ApplyTutorialText()
        {
            if (string.IsNullOrEmpty(currentTutorialId)) return;
            if (!TutorialTexts.TryGetValue(currentTutorialId, out TutorialTextPair pair)) return;

            bool isGamepad = IsCurrentUsingGamepad();
            string textToSet = isGamepad ? pair.gamepadText : pair.keyboardText;

            if (currentTMPText != null)
            {
                currentTMPText.text = textToSet;
            }
            else if (currentLegacyText != null)
            {
                currentLegacyText.text = textToSet;
            }
        }

        private bool IsCurrentUsingGamepad()
        {
            if (Alpha.Managers.PlayerInputManager_Alpha.Instance != null)
            {
                return Alpha.Managers.PlayerInputManager_Alpha.Instance.IsUsingGamepad;
            }
            return false;
        }

        public void OverridePreviousTimeScale(float newTimeScale)
        {
            previousTimeScale = newTimeScale;
        }

        public bool isQueuePaused = false;

        private void Update()
        {
            if (isShowing)
            {
                // 入力デバイスの切り替えをリアルタイム検知してテキストを更新
                bool isGamepad = IsCurrentUsingGamepad();
                if (isGamepad != lastUsingGamepad)
                {
                    lastUsingGamepad = isGamepad;
                    ApplyTutorialText();
                }

                // ポーズ型チュートリアルのクローズ判定
                if (CheckCloseInput())
                {
                    CloseTutorial();
                }
            }
            else
            {
                if (!isQueuePaused && tutorialQueue.Count > 0 && Time.timeScale > 0f)
                {
                    CheckQueue();
                }
            }
        }

        private bool CheckCloseInput()
        {
            // マウス・キーボード入力
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Escape))
            {
                return true;
            }

            // ゲームパッド入力（New Input System）
            if (Gamepad.current != null)
            {
                var gp = Gamepad.current;
                if (gp.buttonSouth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame ||
                    gp.startButton.wasPressedThisFrame || gp.selectButton.wasPressedThisFrame ||
                    gp.leftShoulder.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame)
                {
                    return true;
                }
            }

            return false;
        }

        private void ResetCurrentTutorialState()
        {
            currentTutorialObject = null;
            currentTutorialId = "";
            currentTMPText = null;
            currentLegacyText = null;
        }

        public void ForceCloseCurrentTutorial()
        {
            if (isFadingTutorial)
            {
                if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
            }
            if (currentTutorialObject != null)
            {
                currentTutorialObject.SetActive(false);
            }
            ResetCurrentTutorialState();

            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(false);
            }
            Time.timeScale = previousTimeScale;
            isShowing = false;
            IsPausingTimeline = false;
            isFadingTutorial = false;
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
            }
            ResetCurrentTutorialState();

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
            if (isQueuePaused) return;
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
