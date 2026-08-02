using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElitePhaseData
{
    [Tooltip("このフェーズの名前（識別用）")]
    public string phaseName = "Phase 1";

    [Tooltip("フェーズ切り替え時にカットインを表示するかどうか")]
    public bool useCutIn = false;

    [Tooltip("カットイン用の立ち絵スプライト")]
    public Sprite cutInSprite;

    [Tooltip("このフェーズの制限時間（秒）。0以下の場合は時間無制限（タイムアウトしない）")]
    public float timeLimit = 30f;
    
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

    private Coroutine phaseSequenceCoroutine;
    private Coroutine movementCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine summonCoroutine;
    private Coroutine timerCoroutine;

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

        // エリート遭遇時のチュートリアル表示
        if (Alpha.UI.TutorialManager_Alpha.Instance != null)
        {
            Alpha.UI.TutorialManager_Alpha.Instance.ShowTutorial("Tutorial_Elite");
        }
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) TargetTransform = playerObj.transform;

        if (eliteHealth != null)
        {
            eliteHealth.OnPhaseBreak += HandlePhaseBreak;
            
            // AI側に登録されたフェーズ数をHealthに伝えておき、
            // HP設定が足りなくても最後までフェーズが進むようにする
            eliteHealth.SetTotalPhases(phases.Count);
        }

        // ボス用のUIに切り替え
        if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
        {
            Alpha.UI.SequenceBarUI_Alpha.Instance.gameObject.SetActive(false);
        }
        if (Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance != null)
        {
            Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance.Show();
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

        // 通常のUIに戻す
        if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
        {
            Alpha.UI.SequenceBarUI_Alpha.Instance.gameObject.SetActive(true);
        }
        if (Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance != null)
        {
            Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance.Hide();
        }

        // エリート死亡時に画面上の敵弾とサーキュレーターを一掃する
        ClearEnemyProjectilesAndMinions();
    }

    private void ClearEnemyProjectilesAndMinions()
    {
        // 敵の弾を全て検索して消去（またはプールに返却）
        Bullet_Base[] bullets = FindObjectsOfType<Bullet_Base>();
        foreach (var bullet in bullets)
        {
            if (bullet != null && bullet.isEnemyBullet && bullet.gameObject.activeInHierarchy)
            {
                if (Alpha_ObjectPoolManager.Instance != null && bullet.sourcePrefab != null)
                {
                    Alpha_ObjectPoolManager.Instance.Return(bullet.gameObject, bullet.sourcePrefab);
                }
                else
                {
                    Destroy(bullet.gameObject);
                }
            }
        }

        // サーキュレーターなどの召喚物（PhaseSpawnedObjectsに登録漏れしていた場合へのフェイルセーフ）も消去
        CirculatorEnemy[] circulators = FindObjectsOfType<CirculatorEnemy>();
        foreach (var c in circulators)
        {
            if (c != null && c.gameObject.activeInHierarchy)
            {
                // サーキュレーターのプール元プレハブが判別しづらいため、安全に非アクティブまたはDestroyする
                if (Alpha_ObjectPoolManager.Instance != null)
                {
                    c.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(c.gameObject);
                }
            }
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

        // 既存のコルーチンを全て停止
        StopAllBehaviors();

        // フェーズ遷移時、画面上の敵弾を全て消去して安全地帯を確保する
        if (Alpha.Flow.StageManager_Alpha.Instance != null)
        {
            Alpha.Flow.StageManager_Alpha.Instance.ClearAllEnemyBullets();
        }

        phaseSequenceCoroutine = StartCoroutine(PhaseSequenceRoutine(phaseIndex));
    }

    private System.Collections.IEnumerator PhaseSequenceRoutine(int phaseIndex)
    {
        ElitePhaseData currentPhase = phases[phaseIndex];

        // カットイン等への通知
        OnPhaseStartEvent?.Invoke(phaseIndex, currentPhase.phaseName);

        // カットイン演出の再生と待機
        if (currentPhase.useCutIn && Alpha.UI.UltCutInController.Instance != null && currentPhase.cutInSprite != null)
        {
            Alpha.UI.UltCutInController.Instance.PlayCutIn(false, currentPhase.cutInSprite, currentPhase.phaseName);
            yield return new WaitForSecondsRealtime(2.7f); // カットインの演出時間分待機（TimeScaleの影響を受けないように変更）
        }

        // 行動を並列で実行
        if (currentPhase.movementBehavior != null)
            movementCoroutine = StartCoroutine(currentPhase.movementBehavior.RunBehavior(this));

        if (currentPhase.attackBehavior != null)
            attackCoroutine = StartCoroutine(currentPhase.attackBehavior.RunBehavior(this));

        if (currentPhase.summonBehavior != null)
            summonCoroutine = StartCoroutine(currentPhase.summonBehavior.RunBehavior(this));

        // タイマーを開始
        if (currentPhase.timeLimit > 0)
        {
            timerCoroutine = StartCoroutine(PhaseTimerRoutine(currentPhase.timeLimit));
        }
    }

    private System.Collections.IEnumerator PhaseTimerRoutine(float timeLimit)
    {
        float elapsed = 0f;
        while (elapsed < timeLimit)
        {
            elapsed += Time.deltaTime;
            if (Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance != null)
            {
                Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance.UpdateProgress(elapsed, timeLimit);
            }
            yield return null;
        }

        // タイムアップ時の強制ブレイク
        if (eliteHealth != null)
        {
            eliteHealth.ForcePhaseBreak(true);
        }
    }

    public void StopAllBehaviors()
    {
        if (phaseSequenceCoroutine != null) StopCoroutine(phaseSequenceCoroutine);
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (summonCoroutine != null) StopCoroutine(summonCoroutine);
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            // HPブレイク等で早期終了した場合はゲージを0に戻す演出を呼ぶ
            if (Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance != null)
            {
                Alpha.UI.BossPhaseSequenceBarUI_Alpha.Instance.DrainToZero();
            }
        }
        
        // 親クラス側の機能で動いているものも念のため停止
        ChangeBehavior(null);
        
        // このフェーズで召喚したオブジェクトを一掃する
        ClearSpawnedObjects();

        phaseSequenceCoroutine = null;
        movementCoroutine = null;
        attackCoroutine = null;
        summonCoroutine = null;
        timerCoroutine = null;

        if (Rb != null) Rb.velocity = Vector2.zero;
    }

    // 攻撃モジュール側からカットインなどを呼び出すためのフック
    public void TriggerAttackEvent(string attackName)
    {
        OnAttackStartEvent?.Invoke(attackName);
    }
}
