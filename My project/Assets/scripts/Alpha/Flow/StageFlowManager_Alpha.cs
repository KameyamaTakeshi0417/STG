using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Boss_Battle,
            Boss_RewardPhase3,
            StageTransition
        }

        [Header("Current State")]
        public StagePhase currentPhase = StagePhase.None;
        public int currentStageLevel = 1;

        [Header("Phase Settings")]
        public float midBossEscapeTime = 60f; // 中ボスの逃亡までの時間（秒）
        
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
            if (Time.timeScale == 0) return; // 報酬画面などで停止中は進行しない

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

            Debug.Log($"[StageFlow] Starting Phase: {currentPhase}");

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
                    // ボス前の報酬フェーズ
                    HealPlayer(0.10f); // 10%回復
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
                    // TODO: 強化・整理・ショップ画面を開く
                    // ショップ画面が閉じられたら StartPhase(StagePhase.Boss_Battle) を呼ぶ想定
                    Debug.Log("[StageFlow] Shop Phase Started.");
                    // 仮実装: すぐに次へ
                    // StartPhase(StagePhase.Boss_Battle);
                    break;

                case StagePhase.Boss_Battle:
                    // TODO: ボスをスポーンさせる
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
                    ProceedToNextStage();
                    break;
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
            
            // 逃亡した場合は報酬フェーズ①をスキップして後半戦へ
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
