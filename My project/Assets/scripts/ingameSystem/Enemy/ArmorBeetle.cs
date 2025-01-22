using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ArmorBeetle : EnemyBase
{
    public bool makeBarrier;

    Coroutine currentCoroutine;
    public Sprite blockSprite;
    public Sprite StandardSprite;

    void Awake()
    {
        base.Init();
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

    protected override IEnumerator Idle()
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
            base.setRotate(chaseWay);
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
        GetComponent<SpriteRenderer>().sprite = blockSprite;
        // バリア生成
        GameObject barrier = Instantiate(
            Resources.Load<GameObject>("Objects/Enemy/barrier"),
            transform.position,
            Quaternion.identity
        );
        barrier.GetComponent<Health>().setHP(myHealth.getHP());
        barrier.GetComponent<Health>().setCurrentHP(myHealth.getHP());
        barrier.GetComponent<barrier>().startDisappear();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
        // 一定時間待機
        yield return new WaitForSeconds(5);

        Destroy(barrier);
        //makeBarrier = false;

        // 次の状態へ移行
        GetComponent<SpriteRenderer>().sprite = StandardSprite;
        rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
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
}
