using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElitePhaseData
{
    [Tooltip("このフェーズの名前（識別用）")]
    public string phaseName = "Phase 1";
    
    [Tooltip("移動パターンの挙動")]
    public EnemyBehaviorData_Base movementBehavior;
    
    [Tooltip("攻撃（弾幕など）の挙動")]
    public EnemyBehaviorData_Base attackBehavior;

    [Tooltip("召喚などの特殊行動（任意）")]
    public EnemyBehaviorData_Base summonBehavior;
}

[RequireComponent(typeof(Alpha_EliteHealth))]
public class Alpha_EliteEnemyAI : Alpha_EnemyAI
{
    [Header("Elite Phases Setup")]
    public List<ElitePhaseData> phases;

    private Coroutine movementCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine summonCoroutine;

    private Alpha_EliteHealth eliteHealth;

    // カットイン用などのイベントフック
    public event Action<int, string> OnPhaseStartEvent;
    public event Action<string> OnAttackStartEvent;

    protected override void Awake()
    {
        base.Awake();
        eliteHealth = GetComponent<Alpha_EliteHealth>();
    }

    protected override void Start()
    {
        // 初期位置の記録などは親クラスを使用
        InitialPosition = transform.position;
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) TargetTransform = playerObj.transform;

        if (eliteHealth != null)
        {
            eliteHealth.OnPhaseBreak += HandlePhaseBreak;
        }

        // 初期フェーズの開始
        if (phases != null && phases.Count > 0)
        {
            StartPhase(0);
        }
    }

    private void OnDestroy()
    {
        if (eliteHealth != null)
        {
            eliteHealth.OnPhaseBreak -= HandlePhaseBreak;
        }
    }

    private void HandlePhaseBreak(int newPhaseIndex)
    {
        // 無敵時間やブレイク演出などを挟む場合はコルーチン等にする
        if (newPhaseIndex < phases.Count)
        {
            StartPhase(newPhaseIndex);
        }
    }

    public void StartPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= phases.Count) return;

        ElitePhaseData currentPhase = phases[phaseIndex];
        
        // 既存のコルーチンを全て停止
        StopAllBehaviors();

        // カットイン等への通知（実装時にUIを紐付け可能）
        OnPhaseStartEvent?.Invoke(phaseIndex, currentPhase.phaseName);

        // 各挙動を並列で実行
        if (currentPhase.movementBehavior != null)
            movementCoroutine = StartCoroutine(currentPhase.movementBehavior.RunBehavior(this));

        if (currentPhase.attackBehavior != null)
            attackCoroutine = StartCoroutine(currentPhase.attackBehavior.RunBehavior(this));

        if (currentPhase.summonBehavior != null)
            summonCoroutine = StartCoroutine(currentPhase.summonBehavior.RunBehavior(this));
    }

    private void StopAllBehaviors()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (summonCoroutine != null) StopCoroutine(summonCoroutine);
        
        movementCoroutine = null;
        attackCoroutine = null;
        summonCoroutine = null;

        if (Rb != null) Rb.velocity = Vector2.zero;
    }

    // 攻撃モジュール側からカットインなどを呼び出すためのフック
    public void TriggerAttackEvent(string attackName)
    {
        OnAttackStartEvent?.Invoke(attackName);
    }
}
