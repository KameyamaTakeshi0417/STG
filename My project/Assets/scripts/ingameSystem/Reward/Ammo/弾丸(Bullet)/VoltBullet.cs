using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alpha.Core;

public class VoltBullet : Bullet_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    protected override void Update() { base.Update(); }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        // 敵または壁に着弾した際、藤色の飛沫エフェクトを発生
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player") || collision.CompareTag("wall"))
        {
            Vector3 hitPos = collision.ClosestPoint(transform.position);
            Vector3 bulletDir = (rb != null && rb.velocity.sqrMagnitude > 0.01f) ? (Vector3)rb.velocity.normalized : rotate.normalized;
            ProceduralPoisonSplash_Alpha.SpawnSplash(hitPos, bulletDir, 7);
        }

        // Bullet_Base の標準着弾処理（貫通・多段ヒット・SE・エフェクト呼び出し・消滅判定）を実行
        base.OnTriggerEnter2D(collision);
    }
}
