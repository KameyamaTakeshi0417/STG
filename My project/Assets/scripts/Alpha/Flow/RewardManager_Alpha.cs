using UnityEngine;
using Alpha.Data;
using Alpha.Battle;

namespace Alpha.Flow
{
    public class RewardManager_Alpha : MonoBehaviour
    {
        public static RewardManager_Alpha Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject orbPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 雑魚撃破時のドロップ判定
        /// </summary>
        public void CheckMobDrop(Vector3 position, float dropChance)
        {
            // 基本ドロップ判定
            if (Random.value <= dropChance)
            {
                SpawnOrb(position, GetRandomMobOrbRarity(), OrbSource_Alpha.Mob);
                
                // 追加ドロップ判定(1%)
                if (Random.value <= 0.01f)
                {
                    SpawnOrb(position + new Vector3(0.5f, 0, 0), GetRandomMobOrbRarity(), OrbSource_Alpha.Mob);
                }
            }
        }

        /// <summary>
        /// 中ボス撃破時のドロップ判定
        /// </summary>
        public void DropMidBossReward(Vector3 position)
        {
            // 100%ドロップ
            SpawnOrb(position, GetRandomMidBossOrbRarity(), OrbSource_Alpha.MidBoss);

            // 追加ドロップ判定(10%)
            if (Random.value <= 0.10f)
            {
                SpawnOrb(position + new Vector3(0.5f, 0, 0), GetRandomMidBossOrbRarity(), OrbSource_Alpha.MidBoss);
            }
        }

        /// <summary>
        /// ボス撃破時のドロップ判定
        /// </summary>
        public void DropBossReward(Vector3 position, string bossId)
        {
            // 100%ドロップ
            SpawnOrb(position, GetRandomBossOrbRarity(), OrbSource_Alpha.Boss, bossId);

            // 追加ドロップ判定(20%)
            if (Random.value <= 0.20f)
            {
                SpawnOrb(position + new Vector3(0.5f, 0, 0), GetRandomBossOrbRarity(), OrbSource_Alpha.Boss, bossId);
            }
        }

        /// <summary>
        /// 指定位置にオーブをスポーンさせる
        /// </summary>
        public void SpawnOrb(Vector3 position, int rarity, OrbSource_Alpha source, string bossId = "")
        {
            if (orbPrefab == null)
            {
                Debug.LogError("[RewardManager] Orb Prefab is not assigned!");
                return;
            }

            GameObject obj = Instantiate(orbPrefab, position, Quaternion.identity);
            OrbItem_Alpha orbItem = obj.GetComponent<OrbItem_Alpha>();
            if (orbItem != null)
            {
                orbItem.orbData = new OrbData_Alpha(rarity, source, bossId);
            }
        }

        /// <summary>
        /// スキップ報酬を直接TreasureManagerに付与する（画面に出さない）
        /// </summary>
        public void GrantSkipReward(float remainingRatio)
        {
            if (remainingRatio >= 0.70f)
            {
                // Rarity 2 確定
                treasureManager_Alpha.Instance.PushOrb(new OrbData_Alpha(2, OrbSource_Alpha.Skip));
                RollSkipAdditionalOrb();
            }
            else if (remainingRatio >= 0.30f)
            {
                // Rarity 1 確定
                treasureManager_Alpha.Instance.PushOrb(new OrbData_Alpha(1, OrbSource_Alpha.Skip));
                RollSkipAdditionalOrb();
            }
            else
            {
                // 30%未満は経験値ドロップ
                GrantExp(remainingRatio);
            }
        }

        private void RollSkipAdditionalOrb()
        {
            if (Random.value <= 0.01f)
            {
                // 追加のレアリティはとりあえずR1とする（必要なら雑魚テーブルなどを引く）
                treasureManager_Alpha.Instance.PushOrb(new OrbData_Alpha(1, OrbSource_Alpha.Skip));
            }
        }

        private void GrantExp(float amount)
        {
            // TODO: EXP付与のロジックをここに繋げる
            Debug.Log($"[RewardManager] Granted {amount} EXP from Skip.");
        }

        // --- レアリティ抽選テーブル ---

        private int GetRandomMobOrbRarity()
        {
            float rand = Random.value * 100f; // 0〜100
            if (rand <= 60f) return 1;
            if (rand <= 95f) return 2; // 60 + 35
            if (rand <= 99.9f) return 3; // 95 + 4.9
            return 4; // 0.1
        }

        private int GetRandomMidBossOrbRarity()
        {
            float rand = Random.value * 100f;
            // R1=0, R2=60, R3=37, R4=3
            if (rand <= 60f) return 2;
            if (rand <= 97f) return 3; // 60 + 37
            return 4; // 3
        }

        private int GetRandomBossOrbRarity()
        {
            float rand = Random.value * 100f;
            // R1=0, R2=0, R3=80, R4=20
            if (rand <= 80f) return 3;
            return 4;
        }
    }
}
