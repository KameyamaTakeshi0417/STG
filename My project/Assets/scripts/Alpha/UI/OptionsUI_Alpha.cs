using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alpha.Managers;
using Alpha.Audio;

namespace Alpha.UI
{
    public class OptionsUI_Alpha : MonoBehaviour
    {
        [Header("Volume Sliders")]
        public Slider masterVolumeSlider;
        public TextMeshProUGUI masterVolumeText;
        public Slider bgmVolumeSlider;
        public TextMeshProUGUI bgmVolumeText;
        public Slider seVolumeSlider;
        public TextMeshProUGUI seVolumeText;

        [Header("Display Settings")]
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown windowModeDropdown;
        public Toggle fullscreenToggle; // 互換用（ドロップダウンがない場合にも対応）
        public Toggle vSyncToggle;
        public TMP_Dropdown frameRateDropdown;

        [Header("Gameplay & Visual Toggles")]
        public Toggle screenShakeToggle;
        public Toggle damageNumbersToggle;
        public Toggle vibrationToggle;

        [Header("Buttons & Panels")]
        public GameObject optionsPanel;
        public Button saveButton;
        public Button closeButton;
        public Button resetDefaultButton;

        private struct ResolutionItem
        {
            public int width;
            public int height;
            public ResolutionItem(int w, int h) { width = w; height = h; }
        }

        private readonly List<ResolutionItem> standardResolutions = new List<ResolutionItem>()
        {
            new ResolutionItem(1280, 720),
            new ResolutionItem(1366, 768),
            new ResolutionItem(1600, 900),
            new ResolutionItem(1920, 1080),
            new ResolutionItem(2560, 1440),
            new ResolutionItem(3840, 2160)
        };

        private void Start()
        {
            InitializeUI();
        }

        public void OpenOptions()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
            InitializeUI();
        }

        public void CloseOptions()
        {
            // 閉じる際に設定を自動セーブ
            if (GameSettingsManager_Alpha.Instance != null)
            {
                GameSettingsManager_Alpha.Instance.SaveSettings();
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void InitializeUI()
        {
            if (GameSettingsManager_Alpha.Instance == null)
            {
                Debug.LogWarning("[OptionsUI] GameSettingsManager_Alpha instance is missing!");
                return;
            }

            var settings = GameSettingsManager_Alpha.Instance.CurrentSettings;

            // 1. ボリュームスライダー
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = settings.masterVolume;
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
                UpdateVolumeText(masterVolumeText, settings.masterVolume);
            }
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = settings.bgmVolume;
                bgmVolumeSlider.onValueChanged.RemoveAllListeners();
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
                UpdateVolumeText(bgmVolumeText, settings.bgmVolume);
            }
            if (seVolumeSlider != null)
            {
                seVolumeSlider.value = settings.seVolume;
                seVolumeSlider.onValueChanged.RemoveAllListeners();
                seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);
                UpdateVolumeText(seVolumeText, settings.seVolume);
            }

            // 2. 解像度ドロップダウン
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                List<string> options = new List<string>();
                int selectedIndex = 3; // デフォルトは 1920x1080

                for (int i = 0; i < standardResolutions.Count; i++)
                {
                    var res = standardResolutions[i];
                    options.Add($"{res.width} x {res.height}");
                    if (res.width == settings.resolutionWidth && res.height == settings.resolutionHeight)
                    {
                        selectedIndex = i;
                    }
                }
                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = selectedIndex;
                resolutionDropdown.onValueChanged.RemoveAllListeners();
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            // 3. ウィンドウモード ドロップダウン
            if (windowModeDropdown != null)
            {
                windowModeDropdown.ClearOptions();
                windowModeDropdown.AddOptions(new List<string>()
                {
                    "ボーダーレス (Borderless)",
                    "フルスクリーン (Exclusive Fullscreen)",
                    "ウィンドウ表示 (Windowed)"
                });
                windowModeDropdown.value = Mathf.Clamp(settings.windowModeIndex, 0, 2);
                windowModeDropdown.onValueChanged.RemoveAllListeners();
                windowModeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
            }

            // 4. トグル群
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = settings.isFullscreen;
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.isOn = settings.vSyncIndex > 0;
                vSyncToggle.onValueChanged.RemoveAllListeners();
                vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }

            if (frameRateDropdown != null)
            {
                frameRateDropdown.ClearOptions();
                frameRateDropdown.AddOptions(new List<string>() { "60 FPS", "120 FPS", "無制限 (Unlimited)" });
                if (settings.frameRateLimit == 60) frameRateDropdown.value = 0;
                else if (settings.frameRateLimit == 120) frameRateDropdown.value = 1;
                else frameRateDropdown.value = 2;

                frameRateDropdown.onValueChanged.RemoveAllListeners();
                frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
            }

            if (screenShakeToggle != null)
            {
                screenShakeToggle.isOn = settings.isScreenShakeEnabled;
                screenShakeToggle.onValueChanged.RemoveAllListeners();
                screenShakeToggle.onValueChanged.AddListener(OnScreenShakeChanged);
            }

            if (damageNumbersToggle != null)
            {
                damageNumbersToggle.isOn = settings.showDamageNumbers;
                damageNumbersToggle.onValueChanged.RemoveAllListeners();
                damageNumbersToggle.onValueChanged.AddListener(OnDamageNumbersChanged);
            }

            if (vibrationToggle != null)
            {
                vibrationToggle.isOn = settings.isVibrationEnabled;
                vibrationToggle.onValueChanged.RemoveAllListeners();
                vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
            }

            // 5. ボタン群
            if (saveButton != null)
            {
                saveButton.onClick.RemoveAllListeners();
                saveButton.onClick.AddListener(OnSaveButtonClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseOptions);
            }
            if (resetDefaultButton != null)
            {
                resetDefaultButton.onClick.RemoveAllListeners();
                resetDefaultButton.onClick.AddListener(OnResetDefaultClicked);
            }
        }

        private float lastSEPreviewTime = -1f;

        private void OnMasterVolumeChanged(float val)
        {
            GameSettingsManager_Alpha.Instance?.SetMasterVolume(val);
            UpdateVolumeText(masterVolumeText, val);
            PlayPreviewSE();
        }

        private void OnBGMVolumeChanged(float val)
        {
            GameSettingsManager_Alpha.Instance?.SetBGMVolume(val);
            UpdateVolumeText(bgmVolumeText, val);
        }

        private void OnSEVolumeChanged(float val)
        {
            GameSettingsManager_Alpha.Instance?.SetSEVolume(val);
            UpdateVolumeText(seVolumeText, val);
            PlayPreviewSE();
        }

        private void PlayPreviewSE()
        {
            if (Time.unscaledTime - lastSEPreviewTime >= 0.15f)
            {
                lastSEPreviewTime = Time.unscaledTime;
                if (Alpha.Audio.SoundManager_Alpha.Instance != null && PauseMenuManager_Alpha.Instance != null && PauseMenuManager_Alpha.Instance.tabSwitchSEClip != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(PauseMenuManager_Alpha.Instance.tabSwitchSEClip, 1f);
                }
            }
        }

        private void UpdateVolumeText(TextMeshProUGUI tmp, float val)
        {
            if (tmp != null)
            {
                tmp.text = $"{Mathf.RoundToInt(val * 100f)}%";
            }
        }

        private void OnResolutionChanged(int index)
        {
            if (index >= 0 && index < standardResolutions.Count)
            {
                var target = standardResolutions[index];
                GameSettingsManager_Alpha.Instance?.SetResolution(target.width, target.height);
            }
        }

        private void OnWindowModeChanged(int index)
        {
            GameSettingsManager_Alpha.Instance?.SetWindowMode(index);
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = (index != 2);
            }
        }

        private void OnFullscreenToggleChanged(bool isFull)
        {
            GameSettingsManager_Alpha.Instance?.SetFullscreen(isFull);
            if (windowModeDropdown != null)
            {
                windowModeDropdown.value = isFull ? 0 : 2;
            }
        }

        private void OnVSyncChanged(bool enabled)
        {
            GameSettingsManager_Alpha.Instance?.SetVSync(enabled ? 1 : 0);
        }

        private void OnFrameRateChanged(int index)
        {
            int fps = 60;
            if (index == 0) fps = 60;
            else if (index == 1) fps = 120;
            else if (index == 2) fps = -1;
            GameSettingsManager_Alpha.Instance?.SetTargetFrameRate(fps);
        }

        private void OnScreenShakeChanged(bool enabled)
        {
            GameSettingsManager_Alpha.Instance?.SetScreenShake(enabled);
        }

        private void OnDamageNumbersChanged(bool enabled)
        {
            GameSettingsManager_Alpha.Instance?.SetShowDamageNumbers(enabled);
        }

        private void OnVibrationChanged(bool enabled)
        {
            GameSettingsManager_Alpha.Instance?.SetVibration(enabled);
        }

        private void OnSaveButtonClicked()
        {
            GameSettingsManager_Alpha.Instance?.SaveSettings();
        }

        private void OnResetDefaultClicked()
        {
            if (GameSettingsManager_Alpha.Instance != null)
            {
                GameSettingsManager_Alpha.Instance.SetMasterVolume(1.0f);
                GameSettingsManager_Alpha.Instance.SetBGMVolume(0.7f);
                GameSettingsManager_Alpha.Instance.SetSEVolume(0.8f);
                GameSettingsManager_Alpha.Instance.SetResolution(1920, 1080);
                GameSettingsManager_Alpha.Instance.SetWindowMode(0);
                GameSettingsManager_Alpha.Instance.SetVSync(1);
                GameSettingsManager_Alpha.Instance.SetTargetFrameRate(60);
                GameSettingsManager_Alpha.Instance.SetScreenShake(true);
                GameSettingsManager_Alpha.Instance.SetShowDamageNumbers(true);
                GameSettingsManager_Alpha.Instance.SaveSettings();
                InitializeUI();
            }
        }
    }
}
