using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    public float HP;
    public float Pow;
    public float moveSpeed;
    public GameObject bullet;
    public float bulletSpeed;
    public float BulletSpan;
    public Vector3 watch;

    // Start is called before the first frame update
    void Start()
    {
       // bullet = Instantiate(bullet, transform.position, Quaternion.identity);

        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // 入力の角度を取得する
        float moveAngle = Mathf.Atan2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * Mathf.Rad2Deg;
        watch = new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float ratio = 1.5f;
            Vector3 createPos = transform.position + new Vector3(watch.x * ratio, watch.y * ratio, watch.z);
            GameObject bulletPrefab = Instantiate(bullet, createPos, Quaternion.identity);
            bulletPrefab.GetComponent<Bullet_Base>().setRotate(watch);
            Destroy(bulletPrefab, 3);
        }
        // 入力がない場合は何もしない
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }

            // キャラクターを移動させる
            rb.velocity = Quaternion.AngleAxis(moveAngle, Vector3.forward) * Vector2.right * moveSpeed;

        // キャラクターの向きを変える
        transform.rotation = Quaternion.AngleAxis(moveAngle, Vector3.forward);
    }
}
