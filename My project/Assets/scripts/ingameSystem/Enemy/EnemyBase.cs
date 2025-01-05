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

    protected virtual void Init()
    {
        Player = GameObject.Find("Player");
        myHealth = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
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

    protected virtual IEnumerator ChasePlayer()
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
}
