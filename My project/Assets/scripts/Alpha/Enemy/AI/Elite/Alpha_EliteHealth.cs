using System.Collections.Generic;
using UnityEngine;

public class Alpha_EliteHealth : Health
{
    [Header("Elite Multi-Phase Settings")]
    [Tooltip("各フェーズの最大HP。要素数がそのままフェーズ数になります")]
    public List<float> phaseHPs = new List<float>() { 1000f, 1500f };

    public int CurrentPhaseIndex { get; private set; } = 0;

    // ブレイク時のイベント（新しいフェーズのインデックスを渡す）
    public delegate void PhaseBreakHandler(int newPhaseIndex);
    public event PhaseBreakHandler OnPhaseBreak;

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
        
        // baseのStartは使用しない（currentHPを再上書きされるのを防ぐため）
    }

    public override void TakeDamage(float damage)
    {
        if (VulnerableFlg) return; // 無敵中などの判定が必要ならここで行う

        currentHP -= damage;
        
        if (hpSlider != null)
        {
            SliderUpdate();
            ShowDamage(damage);
        }

        // HPが0以下になった際のブレイク（フェーズ移行）判定
        if (currentHP <= 0)
        {
            // 次のフェーズが存在するか？
            if (CurrentPhaseIndex < phaseHPs.Count - 1)
            {
                BreakToNextPhase();
            }
            else
            {
                // 最後のフェーズなら本来の死亡処理へ委譲する
                // base.TakeDamageの条件(currentHP <= 0)を満たす形で0ダメージを流し込み、内部のprivate Die()を発火させる
                currentHP = 0;
                base.TakeDamage(0); 
            }
        }
    }

    private void BreakToNextPhase()
    {
        CurrentPhaseIndex++;
        
        // 新しいフェーズのHPをセットして全回復
        HP = phaseHPs[CurrentPhaseIndex];
        currentHP = HP;
        
        if (hpSlider != null)
        {
            hpSlider.maxValue = HP;
            SliderUpdate();
        }

        Debug.Log($"<color=orange>[Elite Break]</color> Phase transition to {CurrentPhaseIndex + 1} / {phaseHPs.Count}");
        
        // AI側へ通知
        OnPhaseBreak?.Invoke(CurrentPhaseIndex);
    }
}
