using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slimeType : MonoBehaviour
{
  public int separateCount;//被弾時に分裂する能力
  public GameObject Player;
  public float chaseSpeed;
  public Health myHealth;
  private Rigidbody2D rb;
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
      if (gameObject.GetComponent<Health>().getCurrentHP() <= (gameObject.GetComponent<Health>().getHP()) * 0.5f)
      {
        Debug.Log("体力半分");
        yield return separate();
      }
      chaseWay = (Vector3)(Player.transform.position - gameObject.transform.position);
      chaseWay.Normalize();
      transform.position += chaseWay * chaseSpeed;

      yield return new WaitForSeconds(1f);
    }
    yield return null;
  }
  private IEnumerator separate()
  {
    //体力が半分以下になったら数秒待って、現在の体力の半分のスライムを作成する
    int count = 0;
    const int COUNT_MAX = 3;
    //分裂予備動作
    while (true)
    {
      count += 1;
      if (count >= COUNT_MAX)
      {
        break;
      }
      count++;
      yield return new WaitForSeconds(3);
    }
    //分裂処理
    GameObject slime1 = Instantiate(Resources.Load<GameObject>("slime"), gameObject.transform.position - new Vector3(3, 2, 0), Quaternion.identity);
    GameObject slime2 = Instantiate(Resources.Load<GameObject>("slime"), gameObject.transform.position - new Vector3(-3, 2, 0), Quaternion.identity);
    slime1.GetComponent<slimeType>().separateCount = separateCount -= 1;
    slime2.GetComponent<slimeType>().separateCount = separateCount -= 1;
    slime1.transform.localScale = (gameObject.transform.localScale) * 0.75f;
    slime2.transform.localScale = (gameObject.transform.localScale) * 0.75f;
    Destroy(this.gameObject);
    yield return null;
  }
}
