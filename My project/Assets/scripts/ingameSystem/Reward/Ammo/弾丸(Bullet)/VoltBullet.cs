using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltBullet : Bullet_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    protected override void Update() { base.Update(); }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            base.callHitEffect();


            {

                
            }

            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                health.ApplyDamage(dmg);
            }
            DestroyCheck();
        }
        else if (collision.CompareTag("wall"))
        {
            base.callHitEffect();

            {

                
            }
            DestroyCheck();
        }
    }
}


