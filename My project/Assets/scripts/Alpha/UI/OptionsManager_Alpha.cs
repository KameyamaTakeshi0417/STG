using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace Alpha.UI
{
    public class OptionsManager_Alpha : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject optionsPanel;
        public GameObject underDevelopmentPopup;

        [Header("Volume Sliders")]
        public Slider masterVolumeSlider;
        public Slider systemVolumeSlider;
        public Slider battleVolumeSlider;

        [Header("Buttons")]
        public Button keyConfigButton;
        public Button backButton;

        [Header("Managers")]
        public KeyConfigManager_Alpha keyConfigManager;

        private void Start()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);

            // 初期ボリュームをマネージャーから反映
            float masterVol = 1f;
            float seVol = 0.8f;
            if (Alpha.Managers.GameSettingsManager_Alpha.Instance != null)
            {
                masterVol = Alpha.Managers.GameSettingsManager_Alpha.Instance.CurrentSettings.masterVolume;
                seVol = Alpha.Managers.GameSettingsManager_Alpha.Instance.CurrentSettings.seVolume;
            }
            else if (Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                seVol = Alpha.Audio.SoundManager_Alpha.Instance.masterSEVolume;
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = masterVol;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            if (systemVolumeSlider != null)
            {
                systemVolumeSlider.value = seVol;
                systemVolumeSlider.onValueChanged.AddListener(OnSystemVolumeChanged);
            }
            if (battleVolumeSlider != null)
            {
                battleVolumeSlider.value = seVol;
                battleVolumeSlider.onValueChanged.AddListener(OnBattleVolumeChanged);
            }

            if (keyConfigButton != null) keyConfigButton.onClick.AddListener(OnKeyConfigClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        public void OpenOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (Alpha.Managers.GameSettingsManager_Alpha.Instance != null)
            {
                Alpha.Managers.GameSettingsManager_Alpha.Instance.SetMasterVolume(value);
            }
            else
            {
                AudioListener.volume = value;
            }
        }

        private void OnSystemVolumeChanged(float value)
        {
            ApplySEVolume(value);
        }

        private void OnBattleVolumeChanged(float value)
        {
            ApplySEVolume(value);
        }

        private void ApplySEVolume(float value)
        {
            if (Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.UpdateSEVolume(value);
            }
            if (Alpha.Managers.GameSettingsManager_Alpha.Instance != null)
            {
                Alpha.Managers.GameSettingsManager_Alpha.Instance.SetSEVolume(value);
            }
        }

        private void OnKeyConfigClicked()
        {
            if (keyConfigManager != null)
            {
                keyConfigManager.OpenKeyConfig();
            }
            else
            {
                ShowUnderDevelopment();
            }
        }

        private void OnBackClicked()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void ShowUnderDevelopment()
        {
            if (underDevelopmentPopup != null) underDevelopmentPopup.SetActive(true);
        }
    }
}
