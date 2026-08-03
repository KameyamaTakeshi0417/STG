using UnityEngine;
using UnityEngine.UI;
using Alpha.Managers; // PlayerInputManager_Alpha

namespace Alpha.UI
{
    public class PauseMenuManager_Alpha : MonoBehaviour
    {
        public static PauseMenuManager_Alpha Instance { get; private set; }

        [Header("UI References")]
        public GameObject pauseMenuContainer; 
        
        [Header("Tabs")]
        public Button equipmentTabButton;
        public Button settingsTabButton;
        public Button controlsTabButton;
        public Button quitTabButton;

        [Header("Panels")]
        public InventoryUI_Alpha inventoryUI;
        public GameObject settingsPanel;
        public GameObject controlsPanel; 

        private bool isPaused = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("[PauseMenuManager_Alpha] Awake called.");
        }

        private void Start()
        {
            if (pauseMenuContainer != null) pauseMenuContainer.SetActive(false);

            if (equipmentTabButton != null) equipmentTabButton.onClick.AddListener(() => SwitchTab(0));
            if (settingsTabButton != null) settingsTabButton.onClick.AddListener(() => SwitchTab(1));
            if (controlsTabButton != null) controlsTabButton.onClick.AddListener(() => SwitchTab(2));
            if (quitTabButton != null) quitTabButton.onClick.AddListener(OnQuitClicked);
            
            Debug.Log("[PauseMenuManager_Alpha] Start called. Container assigned? " + (pauseMenuContainer != null));
        }

        private void Update()
        {
            if (PlayerInputManager_Alpha.Instance == null) return;

            // Direct fallback check using new input system if WasPausePressed is false
            if (PlayerInputManager_Alpha.Instance.WasPausePressed || UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[PauseMenuManager_Alpha] Esc pressed! Toggling menu...");
                TogglePauseMenu();
            }
        }

        public void TogglePauseMenu()
        {
            if (inventoryUI != null && inventoryUI.currentMode == InventoryUI_Alpha.InventoryUIMode.SelectForBlacksmith)
            {
                Debug.Log("[PauseMenuManager_Alpha] Blocked because Inventory is in Blacksmith mode.");
                return; 
            }

            isPaused = !isPaused;
            Debug.Log("[PauseMenuManager_Alpha] Toggle! isPaused is now: " + isPaused);

            if (isPaused)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }

        private void OpenMenu()
        {
            Time.timeScale = 0f;
            if (pauseMenuContainer != null) pauseMenuContainer.SetActive(true);

            if (TutorialManager_Alpha.Instance != null && TutorialManager_Alpha.Instance.IsShowing)
            {
                TutorialManager_Alpha.Instance.isQueuePaused = true;
                TutorialManager_Alpha.Instance.ForceCloseCurrentTutorial();
            }

            SwitchTab(0);
        }

        public void CloseMenu()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            if (pauseMenuContainer != null) pauseMenuContainer.SetActive(false);
            
            if (inventoryUI != null) inventoryUI.CloseAsTab();
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(false);

            if (TutorialManager_Alpha.Instance != null)
            {
                TutorialManager_Alpha.Instance.isQueuePaused = false;
            }
        }

        private void SwitchTab(int tabIndex)
        {
            if (inventoryUI != null) inventoryUI.CloseAsTab();
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(false);
            
            SetButtonColor(equipmentTabButton, Color.white);
            SetButtonColor(settingsTabButton, Color.white);
            SetButtonColor(controlsTabButton, Color.white);

            switch (tabIndex)
            {
                case 0:
                    if (inventoryUI != null) inventoryUI.OpenAsTab();
                    SetButtonColor(equipmentTabButton, Color.green);
                    break;
                case 1:
                    if (settingsPanel != null) settingsPanel.SetActive(true);
                    SetButtonColor(settingsTabButton, Color.green);
                    break;
                case 2:
                    if (controlsPanel != null) controlsPanel.SetActive(true);
                    SetButtonColor(controlsTabButton, Color.green);
                    break;
            }
        }

        private void SetButtonColor(Button btn, Color col)
        {
            if (btn != null)
            {
                var colors = btn.colors;
                colors.normalColor = col;
                btn.colors = colors;
            }
        }

        private void OnQuitClicked()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title_Alpha");
        }
    }
}
