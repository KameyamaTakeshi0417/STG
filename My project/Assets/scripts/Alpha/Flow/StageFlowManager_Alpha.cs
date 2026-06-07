using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Alpha.Flow
{
    public class StageFlowManager_Alpha : MonoBehaviour
    {
        public static StageFlowManager_Alpha Instance { get; private set; }

        public enum StagePhase
        {
            None,
            FirstHalf_MobWave,
            MidBoss_Battle,
            RewardPhase1,
            SecondHalf_MobWave,
            RewardPhase2,
            EnhancementShop,
            PreBossADV,
            Boss_Battle,
            Boss_ClearSequence,
            Boss_RewardPhase3,
            StageTransition
        }

        [Header("Current State")]
        public StagePhase currentPhase = StagePhase.None;
        public int currentStageLevel = 1;

        [Header("Phase Settings")]
        public float midBossEscapeTime = 60f; // 中ボスの逃走までの時間（秒）
        
        [Header("Pre-Boss Sequence")]
        [Tooltip("ボス戦前に再生するADVデータ")]
        public Data.ADVData_Alpha preBossADV;

        [Header("Boss Clear Sequence")]
        [Tooltip("ボス撃破時に展開する草生成スクリプト")]
        public Environment.ProceduralGrassGenerator_Alpha grassGenerator;
        [Tooltip("StageClearの文字を表示するCanvasGroup（初期透明度0）")]
        public CanvasGroup stageClearBoard;
        [Tooltip("画面を暗転させる黒背景のCanvasGroup（初期透明度0）")]
        public CanvasGroup blackFadeBoard;
        [Tooltip("ボス撃破後に再生するADVデータ")]
        public Data.ADVData_Alpha postClearADV;
        
        private float phaseTimer = 0f;
        private bool isMidBossDefeated = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // ゲーム開始時は前半戦からスタート
            StartPhase(StagePhase.FirstHalf_MobWave);
        }

        private void Update()
        {
            if (currentPhase == StagePhase.None) return;

            phaseTimer += Time.deltaTime;

            // フェーズごとの毎フレーム処理（タイマー監視など）
            switch (currentPhase)
            {
                case StagePhase.MidBoss_Battle:
                    if (phaseTimer >= midBossEscapeTime)
                    {
                        MidBossEscaped();
                    }
                    break;
                // 他のウェーブ時間管理などもここで行う
            }
        }

        public void StartPhase(StagePhase nextPhase)
        {
            currentPhase = nextPhase;
            phaseTimer = 0f;

            Debug.Log($"[StageFlow] Transitioning to Phase: {currentPhase}");

            switch (currentPhase)
            {
                case StagePhase.FirstHalf_MobWave:
                    // TODO: 前半の雑魚スポーナーを起動
                    break;

                case StagePhase.MidBoss_Battle:
                    isMidBossDefeated = false;
                    // TODO: 中ボスをスポーンさせる
                    break;

                case StagePhase.RewardPhase1:
                    // 中ボス撃破時の報酬フェーズ
                    HealPlayer(0.10f); // 10%回復
                    StartCoroutine(WaitUntilAllOrbsCollected(() => {
                        // オーブ回収後に報酬UIを開く
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                StartPhase(StagePhase.SecondHalf_MobWave);
                            });
                        }
                        else
                        {
                            StartPhase(StagePhase.SecondHalf_MobWave);
                        }
                    }));
                    break;

                case StagePhase.SecondHalf_MobWave:
                    // TODO: 後半の雑魚スポーナーを起動
                    break;

                case StagePhase.RewardPhase2:
                    // 雑魚ウェーブ終了時の報酬フェーズ（回復は鍛冶フェーズで行うため削除）
                    StartCoroutine(WaitUntilAllOrbsCollected(() => {
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                StartPhase(StagePhase.EnhancementShop);
                            });
                        }
                        else
                        {
                            StartPhase(StagePhase.EnhancementShop);
                        }
                    }));
                    break;

                case StagePhase.EnhancementShop:
                    Debug.Log("[StageFlow] Shop Phase Started.");
                    if (Alpha.UI.BlacksmithManager_Alpha.Instance != null)
                    {
                        Alpha.UI.BlacksmithManager_Alpha.Instance.OpenBlacksmith();
                    }
                    else
                    {
                        Debug.LogWarning("[StageFlow] BlacksmithManager is missing! Skipping phase.");
                        StartPhase(StagePhase.PreBossADV);
                    }
                    break;

                case StagePhase.PreBossADV:
                    // ボス前ADVを展開
                    if (preBossADV != null && Alpha.UI.ADV.ADVManager_Alpha.Instance != null)
                    {
                        Alpha.UI.ADV.ADVManager_Alpha.Instance.StartADV(preBossADV, () => 
                        {
                            StartPhase(StagePhase.Boss_Battle);
                        });
                    }
                    else
                    {
                        // データが無ければ即ボス戦へ
                        StartPhase(StagePhase.Boss_Battle);
                    }
                    break;

                case StagePhase.Boss_Battle:
                    // TODO: ボスをスポーンさせる
                    break;

                case StagePhase.Boss_ClearSequence:
                    StartCoroutine(BossClearSequenceRoutine());
                    break;

                case StagePhase.Boss_RewardPhase3:
                    // ボス戦報酬フェーズの最初でフリースロットを拡張
                    ExpandFreeSlot();
                    HealPlayer(0.20f); // 20%回復
                    
                    StartCoroutine(WaitUntilAllOrbsCollected(() => {
                        if (RewardSequenceManager_Alpha.Instance != null)
                        {
                            RewardSequenceManager_Alpha.Instance.StartRewardSequence(() => {
                                StartPhase(StagePhase.StageTransition);
                            });
                        }
                        else
                        {
                            StartPhase(StagePhase.StageTransition);
                        }
                    }));
                    break;

                case StagePhase.StageTransition:
                    // 次のステージへ
                    Debug.Log("[StageFlow] Stage Transition");
                    currentStageLevel++;
                    
                    // ステージ遷移時に報酬ゲージを初期化する
                    if (RewardManager_Alpha.Instance != null)
                    {
                        RewardManager_Alpha.Instance.ResetRewardCycle();
                    }

                    // TODO: ステージ遷移処理（演出、レベルアップ、敵の強化など）
                    ProceedToNextStage();
                    break;
            }
        }

        private IEnumerator BossClearSequenceRoutine()
        {
            // 1. GrassGeneratorを展開して草をはやす
            if (grassGenerator != null)
            {
                grassGenerator.gameObject.SetActive(true); // アクティブ化を保証
                grassGenerator.GenerateGrass();
                Debug.Log("[StageFlow] Generated Grass on Boss Clear.");
            }
            else
            {
                Debug.LogWarning("[StageFlow] GrassGenerator is not assigned in the inspector!");
            }

            // 少し待機（草が生え揃うのを待つ）
            yield return new WaitForSeconds(1.5f);

            // 2. StageClearのボードをフェードイン
            if (stageClearBoard != null)
            {
                stageClearBoard.gameObject.SetActive(true);
                stageClearBoard.alpha = 0f;
                yield return stageClearBoard.DOFade(1f, 1f).WaitForCompletion();
                
                // フェードイン後、少しの間文字を見せる
                yield return new WaitForSeconds(2f);
            }

            // 3. 黒ボードでゆっくりフェードアウト (文字も上から覆われて消える想定)
            if (blackFadeBoard != null)
            {
                blackFadeBoard.gameObject.SetActive(true);
                blackFadeBoard.alpha = 0f;
                yield return blackFadeBoard.DOFade(1f, 2f).WaitForCompletion();
            }

            // 4. クリア後のADVを出す
            if (postClearADV != null && Alpha.UI.ADV.ADVManager_Alpha.Instance != null)
            {
                Alpha.UI.ADV.ADVManager_Alpha.Instance.StartADV(postClearADV, () => 
                {
                    // ADV終了後の処理（暗転したまま報酬画面へ）
                    StartPhase(StagePhase.Boss_RewardPhase3);
                });
            }
            else
            {
                // ADVデータが無い場合はそのまま報酬へ
                StartPhase(StagePhase.Boss_RewardPhase3);
            }
        }

        // --- 中ボスイベント ---
        public void OnMidBossDefeated()
        {
            if (currentPhase != StagePhase.MidBoss_Battle) return;
            isMidBossDefeated = true;
            Debug.Log("[StageFlow] Mid-Boss Defeated!");
            
            // ドロップ処理を待ってからフェーズ移行
            StartPhase(StagePhase.RewardPhase1);
        }

        private void MidBossEscaped()
        {
            if (currentPhase != StagePhase.MidBoss_Battle) return;
            Debug.Log("[StageFlow] Mid-Boss Escaped!");
            
            // 逃走した場合は報酬フェーズ①をスキップして後半戦へ
            StartPhase(StagePhase.SecondHalf_MobWave);
        }

        // --- ボスイベント ---
        public void OnBossDefeated()
        {
            if (currentPhase != StagePhase.Boss_Battle) return;
            Debug.Log("[StageFlow] Boss Defeated!");
            StartPhase(StagePhase.Boss_RewardPhase3);
        }

        // --- ユーティリティ・処理 ---

        private IEnumerator WaitUntilAllOrbsCollected(System.Action onComplete)
        {
            // TODO: 画面上のオーブが全て回収されるまで待機する処理
            // 例: FindObjectsOfType<OrbItem_Alpha>().Length == 0 になるまで待つ、
            // もしくは自動回収（マグネット化）を行って吸い込み終わるまで待つ。
            
            // 仮実装: 2秒待つ
            Debug.Log("[StageFlow] Waiting for orbs to be collected...");
            yield return new WaitForSeconds(2f);
            
            onComplete?.Invoke();
        }

        private void HealPlayer(float percentage)
        {
            var pStatus = FindObjectOfType<playerStatusManager_Alpha>();
            if (pStatus != null)
            {
                pStatus.Heal(pStatus.HP * percentage);
            }
            Debug.Log($"[StageFlow] Healed player by {percentage * 100}% of Max HP.");
        }

        private void ExpandFreeSlot()
        {
            if (InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.Instance.AddFreeSlot();
            }
            Debug.Log("[StageFlow] Expanded Free Slot by 1!");
        }

        private void ProceedToNextStage()
        {
            Debug.Log($"[StageFlow] Proceeding to Next Stage... (Current: {currentStageLevel})");
            
            // 1. テンポラリーインベントリ内のアイテムを経験値へ変換
            ConvertTempInventoryToExp();

            // 2. ステージレベル加算
            currentStageLevel++;

            // 3. 敵のステータス倍率等の更新処理
            
            // 4. 次のステージの最初（前半戦）へ戻る
            StartPhase(StagePhase.FirstHalf_MobWave);
        }

        private void ConvertTempInventoryToExp()
        {
            if (InventoryManager_Alpha.Instance != null)
            {
                InventoryManager_Alpha.Instance.SellTemporaryItems();
            }
            Debug.Log("[StageFlow] Converted all temporary items to EXP.");
        }
    }
}
