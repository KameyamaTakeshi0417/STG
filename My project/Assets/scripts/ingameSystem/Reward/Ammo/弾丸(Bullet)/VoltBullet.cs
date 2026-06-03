using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltBullet : Bullet_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            base.callHitEffect();

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
                // 蠑ｾ閾ｪ菴薙・蝓ｺ譛ｬ繝繝｡繝ｼ繧ｸ繧剃ｸ弱∴繧・
                health.ApplyDamage(dmg);
            }

            // 蠑ｾ繧堤ｴ螢奇ｼ郁ｲｫ騾壹↑縺ｩ縺ｮ繝√ぉ繝・け・・
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

