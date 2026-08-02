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
        
        [Tooltip("EXPのプレハブ")]
        public GameObject expPrefab;
        
        [Tooltip("花弁のプレハブ")]
        public GameObject petalPrefab;

        [Header("Drop Tables")]
        [Tooltip("雑魚敵のドロップ確率とレアリティテーブル")]
        public DropTable mobDropTable = new DropTable { extraDropChance = 0.01f, weightR1 = 60f, weightR2 = 35f, weightR3 = 4.9f, weightR4 = 0.1f };
        
        [Tooltip("中ボスのドロップ確率とレアリティテーブル")]
        public DropTable midBossDropTable = new DropTable { extraDropChance = 0.10f, weightR1 = 0f, weightR2 = 60f, weightR3 = 37f, weightR4 = 3f };
        
        [Tooltip("ボスのドロップ確率とレアリティテーブル")]
        public DropTable bossDropTable = new DropTable { extraDropChance = 0.20f, weightR1 = 0f, weightR2 = 0f, weightR3 = 80f, weightR4 = 20f };

        [Header("Reward Gauge System")]
        public int currentPoints = 0;
        public int targetPoints = 100;
        public int targetQuality = 1;
        public int currentRewardIndex = 1;

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
            DetermineNextReward();
            if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
            {
                Alpha.UI.SequenceBarUI_Alpha.Instance.UpdateRewardGauge(currentPoints, targetPoints, targetQuality);
            }
        }

        private int visualPoints = 0;
        private int pendingPoints = 0;
        private bool isAnimatingPoints = false;

        /// <summary>
        /// 報酬ポイントを加算する
        /// </summary>
        public void AddPoints(int points)
        {
            pendingPoints += points;
            
            if (!isAnimatingPoints)
            {
                StartCoroutine(ProcessPointsQueue());
            }
        }

        public System.Collections.IEnumerator AddPointsSequence(int points)
        {
            AddPoints(points);
            while (isAnimatingPoints)
            {
                yield return null;
            }
        }

        private System.Collections.IEnumerator ProcessPointsQueue()
        {
            isAnimatingPoints = true;
            visualPoints = currentPoints;

            while (pendingPoints > 0)
            {
                int startVis = visualPoints;
                int targetVis = visualPoints + pendingPoints;
                pendingPoints = 0;

                float duration = 0.5f;
                float elapsed = 0f;
                float particleTimer = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    visualPoints = Mathf.RoundToInt(Mathf.Lerp(startVis, targetVis, elapsed / duration));
                    
                    if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
                    {
                        Alpha.UI.SequenceBarUI_Alpha.Instance.UpdateRewardGauge(Mathf.Min(visualPoints, targetPoints), targetPoints, targetQuality);

                        particleTimer += Time.deltaTime;
                        if (particleTimer > 0.05f)
                        {
                            particleTimer = 0f;
                            if (Alpha.Core.ProceduralJuiceManager_Alpha.Instance != null && Alpha.UI.SequenceBarUI_Alpha.Instance.rewardGaugeImage != null)
                            {
                                Alpha.Core.ProceduralJuiceManager_Alpha.Instance.SpawnUIParticles(Alpha.UI.SequenceBarUI_Alpha.Instance.rewardGaugeImage.rectTransform);
                            }
                        }
                    }

                    if (visualPoints >= targetPoints)
                    {
                        break;
                    }
                    yield return null;
                }

                if (visualPoints >= targetPoints)
                {
                    if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
                        Alpha.UI.SequenceBarUI_Alpha.Instance.UpdateRewardGauge(targetPoints, targetPoints, targetQuality);

                    // 報酬をフィールドに出す
                    SpawnOrb(new Vector3(0, 5f, 0), targetQuality, OrbSource_Alpha.Mob);
                    
                    // UI満たされたまま0.5秒待機
                    yield return new WaitForSeconds(0.5f);

                    // 一気に0にカウントダウン
                    float drainTime = 0.25f;
                    float dElapsed = 0f;
                    while (dElapsed < drainTime)
                    {
                        dElapsed += Time.deltaTime;
                        int drainVis = Mathf.RoundToInt(Mathf.Lerp(targetPoints, 0, dElapsed / drainTime));
                        if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
                            Alpha.UI.SequenceBarUI_Alpha.Instance.UpdateRewardGauge(drainVis, targetPoints, targetQuality);
                        yield return null;
                    }

                    int excess = visualPoints - targetPoints;
                    
                    currentRewardIndex++;
                    DetermineNextReward();
                    
                    visualPoints = 0;
                    currentPoints = excess;

                    if (excess > 0)
                    {
                        pendingPoints += excess;
                    }
                }
                else
                {
                    visualPoints = targetVis;
                    currentPoints = visualPoints;
                    if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
                        Alpha.UI.SequenceBarUI_Alpha.Instance.UpdateRewardGauge(visualPoints, targetPoints, targetQuality);
                }
            }

            isAnimatingPoints = false;
        }

        /// <summary>
        /// 報酬サイクルをリセットする（ステージ遷移時などに使用）
        /// </summary>
        public void ResetRewardCycle()
        {
            currentPoints = 0;
            visualPoints = 0;
            pendingPoints = 0;
            isAnimatingPoints = false;
            currentRewardIndex = 1;
            DetermineNextReward();
            
            if (Alpha.UI.SequenceBarUI_Alpha.Instance != null)
            {
                Alpha.UI.SequenceBarUI_Alpha.Instance.UpdateRewardGauge(currentPoints, targetPoints, targetQuality);
            }
            
            Debug.Log("[RewardManager] Reward cycle has been reset.");
        }

        private void DetermineNextReward()
        {
            switch (currentRewardIndex)
            {
                case 1: targetQuality = 1; break;
                case 2: targetQuality = mobDropTable.GetRandomRarity(); break;
                case 3: targetQuality = 2; break;
                case 4: targetQuality = 3; break;
                case 5: targetQuality = 4; break;
                default: targetQuality = mobDropTable.GetRandomRarity(); break;
            }
            targetPoints = targetQuality * 100;
        }

        /// <summary>
        /// 中ボス撃破時のドロップ判定
        /// </summary>
        public void DropMidBossReward(Vector3 position, bool forceQuality1 = false)
        {
            int dropCount = Alpha.Flow.StageManager_Alpha.Instance != null ? Alpha.Flow.StageManager_Alpha.Instance.GetCurrentRewardDropCount() : 1;
            
            for (int i = 0; i < dropCount; i++)
            {
                Vector3 spawnPos = position + new Vector3(i * 0.5f - ((dropCount - 1) * 0.25f), 0, 0);
                int quality = forceQuality1 ? 1 : midBossDropTable.GetRandomRarity();
                SpawnOrb(spawnPos, quality, OrbSource_Alpha.MidBoss);
                if (Random.value <= midBossDropTable.extraDropChance)
                {
                    int extraQuality = forceQuality1 ? 1 : midBossDropTable.GetRandomRarity();
                    SpawnOrb(spawnPos + new Vector3(0.2f, 0.2f, 0), extraQuality, OrbSource_Alpha.MidBoss);
                }
            }
        }

        /// <summary>
        /// ボス撃破時のドロップ判定
        /// </summary>
        public void DropBossReward(Vector3 position, string bossId, bool forceQuality1 = false)
        {
            int dropCount = Alpha.Flow.StageManager_Alpha.Instance != null ? Alpha.Flow.StageManager_Alpha.Instance.GetCurrentRewardDropCount() : 1;

            for (int i = 0; i < dropCount; i++)
            {
                Vector3 spawnPos = position + new Vector3(i * 0.5f - ((dropCount - 1) * 0.25f), 0, 0);
                int quality = forceQuality1 ? 1 : bossDropTable.GetRandomRarity();
                SpawnOrb(spawnPos, quality, OrbSource_Alpha.Boss, bossId);
                if (Random.value <= bossDropTable.extraDropChance)
                {
                    int extraQuality = forceQuality1 ? 1 : bossDropTable.GetRandomRarity();
                    SpawnOrb(spawnPos + new Vector3(0.2f, 0.2f, 0), extraQuality, OrbSource_Alpha.Boss, bossId);
                }
            }
        }

        /// <summary>
        /// 指定位置にオーブをスポーンさせる
        /// </summary>
        public void SpawnOrb(Vector3 position, int rarity, OrbSource_Alpha source, string bossId = "")
        {
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
        /// 経験値オブジェクトをスポーンさせる
        /// </summary>
        public void SpawnExp(Vector3 position, int value, float scale, int count)
        {
            if (expPrefab == null) return;
            for (int i = 0; i < count; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
                randomOffset.z = 0; // 2Dの場合はZを0にするなど
                GameObject expObj = Instantiate(expPrefab, position + randomOffset, Quaternion.identity);
                expObj.transform.localScale = Vector3.one * scale;

                Alpha.Item.ExpItem_Alpha expScript = expObj.GetComponent<Alpha.Item.ExpItem_Alpha>();
                if (expScript != null)
                {
                    expScript.expValue = value;
                }
            }
        }

        /// <summary>
        /// 花弁をスポーンさせる
        /// </summary>
        public void SpawnPetal(Vector3 position, int count)
        {
            if (petalPrefab == null) return;
            for (int i = 0; i < count; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
                randomOffset.z = 0;
                Instantiate(petalPrefab, position + randomOffset, Quaternion.identity);
            }
        }
    }
}

