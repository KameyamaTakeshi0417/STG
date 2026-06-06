using System.Collections.Generic;
using UnityEngine;

public class Alpha_EliteHealth : Health
{
    [System.Serializable]
    public class PhaseCutInInfo
    {
        [Tooltip("このフェーズ突入時にカットインを表示するか")]
        public bool useCutIn = false;
        [Tooltip("カットイン用のスプライト（立ち絵など）")]
        public Sprite cutInSprite;
        [Tooltip("カットインに表示する技名やフェーズ名")]
        public string skillName = "PHASE CHANGE";
    }

    [Header("Elite Multi-Phase Settings")]
    [Tooltip("各フェーズの最大HP。要素数がフェーズ数になる")]
    public List<float> phaseHPs = new List<float>() { 1000f, 1500f };

    [Tooltip("各フェーズ突入時のカットイン設定（要素数はフェーズ数と合わせるのが理想です）")]
    public List<PhaseCutInInfo> phaseCutIns = new List<PhaseCutInInfo>();

    // ---------- Elite HP Canvas ----------
    [Header("UI Settings")]
    [Tooltip("Elite 用円形 HP Canvas（Resources 配下）")]
    public string eliteCanvasResourcePath = "UI/CircleHPBar/EliteEnemyHPCanvas";
    private Alpha.UI.Alpha_EliteCircleHPBar eliteHPBar;

    public int CurrentPhaseIndex { get; private set; } = 0;

    // ブレイク時のイベント（新しいフェーズのインデックスを渡す）
    public delegate void PhaseBreakHandler(int newPhaseIndex);
    public event PhaseBreakHandler OnPhaseBreak;

    private int expectedTotalPhases = 1;

    public void SetTotalPhases(int count)
    {
        expectedTotalPhases = Mathf.Max(1, count);
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

        // 初期フェーズのカットインを再生
        TryPlayCutIn(CurrentPhaseIndex);
    }

    private void TryPlayCutIn(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < phaseCutIns.Count)
        {
            var info = phaseCutIns[phaseIndex];
            if (info.useCutIn && Alpha.UI.UltCutInController.Instance != null)
            {
                // エリート側としてカットイン再生 (isPlayer = false)
                Alpha.UI.UltCutInController.Instance.PlayCutIn(false, info.cutInSprite, info.skillName);
            }
        }
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
                    BreakToNextPhase();
                    // ループが続き、remainingDamage が次のフェーズの体力から引かれます（貫通ダメージ）
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

    private void BreakToNextPhase()
    {
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
}
