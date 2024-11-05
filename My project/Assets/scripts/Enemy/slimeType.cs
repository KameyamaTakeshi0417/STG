using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class slimeType : MonoBehaviour
{
  public int separateCount;//被弾時に分裂する能力
  public GameObject Player;
  public float chaseSpeed;
  public Health myHealth;
  private Rigidbody2D rb;

  public Vector3 rotate; //プレイヤーに向かった時の角度
  public int pow;
  public float radius = 2.0f;    // 半径（中心からの距離）
  private float angle = 0.0f; // 現在の角度
  public int moneyCount;
  // Start is called before the first frame update
  void Awake()
  {
    Player = GameObject.Find("Player");
    myHealth = gameObject.GetComponent<Health>();
    myHealth.setMoneyCount(moneyCount);
    StartCoroutine(Idle());
  }

  // Update is called once per frame
  void Update()
  {

  }
  private IEnumerator Idle()
  {
    //被弾していない間は動かない
    while (true)
    {
      if (gameObject.GetComponent<Health>().getCurrentHP() <= gameObject.GetComponent<Health>().getHP())
      {
        yield return PlayerChase();
        break;
      }
      yield return new WaitForSeconds(1);
    }
    yield return null;
  }
  private IEnumerator PlayerChase()
  {
    int chaseTime = UnityEngine.Random.Range(3, 7);
    Vector3 chaseWay = new Vector3(0, 0, 0);
    rb = gameObject.GetComponent<Rigidbody2D>();
    int count = 0;
    //プレイヤーに衝突してダメージを与えるまで追跡し続ける
    while (count < 1000)
    {
      if (separateCount > 0 && gameObject.GetComponent<Health>().getCurrentHP() <= (gameObject.GetComponent<Health>().getHP()) * 0.5f)
      {
        Debug.Log("体力半分");
        yield return separate();
      }
      chaseWay = (Vector3)(GameObject.Find("Player").transform.position - gameObject.transform.position);
      chaseWay.Normalize();
      setRotate(chaseWay);
      Vector2 force = new Vector2(rotate.x, rotate.y);
      rb.AddForce(force * chaseSpeed);
      count++;
      yield return new WaitForEndOfFrame();
    }
    while (true)
    {
      if (separateCount > 0 && gameObject.GetComponent<Health>().getCurrentHP() <= (gameObject.GetComponent<Health>().getHP()) * 0.5f)
      {
        Debug.Log("体力半分");
        yield return separate();
      }
      if (rb.velocity.magnitude > 0.01f) // 微小な値で判定
      {
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
      }
      else
      {
        rb.velocity = Vector2.zero; // 完全に停止
        break;
      }
    }
    yield return PlayerChase();
  }
  private IEnumerator separate()
  {
    rb.velocity = Vector2.zero;
    yield return new WaitForSeconds(1);
    //体力が半分以下になったら数秒待って、現在の体力の半分のスライムを作成する
    const int COUNT_MAX = 1;
    //分裂予備動作

    yield return new WaitForSeconds(COUNT_MAX);

    //分裂処理
    createNewSlime(-3);
    createNewSlime(3);
    Destroy(this.gameObject);
    yield return null;
  }
  private void createNewSlime(int plusMinus)
  {
    GameObject slime1 = Instantiate(Resources.Load<GameObject>("slime"), gameObject.transform.position - new Vector3(plusMinus, 2, 0), Quaternion.identity);
    slime1.GetComponent<slimeType>().separateCount = separateCount - 1;
    slime1.GetComponent<slimeType>().pow = (int)((float)pow * 0.75f);
    slime1.GetComponent<slimeType>().chaseSpeed = chaseSpeed * 1.25f;
    slime1.GetComponent<Health>().setHP(gameObject.GetComponent<Health>().getCurrentHP());
    slime1.GetComponent<Health>().setCurrentHP(gameObject.GetComponent<Health>().getCurrentHP());
    slime1.transform.localScale = (gameObject.transform.localScale) * 0.75f;

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
  private IEnumerator rolling()
  {
    Vector3 targetLen = (transform.position - GameObject.Find("Player").transform.position);
    radius = targetLen.magnitude;
    GameObject centerObject = GameObject.Find("Player");
    while (true)
    {
      // 徐々に180度に近づける
      angle = Mathf.Lerp(angle, Mathf.PI, chaseSpeed * Time.deltaTime);

      // 新しい位置を計算
      float x = centerObject.transform.position.x + Mathf.Cos(angle) * radius;
      float y = centerObject.transform.position.y + Mathf.Sin(angle) * radius;

      // オブジェクトを移動
      transform.position = new Vector2(x, y);

      // 180度に到達したか確認
      if (Mathf.Abs(angle - Mathf.PI) < 0.01f)
      {
        yield return PlayerChase();
      }
    }

  }
  public void StartMovingToOpposite()
  {
    // 現在位置からの角度を取得
    Vector2 direction = transform.position - GameObject.Find("Player").transform.position;
    angle = Mathf.Atan2(direction.y, direction.x);


  }
}

