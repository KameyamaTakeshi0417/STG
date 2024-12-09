using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingCase : Case_Base
{
    GameObject targetEnemy;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    protected override IEnumerator move()
    {
        int count = 0;
        Rigidbody2D rb;
        //弾の発射
        rb = gameObject.GetComponent<Rigidbody2D>();
        GameObject player = GameObject.Find("Player");
        targetEnemy = GameObject.Find("Player").GetComponent<Player>().getTargetEnemy();
        Vector3 targetWay = new Vector3(0, 0, 0);
        Vector2 force = new Vector2(rotate.x, rotate.y) * Speed;
        if (targetEnemy != null)
        {
            targetWay = targetEnemy.transform.position - transform.position;
            Vector3.Normalize(targetWay);
            force = new Vector2(targetWay.x, targetWay.y) * (Speed * 0.001f);
        }
        else
        {
            targetWay = player.GetComponent<Player>().watch;
            Vector3.Normalize(targetWay);
            force = new Vector2(targetWay.x, targetWay.y) * (Speed * 0.001f);
            DestroyTime = 100f;
        }
        rb.velocity = force;

        while (count <= DestroyTime)
        {
            // 弾の位置を更新する
            if (targetEnemy != null)
            {
                targetEnemy = GameObject.Find("Player").GetComponent<Player>().getTargetEnemy();

                rb.velocity = Vector3.zero;
                targetWay = targetEnemy.transform.position - transform.position;
                Vector3.Normalize(targetWay);
                force = new Vector2(targetWay.x, targetWay.y) * (Speed * 0.001f);
                rb.velocity = force;
            }
            else
            {
                force = new Vector2(targetWay.x, targetWay.y) * (Speed * 0.01f);
                rb.velocity = force;
            }
            count++;
            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
        yield break;
    }
}
