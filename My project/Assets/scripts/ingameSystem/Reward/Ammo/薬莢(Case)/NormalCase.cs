using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCase : Case_Base
{
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    //最終的にはこれを装備している弾は威力増加させたい
    protected override IEnumerator move()
    {
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

    public override void ApplyCaseEffect(GameObject bullet)
    {
        base.ApplyCaseEffect(bullet);
        GetComponent<Bullet_Base>().dmg += 5 * rarelity;
    }
}
