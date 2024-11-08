using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class ArmorBeetle : MonoBehaviour
{

    public GameObject Player;
    public float chaseSpeed;
    public Health myHealth;
    private Rigidbody2D rb;

    public int blockPoint;
    public bool enemyType;//それぞれ攻撃か妨害かを選べる
    public int pow;
    public Vector3 rotate; //プレイヤーに向かった時の角度
    public float speedMag;
    public bool makeBarrier;
    private GameObject canvasPrefab; // CanvasのPrefabをインスペクターで設定
    // Start is called before the first frame update

    void Awake()
    {
        gameObject.GetComponent<Health>().setSlideHPBar();
        Player = GameObject.Find("Player");
        myHealth = gameObject.GetComponent<Health>();
        makeBarrier = false;
        Player = GameObject.Find("Player");
        if (Player == null)
        {
            Debug.LogError("Player object not found!");
        }

        myHealth = gameObject.GetComponent<Health>();
        if (myHealth == null)
        {
            Debug.LogError("Health component not found!");
        }

        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found!");
        }

        makeBarrier = false;

        // 初期化がすべて正常に行われた場合のみコルーチンを開始
        if (Player != null && myHealth != null && rb != null)
        {
            StartCoroutine(Idle());
        }
        else
        {
            Debug.LogError("Initialization failed. Coroutine will not be started.");
        }
        // StartCoroutine(Idle());
    }

    // Update is called once per frame
    void Update()
    {

    }
    private IEnumerator Idle()
    {//ダンジョン開始即追尾開始
        Debug.Log("StartIdle");
        stopMovingByVelocity();
        yield return new WaitForSeconds(1);
        if (makeBarrier == false && gameObject.GetComponent<Health>().getCurrentHP() < gameObject.GetComponent<Health>().getHP())
        {

            yield return blocking();

        }
        yield return chase();
    }
    private IEnumerator chase()
    {
        int chaseTime = 3000;//UnityEngine.Random.Range(3, 7);
        int count = 0;
        Vector3 chaseWay = new Vector3(0, 0, 0);
        rb = gameObject.GetComponent<Rigidbody2D>();
        Debug.Log("StartChase");
        //プレイヤーを一定時間追いかける
        while (count <= chaseTime)
        {
            if (makeBarrier == false && gameObject.GetComponent<Health>().getCurrentHP() < gameObject.GetComponent<Health>().getHP())
            {

                yield return blocking();

            }
            chaseWay = (Vector3)(GameObject.Find("Player").transform.position - gameObject.transform.position);
            chaseWay.Normalize();
            setRotate(chaseWay);
            Vector2 force = new Vector2(rotate.x, rotate.y);
            rb.AddForce(force * speedMag);
            count++;
            yield return new WaitForEndOfFrame();
        }
        rb.velocity = Vector2.zero;
        yield return Idle();
    }
    private IEnumerator blocking()
    {
        Debug.Log("StartBlock");
        stopMovingByVelocity();
        makeBarrier = true;

        //被弾したときに初期体力に等しいブロックを生成して身を守る
        GameObject barrier = Instantiate(Resources.Load<GameObject>("barrier"), gameObject.transform.position, Quaternion.identity);
        barrier.GetComponent<Health>().setHP(gameObject.GetComponent<Health>().getHP());
        barrier.GetComponent<Health>().setCurrentHP(gameObject.GetComponent<Health>().getHP());
        barrier.GetComponent<barrier>().startDisappear();
        yield return new WaitForSeconds(3);
        Destroy(barrier);

        yield return chase();

    }
    private IEnumerator attacking()
    {
        //いつもより素早く動いて接触によるダメージを試みる
        yield return null;
    }
    private IEnumerator debuffing()
    {
        //粘液をはいてデバフを試みる
        yield return null;
    }
    private void stopMovingByVelocity()
    {
        rb.angularVelocity = 0f;
        rb.velocity = Vector2.zero;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                // HPを減らす
                health.TakeDamage(pow);
            }
        }

    }
    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(0, 0, MathF.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90);
        rotate = rot.normalized;

    }
}
