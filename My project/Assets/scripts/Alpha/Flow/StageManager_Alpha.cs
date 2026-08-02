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
        [Tooltip("")]
        public StageData_Alpha currentStageData;
        [Tooltip("")]
        public StageData_Alpha[] stageList;
        public int currentStageIndex = 0;

        [Header("References")]
        public SpawnManager_Alpha spawnManager;
        public SequenceBarUI_Alpha sequenceBarUI;
        public FadeController_Alpha fadeController;
        [Tooltip("")]
        public TMPro.TextMeshProUGUI stageClearText;
        [Tooltip("")]
        public TMPro.TextMeshProUGUI stageTitleText;

        [Header("State (Read Only)")]
        public StageState_Alpha currentState = StageState_Alpha.None;
        public float currentSequenceTime = 0f;

        public delegate void BossBattleStateChangedHandler(bool isActive);
        public static event BossBattleStateChangedHandler OnBossBattleStateChanged;
        public bool IsBossBattleActive => currentState == StageState_Alpha.MidBossFight || currentState == StageState_Alpha.BossFight;

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void Start()
        {
            SetState(StageState_Alpha.WaitToStartFirstHalf);
            
            if (stageList != null && stageList.Length > 0)
            {
                currentStageData = stageList[currentStageIndex];
            }

            // 郢ｧ・ｲ郢晢ｽｼ郢晢｣ｰ鬮｢蜿･・ｧ蛹ｺ蜃ｾ邵ｺ・ｮ郢晁ｼ斐♂郢晢ｽｼ郢晏ｳｨ縺・ｹ晢ｽｳ
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
                // 郢晁ｼ斐♂郢晢ｽｼ郢晏ｳｨ縺慕ｹ晢ｽｳ郢晏現ﾎ溽ｹ晢ｽｼ郢晢ｽｩ郢晢ｽｼ邵ｺ蠕娯・邵ｺ繝ｻ・ｰ・ｴ陷ｷ蛹ｻ繝ｻ邵ｺ譏ｴ繝ｻ邵ｺ・ｾ邵ｺ・ｾ鬮｢蜿･・ｧ荵昶・郢ｧ繝ｻ
                if (stageTitleText != null && currentStageData != null)
                {
                    stageTitleText.text = currentStageData.stageName;
                    stageTitleText.gameObject.SetActive(true);
                    StartCoroutine(HideTitleTextAfterSeconds(3f));
                }
                
                if (currentState == StageState_Alpha.WaitToStartFirstHalf)
                {
                    StartFirstHalf();
                }
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
                    ClearAllEnemies();
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
                                    // 郢晄㈱縺幄恆讎奇｣ｰ・ｱ鬩滂ｽｬ郢晁ｼ斐♂郢晢ｽｼ郢ｧ・ｺ郢ｧ雋橸ｽｱ證ｮ蟷・
                                    if (fadeController != null)
                                    {
                                        fadeController.FadeOut(() => {
                                            if (RewardSequenceManager_Alpha.Instance != null)
                                            {
                                                RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                                    StartPreBossADVAndFight();
                                                });
                                            }
                                            else
                                            {
                                                StartPreBossADVAndFight();
                                            }
                                        });
                                    }
                                    else
                                    {
                                        if (RewardSequenceManager_Alpha.Instance != null)
                                        {
                                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                                StartPreBossADVAndFight();
                                            });
                                        }
                                        else
                                        {
                                            StartPreBossADVAndFight();
                                        }
                                    }
                            }));
                        }
                        else
                        {
                            if (Alpha.Audio.SoundManager_Alpha.Instance != null && currentStageData.midBossBGM != null)
                            {
                                Alpha.Audio.SoundManager_Alpha.Instance.PlayBGM(currentStageData.midBossBGM, 0.5f);
                            }
                            SetState(StageState_Alpha.MidBossFight);
                            spawnManager.SpawnBoss(activeSequence.bossPrefab);
                        }
                    }
                    break;
            }
        }

        public void RestartStageFromFirstHalf()
        {
            // HP邵ｺ・ｪ邵ｺ・ｩ邵ｺ・ｮ郢晏干ﾎ樒ｹｧ・､郢晢ｽ､郢晢ｽｼ霑･・ｶ隲ｷ荵昴・陜玲ｧｫ・ｾ・ｩ
            if (playerStatusManager_Alpha.Instance != null)
            {
                playerStatusManager_Alpha.Instance.currentHP = playerStatusManager_Alpha.Instance.HP;
            }

            // 隰ｨ・ｵ陟托ｽｾ郢晢ｽｻ隰ｨ・ｵ邵ｺ・ｮ郢晢ｽｪ郢ｧ・ｻ郢昴・繝ｨ
            ClearAllEnemyBullets();
            if (spawnManager != null)
            {
                // 陟｢繝ｻ・ｦ竏壺・陟｢諛環ｧ邵ｺ・ｦ隰ｨ・ｵ邵ｺ・ｮ陷茨ｽｨ雋翫・・・ｿ･・ｶ隲ｷ荵斟懃ｹｧ・ｻ郢昴・繝ｨ陷・ｽｦ騾・・
            }

            // 郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ郢ｧ雋樒√陷企大ｧｶ邵ｺ荵晢ｽ芽怙蝓ｼ蟷・
            StartFirstHalf();
        }

        private bool isSequenceAnimating = false;

        private void UpdateSequence()
        {
            if (activeSequence == null) return;

            // 繝昴・繧ｺ荳ｭ繧・メ繝･繝ｼ繝医Μ繧｢繝ｫ陦ｨ遉ｺ荳ｭ縺ｯ譎る俣繧帝ｲ繧√↑縺・
            if (Time.timeScale == 0f) return;
            if (Alpha.UI.TutorialManager_Alpha.Instance != null && Alpha.UI.TutorialManager_Alpha.Instance.IsPausingTimeline) return;
            if (isSequenceAnimating) return;

            currentSequenceTime += Time.deltaTime;
            
            // 郢昶・ﾎ礼ｹ晢ｽｼ郢晏現ﾎ懃ｹｧ・｢郢晢ｽｫ邵ｺ・ｮ郢昶・縺臥ｹ昴・縺・
            CheckTutorials();

            // 郢ｧ・ｦ郢ｧ・ｧ郢晢ｽｼ郢晄じ繝ｻ郢昶・縺臥ｹ昴・縺・
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
            // 繝昴・繧ｺ荳ｭ縺ｮ繧ｹ繧ｭ繝・・繧貞女縺台ｻ倥￠縺ｪ縺・
            if (Time.timeScale == 0f) return;
            if (isSequenceAnimating) return;

            bool isSkipInput = false;
            if (Alpha.Core.InputManager_Alpha.Instance != null)
            {
                isSkipInput = Alpha.Core.InputManager_Alpha.Instance.GetActionDown(Alpha.Core.ActionType_Alpha.WaveSkip);
            }
            else
            {
                isSkipInput = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift);
            }

            if (isSkipInput)
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
                    
                    int pointsToGain = 0;
                    if (RewardManager_Alpha.Instance != null)
                    {
                        pointsToGain = Mathf.RoundToInt(RewardManager_Alpha.Instance.targetPoints * skipRatio);
                    }

                    StartCoroutine(WaveSkipSequenceRoutine(targetTime, pointsToGain));
                }
            }
        }

        private System.Collections.IEnumerator WaveSkipSequenceRoutine(float targetTime, int pointsToGain)
        {
            isSequenceAnimating = true;

            // 1. 谺｡縺ｮ遘ｻ蜍穂ｽ咲ｽｮ縺ｫ繧ｷ繝ｼ繧ｯ繧ｨ繝ｳ繧ｹ繝舌・縺ｮ繝上Φ繝峨Ν繧堤ｧｻ蜍輔＆縺帙ｋ
            float startProgress = activeSequence.duration > 0f ? currentSequenceTime / activeSequence.duration : 0f;
            float endProgress = activeSequence.duration > 0f ? targetTime / activeSequence.duration : 1f;
            
            yield return StartCoroutine(sequenceBarUI.AnimateProgress(startProgress, endProgress, 0.5f));

            currentSequenceTime = targetTime;
            sequenceBarUI.UpdateProgress(endProgress);

            // 2. 謨ｵ縺ｮ繧ｹ繝昴・繝ｳ
            spawnManager.CheckSpawn(currentSequenceTime);

            // 3. 繧ｹ繧ｳ繧｢陦ｨ遉ｺUI縺ｫ繧ｹ繧ｳ繧｢繧貞刈邂・
            if (RewardManager_Alpha.Instance != null && pointsToGain > 0)
            {
                yield return StartCoroutine(RewardManager_Alpha.Instance.AddPointsSequence(pointsToGain));
            }

            isSequenceAnimating = false;
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
                    Alpha.UI.TutorialManager_Alpha.Instance.ShowTutorial(tEvent.tutorialId, tEvent.useFadeMode, tEvent.displayDuration, tEvent.pauseTimeline);
                }
                currentTutorialIndex++;
            }
        }

        public void SetState(StageState_Alpha newState)
        {
            currentState = newState;
            Debug.Log($"[StageManager] Changed State to: {currentState}");

            bool isCombat = (currentState != StageState_Alpha.Transition &&
                             currentState != StageState_Alpha.StageClear &&
                             currentState != StageState_Alpha.WaitToStartFirstHalf &&
                             currentState != StageState_Alpha.WaitToStartSecondHalf);
            
            if (Alpha.Core.Utils.CursorManager_Alpha.Instance != null)
            {
                Alpha.Core.Utils.CursorManager_Alpha.Instance.SetCombatMode(isCombat);
            }
            
            // 繝懊せ謌ｦ迥ｶ諷九・螟画峩繧帝夂衍
            OnBossBattleStateChanged?.Invoke(IsBossBattleActive);
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
            
            if (Alpha.Audio.SoundManager_Alpha.Instance != null && currentStageData.stageBGM != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlayBGM(currentStageData.stageBGM, 0.5f);
            }

            SetState(StageState_Alpha.FirstHalf);
        }

        private void StartBossFight()
        {
            if (Alpha.Audio.SoundManager_Alpha.Instance != null && currentStageData.bossBGM != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlayBGM(currentStageData.bossBGM, 0.5f);
            }

            SetState(StageState_Alpha.BossFight);
            spawnManager.SpawnBoss(activeSequence.bossPrefab);
        }

        public void ClearAllEnemyBullets()
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

        public void ClearAllEnemies()
        {
            _Health_Base[] allEnemies = FindObjectsOfType<_Health_Base>();
            foreach (var enemy in allEnemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    // 繝励Ξ繧､繝､繝ｼ閾ｪ霄ｫ縺ｯ豸医＆縺ｪ縺・ｈ縺・↓縺吶ｋ
                    if (enemy.gameObject.CompareTag("Player")) continue;
                    
                    Destroy(enemy.gameObject);
                }
            }
            Debug.Log("[StageManager] Cleared all remaining enemies/bosses.");
        }

        public void OnBossDefeated()
        {
            if (Alpha.Audio.SoundManager_Alpha.Instance != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.StopBGM(1.0f);
            }
            ClearAllEnemyBullets();
            
            if (currentState == StageState_Alpha.MidBossFight)
            {
                SetState(StageState_Alpha.Transition);
                
                StartCoroutine(WaitUntilAllOrbsCollected(() => 
                {
                    fadeController.FadeOut(() => 
                    {
                        Debug.Log("[StageManager] Fade Out Complete. Hook for Equipment Turn.");
                        
                        // 闕ｳ・ｭ郢晄㈱縺幄ｬｦ繝ｻ・ｰ・ｴ陜｣・ｱ鬩滂ｽｬ邵ｺ・ｮ郢ｧ・ｪ郢晢ｽｼ郢晄じ・定叉ﾂ隴∬崟蟷戊氣繝ｻ
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                // 陜｣・ｱ鬩滂ｽｬ騾包ｽｻ鬮ｱ・｢邵ｺ讙趣ｽｵ繧・ｽ冗ｸｺ・｣邵ｺ・ｦ邵ｺ荵晢ｽ芽墓ぁ豼陟輔・・ｩ貅倪・
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
                    StartBossClearSequence();
                }));
            }
        }

        public void StartPreBlacksmithADV()
        {
            if (currentStageData.preBlacksmithADV != null && ADVManager_Alpha.Instance != null && currentStageData.preBlacksmithADV.pages != null && currentStageData.preBlacksmithADV.pages.Count > 0)
            {
                ADVManager_Alpha.Instance.StartADV(currentStageData.preBlacksmithADV, () =>
                {
                    if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                        Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                    else
                        StartPostBlacksmithADV();
                });
            }
            else
            {
                if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                    Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                else
                    StartPostBlacksmithADV();
            }
        }

        public void StartPostBlacksmithADV()
        {
            if (currentStageData.postBlacksmithADV != null && ADVManager_Alpha.Instance != null && currentStageData.postBlacksmithADV.pages != null && currentStageData.postBlacksmithADV.pages.Count > 0)
            {
                ADVManager_Alpha.Instance.StartADV(currentStageData.postBlacksmithADV, () =>
                {
                    ExecuteBossRewardPhase();
                });
            }
            else
            {
                ExecuteBossRewardPhase();
            }
        }

        private void ExecuteBossRewardPhase()
        {
            if (RewardSequenceManager_Alpha.Instance != null)
            {
                RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                    ExecuteStageClearBackEnd();
                });
            }
            else
            {
                ExecuteStageClearBackEnd();
            }
        }

        public void StartPreBossADVAndFight()
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
                if (fadeController != null)
                {
                    fadeController.FadeIn(() => {
                        StartBossFight();
                    });
                }
                else
                {
                    StartBossFight();
                }
            }
        }

        private void StartBossClearSequence()
        {
            StartCoroutine(BossClearSequenceRoutine());
        }

        private System.Collections.IEnumerator BossClearSequenceRoutine()
        {
            // 1. 髣戊・蜃ｽ邵ｺ繝ｻ
            var grassGenerator = FindObjectOfType<Environment.ProceduralGrassGenerator_Alpha>();
            if (grassGenerator != null)
            {
                grassGenerator.gameObject.SetActive(true);
                grassGenerator.GenerateGrass(0.2f); // 0.2驕伜・ﾂｰ邵ｺ莉｣窶ｻ鬨ｾ・｣驍ｯ螟ょ・隰後・
            }

            // 闖ｴ蜥取ｸ・
            yield return new WaitForSeconds(1.5f);

            // 2. 郢ｧ・ｯ郢晢ｽｪ郢ｧ・｢雋肴ｳ後・ (Stage Clear郢昴・縺冗ｹｧ・ｹ郢晞メ・｡・ｨ驕会ｽｺ)
            if (stageClearText != null)
            {
                if (Alpha.Audio.SoundManager_Alpha.Instance != null && currentStageData != null && currentStageData.stageClearSE != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(currentStageData.stageClearSE);
                }

                stageClearText.gameObject.SetActive(true);
                CanvasGroup cg = stageClearText.gameObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = stageClearText.gameObject.AddComponent<CanvasGroup>();
                
                // 郢晁ｼ斐♂郢晢ｽｼ郢晏ｳｨ縺・ｹ晢ｽｳ
                float t = 0;
                while (t < 1f)
                {
                    t += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, t / 1f);
                    yield return null;
                }
                cg.alpha = 1f;
                
                yield return new WaitForSeconds(2f);
            }

            // 3. 邵ｺ譏ｴ繝ｻ霑･・ｶ隲ｷ荵昴・邵ｺ・ｾ邵ｺ・ｾ郢晁ｼ斐♂郢晢ｽｼ郢晏ｳｨ縺・ｹｧ・ｦ郢晏現・邵ｺ・ｦADV邵ｺ・ｸ
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

            // STAGE CLEAR 郢昴・縺冗ｹｧ・ｹ郢晏現繝ｻ隴芽挙・ｻ・｢陟募ｾ娯・鬮ｱ讚・ｽ｡・ｨ驕会ｽｺ邵ｺ・ｫ隰鯉ｽｻ邵ｺ繝ｻ
            if (stageClearText != null) stageClearText.gameObject.SetActive(false);

            // 4. 郢晄㈱縺幄慕ｪ櫂V
            if (currentStageData.postBossADV != null && ADVManager_Alpha.Instance != null && currentStageData.postBossADV.pages != null && currentStageData.postBossADV.pages.Count > 0)
            {
                // 隴芽挙・ｻ・｢邵ｺ蜉ｱ笳・ｸｺ・ｾ邵ｺ・ｾADV郢ｧ蟶晏ｹ戊沂繝ｻ
                ADVManager_Alpha.Instance.StartADV(currentStageData.postBossADV, () => {
                    // ADV終了後、売却フェーズへ
                    StartPreBlacksmithADV();
                    });
            }
            else
            {
                if (grassGenerator != null) grassGenerator.ClearGrass();
                StartPreBlacksmithADV();
            }
        }

        private void ExecuteStageClearBackEnd()
        {
            // 隹ｺ・｡邵ｺ・ｮ郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ邵ｺ・ｸ鬩包ｽｷ驕假ｽｻ邵ｺ蜷ｶ・玖恆髦ｪ竊馴藍蟲ｨ・定楜謔溘・邵ｺ・ｫ雎ｸ莠･謔臥ｸｺ蜷ｶ・・
            var grassGenerators = Resources.FindObjectsOfTypeAll<Environment.ProceduralGrassGenerator_Alpha>();
            Debug.Log($"[StageManager] Found {grassGenerators.Length} grass generators to clear.");
            foreach (var generator in grassGenerators)
            {
                // prefab assets are also returned by Resources.FindObjectsOfTypeAll, so we filter them out
                if (generator.gameObject.scene.name != null)
                {
                    generator.ClearGrass();
                    generator.gameObject.SetActive(false);
                }
            }

            SetState(StageState_Alpha.StageClear);
            Debug.Log("[StageManager] STAGE CLEAR (BackEnd)!");

            // 1. 陜｣・ｱ鬩滂ｽｬ郢ｧ・ｲ郢晢ｽｼ郢ｧ・ｸ邵ｺ・ｮ郢晢ｽｪ郢ｧ・ｻ郢昴・繝ｨ
            if (RewardManager_Alpha.Instance != null)
            {
                RewardManager_Alpha.Instance.ResetRewardCycle();
            }

            // 2. 郢晏干ﾎ樒ｹｧ・､郢晢ｽ､郢晢ｽｼ邵ｺ・ｮ陜玲ｧｫ・ｾ・ｩ陷・ｽｦ騾・・
            if (playerStatusManager_Alpha.Instance != null)
            {
                // 郢ｧ・ｹ郢ｧ・ｿ郢晄ｺ倥Μ陷茨ｽｨ陟｢・ｫ
                playerStatusManager_Alpha.Instance.currentStamina = playerStatusManager_Alpha.Instance.maxStamina;
                
                // HP郢ｧ蜻域呵棔・ｧHP邵ｺ・ｮ30%陜玲ｧｫ・ｾ・ｩ (郢ｧ・ｪ郢晢ｽｼ郢晁・繝ｻ郢晁ｼ釆溽ｹ晢ｽｼ陷・ｽｦ騾・・繝ｻ Heal 陷繝ｻ縲定汞・ｾ陟｢諛茨ｽｸ蛹ｻ竏ｩ)
                float healAmount = playerStatusManager_Alpha.Instance.HP * 0.3f;
                playerStatusManager_Alpha.Instance.Heal(healAmount);
                
                Debug.Log($"[StageManager] Player recovered. Healed {healAmount} HP.");
            }

            // 3. 郢晁ｼ釆懃ｹ晢ｽｼ郢ｧ・ｹ郢晢ｽｭ郢昴・繝ｨ邵ｺ・ｮ髴托ｽｽ陷会｣ｰ
            if (InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.Instance.AddFreeSlot();
            }

            // 4. 隹ｺ・｡郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ邵ｺ・ｸ邵ｺ・ｮ鬩包ｽｷ驕假ｽｻ雋・摩・・(邵ｺ蜷ｶ縲堤ｸｺ・ｫ隴芽挙・ｻ・｢邵ｺ蜉ｱ窶ｻ邵ｺ繝ｻ・玖ｫ・ｳ陞ｳ繝ｻ
            // StartCoroutine(StageClearTransitionRoutine()) 邵ｺ・ｮ闔会ｽ｣郢ｧ荳奇ｽ顔ｸｺ・ｫ邵ｺ譏ｴ繝ｻ邵ｺ・ｾ邵ｺ・ｾ陷・ｽｦ騾・・
            currentStageIndex++;
            if (stageList != null && currentStageIndex < stageList.Length && stageList[currentStageIndex] != null)
            {
                currentStageData = stageList[currentStageIndex];
                
                // 隰ｨ・ｵ郢ｧ繝ｻ・ｼ・ｾ邵ｺ・ｪ邵ｺ・ｩ郢ｧ蜻育･蛾ｫｯ・､
                ClearAllEnemyBullets();
                ClearAllEnemies();
                
                // 隹ｺ・｡郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ邵ｺ・ｮ陋ｻ譎・ｄ陋ｹ繝ｻ
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
                // 陷茨ｽｨ郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ郢ｧ・ｯ郢晢ｽｪ郢ｧ・｢
                Debug.Log("[StageManager] ALL STAGES CLEARED!");
                // 隲｡・ｰ霓､・ｹ郢ｧ繝ｻ縺｡郢ｧ・､郢晏現ﾎ晉ｸｺ・ｫ隰鯉ｽｻ郢ｧ蜿･繝ｻ騾・・・帝坎蛟ｩ・ｿ・ｰ
            }
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
            // STAGE CLEAR 郢昴・縺冗ｹｧ・ｹ郢晏現繝ｻ郢晁ｼ斐♂郢晢ｽｼ郢晁歓・｡・ｨ驕会ｽｺ
            if (stageClearText != null)
            {
                yield return StartCoroutine(FadeTextRoutine(stageClearText, 2f));
            }
            else
            {
                yield return new WaitForSecondsRealtime(3f);
            }

            // 郢晁ｼ斐♂郢晢ｽｼ郢晏ｳｨ縺・ｹｧ・ｦ郢晁肩・ｼ蝓溷專髴・ｽ｢繝ｻ繝ｻ
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

            // --- 邵ｺ阮呻ｼ・ｸｺ・ｧ髯ｬ荳槭・邵ｺ・ｮ郢ｧ・ｯ郢晢ｽｪ郢晢ｽｼ郢晢ｽｳ郢ｧ・｢郢昴・繝ｻ邵ｺ・ｨ隹ｺ・｡郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ雋・摩・・---
            currentStageIndex++;
            if (stageList != null && currentStageIndex < stageList.Length && stageList[currentStageIndex] != null)
            {
                currentStageData = stageList[currentStageIndex];
                
                // 隰ｨ・ｵ郢ｧ繝ｻ・ｼ・ｾ邵ｺ・ｪ邵ｺ・ｩ郢ｧ蜻育･蛾ｫｯ・､
                ClearAllEnemyBullets();
                ClearAllEnemies();
                
                // 隹ｺ・｡郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ邵ｺ・ｮ陋ｻ譎・ｄ陋ｹ繝ｻ
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
                // 陷茨ｽｨ郢ｧ・ｹ郢昴・繝ｻ郢ｧ・ｸ郢ｧ・ｯ郢晢ｽｪ郢ｧ・｢
                Debug.Log("[StageManager] ALL STAGES CLEARED!");
                // 隲｡・ｰ霓､・ｹ郢ｧ繝ｻ縺｡郢ｧ・､郢晏現ﾎ晉ｸｺ・ｫ隰鯉ｽｻ郢ｧ蜿･繝ｻ騾・・・帝坎蛟ｩ・ｿ・ｰ
            }
        }

        private System.Collections.IEnumerator WaitUntilAllOrbsCollected(System.Action onComplete)
        {
            Debug.Log("[StageManager] Waiting for orbs, items, exp, and petals to be collected...");
            
            // 陷茨ｽｨ邵ｺ・ｦ邵ｺ・ｮ郢ｧ・ｪ郢晢ｽｼ郢晄じﾂ竏壹＞郢ｧ・､郢昴・ﾎ堤ｸｲ竏ｫ・ｵ遒・ｽｨ轣伉・､邵ｲ竏ｬ蟷ｲ陟鯛・窶ｲ騾包ｽｻ鬮ｱ・｢闕ｳ鄙ｫﾂｰ郢ｧ逕ｻ・ｶ蛹ｻ竏ｴ郢ｧ蜈ｷ・ｼ莠･蜿呵募干・・ｹｧ蠕鯉ｽ九・蟲ｨ竏ｪ邵ｺ・ｧ陟輔・・ｩ繝ｻ
            while (FindObjectsOfType<OrbControll_Alpha>().Length > 0 || 
                   FindObjectsOfType<Alpha.Battle.OrbItem_Alpha>().Length > 0 ||
                   FindObjectsOfType<ItemPickUp>().Length > 0 ||
                   FindObjectsOfType<Alpha.Item.ExpItem_Alpha>().Length > 0 ||
                   FindObjectsOfType<Alpha.Item.PetalItem_Alpha>().Length > 0)
            {
                yield return new WaitForSeconds(0.25f);
            }
            
            Debug.Log("[StageManager] All items collected!");
            onComplete?.Invoke();
        }

        private void StartSecondHalf()
        {
            currentSequenceTime = 0f;
            currentTutorialIndex = 0;
            sequenceBarUI.UpdateProgress(0f);
            
            if (Alpha.Audio.SoundManager_Alpha.Instance != null && currentStageData != null && currentStageData.stageBGM != null)
            {
                Alpha.Audio.SoundManager_Alpha.Instance.PlayBGM(currentStageData.stageBGM, 0.5f);
            }
            
            SetState(StageState_Alpha.SecondHalf);
        }

        public void TriggerSlowMotion(float targetTimeScale = 0.7f, float duration = 3f)
        {
            StartCoroutine(SlowMotionRoutine(targetTimeScale, duration));
        }

        private System.Collections.IEnumerator SlowMotionRoutine(float targetTimeScale, float duration)
        {
            Time.timeScale = targetTimeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }
}

