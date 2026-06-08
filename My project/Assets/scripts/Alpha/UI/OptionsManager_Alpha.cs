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

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (systemVolumeSlider != null) systemVolumeSlider.onValueChanged.AddListener(OnSystemVolumeChanged);
            if (battleVolumeSlider != null) battleVolumeSlider.onValueChanged.AddListener(OnBattleVolumeChanged);

            if (keyConfigButton != null) keyConfigButton.onClick.AddListener(OnKeyConfigClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        public void OpenOptions()
        {
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void OnMasterVolumeChanged(float value)
        {
            // TODO: AudioMixer への反映
            ShowUnderDevelopment();
        }

        private void OnSystemVolumeChanged(float value)
        {
            // TODO: AudioMixer への反映
            ShowUnderDevelopment();
        }

        private void OnBattleVolumeChanged(float value)
        {
            // TODO: AudioMixer への反映
            ShowUnderDevelopment();
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
