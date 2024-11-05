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
  // Start is called before the first frame update
  void Awake()
  {
    Player = GameObject.Find("Player");
    myHealth = gameObject.GetComponent<Health>();

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
    //プレイヤーに衝突してダメージを与えるまで追跡し続ける
    while (true)
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
      yield return new WaitForEndOfFrame();
    }
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
    slime1.GetComponent<slimeType>().chaseSpeed = chaseSpeed * 0.75f;
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
}

