using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingCase : Case_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    protected override IEnumerator move()
    {
        //最終的には1秒待機して2.5倍速で相手に突っ込んでほしい

        int count = 0;
        Rigidbody2D rb;
        //弾の発射
        rb = gameObject.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(rotate.x, rotate.y) * (Speed * 1.5f);
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
