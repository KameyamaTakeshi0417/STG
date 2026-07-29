using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBullet : Bullet_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    protected override void Update() { base.Update(); }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            base.callHitEffect();

            // 弾の追加効果（エフェクトなど）の判定・発動
            if (activeEffects != null)
            {
                foreach (var effect in activeEffects)
                {
                    effect.OnHit(this, collision);
                }
            }

            // HPを持っているコンポーネントを取得
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                // HPを減らす
                health.ApplyDamage(dmg);
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
        else if (collision.CompareTag("wall"))
        {
            base.callHitEffect();
            if (activeEffects != null)
            {
                foreach (var effect in activeEffects)
                {
                    effect.OnHit(this, collision);
                }
            }
            DestroyCheck();
        }
    }
}
