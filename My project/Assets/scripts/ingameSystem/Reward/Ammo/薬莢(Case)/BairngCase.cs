using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BairngCase : Case_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    protected override IEnumerator move()
    {
        //移動中にベアリング弾を拡散させてほしいなあ
        int count = 0;
        Rigidbody2D rb;
        //弾の発射
        rb = gameObject.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(rotate.x, rotate.y) * Speed;
        rb.AddForce(force);
        while (count <= DestroyTime)
        {
            // 弾の位置を更新する
            count++;
            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
        yield break;
    }
}
