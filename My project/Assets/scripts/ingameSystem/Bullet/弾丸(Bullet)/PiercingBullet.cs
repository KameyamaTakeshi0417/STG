using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingBullet : Bullet_Base
{
    // Start is called before the first frame update
    int piercingCount = 1;
    void Start()
    {
        piercingCount = rarelity;
    }
    void Awake()
    {
        //penetrateCount = rarelity;
    }
    // Update is called once per frame
    void Update()
    {

    }
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
                health.TakeDamage(dmg);

            }

            if (piercingCount == 0)
            {
                // 弾を破壊
                Destroy(this.gameObject);
            }
            piercingCount -= 1;
        }

        if (collision.CompareTag("wall")) Destroy(this.gameObject);
    }
}
