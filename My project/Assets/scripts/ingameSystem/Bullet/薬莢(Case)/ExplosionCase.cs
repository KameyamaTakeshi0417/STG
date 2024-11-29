using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCase : Case_Base
{
    // Start is called before the first frame update
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
                GameObject bulletPrefab = Instantiate(
                    Resources.Load<GameObject>("Objects/Effect_Explosion"),
                    gameObject.transform.position,
                    Quaternion.identity
                );
                bulletPrefab.GetComponent<Effect_Explosion>().startExplosion(30, 50);
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
        yield break;
    }

    public override void setScriptableData()
    {
        mydata.setDataItemInfo("Explosion_Case", 1);
        mydata.setDataforPlayer(0, 0, 0);
        mydata.setDataforBullet(dmg, Speed, 0);
    }
}
