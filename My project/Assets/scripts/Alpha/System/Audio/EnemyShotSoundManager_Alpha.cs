using System.Collections.Generic;
using UnityEngine;
using Alpha.Audio;

namespace Alpha.Audio
{
    /// <summary>
    /// 敵の攻撃（弾幕斉射・弾生成）時のSEを一括管理するマネージャー。
    /// 斉射タイミング（メイン）と弾一発ごとの生成（特殊パターン）の両方に対応し、
    /// ザコ敵用の東方風「じゅわーん！」とボス・強攻撃用の強化版「ヘビージュワーン！」を制御します。
    /// </summary>
    public class EnemyShotSoundManager_Alpha : MonoBehaviour
    {
        private static EnemyShotSoundManager_Alpha _instance;
        public static EnemyShotSoundManager_Alpha Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EnemyShotSoundManager_Alpha>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("EnemyShotSoundManager_Alpha");
                        _instance = go.AddComponent<EnemyShotSoundManager_Alpha>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Clips")]
        [Tooltip("最弱ザコ敵（mob1など）用：軽い小粒ショット発射音")]
        public AudioClip lightShotClip;

        [Tooltip("ザコ敵用：東方風じゅわーん！発射音")]
        public AudioClip normalJuwanClip;

        [Tooltip("ボス・強攻撃用：重厚・低音強化版じゅわーん！発射音")]
        public AudioClip heavyJuwanClip;

        [Tooltip("特殊パターン用：弾1発ごとの生成音")]
        public AudioClip singleBulletClip;

        [Header("Audio Settings")]
        [Tooltip("中型敵や通常弾幕で強化版重厚SE（SE_EnemyShot_Touhou_Juwan_Heavy）を優先するか")]
        public bool alwaysUseHeavySE = true;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float lightShotVolume = 0.65f;
        [Range(0f, 1f)] public float masterShotVolume = 0.75f;
        [Range(0f, 1f)] public float heavyShotVolume = 0.9f;
        [Range(0f, 1f)] public float singleBulletVolume = 0.5f;

        [Header("Limiter & Concurrency Settings")]
        [Tooltip("同じ種類のSEが連続して重なるのを抑える最小間隔（秒）。0.04秒程度で音割れや爆音を防止します。")]
        public float minLightInterval = 0.035f;
        public float minBarrageInterval = 0.045f;
        public float minHeavyInterval = 0.08f;
        public float minSingleBulletInterval = 0.025f;

        private float lastLightShotTime = -1f;
        private float lastNormalBarrageTime = -1f;
        private float lastHeavyBarrageTime = -1f;
        private float lastSingleBulletTime = -1f;

        private AudioSource dedicatedSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureClipsLoaded();
            EnsureAudioSource();
        }

        private void EnsureAudioSource()
        {
            if (dedicatedSource == null)
            {
                dedicatedSource = gameObject.AddComponent<AudioSource>();
                dedicatedSource.playOnAwake = false;
                dedicatedSource.spatialBlend = 0f; // 2D明瞭再生
            }
        }

        public void EnsureClipsLoaded()
        {
            if (lightShotClip == null)
            {
                lightShotClip = Resources.Load<AudioClip>("SE/SE_EnemyShot_Mob_Light");
            }
            if (normalJuwanClip == null)
            {
                normalJuwanClip = Resources.Load<AudioClip>("SE/SE_EnemyShot_Touhou_Juwan");
            }
            if (heavyJuwanClip == null)
            {
                heavyJuwanClip = Resources.Load<AudioClip>("SE/SE_EnemyShot_Touhou_Juwan_Heavy");
            }
            if (singleBulletClip == null)
            {
                singleBulletClip = Resources.Load<AudioClip>("SE/SE_Shot_Touhou_01_Zu");
                if (singleBulletClip == null)
                {
                    singleBulletClip = Resources.Load<AudioClip>("SE/SE_EnemyShot_01_Po");
                }
            }
        }

        /// <summary>
        /// 【mob1など最弱ザコ用】軽快で小粒なショット音を再生します。
        /// </summary>
        public void PlayLightShotSE(Vector3 position, float volumeScale = 1.0f)
        {
            EnsureClipsLoaded();

            float currentTime = Time.unscaledTime;
            if (currentTime - lastLightShotTime < minLightInterval) return;
            lastLightShotTime = currentTime;

            AudioClip clip = lightShotClip != null ? lightShotClip : normalJuwanClip;
            if (clip != null)
            {
                PlayClipWithPitch(clip, lightShotVolume * volumeScale, Random.Range(0.97f, 1.05f));
            }
        }

        /// <summary>
        /// 【メイン処理】弾幕の生成・斉射タイミングでSEを再生します。
        /// </summary>
        /// <param name="position">発射元の敵ワールド座標</param>
        /// <param name="isHeavy">trueならボス・強攻撃用の強化版重厚SE、falseならザコ用東方風じゅわーん！SE（alwaysUseHeavySEが有効な場合は常にHeavy版を再生）</param>
        /// <param name="volumeScale">音量の倍率</param>
        public void PlayBarrageShotSE(Vector3 position, bool isHeavy = false, float volumeScale = 1.0f)
        {
            EnsureClipsLoaded();

            float currentTime = Time.unscaledTime;
            bool useHeavy = alwaysUseHeavySE || isHeavy;

            if (useHeavy)
            {
                if (currentTime - lastHeavyBarrageTime < minHeavyInterval) return;
                lastHeavyBarrageTime = currentTime;

                AudioClip clipToPlay = heavyJuwanClip != null ? heavyJuwanClip : normalJuwanClip;
                if (clipToPlay != null)
                {
                    PlayClipWithPitch(clipToPlay, heavyShotVolume * volumeScale, Random.Range(0.96f, 1.04f));
                }
            }
            else
            {
                if (currentTime - lastNormalBarrageTime < minBarrageInterval) return;
                lastNormalBarrageTime = currentTime;

                if (normalJuwanClip != null)
                {
                    PlayClipWithPitch(normalJuwanClip, masterShotVolume * volumeScale, Random.Range(0.97f, 1.03f));
                }
            }
        }

        /// <summary>
        /// 【特殊パターン用】弾一発ごとの生成時にSEを再生します。
        /// </summary>
        /// <param name="position">弾の生成位置</param>
        /// <param name="isHeavy">trueならやや低音、falseなら通常</param>
        /// <param name="volumeScale">音量の倍率</param>
        public void PlaySingleBulletSE(Vector3 position, bool isHeavy = false, float volumeScale = 1.0f)
        {
            EnsureClipsLoaded();

            float currentTime = Time.unscaledTime;
            if (currentTime - lastSingleBulletTime < minSingleBulletInterval) return;
            lastSingleBulletTime = currentTime;

            AudioClip clip = singleBulletClip != null ? singleBulletClip : normalJuwanClip;
            if (clip != null)
            {
                float pitch = isHeavy ? Random.Range(0.85f, 0.95f) : Random.Range(1.05f, 1.18f);
                PlayClipWithPitch(clip, singleBulletVolume * volumeScale, pitch);
            }
        }

        private void PlayClipWithPitch(AudioClip clip, float volume, float pitch)
        {
            if (clip == null) return;

            // SoundManager_Alpha または 内蔵AudioSourceを使用
            if (SoundManager_Alpha.Instance != null)
            {
                SoundManager_Alpha.Instance.PlaySE(clip, volume);
            }
            else
            {
                EnsureAudioSource();
                dedicatedSource.pitch = pitch;
                dedicatedSource.PlayOneShot(clip, volume);
            }
        }
    }
}
