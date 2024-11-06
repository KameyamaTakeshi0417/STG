using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public GameObject Player;
    public float chaseSpeed;
    public Health myHealth;
    protected Rigidbody2D rb;
    public int pow;
    public Vector3 rotate;

    protected virtual void Awake()
    {
        Player = GameObject.Find("Player");
        myHealth = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
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

    public void setRotate(Vector3 rot)
    {
        transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(rot.y, rot.x) * Mathf.Rad2Deg + 90);
        rotate = rot.normalized;
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
}
