using UnityEngine;
using System.IO;
using Alpha.Audio;

namespace Alpha.Managers
{
    [System.Serializable]
    public class GameSettingsData
    {
        public float masterVolume = 1.0f;
        public float bgmVolume = 0.5f;
        public float seVolume = 0.8f;
        public bool isFullscreen = true;
        public bool isScreenShakeEnabled = true;
        public bool isVibrationEnabled = true;
    }

    public class GameSettingsManager_Alpha : MonoBehaviour
    {
        public static GameSettingsManager_Alpha Instance { get; private set; }

        public GameSettingsData CurrentSettings { get; private set; } = new GameSettingsData();
        private string savePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    CurrentSettings = JsonUtility.FromJson<GameSettingsData>(json);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to load settings: " + e.Message);
                    CurrentSettings = new GameSettingsData();
                }
            }
            ApplySettings();
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(CurrentSettings, true);
                File.WriteAllText(savePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to save settings: " + e.Message);
            }
        }

        public void ApplySettings()
        {
            // 音量の適用
            AudioListener.volume = CurrentSettings.masterVolume;
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.UpdateBGMVolume(CurrentSettings.bgmVolume);
                SoundManager_Alpha.Instance.masterSEVolume = CurrentSettings.seVolume;
            }

            // 画面モードの適用
            Screen.fullScreen = CurrentSettings.isFullscreen;
        }

        // --- Setter メソッド群（UIから呼ばれる想定） ---

        public void SetMasterVolume(float value)
        {
            CurrentSettings.masterVolume = value;
            AudioListener.volume = value;
        }

        public void SetBGMVolume(float value)
        {
            CurrentSettings.bgmVolume = value;
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.UpdateBGMVolume(value);
            }
        }

        public void SetSEVolume(float value)
        {
            CurrentSettings.seVolume = value;
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.masterSEVolume = value;
            }
        }

        public void SetFullscreen(bool isFullscreen)
        {
            CurrentSettings.isFullscreen = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        public void SetScreenShake(bool enabled)
        {
            CurrentSettings.isScreenShakeEnabled = enabled;
        }

        public void SetVibration(bool enabled)
        {
            CurrentSettings.isVibrationEnabled = enabled;
        }
    }
}
