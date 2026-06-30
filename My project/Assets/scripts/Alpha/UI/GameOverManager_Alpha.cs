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
                if (Alpha.Audio.SoundManager_Alpha.Instance != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.StopBGM(0.5f);
                }

                gameOverPanel.SetActive(true);
                // Pause the game or stop enemies
                Time.timeScale = 0f;
            }
        }

        private void OnRetryClicked()
        {
            Time.timeScale = 1f;
            // 謨怜圏縺励◆繧ｹ繝・・繧ｸ縺ｮ蜑榊濠謌ｦ縺九ｉ髢句ｧ九☆繧具ｼ医せ繝・・繧ｿ繧ｹ蠑ｷ蛹悶↑縺ｩ縺ｯ邯ｭ謖・ｼ・            // StageManager_Alpha 縺ｮ蜀崎ｵｷ蜍募・逅・ｒ蜻ｼ縺ｶ
            if (StageManager_Alpha.Instance != null)
            {
                StageManager_Alpha.Instance.RestartStageFromFirstHalf();
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
            }
            else
            {
                // StageManager縺後↑縺・ｴ蜷医・迴ｾ蝨ｨ縺ｮ繧ｷ繝ｼ繝ｳ繧偵Μ繝ｭ繝ｼ繝・                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnGiveUpClicked()
        {
            Time.timeScale = 1f;
            // 繧ｿ繧､繝医Ν縺ｫ謌ｻ繧矩圀縺ｫ繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ繧貞炎髯､縺励∵ｬ｡蝗槭・繧ｲ繝ｼ繝縺ｯ譁ｰ隕上・繝ｬ繧､縺ｫ縺吶ｋ
            if (SaveManager_Alpha.Instance != null)
            {
                SaveManager_Alpha.Instance.ClearSaveData();
            }

            // 繧ｿ繧､繝医Ν繧ｷ繝ｼ繝ｳ縺ｸ驕ｷ遘ｻ
            SceneManager.LoadScene("Title_Alpha");
        }
    }
}
