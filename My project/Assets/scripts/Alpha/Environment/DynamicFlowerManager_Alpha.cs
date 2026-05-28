using System.Collections.Generic;
using UnityEngine;

namespace Alpha.Environment
{
    public class DynamicFlowerManager_Alpha : MonoBehaviour
    {
        [System.Serializable]
        public class FlowerData
        {
            public string flowerName = "New Flower";
            [Tooltip("高いほど選ばれやすい")]
            public float spawnWeight = 1f;

            [Header("Sprites (アニメーション順)")]
            public Sprite[] spawnSprites;
            public Sprite idleSprite;
            public Sprite[] despawnSprites;

            [Header("Animation Settings")]
            [Tooltip("1秒間に何枚の画像を切り替えるか")]
            public float animationFPS = 12f;
        }

        [Header("References")]
        [Tooltip("花のベースとなる空のプレハブ（SpriteRendererとDynamicFlower_Alphaをアタッチしたもの）")]
        public GameObject flowerPrefab;
        [Tooltip("プレイヤーのTransform")]
        public Transform playerTransform;

        [Header("Flower Types")]
        public List<FlowerData> flowerTypes = new List<FlowerData>();

        [Header("Spawn Settings")]
        [Tooltip("プレイヤーの周囲何メートルの範囲に花を咲かせるか")]
        public float spawnRadius = 8f;
        
        [Tooltip("花の寿命（秒）のランダム幅")]
        public Vector2 lifespanRange = new Vector2(3f, 4f);

        [Tooltip("1秒間に何回生成を試行するか")]
        public float spawnRate = 2f;
        
        [Tooltip("1回の生成で同時に咲く花の数（クラスター）")]
        public Vector2Int spawnClusterSize = new Vector2Int(1, 3);
        
        [Tooltip("1つのクラスター内での花の散らばり具合")]
        public float clusterSpread = 1.5f;

        private float spawnTimer = 0f;

        private void Update()
        {
            if (playerTransform == null || flowerPrefab == null || flowerTypes.Count == 0) return;

            spawnTimer += Time.deltaTime;
            float spawnInterval = 1f / spawnRate;

            if (spawnTimer >= spawnInterval)
            {
                spawnTimer -= spawnInterval;
                SpawnFlowerCluster();
            }
        }

        private void SpawnFlowerCluster()
        {
            // プレイヤー周辺のランダムな基準点を決定
            Vector2 randomDir = Random.insideUnitCircle;
            Vector3 clusterCenter = playerTransform.position + new Vector3(randomDir.x, randomDir.y, 0f) * spawnRadius;

            // クラスター内の生成数を決定
            int count = Random.Range(spawnClusterSize.x, spawnClusterSize.y + 1);

            for (int i = 0; i < count; i++)
            {
                // 基準点からさらに少し散らす
                Vector2 spreadDir = Random.insideUnitCircle * clusterSpread;
                Vector3 spawnPos = clusterCenter + new Vector3(spreadDir.x, spreadDir.y, 0f);

                // 花の種類を抽選
                FlowerData selectedFlower = GetRandomFlower();
                if (selectedFlower == null) continue;

                // 生成
                GameObject obj = Instantiate(flowerPrefab, spawnPos, Quaternion.identity, transform);
                DynamicFlower_Alpha flowerScript = obj.GetComponent<DynamicFlower_Alpha>();
                
                if (flowerScript != null)
                {
                    float lifespan = Random.Range(lifespanRange.x, lifespanRange.y);
                    flowerScript.Initialize(selectedFlower, lifespan);
                }
            }
        }

        private FlowerData GetRandomFlower()
        {
            float totalWeight = 0f;
            foreach (var flower in flowerTypes)
            {
                totalWeight += flower.spawnWeight;
            }

            float randomVal = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var flower in flowerTypes)
            {
                currentWeight += flower.spawnWeight;
                if (randomVal <= currentWeight)
                {
                    return flower;
                }
            }

            return flowerTypes.Count > 0 ? flowerTypes[0] : null;
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(playerTransform.position, spawnRadius);
            }
        }
    }
}
