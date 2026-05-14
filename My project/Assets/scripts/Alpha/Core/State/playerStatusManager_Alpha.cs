using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerStatusManager_Alpha : ObjectStatus_Alpha
{
    // 親クラス(ObjectStatus_Alpha)が HP, currentHP を持っているためここでの再定義は不要

    // HPBar更新用などのイベント（引数で現在HPと最大HPを渡す）
    public delegate void PlayerHPChangedHandler(float current, float max);
    public static event PlayerHPChangedHandler OnPlayerHPChanged;

    [Header("Movement Settings")]
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

    // --- Player Health Management API ---
    
    public void ApplyDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        
        Debug.Log($"[PlayerStatusManager] ApplyDamage: {amount} | Current HP: {currentHP}");

        // UI更新などのためにイベント発火（受け手が未実装でも安全）
        OnPlayerHPChanged?.Invoke(currentHP, HP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > HP) currentHP = HP;
        
        Debug.Log($"[PlayerStatusManager] Heal: {amount} | Current HP: {currentHP}");

        OnPlayerHPChanged?.Invoke(currentHP, HP);
    }

    private void Die()
    {
        Debug.Log("[PlayerStatusManager] Player HP reached 0. Triggering GameOver.");
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null)
        {
            GameManager manager = gmObj.GetComponent<GameManager>();
            if (manager != null)
            {
                manager.GameOver();
            }
            else
            {
                Debug.LogError("GameManager component not found on GameManager object!");
            }
        }
        else
        {
            Debug.LogError("GameManager object not found in the scene!");
        }
    }
}
