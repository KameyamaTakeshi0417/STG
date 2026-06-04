using UnityEngine;
using Alpha.Core.Utils;
using System.Collections.Generic;

namespace Alpha.Bomb
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Bomb_Basic : MonoBehaviour
    {
        public float maxRadius = 10f;
        public float expandDuration = 1.0f;
        public float persistDuration = 2.0f;
        public float damagePerTick = 20f; // ユーザー指定のtickダメージ量

        private float currentRadius = 0f;
        private float timer = 0f;
        private CircleCollider2D col;

        // ボムの範囲内にいる敵を管理
        private HashSet<GameObject> enemiesInArea = new HashSet<GameObject>();

        private void Awake()
        {
            col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0f;
        }

        private void Start()
        {
            if (Alpha_TickManager.Instance != null)
            {
                Alpha_TickManager.Instance.OnTick += HandleTick;
            }
        }

        private void OnDestroy()
        {
            if (Alpha_TickManager.Instance != null)
            {
                Alpha_TickManager.Instance.OnTick -= HandleTick;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer <= expandDuration)
            {
                currentRadius = Mathf.Lerp(0, maxRadius, timer / expandDuration);
                col.radius = currentRadius;
            }
            else if (timer >= expandDuration + persistDuration)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            IBombDestructible destructible = collision.GetComponentInParent<IBombDestructible>();
            if (destructible != null)
            {
                destructible.OnBombDestruct();
            }

            if (collision.CompareTag("Enemy"))
            {
                _Health_Base health = collision.GetComponentInParent<_Health_Base>();
                if (health != null && !enemiesInArea.Contains(health.gameObject))
                {
                    enemiesInArea.Add(health.gameObject);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                _Health_Base health = collision.GetComponentInParent<_Health_Base>();
                if (health != null && enemiesInArea.Contains(health.gameObject))
                {
                    enemiesInArea.Remove(health.gameObject);
                }
            }
        }

        private void HandleTick()
        {
            // 消滅した敵をリストから除外
            enemiesInArea.RemoveWhere(e => e == null);

            foreach (var enemyObj in enemiesInArea)
            {
                _Health_Base health = enemyObj.GetComponent<_Health_Base>();
                if (health != null)
                {
                    health.ApplyDamage(damagePerTick, null); 
                }
            }
        }
    }
}
