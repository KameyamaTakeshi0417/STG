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
        public static StageManager_Alpha Instance { get; private set; }

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
        private int currentTutorialIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

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
                    // 雑魚が全滅したら次の処理へ
                    if (spawnManager.IsMobCleared())
                    {
                        if (currentState == StageState_Alpha.BossWait)
                        {
                            SetState(StageState_Alpha.Transition); // 待機用ステートへ移行
                            StartCoroutine(WaitUntilAllOrbsCollected(() => {
                                // ステージクリア時にフリースロットを1つ追加
                                if (InventoryManager_Alpha.Instance != null)
                                {
                                    InventoryManager_Alpha.Instance.AddFreeSlot();
                                }
                                
                                // ボス前の休憩（待機）到達時にオーブを一斉開封
                                if (RewardSequenceManager_Alpha.Instance != null)
                                {
                                    RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                        StartBossFight();
                                    });
                                }
                                else
                                {
                                    StartBossFight();
                                }
                            }));
                        }
                        else
                        {
                            StartBossFight();
                        }
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

        public int GetCurrentRewardDropCount()
        {
            if (activeSequence != null && activeSequence.rewardDropCount > 0)
            {
                return activeSequence.rewardDropCount;
            }
            return 1;
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
            currentTutorialIndex = 0;

            // ステージ（前半）開始時にテンポラリー枠のアイテムを自動売却
            if (InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.Instance.SellTemporaryItems();
            }
            
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

            // チュートリアルイベントのチェック
            if (activeSequence.tutorialEvents != null)
            {
                while (currentTutorialIndex < activeSequence.tutorialEvents.Count &&
                       currentSequenceTime >= activeSequence.tutorialEvents[currentTutorialIndex].time)
                {
                    if (TutorialManager_Alpha.Instance != null)
                    {
                        TutorialManager_Alpha.Instance.ShowTutorial(activeSequence.tutorialEvents[currentTutorialIndex].tutorialId);
                    }
                    currentTutorialIndex++;
                }
            }

            if (currentSequenceTime >= activeSequence.duration)
            {
                // 時間到達で待機状態へ
                if (currentState == StageState_Alpha.FirstHalf)
                    SetState(StageState_Alpha.MidBossWait);
                else
                {
                    SetState(StageState_Alpha.BossWait);
                }
            }
        }

        private void HandleWaveSkip()
        {
            // ポーズ時やチュートリアル表示時はスキップを受け付けない
            if (Time.timeScale == 0f) return;
            if (Alpha.UI.TutorialManager_Alpha.Instance != null && Alpha.UI.TutorialManager_Alpha.Instance.IsShowing) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (activeSequence == null) return;

                // 現在時刻(currentSequenceTime)より未来にある最も近いウェーブの時間を検索する
                float targetTime = activeSequence.duration;
                float previousTime = 0f;
                bool foundNextWave = false;

                foreach (var wave in activeSequence.waves)
                {
                    if (wave.time <= currentSequenceTime + 0.01f)
                    {
                        previousTime = wave.time;
                    }
                    
                    // わずかな浮動小数点誤差を考慮して少し余裕を持たせる
                    if (wave.time > currentSequenceTime + 0.01f)
                    {
                        targetTime = wave.time;
                        foundNextWave = true;
                        break;
                    }
                }

                if (targetTime > currentSequenceTime)
                {
                    // スキップ時の残り割合の計算
                    float totalDistance = targetTime - previousTime;
                    float remainingTime = targetTime - currentSequenceTime;
                    float remainingRatio = totalDistance > 0f ? remainingTime / totalDistance : 0f;

                    Debug.Log($"[StageManager] Wave Skipped! Jumped from {currentSequenceTime:F1} to {targetTime:F1}. Remaining Ratio: {remainingRatio:F2}");
                    
                    // 報酬付与
                    if (RewardManager_Alpha.Instance != null)
                    {
                        RewardManager_Alpha.Instance.GrantSkipReward(remainingRatio);
                    }

                    // 次のウェーブの開始時間へ正確にジャンプ
                    currentSequenceTime = targetTime;
                    
                    // UIとスポーンを即時更新
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
                
                StartCoroutine(WaitUntilAllOrbsCollected(() => 
                {
                    // 暗転開始
                    fadeController.FadeOut(() => 
                    {
                        // 暗転中（真っ黒）のフック処理
                        Debug.Log("[StageManager] Fade Out Complete. Hook for Equipment Turn.");
                        
                        // 中ボス撃破報酬のオーブを一斉開封
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                // 報酬画面が終わってから後半待機へ
                                SetState(StageState_Alpha.WaitToStartSecondHalf);
                                
                                // UIをクリアしておく、または後半用に再構築
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
                        else
                        {
                            SetState(StageState_Alpha.WaitToStartSecondHalf);
                            
                            if (currentStageData != null && currentStageData.secondHalf != null)
                            {
                                activeSequence = currentStageData.secondHalf;
                                sequenceBarUI.Setup(activeSequence);
                                spawnManager.SetupSequence(activeSequence);
                            }
                            else
                            {
                                activeSequence = null;
                            }
                            
                            fadeController.FadeIn();
                        }
                    });
                }));
            }
            else if (currentState == StageState_Alpha.BossFight)
            {
                SetState(StageState_Alpha.StageClear);
                Debug.Log("[StageManager] STAGE CLEAR!");
            }
        }

        private System.Collections.IEnumerator WaitUntilAllOrbsCollected(System.Action onComplete)
        {
            Debug.Log("[StageManager] Waiting for orbs to be collected...");
            
            // 全てのオーブが画面上から消える（取得される）まで待機
            while (FindObjectsOfType<OrbControll_Alpha>().Length > 0 || FindObjectsOfType<Alpha.Battle.OrbItem_Alpha>().Length > 0)
            {
                yield return new WaitForSeconds(0.25f);
            }
            
            // 回収後の処理ズレを防ぐため少し待機
            yield return new WaitForSeconds(0.5f);

            onComplete?.Invoke();
        }

        private void StartSecondHalf()
        {
            currentSequenceTime = 0f;
            currentTutorialIndex = 0;
            sequenceBarUI.UpdateProgress(0f);
            SetState(StageState_Alpha.SecondHalf);
        }
    }
}
