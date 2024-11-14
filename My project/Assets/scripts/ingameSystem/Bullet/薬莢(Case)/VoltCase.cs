using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltCase : Case_Base
{
    protected override IEnumerator move()
    {
        int count = 0;
        Rigidbody2D rb;
        //弾の発射
        rb = gameObject.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(rotate.x, rotate.y) * Speed;
        rb.AddForce(force);

        while (count <= 50)
        {
            // 弾の位置を更新する
            count++;
            if (count % 10 == 0)
            {
                GameObject voltPrefab = Instantiate(
                    Resources.Load<GameObject>("Objects/Effect_Volt"),
                    gameObject.transform.position,
                    Quaternion.identity
                );
                voltPrefab.GetComponent<Effect_Volt>().startVolt(30, 50, 2);
            }

            yield return new WaitForSeconds(0.01f);
        }

        Destroy(this.gameObject);
        yield break;
    }

    public override void setScriptableData()
    {
        mydata.setDataItemInfo("Volt_Case", 1);
        mydata.setDataforPlayer(0, 0, 0);
        mydata.setDataforBullet(dmg, Speed, 0);
    }
}
