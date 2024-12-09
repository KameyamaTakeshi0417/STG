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
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                // HPを減らす
                health.TakeDamage((dmg + (dmg * (rarelity * AddDamageRatio))));
            }

            // 弾を破壊
            DestroyCheck();
        }

        if (collision.CompareTag("wall"))
            DestroyCheck();
    }
}
