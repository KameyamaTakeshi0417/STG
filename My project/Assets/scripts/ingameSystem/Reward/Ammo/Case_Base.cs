using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Case_Base : MonoBehaviour
{
    public string Objname;
    protected GameObject myBullet;
    public float DestroyTime = 200f; //弾の存在する時間
    public float Speed;
    public Vector3 rotate; //弾の発射角
    public int rarelity = 1;
    public float dmg = 10f; // 弾のダメージ量
    public string CaseName;
    public ItemData mydata;
    protected bool isHoming = false;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public string getName()
    {
        return CaseName;
    }

    public void setStatus(Vector3 Prot, float pSpeed, float pDmg)
    {
        rotate = Prot;
        Speed = pSpeed;
        dmg = pDmg;
    }

    public void startMove()
    {
        StartCoroutine(move());
    }

    public void setTargetBullet(GameObject targetObj)
    {
        myBullet = targetObj;
    }

    public void setRarelity(int rate)
    {
        rarelity = rate;
    }

    protected virtual IEnumerator move()
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
            yield return new WaitForSeconds(0.01f);
        }

        Destroy(myBullet);
        yield break;
    }

    public void HomingMove(GameObject targetEnemy)
    {
        Rigidbody2D rb;
        rb = gameObject.GetComponent<Rigidbody2D>();
        Vector3 targetWay = new Vector3(0, 0, 0);
        Vector2 force = new Vector2(rotate.x, rotate.y) * Speed;
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
    }

    public virtual void ApplyCaseEffect(GameObject bullet)
    {
        StartCoroutine(move());
        // 弾丸が移動中の効果を実装
        //        Debug.Log("Case effect applied during bullet flight.");
        GetComponent<Bullet_Base>().dmg += dmg;
    }

    public void setBulletObj(GameObject bulletObj)
    {
        myBullet = bulletObj;
    }

    public float getDmg()
    {
        return dmg;
    }

    public float getSpeed()
    {
        return Speed;
    }

    public int getRarelity()
    {
        return rarelity;
    }

    public virtual void setScriptableData()
    {
        mydata.setDataItemInfo("Bullet_Base", 1);
        mydata.setDataforPlayer(0, 0, 0);
        mydata.setDataforBullet(dmg, Speed, 0);
    }
}
