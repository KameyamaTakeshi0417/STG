using UnityEngine;
using System.IO;
using Alpha.Audio;

namespace Alpha.Managers
{
    [System.Serializable]
    public class GameSettingsData
    {
        [Header("Audio Settings")]
        public float masterVolume = 1.0f;
        public float bgmVolume = 0.7f;
        public float seVolume = 0.8f;

        [Header("Display Settings")]
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int windowModeIndex = 0; // 0: FullScreenWindow (Borderless), 1: ExclusiveFullScreen, 2: Windowed
        public bool isFullscreen = true; // 互換性用
        public int vSyncIndex = 1; // 0: Don't Sync, 1: Every VBlank
        public int frameRateLimit = 60; // -1: Unlimited, 60, 120, 144

        [Header("Gameplay & Visual Settings")]
        public bool isScreenShakeEnabled = true;
        public bool showDamageNumbers = true;
        public bool isVibrationEnabled = true;
    }

    public class GameSettingsManager_Alpha : MonoBehaviour
    {
        private static GameSettingsManager_Alpha _instance;
        public static GameSettingsManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameSettingsManager_Alpha>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameSettingsManager_Alpha");
                        _instance = go.AddComponent<GameSettingsManager_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        public GameSettingsData CurrentSettings { get; private set; } = new GameSettingsData();
        private string savePath;
        private bool isInitialized = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitIfNeeded();
        }

        private void InitIfNeeded()
        {
            if (isInitialized) return;
            isInitialized = true;
            savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            }

            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    CurrentSettings = JsonUtility.FromJson<GameSettingsData>(json);
                    Debug.Log("[GameSettings] Settings loaded from file: Master=" + CurrentSettings.masterVolume + ", BGM=" + CurrentSettings.bgmVolume + ", SE=" + CurrentSettings.seVolume);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[GameSettings] Failed to load settings: " + e.Message);
                    CurrentSettings = new GameSettingsData();
                }
            }
            else
            {
                CurrentSettings = new GameSettingsData();
                Debug.Log("[GameSettings] No save file found, initialized with default settings.");
            }
            ApplySettings();
        }

        public void SaveSettings()
        {
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            }

            try
            {
                string json = JsonUtility.ToJson(CurrentSettings, true);
                File.WriteAllText(savePath, json);
                Debug.Log("[GameSettings] Settings saved successfully: " + savePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[GameSettings] Failed to save settings: " + e.Message);
            }
        }

        public void ApplySettings()
        {
            // 1. 音量の適用
            AudioListener.volume = Mathf.Clamp01(CurrentSettings.masterVolume);
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.UpdateBGMVolume(CurrentSettings.bgmVolume);
                SoundManager_Alpha.Instance.UpdateSEVolume(CurrentSettings.seVolume);
            }

            // 2. ディスプレイ・解像度の適用
            FullScreenMode mode = FullScreenMode.FullScreenWindow;
            switch (CurrentSettings.windowModeIndex)
            {
                case 1:
                    mode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 2:
                    mode = FullScreenMode.Windowed;
                    break;
                default:
                    mode = FullScreenMode.FullScreenWindow;
                    break;
            }

            int w = CurrentSettings.resolutionWidth > 0 ? CurrentSettings.resolutionWidth : 1920;
            int h = CurrentSettings.resolutionHeight > 0 ? CurrentSettings.resolutionHeight : 1080;
            Screen.SetResolution(w, h, mode);
            CurrentSettings.isFullscreen = (mode != FullScreenMode.Windowed);

            // 3. VSync & フレームレート制限の適用
            QualitySettings.vSyncCount = Mathf.Clamp(CurrentSettings.vSyncIndex, 0, 2);
            Application.targetFrameRate = CurrentSettings.frameRateLimit;

            Debug.Log($"[GameSettings] Applied settings: MasterVol={CurrentSettings.masterVolume}, BGMVol={CurrentSettings.bgmVolume}, SEVol={CurrentSettings.seVolume}");
        }

        // --- Setter メソッド群（UIからのリアルタイム呼び出し用） ---

        public void SetMasterVolume(float value)
        {
            InitIfNeeded();
            CurrentSettings.masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = CurrentSettings.masterVolume;
            Debug.Log($"[GameSettings] Master Volume changed to: {Mathf.RoundToInt(CurrentSettings.masterVolume * 100)}%");
        }

        public void SetBGMVolume(float value)
        {
            InitIfNeeded();
            CurrentSettings.bgmVolume = Mathf.Clamp01(value);
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.UpdateBGMVolume(CurrentSettings.bgmVolume);
            }
            Debug.Log($"[GameSettings] BGM Volume changed to: {Mathf.RoundToInt(CurrentSettings.bgmVolume * 100)}%");
        }

        public void SetSEVolume(float value)
        {
            InitIfNeeded();
            CurrentSettings.seVolume = Mathf.Clamp01(value);
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.UpdateSEVolume(CurrentSettings.seVolume);
            }
            Debug.Log($"[GameSettings] SE Volume changed to: {Mathf.RoundToInt(CurrentSettings.seVolume * 100)}%");
        }

        public void SetResolution(int width, int height)
        {
            InitIfNeeded();
            CurrentSettings.resolutionWidth = width;
            CurrentSettings.resolutionHeight = height;
            ApplySettings();
        }

        public void SetWindowMode(int modeIndex)
        {
            InitIfNeeded();
            CurrentSettings.windowModeIndex = modeIndex;
            ApplySettings();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            InitIfNeeded();
            CurrentSettings.windowModeIndex = isFullscreen ? 0 : 2;
            CurrentSettings.isFullscreen = isFullscreen;
            ApplySettings();
        }

        public void SetVSync(int vsyncIndex)
        {
            InitIfNeeded();
            CurrentSettings.vSyncIndex = vsyncIndex;
            QualitySettings.vSyncCount = vsyncIndex;
        }

        public void SetTargetFrameRate(int fps)
        {
            InitIfNeeded();
            CurrentSettings.frameRateLimit = fps;
            Application.targetFrameRate = fps;
        }

        public void SetScreenShake(bool enabled)
        {
            InitIfNeeded();
            CurrentSettings.isScreenShakeEnabled = enabled;
        }

        public void SetShowDamageNumbers(bool enabled)
        {
            InitIfNeeded();
            CurrentSettings.showDamageNumbers = enabled;
        }

        public void SetVibration(bool enabled)
        {
            InitIfNeeded();
            CurrentSettings.isVibrationEnabled = enabled;
        }
    }
}
