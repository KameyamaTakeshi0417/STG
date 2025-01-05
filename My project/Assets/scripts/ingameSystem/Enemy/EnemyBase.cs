using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
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

    public GameObject Exp;
    public int ExpCount = 1;
    public float ExpRadius = 2.0f;

    protected virtual void Init()
    {
        Player = GameObject.Find("Player");
        myHealth = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        Exp = Resources.Load<GameObject>("Objects/MoneyAndExp");
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health health = collision.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(pow);
            }
        }
    }

    public virtual void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(
            0,
            0,
            Mathf.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90
        );
        rotate = rot.normalized;
    }

    protected virtual IEnumerator Idle()
    {
        //被弾していない間は動かない
        while (true)
        {
            if (
                gameObject.GetComponent<Health>().getCurrentHP()
                <= gameObject.GetComponent<Health>().getHP()
            )
            {
                yield return ChasePlayer();
                break;
            }
            yield return new WaitForSeconds(1);
        }
        yield return null;
    }

    protected IEnumerator ChasePlayer()
    {
        Vector3 chaseWay;
        int count = 0;

        while (count < 1000)
        {
            chaseWay = (Player.transform.position - transform.position).normalized;
            setRotate(chaseWay);
            Vector2 force = new Vector2(rotate.x, rotate.y);
            rb.AddForce(force * chaseSpeed);
            count++;
            yield return new WaitForEndOfFrame();
        }
    }

    protected void Die()
    {
        Vector3 position = new Vector3(
            Random.Range(ExpRadius, ExpRadius),
            0,
            Random.Range(ExpRadius, ExpRadius)
        );
        int RandomAddPoint = Random.Range((ExpCount * -1) + 1, ExpCount);
        Vector3 ObjPos = gameObject.transform.position;
        for (int i = 0; i < ExpCount + RandomAddPoint; i++)
        {
            position = new Vector3(
                Random.Range(ExpRadius, ExpRadius),
                0,
                Random.Range(ExpRadius, ExpRadius)
            );
            GameObject bulletPrefab = Instantiate(Exp, ObjPos + position, Quaternion.identity); //弾の生成
        }

        Destroy(this.gameObject);
    }
}
