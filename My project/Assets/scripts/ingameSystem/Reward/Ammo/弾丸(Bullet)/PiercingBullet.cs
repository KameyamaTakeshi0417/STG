using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingBullet : Bullet_Base
{
    // Start is called before the first frame update


    void Start()
    {
        piercingCount = rarelity + 1;
    }

    void Awake()
    {
        //penetrateCount = rarelity;
    }

    // Update is called once per frame
    void Update() { }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.callHitEffect();
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                // HPを減らす
                health.TakeDamage(dmg);
                DestroyCheck();
            }
        }

        if (collision.CompareTag("wall"))
            DestroyCheck();
    }
}
