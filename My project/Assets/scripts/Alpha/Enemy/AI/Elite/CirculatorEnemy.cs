using System.Collections;
using UnityEngine;

// IAlphaPoolableを実装しておくことで、オブジェクトプーリングにも対応可能
[RequireComponent(typeof(Alpha_EnemyAI))]
public class CirculatorEnemy : _Health_Base, IAlphaPoolable
{
    [Header("Circulator Settings")]
    [Tooltip("破壊可能かどうか。falseの場合は弾を当ててもダメージを受けません。")]
    public bool isDestructible = true;

    [Header("Movement Settings")]
    public Vector2 moveDirection = Vector2.right;
    public float moveDistance = 5f;
    public float moveSpeed = 3f;

    [Header("Screen Side Vertical Patrol")]
    [Tooltip("ONにするとカメラの左端または右端に沿って上下に往復し続けます")]
    public bool useScreenSideVerticalPatrol = false;
    [Tooltip("配置する画面端（-1: 左端, +1: 右端）")]
    public int screenSide = -1;
    [Tooltip("左右の画面端からの内側マージン（Viewport比率 0.0〜0.5）")]
    public float screenEdgeMarginX = 0.08f;
    [Tooltip("上下の画面端からの内側マージン（Viewport比率 0.0〜0.45）")]
    public float screenEdgeMarginY = 0.15f;
    [Tooltip("画面端パトロール時の初期Y進行度（0=下端, 0.5=中央, 1=上端）")]
    [Range(0f, 1f)] public float initialVerticalProgress = 0.5f;
    [Tooltip("画面端パトロール時の初期移動方向（true=上方向へ, false=下方向へ）")]
    public bool startMovingUp = true;

    [Header("Rotation Settings")]
    public bool rotationEnabled = true;
    public float angularSpeed = 180f; // 1秒あたりの回転角度

    [Header("Visual Settings")]
    [Tooltip("サーキュレーターの移動ルートをディレクションラインとして表示するかどうか")]
    public bool showDirectionLine = true;
    public float lineWidth = 0.05f;
    public Color lineColor = new Color(1f, 0f, 0f, 0.5f);
    private LineRenderer directionLine;

    [Header("Attack Settings (Behavior)")]
    [Tooltip("攻撃挙動（OmniBarrage等のScriptableObjectをアサイン）。設定されている場合はこちらの挙動が優先されます。")]
    public EnemyBehaviorData_Base attackBehavior;

    [Header("Attack Settings (Simple - Used if Behavior is null)")]
    public bool enableAttack = true;
    public float attackInterval = 1f;
    public int bulletsPerShot = 8;
    public float bulletSpeed = 5f;
    public GameObject bulletPrefab;

    private Alpha_EnemyAI aiComponent;
    private Coroutine attackCoroutine;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool movingToEnd = true;
    private float moveProgress = 0f;

    private float verticalPatrolProgress = 0.5f;
    private bool movingUp = true;

    private float attackTimer = 0f;

    /// <summary>
    /// 画面の左端または右端に沿って上下に往復し続けるモードを設定する
    /// </summary>
    public void ConfigureScreenSidePatrol(int side, float marginX, float marginY, float speed, float initProgress, bool moveUp)
    {
        useScreenSideVerticalPatrol = true;
        screenSide = side < 0 ? -1 : 1;
        screenEdgeMarginX = Mathf.Clamp(marginX, 0.02f, 0.45f);
        screenEdgeMarginY = Mathf.Clamp(marginY, 0.02f, 0.45f);
        if (speed > 0f) moveSpeed = speed;
        initialVerticalProgress = Mathf.Clamp01(initProgress);
        startMovingUp = moveUp;

        InitializePosition();
    }

    protected override void Awake()
    {
        base.Awake();
        // 初期HPを設定
        currentHP = HP;
    }

    private void Start()
    {
        aiComponent = GetComponent<Alpha_EnemyAI>();
        // 自前で移動するため物理演算の影響を受けないようにする
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        InitializePosition();

        if (attackBehavior != null && aiComponent != null)
        {
            attackCoroutine = StartCoroutine(attackBehavior.RunBehavior(aiComponent));
        }
    }

    private void EnsureDirectionLine()
    {
        if (showDirectionLine)
        {
            if (directionLine == null)
            {
                directionLine = gameObject.AddComponent<LineRenderer>();
                directionLine.material = new Material(Shader.Find("Sprites/Default"));
                directionLine.startWidth = lineWidth;
                directionLine.endWidth = lineWidth;
                directionLine.startColor = lineColor;
                directionLine.endColor = lineColor;
                directionLine.positionCount = 2;
                directionLine.useWorldSpace = true;
                directionLine.sortingOrder = -10; // 後ろに描画
            }
            directionLine.enabled = true;
        }
        else if (directionLine != null)
        {
            directionLine.enabled = false;
        }
    }

    private bool TryGetScreenSideEndpoints(out Vector3 bottomWorld, out Vector3 topWorld)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            bottomWorld = transform.position + Vector3.down * 8f;
            topWorld = transform.position + Vector3.up * 8f;
            return false;
        }

        float vx = (screenSide < 0) ? screenEdgeMarginX : (1f - screenEdgeMarginX);
        float zDist = Mathf.Abs(cam.transform.position.z);

        bottomWorld = cam.ViewportToWorldPoint(new Vector3(vx, screenEdgeMarginY, zDist));
        topWorld = cam.ViewportToWorldPoint(new Vector3(vx, 1f - screenEdgeMarginY, zDist));
        bottomWorld.z = 0f;
        topWorld.z = 0f;
        return true;
    }

    public void InitializePosition()
    {
        EnsureDirectionLine();

        if (useScreenSideVerticalPatrol)
        {
            verticalPatrolProgress = Mathf.Clamp01(initialVerticalProgress);
            movingUp = startMovingUp;

            TryGetScreenSideEndpoints(out Vector3 bottomWorld, out Vector3 topWorld);
            transform.position = Vector3.Lerp(bottomWorld, topWorld, verticalPatrolProgress);

            if (directionLine != null && directionLine.enabled)
            {
                directionLine.SetPosition(0, bottomWorld);
                directionLine.SetPosition(1, topWorld);
            }
            return;
        }

        startPos = transform.position;
        // moveDirectionを正規化して使用
        endPos = startPos + (moveDirection.normalized * moveDistance);
        movingToEnd = true;
        moveProgress = 0f;

        if (directionLine != null && directionLine.enabled)
        {
            directionLine.SetPosition(0, startPos);
            directionLine.SetPosition(1, endPos);
        }
    }

    // プールから呼び出された時の初期化
    public void OnRentFromPool()
    {
        currentHP = HP;
        InitializePosition();
        attackTimer = 0f;

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (attackBehavior != null && aiComponent != null)
        {
            attackCoroutine = StartCoroutine(attackBehavior.RunBehavior(aiComponent));
        }
    }

    // プールに戻る時の処理
    public void OnReturnToPool()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = null;
        // エフェクトのクリアなどがあればここに記述
    }

    protected override void Update()
    {
        base.Update();
        HandleMovement();
        HandleRotation();
        HandleAttack();
    }

    private void HandleMovement()
    {
        if (useScreenSideVerticalPatrol)
        {
            if (moveSpeed <= 0f) return;

            TryGetScreenSideEndpoints(out Vector3 bottomWorld, out Vector3 topWorld);
            float span = Mathf.Max(0.1f, Vector2.Distance(bottomWorld, topWorld));
            float deltaProgress = (moveSpeed * Time.deltaTime) / span;

            if (movingUp)
            {
                verticalPatrolProgress += deltaProgress;
                if (verticalPatrolProgress >= 1f)
                {
                    verticalPatrolProgress = 1f - (verticalPatrolProgress - 1f);
                    movingUp = false;
                }
            }
            else
            {
                verticalPatrolProgress -= deltaProgress;
                if (verticalPatrolProgress <= 0f)
                {
                    verticalPatrolProgress = -verticalPatrolProgress;
                    movingUp = true;
                }
            }

            verticalPatrolProgress = Mathf.Clamp01(verticalPatrolProgress);
            transform.position = Vector3.Lerp(bottomWorld, topWorld, verticalPatrolProgress);

            if (directionLine != null && directionLine.enabled)
            {
                directionLine.SetPosition(0, bottomWorld);
                directionLine.SetPosition(1, topWorld);
            }
            return;
        }

        if (moveDistance <= 0 || moveSpeed <= 0) return;

        // Ping-Pong移動の進行度を更新
        float distance = Vector2.Distance(startPos, endPos);
        float timeToComplete = distance / moveSpeed;
        
        moveProgress += Time.deltaTime / timeToComplete;

        if (moveProgress >= 1f)
        {
            moveProgress = 0f;
            movingToEnd = !movingToEnd; // 向きを反転
        }

        // 現在地を計算
        Vector2 target = movingToEnd ? endPos : startPos;
        Vector2 origin = movingToEnd ? startPos : endPos;
        
        // Easing等を入れたい場合はここで moveProgress を加工できる
        transform.position = Vector2.Lerp(origin, target, moveProgress);
    }

    private void HandleRotation()
    {
        if (!rotationEnabled) return;

        transform.Rotate(0, 0, angularSpeed * Time.deltaTime);
    }

    private void HandleAttack()
    {
        // Behaviorが設定されている場合はそちらに任せるので何もしない
        if (attackBehavior != null) return;

        if (!enableAttack || bulletPrefab == null || bulletsPerShot <= 0) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            FireBullets();
            attackTimer = attackInterval;
        }
    }

    private void FireBullets()
    {
        float angleStep = 360f / bulletsPerShot;
        float baseAngle = transform.eulerAngles.z; // 自身の回転に合わせる

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = baseAngle + (angleStep * i);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject bObj = null;
            if (Alpha_ObjectPoolManager.Instance != null)
            {
                bObj = Alpha_ObjectPoolManager.Instance.Rent(bulletPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                bObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            }

            if (bObj != null)
            {
                Bullet_Base bullet = bObj.GetComponent<Bullet_Base>();
                if (bullet != null)
                {
                    bullet.setStatus(dir, bulletSpeed, 10f); // 寿命は10秒等適当に設定
                    bullet.shoot();
                }
            }
        }
    }

    // ダメージ処理の上書き
    public override void TakeDamage(float damage)
    {
        if (!isDestructible) return; // 破壊不能ならダメージを無視

        currentHP -= damage;
        // Debug.Log("Circulator took damage! HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 共通のドロップ処理（経験値、オーブ、花弁）を呼び出す
        DropEnemyRewards();

        // プール対応の場合は破壊せずに返却
        if (Alpha_ObjectPoolManager.Instance != null && gameObject.activeInHierarchy)
        {
            // TODO: 本当はプレハブの参照が必要だが、ここではシンプルに破壊か非アクティブにする
            // 厳密にはAlpha_ObjectPoolManager.Returnにはプレハブの参照が必要。
            // 簡単のため、今回はDestroyするか非アクティブ化する。
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
