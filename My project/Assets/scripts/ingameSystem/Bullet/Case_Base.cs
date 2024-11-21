using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Case_Base : MonoBehaviour
{
    protected GameObject myBullet;
    public float DestroyTime; //弾の存在する時間
    public float Speed;
    public Vector3 rotate; //弾の発射角
    public int rarelity;
    public float dmg; // 弾のダメージ量
    public string CaseName;
    public ItemData mydata;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public string getName() { return CaseName; }
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
    public void setRarelity(int rate) { rarelity = rate; }
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
    public virtual void ApplyCaseEffect(GameObject bullet)
    {

        StartCoroutine(move());
        // 弾丸が移動中の効果を実装
//        Debug.Log("Case effect applied during bullet flight.");
    }
    public void setBulletObj(GameObject bulletObj)
    {
        myBullet = bulletObj;
    }
    public float getDmg() { return dmg; }
    public float getSpeed() { return Speed; }
    public int getRarelity() { return rarelity; }
    public virtual void setScriptableData(){
        mydata.setDataItemInfo("Bullet_Base",1);
        mydata.setDataforPlayer(0,0,0);
        mydata.setDataforBullet(dmg,Speed,0);
    }
}
