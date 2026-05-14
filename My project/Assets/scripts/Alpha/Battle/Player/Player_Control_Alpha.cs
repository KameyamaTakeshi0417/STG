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

    // Start is called before the first frame update

    void Start()
    {
        onCoolTime = false;
        myStatus=GameObject.Find("manager").GetComponent<playerStatusManager_Alpha>();
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
            moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (moveInput.sqrMagnitude > 1) moveInput.Normalize();

            if (Input.GetMouseButtonDown(1) && myStatus.currentSpecialMove != playerStatusManager_Alpha.SpecialMoveType.None)
            {
                TrySpecialMove();
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

        if (myStatus.currentStamina >= cost)
        {
            myStatus.currentStamina -= cost;
            myStatus.lastStaminaConsumeTime = Time.time;
            
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

    protected virtual void FixedUpdate()
    {
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
