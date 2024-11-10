using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Case_Base : MonoBehaviour
{
    GameObject myBullet;
    public float addforce = 1000; //弾のタイプ決定
    public float DestroyTime; //弾の存在する時間
    public Vector3 rotate; //弾の発射角
    public int rarelity;
    public float dmg; // 弾のダメージ量
    public bool moveEnd;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public bool getMoveEnd() { return moveEnd; }
    public void startMove()
    {
        StartCoroutine(move());
    }
    public void setTargetBullet(GameObject targetObj){
        myBullet=targetObj;
    }
    public void setRarelity(int rate) { rarelity = rate; }
    protected virtual IEnumerator move()
    {
        moveEnd=false;
        int count = 0;
        Rigidbody2D rb;
        //弾の発射
        rb = gameObject.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(rotate.x * addforce, rotate.y * addforce);
        rb.AddForce(force);

        while (count <= DestroyTime)
        {
            // 弾の位置を更新する
            //transform.Translate(rotate * Speed * Time.deltaTime, Space.Self);
            count++;
            yield return new WaitForSeconds(0.01f);
        }
        moveEnd=true;
        //Destroy(myBullet);
        yield break;
    }
        public virtual void ApplyCaseEffect(GameObject bullet)
    {
        myBullet=bullet;
        rotate=GameObject.Find("Player").GetComponent<Player>().getRotate();
        StartCoroutine(move());
        // 弾丸が移動中の効果を実装
        Debug.Log("Case effect applied during bullet flight.");
    }
}
