using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Control_Alpha : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator animator; // Animator 郢ｧ・ｳ郢晢ｽｳ郢晄亢繝ｻ郢晞亂ﾎｦ郢晏現・帝恆・ｽ陷会ｿｽ
    public bool onCoolTime;
    playerStatusManager_Alpha myStatus;

    protected Vector2 moveInput;
    protected bool isSpecialMoving = false;
    protected Vector2 specialMoveDirection;
    protected float specialMoveEndTime = 0f;

    [Header("Focus Mode Settings")]
    [Tooltip("Tooltip removed due to encoding error")]
    public float focusSpeedMultiplier = 0.5f;

    [Tooltip("Tooltip removed due to encoding error")]
    public Vector2 normalColliderSize = new Vector2(0.5f, 0.5f);
    [Tooltip("Tooltip removed due to encoding error")]
    public Vector2 focusColliderSize = new Vector2(0.2f, 0.2f);

    [Tooltip("Tooltip removed due to encoding error")]
    public Vector2 normalGrazeSize = new Vector2(1.0f, 1.0f);
    [Tooltip("Tooltip removed due to encoding error")]
    public Vector2 focusGrazeSize = new Vector2(0.6f, 0.6f);
    [Tooltip("Tooltip removed due to encoding error")]

    public GameObject hitboxImage;

    [Tooltip("Tooltip removed due to encoding error")]
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
        animator = GetComponent<Animator>(); // Animator 郢ｧ雋槫徐陟輔・
        
    }
    // Update is called once per frame

    protected virtual void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (!isSpecialMoving)
        {
            // 陷ｿ・ｳ郢ｧ・ｯ郢晢ｽｪ郢昴・縺鷹ｶ・｣髫募私・ｼ蛹ｻ繝ｵ郢ｧ・ｩ郢晢ｽｼ郢ｧ・ｫ郢ｧ・ｹ郢晢ｽ｢郢晢ｽｼ郢昜ｼ夲ｽｼ繝ｻ
            if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Focus)
            {
                // Input System: 郢晏ｸ吶・郢晢ｽｫ郢晉甥諢幄楜繝ｻ
                bool isSpecialPressed = Alpha.Managers.PlayerInputManager_Alpha.Instance.IsSpecialPressed;
                
                if (isSpecialPressed && myStatus.currentStamina > 0 && !myStatus.isStaminaExhausted)
                {
                    isFocusMode = true;
                    if (playerCollider != null) playerCollider.size = focusColliderSize;
                    if (hitboxImage != null) hitboxImage.SetActive(true);
                    if (grazeCollider != null) grazeCollider.size = focusGrazeSize;
                }
                else if (!isSpecialPressed || myStatus.currentStamina <= 0 || myStatus.isStaminaExhausted)
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
                isFocusMode = false;
                if (playerCollider != null) playerCollider.size = normalColliderSize;
                if (hitboxImage != null) hitboxImage.SetActive(false);
                if (grazeCollider != null) grazeCollider.size = normalGrazeSize;
            }

            // Input System邵ｺ荵晢ｽ臥ｸｺ・ｮ驕假ｽｻ陷崎ｼ斐・郢ｧ・ｯ郢晏現ﾎ晁愾髢・ｾ繝ｻ
            moveInput = Alpha.Managers.PlayerInputManager_Alpha.Instance.MoveVector;
            
            if (moveInput.sqrMagnitude > 1) moveInput.Normalize();

            // 霑夲ｽｹ隹ｿ鬘費ｽｧ・ｻ陷崎ｼ斐・郢晏現ﾎ懃ｹｧ・ｬ郢晢ｽｼ繝ｻ蛹ｻ繝�郢ｧ・ｦ郢晢ｽｳ陋ｻ・､陞ｳ螟ｲ・ｼ繝ｻ
            if (Alpha.Managers.PlayerInputManager_Alpha.Instance.WasSpecialPressed && myStatus.currentSpecialMove != playerStatusManager_Alpha.SpecialMoveType.None && myStatus.currentSpecialMove != playerStatusManager_Alpha.SpecialMoveType.Focus)
            {
                // アクティブスキルが装備されている場合は、特殊移動（ダッシュ/ワープ）を上書きする
                if (!myStatus.HasActiveSkill)
                {
                    TrySpecialMove();
                }
            }
        }
        else
        {
            moveInput = Vector2.zero; // 霑夲ｽｹ隹ｿ鬘費ｽｧ・ｻ陷咲ｩゑｽｸ・ｭ邵ｺ・ｯ鬨ｾ螢ｼ・ｸ・ｸ邵ｺ・ｮ驕假ｽｻ陷榊供繝ｻ陷牙ｸ呻ｽ定ｾ滂ｽ｡髫輔・
        }

        // 郢晢ｽｭ郢昴・縺醍ｹｧ・ｪ郢晢ｽｳ陝・ｽｾ髮趣ｽ｡邵ｺ蠕鯉ｼ樒ｹｧ蜿･・ｽ・ｴ陷ｷ蛹ｻﾂ竏壺落邵ｺ・ｮ隴・ｽｹ陷ｷ莉｣竊楢惺莉｣・�郢ｧ蜿･繝ｻ騾・・・ｭ蟲ｨ繝ｻ邵ｺ阮呻ｼ・ｸｺ・ｫ髫ｪ蛟ｩ・ｿ・ｰ

        // Animator 郢昜ｻ｣ﾎ帷ｹ晢ｽ｡郢晢ｽｼ郢ｧ・ｿ邵ｺ・ｮ隴厄ｽｴ隴・ｽｰ
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
                Vector3 aimPos = Alpha.Managers.PlayerInputManager_Alpha.Instance.GetWorldAimPosition(transform.position);
                specialMoveDirection = (aimPos - transform.position).normalized;
            }
            else
            {
                specialMoveDirection = moveInput.normalized;
            }

            if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Dash)
            {
                isSpecialMoving = true;
                specialMoveEndTime = Time.time + duration;
                
                // 髴托ｽｽ陷会ｿｽ: 郢敖郢昴・縺咏ｹ晢ｽ･隴弱ｅ竊楢ｰｿ蜿･繝ｯ郢ｧ蜻域剰怏・ｹ陋ｹ繝ｻ
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
        rb.velocity = Vector2.zero; // 郢晢ｽｯ郢晢ｽｼ郢晏ｶｺ・ｸ・ｭ邵ｺ・ｯ陞ｳ謔溘・陋帶㊧・ｭ・｢
        
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.isInvincible = true;

        // --- 髴托ｽｽ陷会ｿｽ繝ｻ螢ｹﾎ｡郢晢ｽｼ郢晏ｶｺ・ｸ・ｭ邵ｺ・ｮ陞ｳ謔溘・霎滂ｽ｡隰ｨ・ｵ繝ｻ蛹ｻ笘・ｹｧ鬆第�｢邵ｺ謇假ｽｼ迚吶・騾・・---
        // 隴帷甥譟醍ｸｺ・ｪCollider郢ｧ蛛ｵ笘・ｸｺ・ｹ邵ｺ・ｦ闕ｳﾂ隴弱ｉ蝎ｪ邵ｺ・ｫ霎滂ｽ｡陷会ｽｹ陋ｹ謔ｶ・�邵ｲ竏晢ｽｼ・ｾ邵ｺ蠕後Υ郢昴・繝ｨ邵ｺ蜉ｱ竊醍ｸｺ繝ｻ・育ｸｺ繝ｻ竊鍋ｸｺ蜷ｶ・・
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
        
        float blinkInterval = 0.04f; // 鬯ｮ蛟ｬﾂ貊薙・雋翫・
        float timer = 0f;
        bool isVisible = true;

        while (timer < windupTime)
        {
            isVisible = !isVisible;
            foreach (var r in renderers) if (r != null) r.enabled = isVisible;
            
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 髯ｦ・ｨ驕会ｽｺ郢ｧ雋槭・邵ｺ・ｫ隰鯉ｽｻ邵ｺ繝ｻ
        foreach (var r in renderers) if (r != null) r.enabled = true;

        // 霑ｸ・ｬ鬮｢骰具ｽｧ・ｻ陷榊供・ｮ貅ｯ・｡繝ｻ
        rb.position = rb.position + direction * myStatus.warpDistance;

        // --- 髴托ｽｽ陷会ｿｽ繝ｻ螢ｹ縺慕ｹ晢ｽｩ郢ｧ・､郢敖郢晢ｽｼ郢ｧ雋橸ｽｾ・ｩ陷医・---
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
                rb.velocity = Vector2.zero; // 霑夲ｽｩ騾・・縺顔ｹ晢ｽｳ郢ｧ・ｸ郢晢ｽｳ邵ｺ・ｮ隲ｷ・｣隲､・ｧ郢ｧ蜻茨ｽｶ蛹ｻ笘・
                
                if (Time.time >= specialMoveEndTime)
                {
                    isSpecialMoving = false;
                    
                    // 髴托ｽｽ陷会ｿｽ: 郢敖郢昴・縺咏ｹ晢ｽ･驍ｨ繧・ｽｺ繝ｻ蜃ｾ邵ｺ・ｫ隹ｿ蜿･繝ｯ郢ｧ蝣､笏瑚怏・ｹ陋ｹ繝ｻ
                    var trail = GetComponent<Alpha.Core.ProceduralGhostTrail_Alpha>();
                    if (trail != null) trail.EnableTrail(false);
                }
            }
            else if (myStatus.currentSpecialMove == playerStatusManager_Alpha.SpecialMoveType.Warp)
            {
                rb.velocity = Vector2.zero; // 郢晢ｽｯ郢晢ｽｼ郢晏ｶｺ・ｸ・ｭ邵ｺ・ｯ陞ｳ謔溘・陋帶㊧・ｭ・｢
            }
            return;
        }

        // 陷茨ｽ･陷牙ｸ吮ｲ邵ｺ・ｪ邵ｺ繝ｻ・ｽ・ｴ陷ｷ蛹ｻ繝ｻ闖ｴ霈費ｽらｸｺ蜉ｱ竊醍ｸｺ繝ｻ
        if (moveInput == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 郢ｧ・ｭ郢晢ｽ｣郢晢ｽｩ郢ｧ・ｯ郢ｧ・ｿ郢晢ｽｼ郢ｧ蝣､・ｧ・ｻ陷崎ｼ費ｼ・ｸｺ蟶呻ｽ・
        float setSpd = (myStatus.moveSpeed * myStatus.moveSpeedMag * 0.0001f);
        float finalSpeed = setSpd * myStatus.moveSpeedMag_CONST;

        if (setSpd <= 0)
        {
            finalSpeed = 0.79f;
        }
        
        if (isFocusMode)
        {
            finalSpeed *= focusSpeedMultiplier;
        }

        Vector2 targetVelocity = moveInput * finalSpeed;
        rb.MovePosition(rb.position + targetVelocity * Time.fixedDeltaTime);
        rb.velocity = Vector2.zero; // 隲ｷ・｣隲､・ｧ邵ｺ・ｫ郢ｧ蛹ｻ・玖ｲ贋ｻ｣・願汞・ｾ驕ｲ繝ｻ
    }

    protected virtual void UpdateAnimatorParameters()
    {
        // 郢ｧ・ｨ郢ｧ・､郢晢ｿｽ陝・ｽｾ髮趣ｽ｡邵ｺ・ｮ陟趣ｽｧ隶灘生・定愾髢・ｾ繝ｻ
        Vector3 aimPos = Alpha.Managers.PlayerInputManager_Alpha.Instance.GetWorldAimPosition(transform.position);

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
            if (setSpd <= 0) finalSpeed = 0.79f;
            if (isFocusMode) finalSpeed *= focusSpeedMultiplier;
            currentVelocity = moveInput * finalSpeed;
        }

        // 郢ｧ・ｨ郢ｧ・､郢晢ｿｽ隴・ｽｹ陷ｷ謇假ｽｼ繝ｻ髴・ｽｸ繝ｻ蟲ｨ・但nimator郢昜ｻ｣ﾎ帷ｹ晢ｽ｡郢晢ｽｼ郢ｧ・ｿ邵ｺ・ｫ髫ｪ・ｭ陞ｳ繝ｻ
        float mouseXPosition = currentVelocity.x; 

        // 郢晏干ﾎ樒ｹｧ・､郢晢ｽ､郢晢ｽｼ邵ｺ・ｮ驕假ｽｻ陷崎ｼ斐・郢ｧ・ｯ郢晏現ﾎ晉ｸｺ・ｮ陞滂ｽｧ邵ｺ髦ｪ・・ｹｧ螳夲ｽｨ閧ｲ・ｮ繝ｻ
        float moveVectorMag = currentVelocity.magnitude;

        // Animator 郢昜ｻ｣ﾎ帷ｹ晢ｽ｡郢晢ｽｼ郢ｧ・ｿ郢ｧ螳夲ｽｨ・ｭ陞ｳ繝ｻ
        animator.SetFloat("mouseXPosition", mouseXPosition);
        animator.SetFloat("moveVectorMag", moveVectorMag);
    }
}
