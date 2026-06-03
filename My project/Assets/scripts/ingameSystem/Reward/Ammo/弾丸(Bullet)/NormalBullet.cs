using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : Bullet_Base
{
    public float AddDamageRatio = 0.3f;

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

            // 蠑ｾ縺ｮ霑ｽ蜉蜉ｹ譫懶ｼ医い繧ｯ繝・ぅ繝悶お繝輔ぉ繧ｯ繝茨ｼ峨・逹蠑ｾ蜃ｦ逅・ｒ逋ｺ轣ｫ縺輔○繧・
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
                health.ApplyDamage((dmg + (dmg * (rarelity * AddDamageRatio))));
            }

            // 蠑ｾ繧堤ｴ螢・
            DestroyCheck();
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

