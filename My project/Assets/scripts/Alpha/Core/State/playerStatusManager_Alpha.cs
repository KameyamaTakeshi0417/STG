using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStatusManager_Alpha : ObjectStatus_Alpha
{
    // Start is called before the first frame update
    public float moveSpeed;
    public float moveSpeedMag = 1f;
    public float moveSpeedMag_CONST = 1.0f;
    public float bulletSpeed;
    public float bulletSpeedMag = 1.0f;
    public float BulletSpan; // フレーム
    public float BulletSpanMag = 1.0f;

    public enum SpecialMoveType { None, Dash, Warp }
    [Header("Special Move (Dash/Warp) Settings")]
    public SpecialMoveType currentSpecialMove = SpecialMoveType.Dash;

    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRecoveryRate = 10f; // 100 units over 10 seconds
    public float staminaRecoveryDelay = 0.5f;
    [HideInInspector] public float lastStaminaConsumeTime = -100f;

    public float dashDuration = 0.1f;
    public float dashStaminaCost = 30f;
    public float dashDistance = 4f; // 調整可能

    public float warpDuration = 0.2f;  // DBZ瞬間移動のタメ時間
    public float warpStaminaCost = 45f;
    public float warpDistance = 8f; // 調整可能

    [Header("Weapon Synthesis Settings")]
    [Tooltip("特定条件下で1〜3行目のすべての武器効果を使用可能にするフラグ")]
    public bool canUseAllEffects = false;

    [Header("Pierce Settings")]
    [Tooltip("貫通時のダメージ減衰率（デフォルト25%減）。アイテム等で0.10fなどに変動")]
    public float pierceDamageReductionRate = 0.25f;

    void Update()
    {
        if (Time.time > lastStaminaConsumeTime + staminaRecoveryDelay)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
        }
    }
}
