using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Alpha.Audio
{
    public class SoundManager_Alpha : MonoBehaviour
    {
        private static SoundManager_Alpha _instance;
        public static SoundManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundManager_Alpha>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SoundManager_Alpha");
                        _instance = go.AddComponent<SoundManager_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private List<AudioSource> seSources = new List<AudioSource>();

        [Header("Settings")]
        [Range(0f, 1f)] public float masterBGMVolume = 0.5f;
        [Range(0f, 1f)] public float masterSEVolume = 0.8f;
        public int initialSESourceCount = 5;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            if (Alpha.Managers.GameSettingsManager_Alpha.Instance != null)
            {
                var settings = Alpha.Managers.GameSettingsManager_Alpha.Instance.CurrentSettings;
                if (settings != null)
                {
                    masterBGMVolume = settings.bgmVolume;
                    masterSEVolume = settings.seVolume;
                }
            }
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            for (int i = seSources.Count; i < initialSESourceCount; i++)
            {
                AudioSource seSource = gameObject.AddComponent<AudioSource>();
                seSource.playOnAwake = false;
                seSource.spatialBlend = 0f;
                seSources.Add(seSource);
            }
        }

        /// <summary>
        /// BGMを再生します。同じ曲の場合は最初から再生し直さず、そのまま継続します。
        /// </summary>
        public void PlayBGM(AudioClip clip, float fadeDuration = 0.5f)
        {
            if (clip == null) return;

            // 同じ曲が既に鳴っている場合は何もしない（シームレスな移行）
            if (bgmSource.isPlaying && bgmSource.clip == clip)
            {
                // 音量が下がっている場合を考慮して元の音量に戻す
                bgmSource.DOKill();
                bgmSource.DOFade(masterBGMVolume, fadeDuration).SetUpdate(true);
                return;
            }

            // 違う曲を鳴らす場合、フェードアウトしてから再生
            if (bgmSource.isPlaying && fadeDuration > 0f)
            {
                bgmSource.DOKill();
                bgmSource.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    bgmSource.clip = clip;
                    bgmSource.Play();
                    bgmSource.DOFade(masterBGMVolume, fadeDuration).SetUpdate(true);
                });
            }
            else
            {
                bgmSource.DOKill();
                bgmSource.clip = clip;
                bgmSource.volume = fadeDuration > 0f ? 0f : masterBGMVolume;
                bgmSource.Play();
                if (fadeDuration > 0f)
                {
                    bgmSource.DOFade(masterBGMVolume, fadeDuration).SetUpdate(true);
                }
            }
        }

                private bool isBGMPaused = false;

        /// <summary>
        /// ポーズメニュー表示時などにBGMを一時停止します。
        /// </summary>
        public void PauseBGM()
        {
            if (bgmSource == null) InitializeAudioSources();
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Pause();
                isBGMPaused = true;
            }
        }

        /// <summary>
        /// ポーズ解除時にBGMを一時停止位置から再開します。
        /// </summary>
        public void ResumeBGM()
        {
            if (bgmSource == null) InitializeAudioSources();
            if (bgmSource != null && isBGMPaused)
            {
                bgmSource.UnPause();
                isBGMPaused = false;
            }
        }

        public void StopBGM(float fadeDuration = 0.5f)
        {
            if (bgmSource.isPlaying)
            {
                if (fadeDuration > 0f)
                {
                    bgmSource.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                    {
                        bgmSource.Stop();
                        bgmSource.volume = masterBGMVolume;
                    });
                }
                else
                {
                    bgmSource.Stop();
                }
            }
        }

        public void UpdateBGMVolume(float newVolume)
        {
            masterBGMVolume = Mathf.Clamp01(newVolume);
            if (bgmSource == null) InitializeAudioSources();
            if (bgmSource != null)
            {
                bgmSource.DOKill();
                bgmSource.volume = masterBGMVolume;
            }
        }

        /// <summary>
        /// オプション画面などからSE全体の音量を更新します。
        /// </summary>
        public void UpdateSEVolume(float newVolume)
        {
            masterSEVolume = Mathf.Clamp01(newVolume);
            if (seSources == null || seSources.Count == 0) InitializeAudioSources();
            if (seSources != null)
            {
                foreach (var source in seSources)
                {
                    if (source != null)
                    {
                        source.volume = masterSEVolume;
                    }
                }
            }
        }

        /// <summary>
        /// SEを再生します。空いているAudioSourceを自動的に探して鳴らします。
        /// </summary>
        public void PlaySE(AudioClip clip, float volumeScale = 1f)
        {
            PlaySE(clip, volumeScale, 1f);
        }

        /// <summary>
        /// ピッチを指定してSEを再生します（敵被弾音など、発射音を低いピッチで鳴らす場合などに使用）。
        /// </summary>
        public void PlaySE(AudioClip clip, float volumeScale, float pitch)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSESource();
            source.pitch = pitch;
            source.volume = masterSEVolume * volumeScale;
            source.PlayOneShot(clip);
        }

        private AudioSource GetAvailableSESource()
        {
            foreach (var source in seSources)
            {
                if (!source.isPlaying) return source;
            }

            // 全て使用中の場合は新しく追加
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.spatialBlend = 0f;
            seSources.Add(newSource);
            return newSource;
        }
    }
}
