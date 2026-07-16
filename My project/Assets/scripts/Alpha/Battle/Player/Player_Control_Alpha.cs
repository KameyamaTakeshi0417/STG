using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Control_Alpha : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator animator; // Animator コンポーネントを追加
    public bool onCoolTime;
    playerStatusManager_Alpha myStatus;

    protected Vector2 moveInput;
    protected bool isSpecialMoving = false;
    protected Vector2 specialMoveDirection;
    protected float specialMoveEndTime = 0f;

    [Header("Focus Mode Settings")]
    [Tooltip("右クリック時の移動速度倍率")]
    public float focusSpeedMultiplier = 0.5f;

    [Tooltip("通常時のプレイヤー当たり判定のサイズ")]
    public Vector2 normalColliderSize = new Vector2(0.5f, 0.5f);
    [Tooltip("右クリック時のプレイヤー当たり判定のサイズ")]
    public Vector2 focusColliderSize = new Vector2(0.2f, 0.2f);

    [Tooltip("通常時のグレイズ判定のサイズ")]
    public Vector2 normalGrazeSize = new Vector2(1.0f, 1.0f);
    [Tooltip("右クリック時のグレイズ判定のサイズ")]
    public Vector2 focusGrazeSize = new Vector2(0.6f, 0.6f);

    [Tooltip("当たり判定を表示する画像（プレイヤーの子オブジェクト）")]
    public GameObject hitboxImage;

    [Tooltip("グレイズ判定を持つ子オブジェクトのコライダー")]
    public CapsuleCollider2D grazeCollider;

    private CapsuleCollider2D playerCollider;
    private bool isFocusMode = false;

    // Start is called before the first frame update

    void Start()
    {
        onCoolTime = false;
        myStatus = playerStatusManager_Alpha.Instance;

        playerCollider = GetComponent<CapsuleCollider2D>();
        if (playerCollider != null)
        {
            playerCollider.size = normalColliderSize;
        }

        if (hitboxImage != null)
        {
            hitboxImage.SetActive(false);
        }

        if (grazeCollider != null)
        {
            grazeCollider.size = normalGrazeSize;
        }
    }
    protected virtual void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Animator を取得
        
    }
    // Update is called once per frame

    protected virtual void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (!isSpecialMoving)
        {
            // 右クリック監視（フォーカスモード）
            if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Focus)
            {
                if (Input.GetMouseButtonDown(1) && myStatus.currentStamina > 0 && !myStatus.isStaminaExhausted)
                {
                    isFocusMode = true;
                    if (playerCollider != null) playerCollider.size = focusColliderSize;
                    if (hitboxImage != null) hitboxImage.SetActive(true);
                    if (grazeCollider != null) grazeCollider.size = focusGrazeSize;
                }
                else if (Input.GetMouseButtonUp(1) || myStatus.currentStamina <= 0 || myStatus.isStaminaExhausted)
                {
                    if (isFocusMode)
                    {
                        isFocusMode = false;
                        if (playerCollider != null) playerCollider.size = normalColliderSize;
                        if (hitboxImage != null) hitboxImage.SetActive(false);
                        if (grazeCollider != null) grazeCollider.size = normalGrazeSize;
                    }
                }

                if (isFocusMode)
                {
                    myStatus.currentStamina -= myStatus.focusStaminaCostPerSec * Time.deltaTime;
                    myStatus.lastStaminaConsumeTime = Time.time;
                    if (myStatus.currentStamina <= 0)
                    {
                        myStatus.currentStamina = 0;
                        myStatus.isStaminaExhausted = true;
                        isFocusMode = false;
                        if (playerCollider != null) playerCollider.size = normalColliderSize;
                        if (hitboxImage != null) hitboxImage.SetActive(false);
                        if (grazeCollider != null) grazeCollider.size = normalGrazeSize;
                    }
                }
            }
            else
            {
                if (isFocusMode)
                {
                    isFocusMode = false;
                    if (playerCollider != null) playerCollider.size = normalColliderSize;
                    if (hitboxImage != null) hitboxImage.SetActive(false);
                    if (grazeCollider != null) grazeCollider.size = normalGrazeSize;
                }
            }

            if (isFocusMode)
            {
                moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            else
            {
                moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }
            
            if (moveInput.sqrMagnitude > 1) moveInput.Normalize();

            if (Input.GetMouseButtonDown(1) && myStatus.currentSpecialMove != playerStatusManager_Alpha.SpecialMoveType.None && myStatus.currentSpecialMove != playerStatusManager_Alpha.SpecialMoveType.Focus)
            {
                TrySpecialMove();
            }
        }
        else
        {
            // スペシャルムーブ中は強制的にフォーカス解除
            if (isFocusMode)
            {
                isFocusMode = false;
                if (playerCollider != null) playerCollider.size = normalColliderSize;
                if (hitboxImage != null) hitboxImage.SetActive(false);
                if (grazeCollider != null) grazeCollider.size = normalGrazeSize;
            }
        }

        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // ロックオン対象がいる場合、その方向に向ける

        // Animator パラメータの更新
        UpdateAnimatorParameters();
    }

    protected virtual void TrySpecialMove()
    {
        float cost = 0f;
        float duration = 0f;
        
        if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Dash)
        {
            cost = myStatus.dashStaminaCost;
            duration = myStatus.dashDuration;
        }
        else if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Warp)
        {
            cost = myStatus.warpStaminaCost;
            duration = myStatus.warpDuration;
        }

        if (myStatus.currentStamina >= cost && !myStatus.isStaminaExhausted)
        {
            myStatus.currentStamina -= cost;
            myStatus.lastStaminaConsumeTime = Time.time;
            
            if (myStatus.currentStamina <= 0)
            {
                myStatus.currentStamina = 0;
                myStatus.isStaminaExhausted = true;
            }
            
            if (moveInput == Vector2.zero)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;
                specialMoveDirection = (mousePosition - transform.position).normalized;
            }
            else
            {
                specialMoveDirection = moveInput.normalized;
            }

            if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Dash)
            {
                isSpecialMoving = true;
                specialMoveEndTime = Time.time + duration;
                
                // 追加: ダッシュ時に残像を有効化
                var trail = GetComponent<Alpha.Core.ProceduralGhostTrail_Alpha>();
                if (trail == null) trail = gameObject.AddComponent<Alpha.Core.ProceduralGhostTrail_Alpha>();
                trail.EnableTrail(true);
            }
            else if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Warp)
            {
                StartCoroutine(WarpRoutine(duration, specialMoveDirection));
            }
        }
    }

    private System.Collections.IEnumerator WarpRoutine(float windupTime, Vector2 direction)
    {
        isSpecialMoving = true;
        rb.velocity = Vector2.zero; // ワープ中は完全停止
        
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.isInvincible = true;

        // --- 追加：ワープ中の完全無敵（すり抜け）処理 ---
        // 有効なColliderをすべて一時的に無効化し、弾がヒットしないようにする
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        List<Collider2D> activeColliders = new List<Collider2D>();
        foreach (var col in colliders)
        {
            if (col != null && col.enabled)
            {
                activeColliders.Add(col);
                col.enabled = false;
            }
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        
        float blinkInterval = 0.04f; // 高速明滅
        float timer = 0f;
        bool isVisible = true;

        while (timer < windupTime)
        {
            isVisible = !isVisible;
            foreach (var r in renderers) if (r != null) r.enabled = isVisible;
            
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 表示を元に戻す
        foreach (var r in renderers) if (r != null) r.enabled = true;

        // 瞬間移動実行
        rb.position = rb.position + direction * myStatus.warpDistance;

        // --- 追加：コライダーを復元 ---
        foreach (var col in activeColliders)
        {
            if (col != null) col.enabled = true;
        }

        if (health != null) health.isInvincible = false;
        isSpecialMoving = false;
    }

    private bool isKnockback = false;
    private Vector2 knockbackVelocity;

    public void ApplyKnockback(Vector2 direction, float initialForce, float duration)
    {
        StartCoroutine(KnockbackRoutine(direction, initialForce, duration));
    }

    private System.Collections.IEnumerator KnockbackRoutine(Vector2 direction, float initialForce, float duration)
    {
        isKnockback = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float currentForce = Mathf.Lerp(initialForce, 0f, elapsed / duration);
            knockbackVelocity = direction * currentForce;
            elapsed += Time.deltaTime;
            yield return null;
        }

        knockbackVelocity = Vector2.zero;
        isKnockback = false;
    }

    protected virtual void FixedUpdate()
    {
        if (isKnockback)
        {
            rb.velocity = knockbackVelocity;
            return;
        }
        if (isSpecialMoving)
        {
            if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Dash)
            {
                float dist = myStatus.dashDistance;
                float dur = myStatus.dashDuration;
                float specialSpeed = dist / dur;
                
                rb.MovePosition(rb.position + specialMoveDirection * specialSpeed * Time.fixedDeltaTime);
                rb.velocity = Vector2.zero; // 物理エンジンの慣性を消す
                
                if (Time.time >= specialMoveEndTime)
                {
                    isSpecialMoving = false;
                    
                    // 追加: ダッシュ終了時に残像を無効化
                    var trail = GetComponent<Alpha.Core.ProceduralGhostTrail_Alpha>();
                    if (trail != null) trail.EnableTrail(false);
                }
            }
            else if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Warp)
            {
                rb.velocity = Vector2.zero; // ワープ中は完全停止
            }
            return;
        }

        // 入力がない場合は何もしない
        if (moveInput == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftShift)) {
                //ここに入れたい
                rb.angularVelocity = 0f;   // 念のため（回転してないなら不要）
                rb.Sleep();                // 物理的に「寝かせて」微振動/押し出しを止める
            }
            return;
        }

        // キャラクターを移動させる
        float setSpd = (myStatus.moveSpeed * myStatus.moveSpeedMag * 0.0001f);
        float finalSpeed = setSpd * myStatus.moveSpeedMag_CONST;

        if (setSpd <= 0 || Input.GetKey(KeyCode.LeftShift))
        {
            finalSpeed = 0.79f;
        }
        
        if (isFocusMode)
        {
            finalSpeed *= focusSpeedMultiplier;
        }

        Vector2 targetVelocity = moveInput * finalSpeed;
        rb.MovePosition(rb.position + targetVelocity * Time.fixedDeltaTime);
        rb.velocity = Vector2.zero; // 慣性による滑り対策
    }

    protected virtual void UpdateAnimatorParameters()
    {
        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        Vector2 currentVelocity = Vector2.zero;
        if (isSpecialMoving)
        {
            if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Dash)
            {
                float dist = myStatus.dashDistance;
                float dur = myStatus.dashDuration;
                currentVelocity = specialMoveDirection * (dist / dur);
            }
            // Warp is completely zero velocity during windup, so leave currentVelocity = 0
        }
        else if (moveInput != Vector2.zero)
        {
            float setSpd = (myStatus.moveSpeed * myStatus.moveSpeedMag * 0.0001f);
            float finalSpeed = setSpd * myStatus.moveSpeedMag_CONST;
            if (setSpd <= 0 || Input.GetKey(KeyCode.LeftShift)) finalSpeed = 0.79f;
            if (isFocusMode) finalSpeed *= focusSpeedMultiplier;
            currentVelocity = moveInput * finalSpeed;
        }

        // マウス位置のX座標をAnimatorパラメータに設定
        float mouseXPosition = currentVelocity.x; // mousePosition.x;

        // プレイヤーの移動ベクトルの大きさを計算
        float moveVectorMag = currentVelocity.magnitude;

        // Animator パラメータを設定
        animator.SetFloat("mouseXPosition", mouseXPosition);
        animator.SetFloat("moveVectorMag", moveVectorMag);
    }
}
