using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Alpha.Core;
using Alpha.Flow;

namespace Alpha.UI
{
    public class GameOverManager_Alpha : MonoBehaviour
    {
        public static GameOverManager_Alpha Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject gameOverPanel;
        
        [Header("Buttons")]
        public Button retryButton;
        public Button giveUpButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            if (giveUpButton != null) giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            // Subscribe to Player Death event
            if (playerStatusManager_Alpha.Instance != null)
            {
                // playerStatusManager_Alpha might not have an explicit death event yet.
                // Assuming OnGaugeLost or checking in Update.
            }
        }

        private void Update()
        {
            if (playerStatusManager_Alpha.Instance != null && playerStatusManager_Alpha.Instance.currentHP <= 0)
            {
                if (gameOverPanel != null && !gameOverPanel.activeSelf)
                {
                    ShowGameOver();
                }
            }
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                // Pause the game or stop enemies
                Time.timeScale = 0f;
            }
        }

        private void OnRetryClicked()
        {
            Time.timeScale = 1f;
            // 敗北したステージの前半戦から開始する（ステータス強化などは維持）
            // StageManager_Alpha の再起動処理を呼ぶ
            if (StageManager_Alpha.Instance != null)
            {
                StageManager_Alpha.Instance.RestartStageFromFirstHalf();
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
            }
            else
            {
                // StageManagerがない場合は現在のシーンをリロード
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnGiveUpClicked()
        {
            Time.timeScale = 1f;
            // タイトルに戻る際にセーブデータを削除し、次回のゲームは新規プレイにする
            if (SaveManager_Alpha.Instance != null)
            {
                SaveManager_Alpha.Instance.ClearSaveData();
            }

            // タイトルシーンへ遷移
            SceneManager.LoadScene("Title_Alpha");
        }
    }
}
