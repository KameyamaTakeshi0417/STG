using UnityEngine;
using System.Collections.Generic;

namespace Alpha.Enemy.Weapons
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Alpha_GiantMissileExplosion : MonoBehaviour
    {
        public float damage = 100f;
        public float duration = 2f;
        
        private HashSet<PlayerHealth> damagedPlayers = new HashSet<PlayerHealth>();
        private float timer = 0f;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var health = collision.GetComponentInParent<PlayerHealth>();
                if (health != null && !damagedPlayers.Contains(health))
                {
                    // 無敵状態などであればTakeDamage内部で弾かれるが、
                    // 爆発が持続している間に無敵が切れればダメージが入る
                    // 実際にダメージを与えられた場合のみリストに追加して2回目を防ぐ
                    if (!health.isInvincible)
                    {
                        health.TakeDamage(damage);
                        damagedPlayers.Add(health);
                    }
                }
            }
        }
    }
}
