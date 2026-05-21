using UnityEngine;
using UnityEngine.Events;
using Alpha.Data;
using Alpha.UI;

namespace Alpha.Flow
{
    public enum StageState_Alpha
    {
        None,
        WaitToStartFirstHalf,
        FirstHalf,
        MidBossWait,
        MidBossFight,
        Transition,
        WaitToStartSecondHalf,
        SecondHalf,
        BossWait,
        BossFight,
        StageClear
    }

    public class StageManager_Alpha : MonoBehaviour
    {
        [Header("Data")]
        public StageData_Alpha currentStageData;

        [Header("References")]
        public SpawnManager_Alpha spawnManager;
        public SequenceBarUI_Alpha sequenceBarUI;
        public FadeController_Alpha fadeController;

        [Header("State (Read Only)")]
        public StageState_Alpha currentState = StageState_Alpha.None;
        public float currentSequenceTime = 0f;

        private StageSequenceData_Alpha activeSequence;

        void Start()
        {
            SetState(StageState_Alpha.WaitToStartFirstHalf);
        }

        void Update()
        {
            switch (currentState)
            {
                case StageState_Alpha.WaitToStartFirstHalf:
                    // プレイヤーの操作で前半開始
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        StartFirstHalf();
                    }
                    break;

                case StageState_Alpha.FirstHalf:
                case StageState_Alpha.SecondHalf:
                    UpdateSequence();
                    HandleWaveSkip();
                    break;

                case StageState_Alpha.MidBossWait:
                case StageState_Alpha.BossWait:
                    // 雑魚が全滅したらボス戦開始
                    if (spawnManager.IsMobCleared())
                    {
                        StartBossFight();
                    }
                    break;

                case StageState_Alpha.WaitToStartSecondHalf:
                    // プレイヤーの操作で後半開始
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        StartSecondHalf();
                    }
                    break;

                // ボス戦中などは別途クリア条件（ボス撃破）を監視するが
                // 最小実装としてここでは何もしない。外部（EnemyEscape等）から遷移を呼ぶ。
            }
        }

        public void SetState(StageState_Alpha newState)
        {
            currentState = newState;
            Debug.Log($"[StageManager] Changed State to: {currentState}");
        }

        private void StartFirstHalf()
        {
            if (currentStageData == null || currentStageData.firstHalf == null)
            {
                Debug.LogError("[StageManager] currentStageData or its firstHalf is not assigned! Cannot start first half.");
                return;
            }
            if (sequenceBarUI == null || spawnManager == null)
            {
                Debug.LogError("[StageManager] References (SequenceBarUI or SpawnManager) are not assigned in the inspector!");
                return;
            }

            activeSequence = currentStageData.firstHalf;
            currentSequenceTime = 0f;
            
            sequenceBarUI.Setup(activeSequence);
            spawnManager.SetupSequence(activeSequence);
            
            SetState(StageState_Alpha.FirstHalf);
        }

        private void UpdateSequence()
        {
            if (activeSequence == null) return;

            currentSequenceTime += Time.deltaTime;
            sequenceBarUI.UpdateProgress(currentSequenceTime / activeSequence.duration);
            spawnManager.CheckSpawn(currentSequenceTime);

            if (currentSequenceTime >= activeSequence.duration)
            {
                // 時間到達で待機状態へ
                if (currentState == StageState_Alpha.FirstHalf)
                    SetState(StageState_Alpha.MidBossWait);
                else
                    SetState(StageState_Alpha.BossWait);
            }
        }

        private void HandleWaveSkip()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                float nextWaveTime = spawnManager.GetNextWaveTime();
                if (nextWaveTime > currentSequenceTime && nextWaveTime <= activeSequence.duration)
                {
                    Debug.Log($"[StageManager] Wave Skipped! Jumped from {currentSequenceTime:F1} to {nextWaveTime:F1}");
                    
                    // 次のウェーブの時間までジャンプ
                    currentSequenceTime = nextWaveTime;
                    
                    // ジャンプ後の時間でUIとスポーンを即時更新
                    sequenceBarUI.UpdateProgress(currentSequenceTime / activeSequence.duration);
                    spawnManager.CheckSpawn(currentSequenceTime);
                }
            }
        }

        private void StartBossFight()
        {
            if (currentState == StageState_Alpha.MidBossWait)
            {
                SetState(StageState_Alpha.MidBossFight);
                spawnManager.SpawnBoss(activeSequence.bossPrefab);
            }
            else if (currentState == StageState_Alpha.BossWait)
            {
                SetState(StageState_Alpha.BossFight);
                spawnManager.SpawnBoss(activeSequence.bossPrefab);
            }
        }

        public void OnBossDefeated()
        {
            if (currentState == StageState_Alpha.MidBossFight)
            {
                SetState(StageState_Alpha.Transition);
                
                // 暗転開始
                fadeController.FadeOut(() => 
                {
                    // 暗転中（真っ黒）のフック処理
                    Debug.Log("[StageManager] Fade Out Complete. Hook for Equipment Turn.");
                    
                    // 今回はそのまま後半待機へ
                    SetState(StageState_Alpha.WaitToStartSecondHalf);
                    
                    // UIをクリアしておく（または後半用に再構成）
                    if (currentStageData != null && currentStageData.secondHalf != null)
                    {
                        activeSequence = currentStageData.secondHalf;
                        sequenceBarUI.Setup(activeSequence);
                        spawnManager.SetupSequence(activeSequence);
                    }
                    else
                    {
                        Debug.LogError("[StageManager] currentStageData or its secondHalf is not assigned! Cannot prepare second half.");
                        activeSequence = null; // 安全のためnullにする
                    }
                    
                    fadeController.FadeIn();
                });
            }
            else if (currentState == StageState_Alpha.BossFight)
            {
                SetState(StageState_Alpha.StageClear);
                Debug.Log("[StageManager] STAGE CLEAR!");
            }
        }

        private void StartSecondHalf()
        {
            currentSequenceTime = 0f;
            sequenceBarUI.UpdateProgress(0f);
            SetState(StageState_Alpha.SecondHalf);
        }
    }
}
