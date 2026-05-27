using UnityEngine;
using Alpha.Data;
using Alpha.Battle;

namespace Alpha.Flow
{
    [System.Serializable]
    public struct DropTable
    {
        [Tooltip("追加ドロップが発生する確率（0.0 〜 1.0）")]
        public float extraDropChance;
        
        [Tooltip("Rarity 1 (Common) のドロップウェイト（重み）")]
        public float weightR1;
        [Tooltip("Rarity 2 (Uncommon) のドロップウェイト（重み）")]
        public float weightR2;
        [Tooltip("Rarity 3 (Rare) のドロップウェイト（重み）")]
        public float weightR3;
        [Tooltip("Rarity 4 (Epic/Legendary) のドロップウェイト（重み）")]
        public float weightR4;

        /// <summary>
        /// 設定されたウェイトに基づいて1〜4のレアリティを抽選して返します
        /// </summary>
        public int GetRandomRarity()
        {
            float totalWeight = weightR1 + weightR2 + weightR3 + weightR4;
            if (totalWeight <= 0) return 1; // フェールセーフ

            float rand = Random.value * totalWeight;

            if (rand <= weightR1) return 1;
            rand -= weightR1;

            if (rand <= weightR2) return 2;
            rand -= weightR2;

            if (rand <= weightR3) return 3;
            
            return 4;
        }
    }

    public class RewardManager_Alpha : MonoBehaviour
    {
        public static RewardManager_Alpha Instance { get; private set; }

        [Header("Prefabs")]
        [Tooltip("レアリティ1〜4に対応するオーブプレハブ（インデックス0がレアリティ1、インデックス3がレアリティ4）")]
        public GameObject[] orbPrefabs = new GameObject[4];

        [Header("Drop Tables")]
        [Tooltip("雑魚敵のドロップ確率とレアリティテーブル")]
        public DropTable mobDropTable = new DropTable { extraDropChance = 0.01f, weightR1 = 60f, weightR2 = 35f, weightR3 = 4.9f, weightR4 = 0.1f };
        
        [Tooltip("中ボスのドロップ確率とレアリティテーブル")]
        public DropTable midBossDropTable = new DropTable { extraDropChance = 0.10f, weightR1 = 0f, weightR2 = 60f, weightR3 = 37f, weightR4 = 3f };
        
        [Tooltip("ボスのドロップ確率とレアリティテーブル")]
        public DropTable bossDropTable = new DropTable { extraDropChance = 0.20f, weightR1 = 0f, weightR2 = 0f, weightR3 = 80f, weightR4 = 20f };

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
                SpawnOrb(position, mobDropTable.GetRandomRarity(), OrbSource_Alpha.Mob);
                
                // 追加ドロップ判定
                if (Random.value <= mobDropTable.extraDropChance)
                {
                    SpawnOrb(position + new Vector3(0.5f, 0, 0), mobDropTable.GetRandomRarity(), OrbSource_Alpha.Mob);
                }
            }
        }

        /// <summary>
        /// 中ボス撃破時のドロップ判定
        /// </summary>
        public void DropMidBossReward(Vector3 position)
        {
            int dropCount = Alpha.Flow.StageManager_Alpha.Instance != null ? Alpha.Flow.StageManager_Alpha.Instance.GetCurrentRewardDropCount() : 1;
            
            for (int i = 0; i < dropCount; i++)
            {
                // 横に少しずらしてドロップさせる
                Vector3 spawnPos = position + new Vector3(i * 0.5f - ((dropCount - 1) * 0.25f), 0, 0);
                
                // 100%ドロップ
                SpawnOrb(spawnPos, midBossDropTable.GetRandomRarity(), OrbSource_Alpha.MidBoss);

                // 追加ドロップ判定
                if (Random.value <= midBossDropTable.extraDropChance)
                {
                    SpawnOrb(spawnPos + new Vector3(0.2f, 0.2f, 0), midBossDropTable.GetRandomRarity(), OrbSource_Alpha.MidBoss);
                }
            }
        }

        /// <summary>
        /// ボス撃破時のドロップ判定
        /// </summary>
        public void DropBossReward(Vector3 position, string bossId)
        {
            int dropCount = Alpha.Flow.StageManager_Alpha.Instance != null ? Alpha.Flow.StageManager_Alpha.Instance.GetCurrentRewardDropCount() : 1;

            for (int i = 0; i < dropCount; i++)
            {
                Vector3 spawnPos = position + new Vector3(i * 0.5f - ((dropCount - 1) * 0.25f), 0, 0);

                // 100%ドロップ
                SpawnOrb(spawnPos, bossDropTable.GetRandomRarity(), OrbSource_Alpha.Boss, bossId);

                // 追加ドロップ判定
                if (Random.value <= bossDropTable.extraDropChance)
                {
                    SpawnOrb(spawnPos + new Vector3(0.2f, 0.2f, 0), bossDropTable.GetRandomRarity(), OrbSource_Alpha.Boss, bossId);
                }
            }
        }

        /// <summary>
        /// 指定位置にオーブをスポーンさせる
        /// </summary>
        public void SpawnOrb(Vector3 position, int rarity, OrbSource_Alpha source, string bossId = "")
        {
            // レアリティからプレハブの配列インデックスを計算 (rarity: 1〜4 -> index: 0〜3)
            int index = Mathf.Clamp(rarity - 1, 0, orbPrefabs.Length - 1);
            GameObject prefabToSpawn = orbPrefabs[index];

            if (prefabToSpawn == null)
            {
                Debug.LogError($"[RewardManager] Orb Prefab for rarity {rarity} (index {index}) is not assigned!");
                return;
            }

            GameObject obj = Instantiate(prefabToSpawn, position, Quaternion.identity);
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
    }
}

