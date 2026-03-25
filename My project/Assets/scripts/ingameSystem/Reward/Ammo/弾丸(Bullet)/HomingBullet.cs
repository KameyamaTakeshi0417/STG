using System.Collections;
using UnityEngine;

public class HomingBullet : Bullet_Base
{
    public float pullForce = 10f; // 引き寄せる力の強さ

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 敵を引き寄せる独特の処理（レガシー機能の維持）
        if (collision.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector3 directionToPlayer = (transform.position - collision.transform.position).normalized;
                enemyRb.AddForce(directionToPlayer * pullForce, ForceMode2D.Impulse);
            }
        }

        // 基本的なダメージ計算、多段ヒット判定、貫通時の処理、プーリングへの返却などは
        // すべて最新の Bullet_Base 側に任せる
        base.OnTriggerEnter2D(collision);
    }
}
