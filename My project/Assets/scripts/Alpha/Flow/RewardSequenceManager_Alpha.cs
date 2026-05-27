using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Data;
using Alpha.UI;

namespace Alpha.Flow
{
    public class RewardSequenceManager_Alpha : MonoBehaviour
    {
        public static RewardSequenceManager_Alpha Instance { get; private set; }

        private System.Action onSequenceComplete;
        private Queue<OrbData_Alpha> sequenceOrbQueue = new Queue<OrbData_Alpha>();

        [Header("UI References")]
        public OrbStackUI_Alpha orbStackUI;
        public RewardSelectionUI_Alpha rewardSelectionUI;
        public InventoryUI_Alpha inventoryUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inventoryUI != null)
                {
                    inventoryUI.ToggleEscapeInventory();
                }
            }
        }

        /// <summary>
        /// 報酬獲得シーケンスを開始する
        /// </summary>
        public void StartRewardSequence(System.Action onComplete)
        {
            onSequenceComplete = onComplete;
            
            // TreasureManagerからキューを受け取る、もしくは直接参照する
            // ※ここでは TreasureManager が持つ情報をコピーして処理する形とします
            if (treasureManager_Alpha.Instance == null)
            {
                Debug.LogError("[RewardSequence] TreasureManager instance not found!");
                EndSequence();
                return;
            }

            // TimeScale を止めてゲーム進行を一時停止
            Time.timeScale = 0f;

            // TreasureManager から未開封オーブを取得してクリア
            sequenceOrbQueue = treasureManager_Alpha.Instance.FlushOrbQueue();

            if (sequenceOrbQueue.Count == 0)
            {
                // 開封するオーブがなければそのまま終了
                EndSequence();
                return;
            }

            // 1. OrbStackUIを表示し、キューの内容をセット
            if (orbStackUI != null)
            {
                orbStackUI.ShowStack(sequenceOrbQueue);
            }

            // 2. 最初のオーブ開封処理へ
            ProcessNextOrb();
        }

        private void ProcessNextOrb()
        {
            if (sequenceOrbQueue.Count == 0)
            {
                // 全てのオーブを開封し終えたらシーケンス終了
                EndSequence();
                return;
            }

            // キューから1つ取り出す
            OrbData_Alpha currentOrb = sequenceOrbQueue.Dequeue();

            // OrbStackUI の表示を更新（1つ減らす、ハイライトを移すなど）
            if (orbStackUI != null)
            {
                orbStackUI.UpdateStackDisplay();
            }

            // TODO: 演出用メソッドを挟む
            // PlayOrbOpenEffect(() => { ShowRewardSelection(currentOrb); });
            
            ShowRewardSelection(currentOrb);
        }

        private void ShowRewardSelection(OrbData_Alpha orb)
        {
            if (rewardSelectionUI != null)
            {
                // 3択を生成してパネルを表示
                rewardSelectionUI.ShowChoices(orb, OnRewardSelected);
            }
            else
            {
                // UIがない場合は自動選択扱い（デバッグ用）
                Debug.LogWarning("[RewardSequence] RewardSelectionUI is missing! Auto-skipping.");
                OnRewardSelected(null); // Dummy
            }
        }

        private void OnRewardSelected(WeaponPartInstance_Alpha selectedReward)
        {
            // 3択のパネルを閉じる
            if (rewardSelectionUI != null) rewardSelectionUI.Hide();

            if (selectedReward != null)
            {
                // インベントリUIを開き、テンポラリに配置させる
                if (inventoryUI != null)
                {
                    inventoryUI.Show(selectedReward, OnInventoryOrganized);
                }
                else
                {
                    // UIがない場合は自動で裏側のマネージャーに突っ込む（旧仕様）
                    InventoryManager_Alpha.EquipInstance newEquip = new InventoryManager_Alpha.EquipInstance();
                    newEquip.series = selectedReward.series;
                    newEquip.partType = selectedReward.partType;
                    newEquip.rarity = selectedReward.quality;
                    newEquip.currentEffects = selectedReward.currentEffects;
                    newEquip.defId = selectedReward.series.seriesName;
                    
                    if (InventoryManager_Alpha.Instance != null)
                    {
                        InventoryManager_Alpha.Instance.AddItem(newEquip);
                    }
                    OnInventoryOrganized();
                }
            }
            else
            {
                // 報酬なし（スキップ等）の場合はすぐ次へ
                OnInventoryOrganized();
            }
        }

        private void OnInventoryOrganized()
        {
            // インベントリ画面の「次へ/確定」ボタンが押されたら呼ばれる
            if (inventoryUI != null) inventoryUI.Hide();

            // 次のオーブへ
            ProcessNextOrb();
        }

        private void EndSequence()
        {
            // 全てのUIを隠す
            if (orbStackUI != null) orbStackUI.Hide();
            if (rewardSelectionUI != null) rewardSelectionUI.Hide();
            if (inventoryUI != null) inventoryUI.Hide();

            // ゲーム再開
            Time.timeScale = 1f;

            Debug.Log("[RewardSequence] Sequence Completed.");

            // 次のフェーズへ進行を通知
            onSequenceComplete?.Invoke();
        }
    }
}
