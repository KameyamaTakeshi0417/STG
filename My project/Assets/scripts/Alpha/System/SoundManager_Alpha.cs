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
            if (bgmSource.isPlaying)
            {
                bgmSource.DOKill();
                bgmSource.volume = masterBGMVolume;
            }
        }

        /// <summary>
        /// SEを再生します。空いているAudioSourceを自動的に探して鳴らします。
        /// </summary>
        public void PlaySE(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSESource();
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
            seSources.Add(newSource);
            return newSource;
        }
    }
}
