using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Enemy.Chicory
{
    public enum RootAiType
    {
        Smart, // 秀才タイプ: プレイヤーの移動先を先読み
        Loyal, // 真面目タイプ: 停止瞬間のプレイヤー現在地へ直進
        Goofy  // マヌケタイプ: プレイヤー方向から大きくブレてテキトーに伸長
    }

    /// <summary>
    /// 三女・チコリの看板技「3種AIカクカク伸びる根ライン (Root Trace Line)」
    /// 初動直進 -> カクカク・ホーミング(止まる・節点発光・高速直進) -> 壁として滞在 -> フェード消滅
    /// </summary>
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class ChicoryRootLine : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private EdgeCollider2D edgeCollider;
        [SerializeField] private Transform headTransform;
        [SerializeField] private GameObject nodeVfxPrefab;

        [Header("AI & Movement Settings")]
        [SerializeField] private RootAiType aiType = RootAiType.Loyal;
        [SerializeField] private float damage = 10f;
        [Tooltip("初動直進にかける時間（秒）")]
        [SerializeField] private float initialStraightDuration = 1.3f;
        [Tooltip("初動直進の速度")]
        [SerializeField] private float initialSpeed = 8f;
        [Tooltip("カクカク曲がるステップ数")]
        [SerializeField] private int totalSteps = 3;
        [Tooltip("1ステップでの伸長速度")]
        [SerializeField] private float stepSpeed = 12f;
        [Tooltip("1ステップでの伸長距離")]
        [SerializeField] private float stepDistance = 3.8f;
        [Tooltip("方向転換前の停止・予兆時間（秒）")]
        [SerializeField] private float pauseBeforeStep = 0.2f;
        [Tooltip("伸び切った後に壁として滞在する時間（秒）")]
        [SerializeField] private float wallPersistDuration = 2.2f;
        [Tooltip("フェードアウト消滅にかける時間（秒）")]
        [SerializeField] private float fadeDuration = 0.8f;

        public System.Action OnLastStepStarted;

        private List<Vector3> linePoints = new List<Vector3>();
        private List<Vector2> colliderPoints = new List<Vector2>();
        private Transform playerTransform;
        private Rigidbody2D playerRb;
        private bool isForceStopped = false;
        private bool isDamageActive = true;

        private void Awake()
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            if (edgeCollider == null) edgeCollider = GetComponent<EdgeCollider2D>();
            edgeCollider.isTrigger = true;
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerRb = player.GetComponent<Rigidbody2D>();
            }
        }

        public void Initialize(RootAiType type, System.Action onLastStepCallback = null)
        {
            this.aiType = type;
            this.OnLastStepStarted = onLastStepCallback;

            // AIタイプごとの視覚的なさりげない差別化（仕様書準拠）
            Color startCol = new Color(0.9f, 0.95f, 1f, 1f); // 芯の白
            Color endCol = new Color(0.43f, 0.78f, 0.96f, 1f); // チコリブルー(#6EC6F5)

            if (aiType == RootAiType.Smart)
            {
                endCol = new Color(0.7f, 0.9f, 1f, 1f); // 秀才: シャープな青白光
            }
            else if (aiType == RootAiType.Goofy)
            {
                endCol = new Color(0.62f, 0.85f, 0.94f, 1f); // マヌケ: わずかに淡い水色
            }

            lineRenderer.startColor = startCol;
            lineRenderer.endColor = endCol;
            lineRenderer.positionCount = 0;

            StartCoroutine(LineRoutine());
        }

        private IEnumerator LineRoutine()
        {
            Vector3 currentPos = transform.position;
            AddPoint(currentPos);

            // 1. 初動直進（画面左端から右へまっすぐ）
            Vector3 currentDir = Vector3.right;
            float initialElapsed = 0f;

            while (initialElapsed < initialStraightDuration)
            {
                if (isForceStopped) yield break;
                float dt = Time.deltaTime;
                initialElapsed += dt;
                currentPos += currentDir * (initialSpeed * dt);
                UpdateHeadPoint(currentPos);
                yield return null;
            }

            // 頂点を確定
            AddPoint(currentPos);

            // 2. カクカク・ホーミング（止まる -> 節点発光 -> 高速直進 -> 止まる）
            for (int step = 0; step < totalSteps; step++)
            {
                if (isForceStopped) yield break;

                // 最後のステップ開始時に次の4本生成を通知！
                if (step == totalSteps - 1)
                {
                    OnLastStepStarted?.Invoke();
                }

                // 停止＆節点エフェクト（ポッと点灯する光輪）
                SpawnNodeVfx(currentPos);
                yield return new WaitForSeconds(pauseBeforeStep);
                if (isForceStopped) yield break;

                // 次の方向を決定（AIタイプ別）
                currentDir = DetermineNextDirection(currentPos);

                // 高速直進
                float movedDist = 0f;
                while (movedDist < stepDistance)
                {
                    if (isForceStopped) yield break;
                    float dt = Time.deltaTime;
                    float moveDelta = stepSpeed * dt;
                    if (movedDist + moveDelta > stepDistance)
                    {
                        moveDelta = stepDistance - movedDist;
                    }
                    movedDist += moveDelta;
                    currentPos += currentDir * moveDelta;
                    UpdateHeadPoint(currentPos);
                    yield return null;
                }

                // このステップの終端点を確定
                AddPoint(currentPos);
            }

            // 先端ヘッドの非表示
            if (headTransform != null) headTransform.gameObject.SetActive(false);

            // 3. 伸び切った後、壁として一定時間滞在
            yield return new WaitForSeconds(wallPersistDuration);
            if (isForceStopped) yield break;

            // 4. フェードアウト消滅（親切設計：完全に消える少し前にダメージ判定をオフにする）
            isDamageActive = false;
            if (edgeCollider != null) edgeCollider.enabled = false;

            float fadeElapsed = 0f;
            Color initStartCol = lineRenderer.startColor;
            Color initEndCol = lineRenderer.endColor;

            while (fadeElapsed < fadeDuration)
            {
                if (isForceStopped) yield break;
                fadeElapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);

                Color s = initStartCol; s.a = alpha;
                Color e = initEndCol; e.a = alpha;
                lineRenderer.startColor = s;
                lineRenderer.endColor = e;
                yield return null;
            }

            Destroy(gameObject, 0.2f);
        }

        private Vector3 DetermineNextDirection(Vector3 originPos)
        {
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }
            if (playerTransform == null) return Vector3.right;

            Vector3 targetPos = playerTransform.position;

            switch (aiType)
            {
                case RootAiType.Smart:
                    // 秀才タイプ: 移動速度ベクトルから先読み（偏差撃ち）
                    if (playerRb != null && playerRb.velocity.sqrMagnitude > 0.1f)
                    {
                        targetPos += (Vector3)playerRb.velocity * 0.45f;
                    }
                    break;

                case RootAiType.Loyal:
                    // 真面目タイプ: 現在地へ素直に伸びる
                    break;

                case RootAiType.Goofy:
                    // マヌケタイプ: ±35°〜50°のランダム大ブレでテキトーに伸びる
                    Vector3 baseDir = (targetPos - originPos).normalized;
                    float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                    float randomOffset = (Random.value > 0.5f ? 1f : -1f) * Random.Range(35f, 50f);
                    float newAngle = (baseAngle + randomOffset) * Mathf.Deg2Rad;
                    return new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f).normalized;
            }

            Vector3 dir = (targetPos - originPos).normalized;
            return dir != Vector3.zero ? dir : Vector3.right;
        }

        private void AddPoint(Vector3 point)
        {
            linePoints.Add(point);
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());
            UpdateCollider();
        }

        private void UpdateHeadPoint(Vector3 point)
        {
            if (linePoints.Count == 0)
            {
                AddPoint(point);
                return;
            }

            linePoints[linePoints.Count - 1] = point;
            lineRenderer.SetPosition(linePoints.Count - 1, point);

            if (headTransform != null)
            {
                headTransform.position = point;
            }

            UpdateCollider();
        }

        private void UpdateCollider()
        {
            if (edgeCollider == null || linePoints.Count < 2) return;

            colliderPoints.Clear();
            for (int i = 0; i < linePoints.Count; i++)
            {
                // ローカル座標に変換して EdgeCollider にセット
                colliderPoints.Add(transform.InverseTransformPoint(linePoints[i]));
            }
            edgeCollider.SetPoints(colliderPoints);
        }

        private void SpawnNodeVfx(Vector3 pos)
        {
            if (nodeVfxPrefab != null)
            {
                GameObject vfx = Instantiate(nodeVfxPrefab, pos, Quaternion.identity);
                Destroy(vfx, 0.6f);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!isDamageActive || isForceStopped) return;

            if (collision.CompareTag("Player"))
            {
                Health playerHealth = collision.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        /// <summary>
        /// ボス決着演出用：即座に静止し、ダメージ判定を無効化して青い光の粉になって消滅
        /// </summary>
        public void ForceStopAndVanish()
        {
            isForceStopped = true;
            isDamageActive = false;
            StopAllCoroutines();
            if (edgeCollider != null) edgeCollider.enabled = false;
            if (headTransform != null) headTransform.gameObject.SetActive(false);
            StartCoroutine(VanishFadeRoutine());
        }

        private IEnumerator VanishFadeRoutine()
        {
            float elapsed = 0f;
            float duration = 0.4f;
            Color initStartCol = lineRenderer.startColor;
            Color initEndCol = lineRenderer.endColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, elapsed / duration);
                Color s = initStartCol; s.a = a;
                Color e = initEndCol; e.a = a;
                lineRenderer.startColor = s;
                lineRenderer.endColor = e;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
