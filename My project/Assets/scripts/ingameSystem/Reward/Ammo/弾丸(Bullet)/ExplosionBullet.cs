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
        if (!gameObject.activeInHierarchy) return;

        // 陦晉ｪ√＠縺溘が繝悶ず繧ｧ繧ｯ繝医・繧ｿ繧ｰ繧偵メ繧ｧ繝・け
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            base.callHitEffect();

            // 蠑ｾ縺ｮ霑ｽ蜉蜉ｹ譫懶ｼ医お繝輔ぉ繧ｯ繝育ｭ会ｼ峨・蛻ｰ驕泌・逅・ｒ蜻ｼ縺ｳ蜃ｺ縺・
            if (activeEffects != null)
            {
                foreach (var effect in activeEffects)
                {
                    effect.OnHit(this, collision);
                }
            }

            // HP繧呈戟縺､繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ蜿門ｾ・
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                // HP繧呈ｸ帙ｉ縺・
                health.ApplyDamage(dmg);
                GameObject bulletPrefab = Instantiate(
                    Resources.Load<GameObject>("Objects/Effect_Explosion"),
                    transform.position,
                    Quaternion.identity
                );
                bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);
            }

            // 蠑ｾ繧堤ｴ螢・
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

