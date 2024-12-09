using System;
using System.Collections;
using UnityEngine;

public class ArmorBeetle : MonoBehaviour
{
    public GameObject Player;
    public float chaseSpeed;
    public Health myHealth;
    private Rigidbody2D rb;

    public int blockPoint;
    public bool enemyType; //それぞれ攻撃か妨害かを選べる
    public int pow;
    public Vector3 rotate; //プレイヤーに向かった時の角度
    public float speedMag;
    public bool makeBarrier;

    Coroutine currentCoroutine;

    void Awake()
    {
        Player = GameObject.Find("Player");
        myHealth = gameObject.GetComponent<Health>();
        makeBarrier = false;

        rb = gameObject.GetComponent<Rigidbody2D>();

        if (Player == null || myHealth == null || rb == null)
        {
            Debug.LogError("Initialization failed.");
            return;
        }

        gameObject.GetComponent<Health>().setSlideHPBar();

        // 最初のコルーチンを開始
        currentCoroutine = StartCoroutine(Idle());
    }

    private IEnumerator Idle()
    {
        Debug.Log("StartIdle");
        stopMovingByVelocity();
        yield return new WaitForSeconds(0.5f);

        if (!makeBarrier && myHealth.getCurrentHP() < myHealth.getHP())
        {
            currentCoroutine = StartCoroutine(blocking());
        }
        else
        {
            currentCoroutine = StartCoroutine(chase());
        }
    }

    private IEnumerator chase()
    {
        Debug.Log("StartChase");
        int chaseTime = 300;
        int count = 0;

        while (count <= chaseTime)
        {
            // プレイヤーに向かう
            Vector3 chaseWay = Player.transform.position - transform.position;
            chaseWay.Normalize();
            setRotate(chaseWay);
            rb.velocity = chaseWay * speedMag;

            if (!makeBarrier && myHealth.getCurrentHP() < myHealth.getHP())
            {
                // ブロック状態に移行する
                stopMovingByVelocity();
                currentCoroutine = StartCoroutine(blocking());
                yield break;
            }

            count++;
            yield return new WaitForEndOfFrame();
        }

        // チェイスが終わったらアイドルに戻る
        stopMovingByVelocity();
        currentCoroutine = StartCoroutine(Idle());
    }

    private IEnumerator blocking()
    {
        Debug.Log("StartBlock");
        stopMovingByVelocity();
        makeBarrier = true;

        // バリア生成
        GameObject barrier = Instantiate(
            Resources.Load<GameObject>("Objects/Enemy/barrier"),
            transform.position,
            Quaternion.identity
        );
        barrier.GetComponent<Health>().setHP(myHealth.getHP());
        barrier.GetComponent<Health>().setCurrentHP(myHealth.getHP());
        barrier.GetComponent<barrier>().startDisappear();

        // 一定時間待機
        yield return new WaitForSeconds(5);

        Destroy(barrier);
        //makeBarrier = false;

        // 次の状態へ移行
        currentCoroutine = StartCoroutine(Idle());
    }

    private void stopMovingByVelocity()
    {
        rb.angularVelocity = 0f;
        rb.velocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(pow);
            }
        }
    }

    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            Mathf.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }
}
