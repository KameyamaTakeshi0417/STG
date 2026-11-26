using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Tsubaki
{
    /// <summary>
    /// Phase 1: 『落ち椿・時間差トラップ』
    /// 設置時は静かに浮かび、時間差で「ポトリ」と沈み込み発光して自機狙い交差弾を発射する。
    /// ストーリーモードの不完全演出として、一部が起動直前に赤いエラーノイズを吐いて不発になり、花弁だけ散る。
    /// </summary>
    public class TsubakiTrapCamellia : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private float activateDelay = 2.5f;
        [SerializeField] private GameObject needleBulletPrefab;
        [SerializeField] private int needleShotsCount = 2;
        [SerializeField] private float needleSpeed = 9f;
        [SerializeField] private float needleDamage = 15f;
        [SerializeField] private float misfireChance = 0.25f; // ストーリーモード不発率

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer mainRenderer;
        [SerializeField] private GameObject misfireGlitchObj;

        private bool isMisfire = false;
        private bool isActivated = false;
        private bool isVanishing = false;
        private Vector3 initialPos;

        private void Awake()
        {
            if (mainRenderer == null) mainRenderer = GetComponentInChildren<SpriteRenderer>();
            if (misfireGlitchObj != null) misfireGlitchObj.SetActive(false);
        }

        public void Initialize(bool forceMisfire = false, float delay = 2.5f)
        {
            activateDelay = delay;
            isMisfire = forceMisfire || (Random.value < misfireChance);
            isActivated = false;
            isVanishing = false;
            initialPos = transform.position;

            StartCoroutine(TrapSequenceRoutine());
        }

        private IEnumerator TrapSequenceRoutine()
        {
            // 1. 設置待機
            yield return new WaitForSeconds(activateDelay - 0.6f);

            if (isVanishing) yield break;

            if (isMisfire)
            {
                // ストーリーモード不完全演出: 赤いエラーノイズを明滅して不発
                yield return StartCoroutine(MisfireSequenceRoutine());
            }
            else
            {
                // 正常起動: 「ポトリ」と沈み込む予備動作 + 発光
                yield return StartCoroutine(DropAndShootRoutine());
            }
        }

        /// <summary>
        /// 正常な落ち椿動作: 一瞬沈み込んで強く発光し、交差弾を発射
        /// </summary>
        private IEnumerator DropAndShootRoutine()
        {
            float sinkTime = 0.35f;
            float elapsed = 0f;
            Vector3 sinkTarget = initialPos + new Vector3(0, -0.25f, 0);

            Color baseColor = mainRenderer != null ? mainRenderer.color : Color.white;
            Color flashColor = Color.white;

            // ポトリと沈み込み + 発光
            while (elapsed < sinkTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / sinkTime;
                transform.position = Vector3.Lerp(initialPos, sinkTarget, t);
                if (mainRenderer != null)
                {
                    mainRenderer.color = Color.Lerp(baseColor, flashColor, Mathf.PingPong(t * 2f, 1f));
                }
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            if (isVanishing) yield break;

            // 自機狙い交差弾を発射
            ShootNeedlesAtPlayer();

            // 発射後、椿の花は静かに役目を終えて消える
            yield return StartCoroutine(VanishRoutine(0.5f));
        }

        private void ShootNeedlesAtPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector2 targetPos = player != null ? (Vector2)player.transform.position : (Vector2)transform.position + Vector2.down;
            Vector2 baseDir = (targetPos - (Vector2)transform.position).normalized;

            for (int i = 0; i < needleShotsCount; i++)
            {
                // 複数発の場合はわずかに角度をずらして交差
                float spreadAngle = (i - (needleShotsCount - 1) * 0.5f) * 12f;
                Vector2 shotDir = Quaternion.Euler(0, 0, spreadAngle) * baseDir;

                GameObject bulletObj = null;
                if (Alpha_ObjectPoolManager.Instance != null && needleBulletPrefab != null)
                {
                    bulletObj = Alpha_ObjectPoolManager.Instance.Rent(needleBulletPrefab, transform.position, Quaternion.identity);
                }
                else if (needleBulletPrefab != null)
                {
                    bulletObj = Instantiate(needleBulletPrefab, transform.position, Quaternion.identity);
                }

                if (bulletObj != null)
                {
                    var needle = bulletObj.GetComponent<TsubakiNeedleBullet>();
                    if (needle != null)
                    {
                        needle.Initialize(shotDir, needleSpeed, needleDamage);
                    }
                }
            }
        }

        /// <summary>
        /// 不完全演出: 赤いグリッチノイズを吐いて不発になり、花弁が力なく散る
        /// </summary>
        private IEnumerator MisfireSequenceRoutine()
        {
            if (misfireGlitchObj != null) misfireGlitchObj.SetActive(true);

            // 赤グリッチ明滅
            float glitchDuration = 0.5f;
            float elapsed = 0f;
            while (elapsed < glitchDuration)
            {
                elapsed += Time.deltaTime;
                if (mainRenderer != null)
                {
                    mainRenderer.color = (Random.value > 0.5f) ? new Color(1f, 0.3f, 0.4f, 1f) : new Color(0.4f, 0.05f, 0.1f, 0.8f);
                }
                yield return null;
            }

            if (misfireGlitchObj != null) misfireGlitchObj.SetActive(false);

            // 花弁が力なく散りながら落ちる
            yield return StartCoroutine(VanishRoutine(0.8f, true));
        }

        /// <summary>
        /// 外部（ボス決着演出など）からの強制停止・消滅
        /// </summary>
        public void ForceStopAndVanish()
        {
            if (isVanishing) return;
            isVanishing = true;
            StopAllCoroutines();
            if (misfireGlitchObj != null) misfireGlitchObj.SetActive(false);
            StartCoroutine(VanishRoutine(0.4f, true));
        }

        private IEnumerator VanishRoutine(float duration, bool fallDown = false)
        {
            isVanishing = true;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Color startColor = mainRenderer != null ? mainRenderer.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (fallDown)
                {
                    transform.position = startPos + new Vector3(0, -elapsed * 0.8f, 0);
                }

                if (mainRenderer != null)
                {
                    mainRenderer.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
                }
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
