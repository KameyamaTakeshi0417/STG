using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Item
{
    public class ExpItem_Alpha : MonoBehaviour
    {
        public int expValue = 1;
        private Transform playerTransform;
        private bool isCollected = false;

        // オーディオ設定
        [Header("Audio Settings")]
        [Tooltip("ドロップ時のSE（未設定の場合は SE_GetExp_01_Pik が自動ロードされます）")]
        public AudioClip dropSEClip;
        [Tooltip("取得時のSE（未設定の場合は SE_GetExp_04_DryBlip が自動ロードされます）")]
        public AudioClip collectSEClip;
        [Range(0f, 1f)] public float dropSEVolume = 0.65f;
        [Range(0f, 1f)] public float collectSEVolume = 0.75f;

        private static AudioClip cachedDropClip;
        private static AudioClip cachedCollectClip;
        private static float lastDropSoundTime = -1f;
        private static float lastCollectSoundTime = -1f;

        private const float MinDropSoundInterval = 0.06f;    // 同時に複数ドロップした際の重複再生防止
        private const float MinCollectSoundInterval = 0.025f; // 大量回収時の心地よいプチチチ感（同一フレーム重なり防止）

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("Player"))
            {
                Collect();
            }
        }

        void Start()
        {
            // プレイヤーを変数にキャッシュしておく
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            // 出現・ドロップ時SEの再生
            PlayDropSE();

            SetupScatter();
        }

        public void SetupScatter()
        {
            StartCoroutine(Homing());
        }

        private IEnumerator Homing()
        {
            // 3. 散らばるアニメーション
            float scatterDuration = 0.5f;
            float timer = 0f;
            Vector3 startPos = transform.position;
            // ランダムな方向へ散らばる目標位置を計算
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 targetScatterPos = startPos + new Vector3(randomDir.x, randomDir.y, 0f) * Random.Range(1.0f, 2.0f);

            while (timer < scatterDuration)
            {
                if (isCollected) yield break; // すでに回収されていたら中断

                timer += Time.deltaTime;
                // イーズアウト（徐々に減速するような動き）で移動
                float t = timer / scatterDuration;
                t = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(startPos, targetScatterPos, t);
                yield return null;
            }
            if (!isCollected) transform.position = targetScatterPos;

            // 4. 1秒間待機する
            yield return new WaitForSeconds(1.0f);

            // 5. プレイヤーに向かって飛んでいく（ホーミング）
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p == null) p = GameObject.Find("Player");
                if (p != null) playerTransform = p.transform;
            }

            float homingSpeed = 2.0f;
            while (!isCollected && playerTransform != null)
            {
                // 時間経過で徐々に加速する
                homingSpeed += Time.deltaTime * 15f;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, homingSpeed * Time.deltaTime);

                // 念のため、距離が十分に近ければ回収処理を強制的に呼ぶ
                if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
                {
                    Collect();
                    break;
                }

                yield return null;
            }
        }

        private void Collect()
        {
            if (!isCollected)
            {
                isCollected = true;
                
                // 取得時SEの再生
                PlayCollectSE();

                var playerStatus = FindObjectOfType<playerStatusManager_Alpha>();
                if (playerStatus != null)
                {
                    playerStatus.AddExp(expValue);
                }

                if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null)
                {
                    Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnTextPopup(
                        transform.position, 
                        "Orb Acquired!", 
                        new Color(1f, 1f, 0.5f, 1f)
                    );
                }
            }
            // 確実に破壊する
            Destroy(gameObject);
        }

        private void PlayDropSE()
        {
            if (Time.unscaledTime - lastDropSoundTime < MinDropSoundInterval) return;
            lastDropSoundTime = Time.unscaledTime;

            AudioClip clip = dropSEClip;
            if (clip == null)
            {
                if (cachedDropClip == null)
                {
                    cachedDropClip = Resources.Load<AudioClip>("SE/SE_GetExp_01_Pik");
                }
                clip = cachedDropClip;
            }

            if (clip != null)
            {
                if (Alpha.Audio.SoundManager_Alpha.Instance != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(clip, dropSEVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position, dropSEVolume);
                }
            }
        }

        private void PlayCollectSE()
        {
            if (Time.unscaledTime - lastCollectSoundTime < MinCollectSoundInterval) return;
            lastCollectSoundTime = Time.unscaledTime;

            AudioClip clip = collectSEClip;
            if (clip == null)
            {
                if (cachedCollectClip == null)
                {
                    cachedCollectClip = Resources.Load<AudioClip>("SE/SE_GetExp_04_DryBlip");
                }
                clip = cachedCollectClip;
            }

            if (clip != null)
            {
                if (Alpha.Audio.SoundManager_Alpha.Instance != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(clip, collectSEVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position, collectSEVolume);
                }
            }
        }
    }
}
