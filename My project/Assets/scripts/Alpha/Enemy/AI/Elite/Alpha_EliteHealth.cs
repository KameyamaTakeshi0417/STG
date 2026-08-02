using System.Collections.Generic;
using UnityEngine;

public class Alpha_EliteHealth : Health
{
    [Header("Elite Multi-Phase Settings")]
    [Tooltip("各フェーズの最大HP。要素数がフェーズ数になります")]
    public List<float> phaseHPs = new List<float>() { 1000f, 1500f };

    [Header("Defeat Settings")]
    [Tooltip("ボス撃破時のAetherExplosionプレハブ")]
    public GameObject aetherExplosionPrefab;
    [Tooltip("ボス撃破時のやられスプライト（大ボス専用）")]
    public Sprite defeatedSprite;
    [Tooltip("スプライトを変更する対象のSpriteRenderer（未設定時は自動取得）")]
    public SpriteRenderer targetSpriteRenderer;

    // ---------- Elite HP Canvas ----------
    [Header("UI Settings")]
    [Tooltip("Elite 用円形 HP Canvas（Resources 配下）")]
    public string eliteCanvasResourcePath = "UI/CircleHPBar/EliteEnemyHPCanvas";
    private Alpha.UI.Alpha_EliteCircleHPBar eliteHPBar;

    public int CurrentPhaseIndex { get; private set; } = 0;

    [HideInInspector]
    public int timedOutCount = 0;

    // ブレイク時のイベント（新しいフェーズのインデックスを渡す）
    public delegate void PhaseBreakHandler(int newPhaseIndex);
    public event PhaseBreakHandler OnPhaseBreak;

    // リザルト用のフェーズ終了イベント（フェーズインデックス、タイムアウト終了かどうかのフラグ）
    public event System.Action<int, bool> OnPhaseEndWithResult;

    private int expectedTotalPhases = 1;

    public void SetTotalPhases(int count)
    {
        expectedTotalPhases = Mathf.Max(1, count);
    }

    protected virtual void Awake()
    {
        expectedTotalPhases = Mathf.Max(1, phaseHPs.Count);
    }

    protected virtual void Start()
    {
        // 最初のフェーズのHPをセット
        if (phaseHPs.Count > 0)
        {
            HP = phaseHPs[0];
            currentHP = HP;
        }
        else
        {
            Debug.LogWarning("Alpha_EliteHealth: phaseHPs is empty. Using default HP.");
        }
        
        m_handler = gameObject.GetComponent<HPBar_Base>();

        // ----- Elite HP Canvas の生成 -----
        var canvasPrefab = Resources.Load<GameObject>(eliteCanvasResourcePath);
        if (canvasPrefab != null)
        {
            GameObject canvasObj = Instantiate(canvasPrefab, transform);
            // Canvas を World Space に設定（Prefab で設定されているはずだが念のため）
            var canvas = canvasObj.GetComponent<Canvas>();
            if (canvas != null) canvas.renderMode = RenderMode.WorldSpace;

            // UI 管理コンポーネント取得
            // 子階層にスクリプトが無い場合は追加
            eliteHPBar = canvasObj.GetComponentInChildren<Alpha.UI.Alpha_EliteCircleHPBar>(true);
            if (eliteHPBar == null)
            {
                Debug.LogWarning("[Alpha_EliteHealth] Alpha_EliteCircleHPBar not found – adding component dynamically.");
                eliteHPBar = canvasObj.AddComponent<Alpha.UI.Alpha_EliteCircleHPBar>();
            }
            Debug.Log($"[Alpha_EliteHealth] eliteHPBar ready: {eliteHPBar != null}");
            if (eliteHPBar != null)
            {
                eliteHPBar.Initialise(phaseHPs.Count, GetComponent<Alpha_EnemyAI>());
                Debug.Log("[Alpha_EliteHealth] Called Initialise on eliteHPBar");
            }
        }
        else
        {
            Debug.LogError($"[Alpha_EliteHealth] Elite HP Canvas prefab not found at {eliteCanvasResourcePath}");
        }

        // 頭上のスライダーは使用しないので非表示
        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
    }

    public override void TakeDamage(float damage)
    {
        if (VulnerableFlg || isDead) return;

        float remainingDamage = damage;
        ShowDamage(damage); // ダメージテキストは1回だけ表示

        while (remainingDamage > 0 && !isDead)
        {
            if (currentHP > remainingDamage)
            {
                currentHP -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                remainingDamage -= currentHP;
                currentHP = 0;
            }

            // HPバーは使用しないので SliderUpdate は不要。ただしUI がある場合は呼び出す。
            if (hpSlider != null) SliderUpdate();

            // UI に現在フェーズの残HP比率を通知
            if (eliteHPBar != null)
            {
                float ratio = Mathf.Clamp01(currentHP / HP);
                eliteHPBar.SetRingFill(CurrentPhaseIndex, ratio);
            }
            
            // HPが0以下になった際のブレイク（フェーズ移行）判定
            if (currentHP <= 0)
            {
                // 次のフェーズが存在するか？（AI側で設定されたフェーズ数を基準にする）
                if (CurrentPhaseIndex < expectedTotalPhases - 1)
                {
                    // タイムアウトではなく、HPを削り切ったことによるブレイク
                    BreakToNextPhase(false);
                    // フェーズブレイク時は余剰ダメージをカットし、ワンパンを防止する
                    remainingDamage = 0;
                }
                else
                {
                    // 全てのフェーズが終わったら本来の死亡処理へ委譲する
                    currentHP = 0;
                    base.TakeDamage(0); 
                    break;
                }
            }
        }
    }

    public void ForcePhaseBreak(bool isTimeout)
    {
        // UIやAI側から強制的にブレイクさせる（タイムアウトなど）
        if (isTimeout)
        {
            timedOutCount++;
            Debug.Log($"<color=yellow>[Elite Break]</color> Phase timed out! Total timed out phases: {timedOutCount}");
        }

        if (CurrentPhaseIndex < expectedTotalPhases - 1)
        {
            BreakToNextPhase(isTimeout);
        }
        else
        {
            // 最終フェーズでタイムアウトした場合、ボスを死亡させる
            currentHP = 0;
            base.TakeDamage(0);
        }
    }

    private void BreakToNextPhase(bool isTimeout)
    {
        OnPhaseEndWithResult?.Invoke(CurrentPhaseIndex, isTimeout);
        
        CurrentPhaseIndex++;
        
        // 新しいフェーズのHPをセットして全回復（設定されていなければ最後のHPを再利用）
        if (CurrentPhaseIndex < phaseHPs.Count)
        {
            HP = phaseHPs[CurrentPhaseIndex];
        }
        else if (phaseHPs.Count > 0)
        {
            HP = phaseHPs[phaseHPs.Count - 1]; // 安全対策: 設定漏れの場合は直前のHPを引き継ぐ
        }
        
        currentHP = HP;
        
        Debug.Log($"<color=orange>[Elite Break]</color> Phase transition to {CurrentPhaseIndex + 1} / {phaseHPs.Count}");
        
        // AI側へ通知
        OnPhaseBreak?.Invoke(CurrentPhaseIndex);
    }

    protected override void Die()
    {
        // 共通のドロップ処理を呼び出す
        DropEnemyRewards();
        
        if (isBoss)
        {
            // ボスの場合は専用の撃破シーケンスを実行
            var defeatSeq = gameObject.GetComponent<Alpha.Enemy.Effect.BossDefeatSequence_Alpha>();
            if (defeatSeq == null)
            {
                defeatSeq = gameObject.AddComponent<Alpha.Enemy.Effect.BossDefeatSequence_Alpha>();
            }
            defeatSeq.StartDefeatSequence(bossId, defeatedSprite, targetSpriteRenderer, aetherExplosionPrefab);
        }
        else
        {
            // エリート（中ボス含む）単発爆発エフェクト
            GameObject prefabToUse = aetherExplosionPrefab;
            if (prefabToUse == null) prefabToUse = Resources.Load<GameObject>("Objects/Effect/Effect_AetherExplosion");
            if (prefabToUse != null)
            {
                GameObject effect = null;
                if (global::Alpha_ObjectPoolManager.Instance != null)
                {
                    effect = global::Alpha_ObjectPoolManager.Instance.Rent(prefabToUse, transform.position, Quaternion.identity);
                }
                else
                {
                    effect = Instantiate(prefabToUse, transform.position, Quaternion.identity);
                }

                var explosionScript = effect.GetComponent<Alpha.Enemy.Effect.AetherExplosionEffect_Alpha>();
                if (explosionScript != null)
                {
                    explosionScript.sourcePrefab = prefabToUse;
                }
            }

            // スロー演出をトリガー (エリート・中ボス用)
            if (Alpha.Flow.StageManager_Alpha.Instance != null)
            {
                Alpha.Flow.StageManager_Alpha.Instance.TriggerSlowMotion(0.7f, 3f);
            }

            // エリート（中ボス含む）の場合は通常通り消滅させる
            if (Alpha.Flow.RewardManager_Alpha.Instance != null)
            {
                bool forceQuality1 = (timedOutCount >= 2);
                if (isMidBoss)
                {
                    Alpha.Flow.RewardManager_Alpha.Instance.DropMidBossReward(transform.position, forceQuality1);
                    if (Alpha.Flow.StageManager_Alpha.Instance != null)
                    {
                        Alpha.Flow.StageManager_Alpha.Instance.ClearAllEnemyBullets();
                        Alpha.Flow.StageManager_Alpha.Instance.OnBossDefeated();
                    }
                }
                else
                {
                    Alpha.Flow.RewardManager_Alpha.Instance.AddPoints(rewardPoints);
                    // 通常エリートの場合も弾を消す場合はここに追加
                    if (Alpha.Flow.StageManager_Alpha.Instance != null)
                    {
                        Alpha.Flow.StageManager_Alpha.Instance.ClearAllEnemyBullets();
                    }
                }
            }

            Debug.Log(gameObject.name + " (Elite/MidBoss) died.");
            Destroy(gameObject);
        }
    }
}
