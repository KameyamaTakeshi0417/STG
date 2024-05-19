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
    public float BulletSpan; // フレーム
    public bool onCoolTime;
    public Vector3 watch;
    public float lockOnRadius = 5f; // ロックオンの半径

    private Transform lockOnTarget; // ロックオン対象

    void Start()
    {
        onCoolTime = false;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // マウスの位置を取得
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Z座標は0に固定

        // 近くのエネミーをロックオン
        LockOnEnemy(mousePosition);

        // ロックオン対象がいる場合、その方向に向ける
        if (lockOnTarget != null)
        {
            watch = (lockOnTarget.position - transform.position).normalized;
        }
        else
        {
            // ロックオン対象がいない場合はマウスポインタの方向に向ける
            watch = (mousePosition - transform.position).normalized;
        }

        // プレイヤーの向きを変更
        float angle = Mathf.Atan2(watch.y, watch.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // 弾の発射処理
        if (Input.GetMouseButton(0) && !onCoolTime)
        {
            onCoolTime = true;
            float ratio = 1.5f;
            Vector3 createPos = transform.position + watch * ratio;
            GameObject bulletPrefab = Instantiate(bullet, createPos, Quaternion.identity);
            bulletPrefab.GetComponent<Bullet_Base>().setRotate(watch);
            bulletPrefab.GetComponent<Bullet_Base>().setBulletSpeed(bulletSpeed);
            StartCoroutine(CoolTime());
        }

        // 入力がない場合は何もしない
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // キャラクターを移動させる
        float moveAngle = Mathf.Atan2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * Mathf.Rad2Deg;
        rb.velocity = Quaternion.AngleAxis(moveAngle, Vector3.forward) * Vector2.right * moveSpeed;
    }

    private void LockOnEnemy(Vector3 mousePosition)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = lockOnRadius;
        lockOnTarget = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(mousePosition, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                lockOnTarget = enemy.transform;
            }
        }
    }

    private IEnumerator CoolTime()
    {
        int count = 0;
        while (true)
        {
            if (count >= BulletSpan)
            {
                onCoolTime = false;
                yield break;
            }
            count++;
            yield return new WaitForEndOfFrame();
        }
    }
}