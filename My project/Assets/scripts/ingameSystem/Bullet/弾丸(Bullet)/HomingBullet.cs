using System.Collections;
using UnityEngine;

public class HomingBullet : Bullet_Base
{
    public float pullForce = 10f; // 引き寄せる力の強さ

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy"))
        {
            // HPを持つコンポーネントを取得してダメージを与える
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(dmg);
            }

            // 敵を引き寄せる処理
            Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector3 directionToPlayer = (transform.position - collision.transform.position).normalized;
                enemyRb.AddForce(directionToPlayer * pullForce, ForceMode2D.Impulse);
            }

            // 弾を破壊
            Destroy(this.gameObject);
        }

        // 壁に当たった場合は弾を破壊
        if (collision.CompareTag("wall"))
        {
            Destroy(this.gameObject);
        }
    }
}
