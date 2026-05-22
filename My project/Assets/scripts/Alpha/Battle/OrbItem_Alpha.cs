using UnityEngine;
using Alpha.Data;

namespace Alpha.Battle
{
    [RequireComponent(typeof(Collider2D))]
    public class OrbItem_Alpha : MonoBehaviour
    {
        [Header("Orb Data")]
        public OrbData_Alpha orbData;

        [Header("Collection Settings")]
        public float autoCollectDelay = 5f; // 自動回収が始まるまでの時間
        public float collectSpeed = 10f;    // プレイヤーに向かう速度
        public float collectDistance = 0.5f; // 回収完了となる距離

        private float timer = 0f;
        private bool isAutoCollecting = false;
        private Transform playerTransform;

        void Start()
        {
            // プレイヤーを探しておく（Playerタグがついている前提）
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        void Update()
        {
            if (isAutoCollecting && playerTransform != null)
            {
                // プレイヤーに向かって移動
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, collectSpeed * Time.deltaTime);

                // 十分に近づいたら回収
                if (Vector3.Distance(transform.position, playerTransform.position) <= collectDistance)
                {
                    Collect();
                }
            }
            else
            {
                // 自動回収のタイマー
                timer += Time.deltaTime;
                if (timer >= autoCollectDelay && playerTransform != null)
                {
                    isAutoCollecting = true;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // プレイヤーが接触したかチェック
            if (collision.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            // treasureManager_AlphaにOrbを渡す
            if (treasureManager_Alpha.Instance != null && orbData != null)
            {
                treasureManager_Alpha.Instance.PushOrb(orbData);
                Debug.Log($"[OrbItem] Collected Orb (Rarity: {orbData.orbRarity}, Source: {orbData.source})");
            }
            else
            {
                Debug.LogWarning("[OrbItem] treasureManager_Alpha instance is null, but tried to collect orb!");
            }

            // 演出を入れる場合はここで再生

            Destroy(gameObject);
        }
    }
}
