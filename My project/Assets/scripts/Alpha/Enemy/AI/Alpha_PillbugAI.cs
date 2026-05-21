using System.Collections;
using UnityEngine;

public class Alpha_PillbugAI : Alpha_EnemyAI
{
    [Header("Pillbug Settings")]
    [Tooltip("砲台モード（通常時）の挙動")]
    public EnemyBehaviorData_Base behaviorTurret;
    
    [Tooltip("突進モードの挙動（ShieldDashなど）")]
    public EnemyBehaviorData_Base behaviorDash;

    [Tooltip("突進モードに移行するための初回ダメージ閾値")]
    public float baseDamageThreshold = 20f;
    
    [Tooltip("次回以降、閾値に掛かる倍率（例: 1.5 なら 20 -> 30 -> 45）")]
    public float thresholdMultiplier = 1.5f;

    [Tooltip("突進モードが終了して砲台に戻るまでの時間（突進自体の時間を含む）")]
    public float dashModeDuration = 3f;

    private _Health_Base health;
    private float lastCheckHP;
    private float currentThreshold;
    private bool isDashMode = false;

    protected override void Start()
    {
        base.Start(); // 親クラスのStart（TargetTransformの取得など）を実行

        health = GetComponent<_Health_Base>();
        if (health != null)
        {
            lastCheckHP = health.currentHP;
        }
        
        currentThreshold = baseDamageThreshold;

        // 砲台モードから開始
        SetTurretMode();
    }

    protected override void Update()
    {
        base.Update(); // スタン時の停止処理など

        if (health == null) return;

        // 砲台モードの時のみ、ダメージを監視する
        if (!isDashMode)
        {
            float damageTaken = lastCheckHP - health.currentHP;
            if (damageTaken >= currentThreshold)
            {
                // 閾値を超えたら突進モードへ
                StartCoroutine(DashModeRoutine());
            }
            // 回復された場合は基準値を下げる（必要なら）
            else if (damageTaken < 0)
            {
                lastCheckHP = health.currentHP;
            }
        }
    }

    private void SetTurretMode()
    {
        isDashMode = false;
        // 砲台モード：移動はナシ、攻撃スロットにTurret挙動をセット
        StartBehaviors(null, behaviorTurret, null);
        
        if (health != null)
        {
            lastCheckHP = health.currentHP;
        }
    }

    private IEnumerator DashModeRoutine()
    {
        isDashMode = true;
        Debug.Log($"[PillbugAI] Triggered Dash Mode! Took enough damage. Next threshold: {currentThreshold * thresholdMultiplier}");

        // 次回の閾値を上げる
        currentThreshold *= thresholdMultiplier;

        // 突進モード：移動スロットにDash挙動をセット、攻撃は一旦ナシ
        StartBehaviors(behaviorDash, null, null);

        // 一定時間（突進＋クールダウン等）待つ
        yield return new WaitForSeconds(dashModeDuration);

        // スタン中などは解除を待つべきか？（今回はシンプルに時間で戻す）
        
        // 砲台モードに戻る
        SetTurretMode();
    }
}
