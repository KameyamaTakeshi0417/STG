using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Alpha.UI
{
    public class TitleManager_Alpha : MonoBehaviour
    {
        [Header("Buttons")]
        public Button storyModeButton;
        public Button normalPlayButton;
        public Button optionsButton;
        public Button encyclopediaButton;
        public Button exitButton;

        [Header("Panels / Popups")]
        public GameObject underDevelopmentPopup;
        public GameObject resumePopup;
        public GameObject exitConfirmPopup;
        public GameObject optionsPanel;
        // 図鑑用は今回は実装しないが、ボタン押下時は underDevelopmentPopup を出す

        [Header("Resume Popup Elements")]
        public Button resumeYesButton;
        public Button resumeNewGameButton;
        public Button resumeBackButton;

        [Header("Exit Popup Elements")]
        public Button exitYesButton;
        public Button exitNoButton;

        [Header("Under Development Popup Elements")]
        public Button underDevCompleteButton;

        [Header("Fade Settings (Title Scene Only)")]
        [Tooltip("タイトルシーン内にあるフェードコントローラーをアタッチします。使い回しはしません。")]
        public FadeController_Alpha fadeController;

        private void Start()
        {
            // パネル初期化
            if (underDevelopmentPopup != null) underDevelopmentPopup.SetActive(false);
            if (resumePopup != null) resumePopup.SetActive(false);
            if (exitConfirmPopup != null) exitConfirmPopup.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);

            // メインボタンイベント登録
            if (storyModeButton != null) storyModeButton.onClick.AddListener(OnStoryModeClicked);
            if (normalPlayButton != null) normalPlayButton.onClick.AddListener(OnNormalPlayClicked);
            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (encyclopediaButton != null) encyclopediaButton.onClick.AddListener(OnEncyclopediaClicked);
            if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

            // 再開ポップアップイベント登録
            if (resumeYesButton != null) resumeYesButton.onClick.AddListener(OnResumeYes);
            if (resumeNewGameButton != null) resumeNewGameButton.onClick.AddListener(OnResumeNewGame);
            if (resumeBackButton != null) resumeBackButton.onClick.AddListener(() => ClosePopup(resumePopup));

            // 退出ポップアップイベント登録
            if (exitYesButton != null) exitYesButton.onClick.AddListener(OnExitYes);
            if (exitNoButton != null) exitNoButton.onClick.AddListener(() => ClosePopup(exitConfirmPopup));

            // 開発中ポップアップイベント登録
            if (underDevCompleteButton != null) underDevCompleteButton.onClick.AddListener(() => ClosePopup(underDevelopmentPopup));
        }

        private void OnStoryModeClicked()
        {
            if (Alpha.Core.SaveManager_Alpha.Instance != null && Alpha.Core.SaveManager_Alpha.Instance.HasSaveData())
            {
                // セーブデータがある場合は再開確認ポップアップ
                if (resumePopup != null) resumePopup.SetActive(true);
            }
            else
            {
                // ない場合はそのままチュートリアルステージへ
                StartNewGame();
            }
        }

        private void OnResumeYes()
        {
            // TODO: セーブデータから復元して開始する処理（シーンロードなど）
            // ひとまず保存されているステージを読み込む想定
            Debug.Log("[Title] Load Save Data and Start!");
            TransitionToScene("TutorialStage_Alpha");
        }

        private void OnResumeNewGame()
        {
            // 新規プレイの場合はセーブデータを削除して開始
            if (Alpha.Core.SaveManager_Alpha.Instance != null)
            {
                Alpha.Core.SaveManager_Alpha.Instance.ClearSaveData();
            }
            StartNewGame();
        }

        private void StartNewGame()
        {
            // 新規ゲーム開始
            Debug.Log("[Title] Start New Game!");
            TransitionToScene("TutorialStage_Alpha"); // チュートリアル兼イージーモード
        }

        private void TransitionToScene(string sceneName)
        {
            // もしタイトルシーン専用のフェードがアタッチされていれば、フェードアウトしてから遷移する
            if (fadeController != null)
            {
                // ボタンの連続押し防止
                if (storyModeButton != null) storyModeButton.interactable = false;
                
                fadeController.FadeOut(() => {
                    SceneManager.LoadScene(sceneName);
                });
            }
            else
            {
                // フェード画面が無ければ、今まで通り即座に遷移する
                SceneManager.LoadScene(sceneName);
            }
        }

        private void OnNormalPlayClicked()
        {
            if (underDevelopmentPopup != null) underDevelopmentPopup.SetActive(true);
        }

        private void OnOptionsClicked()
        {
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void OnEncyclopediaClicked()
        {
            // 今回は開発中ポップアップを出す
            if (underDevelopmentPopup != null) underDevelopmentPopup.SetActive(true);
        }

        private void OnExitClicked()
        {
            if (exitConfirmPopup != null) exitConfirmPopup.SetActive(true);
        }

        private void OnExitYes()
        {
            Debug.Log("[Title] Quit Game.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // 汎用ポップアップ閉じる処理
        public void ClosePopup(GameObject popup)
        {
            if (popup != null) popup.SetActive(false);
        }
    }
}
