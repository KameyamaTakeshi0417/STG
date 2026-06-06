using UnityEngine;
using UnityEngine.Events;
using Alpha.Data;
using Alpha.UI;
using Alpha.UI.ADV;

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
        [Tooltip("現在のステージデータ")]
        public StageData_Alpha currentStageData;
        [Tooltip("連続プレイするステージのリスト")]
        public StageData_Alpha[] stageList;
        public int currentStageIndex = 0;

        [Header("References")]
        public SpawnManager_Alpha spawnManager;
        public SequenceBarUI_Alpha sequenceBarUI;
        public FadeController_Alpha fadeController;
        [Tooltip("STAGE CLEARを表示するテキスト")]
        public TMPro.TextMeshProUGUI stageClearText;
        [Tooltip("ステージタイトルを表示するテキスト")]
        public TMPro.TextMeshProUGUI stageTitleText;

        [Header("State (Read Only)")]
        public StageState_Alpha currentState = StageState_Alpha.None;
        public float currentSequenceTime = 0f;

        private StageSequenceData_Alpha activeSequence;
        private int currentTutorialIndex = 0;
        private bool wasMobCleared = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (stageClearText != null) stageClearText.gameObject.SetActive(false);
            if (stageTitleText != null) stageTitleText.gameObject.SetActive(false);
        }

        void Start()
        {
            SetState(StageState_Alpha.WaitToStartFirstHalf);
            
            if (stageList != null && stageList.Length > 0)
            {
                currentStageData = stageList[currentStageIndex];
            }

            // ゲーム開始時のフェードイン
            if (fadeController != null)
            {
                if (stageTitleText != null && currentStageData != null)
                {
                    stageTitleText.text = currentStageData.stageName;
                    stageTitleText.gameObject.SetActive(true);
                    StartCoroutine(HideTitleTextAfterSeconds(3f));
                }
                
                fadeController.FadeIn(() => {
                    if (currentState == StageState_Alpha.WaitToStartFirstHalf)
                    {
                        StartFirstHalf();
                    }
                });
            }
            else
            {
                // Fallback
                StartFirstHalf();
            }
        }

        private System.Collections.IEnumerator HideTitleTextAfterSeconds(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (stageTitleText != null) stageTitleText.gameObject.SetActive(false);
        }

        void Update()
        {
            if (spawnManager != null)
            {
                bool currentMobCleared = spawnManager.IsMobCleared();
                if (currentMobCleared && !wasMobCleared && 
                    (currentState == StageState_Alpha.FirstHalf || 
                     currentState == StageState_Alpha.SecondHalf || 
                     currentState == StageState_Alpha.MidBossWait || 
                     currentState == StageState_Alpha.BossWait))
                {
                    ClearAllEnemyBullets();
                }
                wasMobCleared = currentMobCleared;
            }

            switch (currentState)
            {
                case StageState_Alpha.WaitToStartFirstHalf:
                    break;

                case StageState_Alpha.FirstHalf:
                case StageState_Alpha.SecondHalf:
                    UpdateSequence();
                    HandleWaveSkip();
                    break;

                case StageState_Alpha.MidBossWait:
                case StageState_Alpha.BossWait:
                    if (spawnManager.IsMobCleared())
                    {
                        if (currentState == StageState_Alpha.BossWait)
                        {
                            SetState(StageState_Alpha.Transition);
                            StartCoroutine(WaitUntilAllOrbsCollected(() => {
                                StartPreBossADVAndFight();
                            }));
                        }
                        else
                        {
                            SetState(StageState_Alpha.MidBossFight);
                            spawnManager.SpawnBoss(activeSequence.bossPrefab);
                        }
                    }
                    break;
            }
        }

        private void UpdateSequence()
        {
            if (activeSequence == null) return;

            // ポーズ時やチュートリアル表示中は時間を進めない
            if (Time.timeScale == 0f) return;
            if (Alpha.UI.TutorialManager_Alpha.Instance != null && Alpha.UI.TutorialManager_Alpha.Instance.IsShowing) return;

            currentSequenceTime += Time.deltaTime;
            
            // チュートリアルのチェック
            CheckTutorials();

            // ウェーブのチェック
            spawnManager.CheckSpawn(currentSequenceTime);
            sequenceBarUI.UpdateProgress(currentSequenceTime / activeSequence.duration);

            if (currentSequenceTime >= activeSequence.duration)
            {
                if (currentState == StageState_Alpha.FirstHalf)
                {
                    SetState(StageState_Alpha.MidBossWait);
                }
                else if (currentState == StageState_Alpha.SecondHalf)
                {
                    SetState(StageState_Alpha.BossWait);
                }
            }
        }

        private void HandleWaveSkip()
        {
            // ポーズ時はスキップを受け付けない
            if (Time.timeScale == 0f) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (activeSequence == null) return;

                float targetTime = activeSequence.duration;
                float previousTime = 0f;

                foreach (var wave in activeSequence.waves)
                {
                    if (wave.time <= currentSequenceTime + 0.01f)
                    {
                        previousTime = wave.time;
                    }
                    
                    if (wave.time > currentSequenceTime + 0.01f)
                    {
                        targetTime = wave.time;
                        break;
                    }
                }

                if (targetTime > currentSequenceTime)
                {
                    float skipRatio = activeSequence.duration > 0f ? (targetTime - currentSequenceTime) / activeSequence.duration : 0f;
                    
                    if (RewardManager_Alpha.Instance != null)
                    {
                        int pointsToGain = Mathf.RoundToInt(RewardManager_Alpha.Instance.targetPoints * skipRatio);
                        RewardManager_Alpha.Instance.AddPoints(pointsToGain);
                    }

                    currentSequenceTime = targetTime;
                    
                    sequenceBarUI.UpdateProgress(currentSequenceTime / activeSequence.duration);
                    spawnManager.CheckSpawn(currentSequenceTime);
                }
            }
        }

        private void CheckTutorials()
        {
            if (activeSequence.tutorialEvents == null) return;

            while (currentTutorialIndex < activeSequence.tutorialEvents.Count &&
                   currentSequenceTime >= activeSequence.tutorialEvents[currentTutorialIndex].time)
            {
                if (Alpha.UI.TutorialManager_Alpha.Instance != null)
                {
                    var tEvent = activeSequence.tutorialEvents[currentTutorialIndex];
                    Alpha.UI.TutorialManager_Alpha.Instance.ShowTutorial(tEvent.tutorialId, tEvent.useFadeMode, tEvent.displayDuration);
                }
                currentTutorialIndex++;
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
                Debug.LogError("[StageManager] First Half sequence data is missing!");
                return;
            }

            activeSequence = currentStageData.firstHalf;
            currentSequenceTime = 0f;
            currentTutorialIndex = 0;

            sequenceBarUI.Setup(activeSequence);
            spawnManager.SetupSequence(activeSequence);

            SetState(StageState_Alpha.FirstHalf);
        }

        private void StartBossFight()
        {
            SetState(StageState_Alpha.BossFight);
            spawnManager.SpawnBoss(activeSequence.bossPrefab);
        }

        private void ClearAllEnemyBullets()
        {
            Bullet_Base[] bullets = FindObjectsOfType<Bullet_Base>();
            foreach (var b in bullets)
            {
                if (b != null && b.gameObject.activeInHierarchy && b.isEnemyBullet)
                {
                    if (Alpha_ObjectPoolManager.Instance != null && b.sourcePrefab != null)
                    {
                        Alpha_ObjectPoolManager.Instance.Return(b.gameObject, b.sourcePrefab);
                    }
                    else
                    {
                        Destroy(b.gameObject);
                    }
                }
            }
            Debug.Log("[StageManager] Cleared all enemy bullets.");
        }

        public void OnBossDefeated()
        {
            ClearAllEnemyBullets();
            
            if (currentState == StageState_Alpha.MidBossFight)
            {
                SetState(StageState_Alpha.Transition);
                
                StartCoroutine(WaitUntilAllOrbsCollected(() => 
                {
                    fadeController.FadeOut(() => 
                    {
                        Debug.Log("[StageManager] Fade Out Complete. Hook for Equipment Turn.");
                        
                        // 中ボス撃破報酬のオーブを一斉開封
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                // 報酬画面が終わってから後半待機へ
                                fadeController.FadeIn(() => 
                                {
                                    activeSequence = currentStageData.secondHalf;
                                    if (activeSequence != null)
                                    {
                                        sequenceBarUI.Setup(activeSequence);
                                        spawnManager.SetupSequence(activeSequence);
                                        StartSecondHalf();
                                    }
                                });
                            });
                        }
                        else
                        {
                            fadeController.FadeIn(() => 
                            {
                                activeSequence = currentStageData.secondHalf;
                                if (activeSequence != null)
                                {
                                    sequenceBarUI.Setup(activeSequence);
                                    spawnManager.SetupSequence(activeSequence);
                                    StartSecondHalf();
                                }
                            });
                        }
                    });
                }));
            }
            else if (currentState == StageState_Alpha.BossFight)
            {
                SetState(StageState_Alpha.Transition);
                StartCoroutine(WaitUntilAllOrbsCollected(() => 
                {
                    StartPostBossADVAndClear();
                }));
            }
        }

        private void StartPreBossADVAndFight()
        {
            if (currentStageData.preBossADV != null && ADVManager_Alpha.Instance != null && currentStageData.preBossADV.pages != null && currentStageData.preBossADV.pages.Count > 0)
            {
                fadeController.FadeOut(() => {
                    ADVManager_Alpha.Instance.StartADV(currentStageData.preBossADV, () => {
                        fadeController.FadeIn(() => {
                            StartBossFight();
                        });
                    });
                });
            }
            else
            {
                StartBossFight();
            }
        }

        private void StartPostBossADVAndClear()
        {
            if (currentStageData.postBossADV != null && ADVManager_Alpha.Instance != null && currentStageData.postBossADV.pages != null && currentStageData.postBossADV.pages.Count > 0)
            {
                fadeController.FadeOut(() => {
                    ADVManager_Alpha.Instance.StartADV(currentStageData.postBossADV, () => {
                        fadeController.FadeIn(() => {
                            ExecuteStageClear();
                        });
                    });
                });
            }
            else
            {
                ExecuteStageClear();
            }
        }

        private void ExecuteStageClear()
        {
            SetState(StageState_Alpha.StageClear);
            Debug.Log("[StageManager] STAGE CLEAR!");

            // 1. 報酬ゲージのリセット
            if (RewardManager_Alpha.Instance != null)
            {
                RewardManager_Alpha.Instance.ResetRewardCycle();
            }

            // 2. プレイヤーの回復処理
            if (playerStatusManager_Alpha.Instance != null)
            {
                // スタミナ全快
                playerStatusManager_Alpha.Instance.currentStamina = playerStatusManager_Alpha.Instance.maxStamina;
                
                // HPを最大HPの30%回復 (オーバーフロー処理は Heal 内で対応済み)
                float healAmount = playerStatusManager_Alpha.Instance.HP * 0.3f;
                playerStatusManager_Alpha.Instance.Heal(healAmount);
                
                Debug.Log($"[StageManager] Player recovered. Healed {healAmount} HP.");
            }

            // 3. フリースロットの追加
            if (InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.Instance.AddFreeSlot();
            }

            // 4. 次ステージへの遷移演出を開始
            StartCoroutine(StageClearTransitionRoutine());
        }

        private System.Collections.IEnumerator FadeTextRoutine(TMPro.TextMeshProUGUI textUI, float displayDuration)
        {
            if (textUI == null) yield break;
            
            if (!textUI.gameObject.activeSelf) textUI.gameObject.SetActive(true);
            
            CanvasGroup cg = textUI.gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = textUI.gameObject.AddComponent<CanvasGroup>();
            
            // Fade In
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, t / 0.5f);
                yield return null;
            }
            cg.alpha = 1f;
            
            // Wait
            yield return new WaitForSecondsRealtime(displayDuration);
            
            // Fade Out
            t = 0;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
                yield return null;
            }
            cg.alpha = 0f;
            
            textUI.gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator StageClearTransitionRoutine()
        {
            // STAGE CLEAR テキストのフェード表示
            if (stageClearText != null)
            {
                yield return StartCoroutine(FadeTextRoutine(stageClearText, 2f));
            }
            else
            {
                yield return new WaitForSecondsRealtime(3f);
            }

            // フェードアウト（暗転）
            bool isFaded = false;
            if (fadeController != null)
            {
                fadeController.FadeOut(() => { isFaded = true; });
            }
            else
            {
                isFaded = true;
            }
            yield return new WaitUntil(() => isFaded);

            // --- ここで裏側のクリーンアップと次ステージ準備 ---
            currentStageIndex++;
            if (stageList != null && currentStageIndex < stageList.Length && stageList[currentStageIndex] != null)
            {
                currentStageData = stageList[currentStageIndex];
                
                // 敵や弾などを掃除
                ClearAllEnemyBullets();
                
                // 次ステージの初期化
                SetState(StageState_Alpha.WaitToStartFirstHalf);
                wasMobCleared = false;
                
                if (fadeController != null)
                {
                    fadeController.FadeIn(() => {
                        if (stageTitleText != null)
                        {
                            stageTitleText.text = currentStageData.stageName;
                            StartCoroutine(FadeTextRoutine(stageTitleText, 2f));
                        }
                        StartFirstHalf();
                    });
                }
                else
                {
                    if (stageTitleText != null)
                    {
                        stageTitleText.text = currentStageData.stageName;
                        StartCoroutine(FadeTextRoutine(stageTitleText, 2f));
                    }
                    StartFirstHalf();
                }
            }
            else
            {
                // 全ステージクリア
                Debug.Log("[StageManager] ALL STAGES CLEARED!");
                // 拠点やタイトルに戻る処理を記述
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
