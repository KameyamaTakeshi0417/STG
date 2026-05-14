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
    [Tooltip("このリストの0番目が最初の攻撃フェーズになります。InitialBehaviorは使用しません。")]
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
        // 親クラス(Alpha_EnemyAI)のStartを呼ばないことで、
        // 誤ってInitialBehaviorが裏で無限に動き続けるのを完全に防ぎます。
        InitialPosition = transform.position;
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) TargetTransform = playerObj.transform;

        if (eliteHealth != null)
        {
            eliteHealth.OnPhaseBreak += HandlePhaseBreak;
            
            // AI側に登録されたフェーズ数をHealthに伝えておき、
            // HP設定が足りなくても最後までフェーズが進むようにする
            eliteHealth.SetTotalPhases(phases.Count);
        }

        // 常にphasesリストの先頭（0番目）から確実にスタートする
        if (phases != null && phases.Count > 0)
        {
            StartPhase(0);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy(); // 親クラスのOnDestroy（召喚物のクリア処理）を呼ぶ
        
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
        
        // 親クラス側の機能で動いているものも念のため停止
        ChangeBehavior(null);
        
        // このフェーズで召喚したオブジェクトを一掃する
        ClearSpawnedObjects();

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
