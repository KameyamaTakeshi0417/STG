using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet_Base : MonoBehaviour
{
    public string Objname;
    protected Rigidbody2D rb;
    public float dmg; // 弾のダメージ量
    public float Speed; //弾の出る速度
    public float DestroyTime; //弾の存在する時間
    public float bullettype = 0; //弾のタイプ決定
    public Vector3 rotate; //弾の発射角

    public int rarelity; //オブジェクトの挙動が変わるもの
    public string bulletName;
    public float addDmg; //ダメージ倍率のかからない固定ダメージ
    public int piercingCount = 0;

    // Start is called before the first frame update
    void Start() { }

    public string getBulletName()
    {
        return bulletName;
    }

    // Update is called once per frame
    void Update() { }

    public void setDmg(float damage)
    {
        dmg = damage;
    }

    //弾の撃つ角度の正規化
    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }

    //弾の速度決定
    public void setBulletSpeed(float mag) { }

    //弾の特性決定

    //弾丸の貫通回数設定


    public void setStatus(Vector3 Prot, float pSpeed, float pDmg)
    {
        rotate = Prot;
        Speed = pSpeed;
        dmg = pDmg;
    }

    public void shoot()
    {
        StartCoroutine(move());
    }

    public void fire()
    {
        gameObject.GetComponent<Case_Base>().setStatus(rotate, Speed, dmg);
        gameObject.GetComponent<Case_Base>().ApplyCaseEffect(this.gameObject);
    }

    //弾を撃ち出す
    protected virtual IEnumerator move()
    {
        int count = 0;

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
        Destroy(this.gameObject);
        yield break;
    }

    public void callHitEffect()
    {
        DrainEffect targetScript;
        targetScript = GetComponent<DrainEffect>();
        if (targetScript != null)
        {
            targetScript.MakeEffect();
        }
        StartCoroutine(hitEffect());
    }

    protected IEnumerator hitEffect()
    {
        yield return null;
    }

    protected void DestroyCheck()
    {
        piercingCount--;

        if (piercingCount <= 0)
        {
            Destroy(this.gameObject);
        }
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

            // 弾を破壊
            Destroy(this.gameObject);
        }

        if (collision.CompareTag("wall"))
            Destroy(this.gameObject);
    }

    protected float damageCaluculator(float pow, float mag)
    {
        float ret = 0f;
        ret = addDmg + (pow + dmg) * mag;

        return ret;
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
}
