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

            // 繧ｲ繝ｼ繝髢句ｧ区凾縺ｮ繝輔ぉ繝ｼ繝峨う繝ｳ
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
                // 繝輔ぉ繝ｼ繝峨さ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺後↑縺・ｴ蜷医・縺昴・縺ｾ縺ｾ髢句ｧ九☆繧・
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
                                // 繝懊せ蜑榊ｱ驟ｬ繝輔ぉ繝ｼ繧ｺ繝ｻ骰帛・繝輔ぉ繝ｼ繧ｺ繧貞ｱ暮幕・医ヵ繧ｧ繝ｼ繝峨い繧ｦ繝医＆縺帙※縺九ｉ髢句ｧ具ｼ・
                                if (fadeController != null)
                                {
                                    fadeController.FadeOut(() => {
                                        if (RewardSequenceManager_Alpha.Instance != null)
                                        {
                                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                                if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                                                {
                                                    Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                                                }
                                                else
                                                {
                                                    StartPreBossADVAndFight();
                                                }
                                            });
                                        }
                                        else
                                        {
                                            if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                                            {
                                                Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                                            }
                                            else
                                            {
                                                StartPreBossADVAndFight();
                                            }
                                        }
                                    });
                                }
                                else
                                {
                                    // 繝輔ぉ繝ｼ繝峨さ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺檎┌縺・ｴ蜷医・繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
                                    if (RewardSequenceManager_Alpha.Instance != null)
                                    {
                                        RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                            if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                                            {
                                                Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                                            }
                                            else
                                            {
                                                StartPreBossADVAndFight();
                                            }
                                        });
                                    }
                                    else
                                    {
                                        if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                                        {
                                            Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                                        }
                                        else
                                        {
                                            StartPreBossADVAndFight();
                                        }
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
            // HP縺ｪ縺ｩ縺ｮ繝励Ξ繧､繝､繝ｼ迥ｶ諷九・蝗槫ｾｩ
            if (playerStatusManager_Alpha.Instance != null)
            {
                playerStatusManager_Alpha.Instance.currentHP = playerStatusManager_Alpha.Instance.HP;
            }

            // 謨ｵ蠑ｾ繝ｻ謨ｵ縺ｮ繝ｪ繧ｻ繝・ヨ
            ClearAllEnemyBullets();
            if (spawnManager != null)
            {
                // 蠢・ｦ√↓蠢懊§縺ｦ謨ｵ縺ｮ蜈ｨ貊・ｄ迥ｶ諷九Μ繧ｻ繝・ヨ蜃ｦ逅・
            }

            // 繧ｹ繝・・繧ｸ繧貞燕蜊頑姶縺九ｉ蜀埼幕
            StartFirstHalf();
        }

        private void UpdateSequence()
        {
            if (activeSequence == null) return;

            // 繝昴・繧ｺ譎ゅｄ繝√Η繝ｼ繝医Μ繧｢繝ｫ陦ｨ遉ｺ荳ｭ縺ｯ譎る俣繧帝ｲ繧√↑縺・
            if (Time.timeScale == 0f) return;
            if (Alpha.UI.TutorialManager_Alpha.Instance != null && Alpha.UI.TutorialManager_Alpha.Instance.IsPausingTimeline) return;

            currentSequenceTime += Time.deltaTime;
            
            // 繝√Η繝ｼ繝医Μ繧｢繝ｫ縺ｮ繝√ぉ繝・け
            CheckTutorials();

            // 繧ｦ繧ｧ繝ｼ繝悶・繝√ぉ繝・け
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
            // 繝昴・繧ｺ譎ゅ・繧ｹ繧ｭ繝・・繧貞女縺台ｻ倥￠縺ｪ縺・
            if (Time.timeScale == 0f) return;

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
                        
                        // 荳ｭ繝懊せ謦・ｴ蝣ｱ驟ｬ縺ｮ繧ｪ繝ｼ繝悶ｒ荳譁蛾幕蟆・
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                // 蝣ｱ驟ｬ逕ｻ髱｢縺檎ｵゅｏ縺｣縺ｦ縺九ｉ蠕悟濠蠕・ｩ溘∈
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
            // 1. 闕臥函縺・
            var grassGenerator = FindObjectOfType<Environment.ProceduralGrassGenerator_Alpha>();
            if (grassGenerator != null)
            {
                grassGenerator.gameObject.SetActive(true);
                grassGenerator.GenerateGrass(0.2f); // 0.2遘偵°縺代※騾｣邯夂函謌・
            }

            // 菴咎渊
            yield return new WaitForSeconds(1.5f);

            // 2. 繧ｯ繝ｪ繧｢貍泌・ (Stage Clear繝・く繧ｹ繝郁｡ｨ遉ｺ)
            if (stageClearText != null)
            {
                if (Alpha.Audio.SoundManager_Alpha.Instance != null && currentStageData != null && currentStageData.stageClearSE != null)
                {
                    Alpha.Audio.SoundManager_Alpha.Instance.PlaySE(currentStageData.stageClearSE);
                }

                stageClearText.gameObject.SetActive(true);
                CanvasGroup cg = stageClearText.gameObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = stageClearText.gameObject.AddComponent<CanvasGroup>();
                
                // 繝輔ぉ繝ｼ繝峨う繝ｳ
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

            // 3. 縺昴・迥ｶ諷九・縺ｾ縺ｾ繝輔ぉ繝ｼ繝峨い繧ｦ繝医＠縺ｦADV縺ｸ
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

            // STAGE CLEAR 繝・く繧ｹ繝医・證苓ｻ｢蠕後↓髱櫁｡ｨ遉ｺ縺ｫ謌ｻ縺・
            if (stageClearText != null) stageClearText.gameObject.SetActive(false);

            // 4. 繝懊せ蠕窟DV
            if (currentStageData.postBossADV != null && ADVManager_Alpha.Instance != null && currentStageData.postBossADV.pages != null && currentStageData.postBossADV.pages.Count > 0)
            {
                // 證苓ｻ｢縺励◆縺ｾ縺ｾADV繧帝幕蟋・
                ADVManager_Alpha.Instance.StartADV(currentStageData.postBossADV, () => {
                    // ADV邨ゆｺ・ｾ後∵ｬ｡繧ｹ繝・・繧ｸ驕ｷ遘ｻ貅門ｙ縺ｸ
                    ExecuteStageClearBackEnd();
                });
            }
            else
            {
                if (grassGenerator != null) grassGenerator.ClearGrass();
                ExecuteStageClearBackEnd();
            }
        }

        private void ExecuteStageClearBackEnd()
        {
            // 谺｡縺ｮ繧ｹ繝・・繧ｸ縺ｸ驕ｷ遘ｻ縺吶ｋ蜑阪↓闕峨ｒ螳悟・縺ｫ豸亥悉縺吶ｋ
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

            // 1. 蝣ｱ驟ｬ繧ｲ繝ｼ繧ｸ縺ｮ繝ｪ繧ｻ繝・ヨ
            if (RewardManager_Alpha.Instance != null)
            {
                RewardManager_Alpha.Instance.ResetRewardCycle();
            }

            // 2. 繝励Ξ繧､繝､繝ｼ縺ｮ蝗槫ｾｩ蜃ｦ逅・
            if (playerStatusManager_Alpha.Instance != null)
            {
                // 繧ｹ繧ｿ繝溘リ蜈ｨ蠢ｫ
                playerStatusManager_Alpha.Instance.currentStamina = playerStatusManager_Alpha.Instance.maxStamina;
                
                // HP繧呈怙螟ｧHP縺ｮ30%蝗槫ｾｩ (繧ｪ繝ｼ繝舌・繝輔Ο繝ｼ蜃ｦ逅・・ Heal 蜀・〒蟇ｾ蠢懈ｸ医∩)
                float healAmount = playerStatusManager_Alpha.Instance.HP * 0.3f;
                playerStatusManager_Alpha.Instance.Heal(healAmount);
                
                Debug.Log($"[StageManager] Player recovered. Healed {healAmount} HP.");
            }

            // 3. 繝輔Μ繝ｼ繧ｹ繝ｭ繝・ヨ縺ｮ霑ｽ蜉
            if (InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.Instance.AddFreeSlot();
            }

            // 4. 谺｡繧ｹ繝・・繧ｸ縺ｸ縺ｮ驕ｷ遘ｻ貅門ｙ (縺吶〒縺ｫ證苓ｻ｢縺励※縺・ｋ諠ｳ螳・
            // StartCoroutine(StageClearTransitionRoutine()) 縺ｮ莉｣繧上ｊ縺ｫ縺昴・縺ｾ縺ｾ蜃ｦ逅・
            currentStageIndex++;
            if (stageList != null && currentStageIndex < stageList.Length && stageList[currentStageIndex] != null)
            {
                currentStageData = stageList[currentStageIndex];
                
                // 謨ｵ繧・ｼｾ縺ｪ縺ｩ繧呈祉髯､
                ClearAllEnemyBullets();
                
                // 谺｡繧ｹ繝・・繧ｸ縺ｮ蛻晄悄蛹・
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
                // 蜈ｨ繧ｹ繝・・繧ｸ繧ｯ繝ｪ繧｢
                Debug.Log("[StageManager] ALL STAGES CLEARED!");
                // 諡轤ｹ繧・ち繧､繝医Ν縺ｫ謌ｻ繧句・逅・ｒ險倩ｿｰ
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
            // STAGE CLEAR 繝・く繧ｹ繝医・繝輔ぉ繝ｼ繝芽｡ｨ遉ｺ
            if (stageClearText != null)
            {
                yield return StartCoroutine(FadeTextRoutine(stageClearText, 2f));
            }
            else
            {
                yield return new WaitForSecondsRealtime(3f);
            }

            // 繝輔ぉ繝ｼ繝峨い繧ｦ繝茨ｼ域囓霆｢・・
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

            // --- 縺薙％縺ｧ陬丞・縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺ｨ谺｡繧ｹ繝・・繧ｸ貅門ｙ ---
            currentStageIndex++;
            if (stageList != null && currentStageIndex < stageList.Length && stageList[currentStageIndex] != null)
            {
                currentStageData = stageList[currentStageIndex];
                
                // 謨ｵ繧・ｼｾ縺ｪ縺ｩ繧呈祉髯､
                ClearAllEnemyBullets();
                
                // 谺｡繧ｹ繝・・繧ｸ縺ｮ蛻晄悄蛹・
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
                // 蜈ｨ繧ｹ繝・・繧ｸ繧ｯ繝ｪ繧｢
                Debug.Log("[StageManager] ALL STAGES CLEARED!");
                // 諡轤ｹ繧・ち繧､繝医Ν縺ｫ謌ｻ繧句・逅・ｒ險倩ｿｰ
            }
        }

        private System.Collections.IEnumerator WaitUntilAllOrbsCollected(System.Action onComplete)
        {
            Debug.Log("[StageManager] Waiting for orbs, items, exp, and petals to be collected...");
            
            // 蜈ｨ縺ｦ縺ｮ繧ｪ繝ｼ繝悶√い繧､繝・Β縲∫ｵ碁ｨ灘､縲∬干蠑√′逕ｻ髱｢荳翫°繧画ｶ医∴繧具ｼ亥叙蠕励＆繧後ｋ・峨∪縺ｧ蠕・ｩ・
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
    }
}
