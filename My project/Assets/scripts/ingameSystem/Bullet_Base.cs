using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet_Base : MonoBehaviour
{
    protected Rigidbody2D rb;
    public float dmg; // 弾のダメージ量
    public float Speed; //弾の出る速度
    public float DestroyTime; //弾の存在する時間
    public float bullettype = 0; //弾のタイプ決定
    public float addforce = 1000; //弾のタイプ決定
    public Vector3 rotate; //弾の発射角

    public int penetorateCount;//弾丸を貫通する回数
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }
    public void setDmg(float damage)
    {

        dmg = damage;
    }
    //弾の撃つ角度の正規化
    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(0, 0, MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90);
        rotate = rot.normalized;
    }
    //弾の速度決定
    public void setBulletSpeed(float mag)
    {
        rotate *= mag;
        StartCoroutine(move());
    }
    //弾の特性決定
    public void setBulletType(float type)
    {
        bullettype = type;
        if (type != 1) setBulletPenetrate(1);
    }
    //弾丸の貫通回数設定
    public void setBulletPenetrate(int count)
    {
        penetorateCount = count;
    }

    //弾を撃ち出す
    protected virtual IEnumerator move()
    {
        int count = 0;

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
        Destroy(this.gameObject);
        yield break;
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのタグをチェック
        if (collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                // HPを減らす
                health.TakeDamage(dmg);
            }
            //貫通弾では弾を破壊しない
            if (bullettype == 1)
            {
                //何もしない
            }
            else
            {
                // 弾を破壊
                Destroy(this.gameObject);
            }
        }

        if (collision.CompareTag("wall")) Destroy(this.gameObject);
    }
}

