using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Alpha.UI
{
    public class OptionsUI_Alpha : MonoBehaviour
    {
        [Header("Volume Sliders")]
        public Slider masterVolumeSlider;
        public Slider bgmVolumeSlider;
        public Slider seVolumeSlider;

        [Header("Toggles")]
        public Toggle fullscreenToggle;
        public Toggle screenShakeToggle;
        public Toggle vibrationToggle;

        [Header("Buttons")]
        public Button saveButton;
        public Button closeButton;

        private void Start()
        {
            // 設定�Eネ�Eジャーが存在しなければ何もしなぁE            if (Alpha.Managers.GameSettingsManager_Alpha.Instance == null)
            {
                Debug.LogWarning("[OptionsUI] GameSettingsManager is missing!");
                return;
            }

            var settings = Alpha.Managers.GameSettingsManager_Alpha.Instance.CurrentSettings;

            // UIの初期値を現在の設定に合わせる
            if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVolume;
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = settings.bgmVolume;
            if (seVolumeSlider != null) seVolumeSlider.value = settings.seVolume;

            if (fullscreenToggle != null) fullscreenToggle.isOn = settings.isFullscreen;
            if (screenShakeToggle != null) screenShakeToggle.isOn = settings.isScreenShakeEnabled;
            if (vibrationToggle != null) vibrationToggle.isOn = settings.isVibrationEnabled;

            // スライダー変更時�Eイベント登録
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(Alpha.Managers.GameSettingsManager_Alpha.Instance.SetMasterVolume);
            
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(Alpha.Managers.GameSettingsManager_Alpha.Instance.SetBGMVolume);
            
            if (seVolumeSlider != null)
                seVolumeSlider.onValueChanged.AddListener(Alpha.Managers.GameSettingsManager_Alpha.Instance.SetSEVolume);

            // トグル変更時�Eイベント登録
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(Alpha.Managers.GameSettingsManager_Alpha.Instance.SetFullscreen);
            
            if (screenShakeToggle != null)
                screenShakeToggle.onValueChanged.AddListener(Alpha.Managers.GameSettingsManager_Alpha.Instance.SetScreenShake);
            
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(Alpha.Managers.GameSettingsManager_Alpha.Instance.SetVibration);

            // ボタンイベント登録
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(() =>
                {
                    Alpha.Managers.GameSettingsManager_Alpha.Instance.SaveSettings();
                    Debug.Log("Settings saved.");
                });
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    Alpha.Managers.GameSettingsManager_Alpha.Instance.SaveSettings();
                    gameObject.SetActive(false); // パネルを閉じる
                });
            }
        }
    }
}
