using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBullet : Bullet_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.callHitEffect();
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                // HPを減らす
                health.TakeDamage(dmg);
                GameObject bulletPrefab = Instantiate(
                    Resources.Load<GameObject>("Objects/Effect_Explosion"),
                    transform.position,
                    Quaternion.identity
                );
                bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);
            }

            // 弾を破壊
            Destroy(this.gameObject);
        }

        if (collision.CompareTag("wall"))
            DestroyCheck();
    }
}
