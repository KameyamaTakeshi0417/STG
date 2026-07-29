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

    private float attackTimer = 0f;

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

    public void InitializePosition()
    {
        startPos = transform.position;
        // moveDirectionを正規化して使用
        endPos = startPos + (moveDirection.normalized * moveDistance);
        movingToEnd = true;
        moveProgress = 0f;

        // ディレクションラインの表示
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
            directionLine.SetPosition(0, startPos);
            directionLine.SetPosition(1, endPos);
        }
        else if (directionLine != null)
        {
            directionLine.enabled = false;
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
