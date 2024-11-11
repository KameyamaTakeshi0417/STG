using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Volt : MonoBehaviour
{
    private float dmg = 30f;
    private int voltTime = 75;
    private Vector3 scale = new Vector3(1, 1, 0);
    int shockCount = 2;
    public Queue<GameObject> hitQueue = new Queue<GameObject>(); // 当たったオブジェクトを格納するキュー
    public GameObject VoltZone;
    // Start is called before the first frame update
    void Start()
    {
        VoltZone.GetComponent<Voltpropagation_Effect>().CreateVolt(hitQueue, shockCount);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startVolt(float setdmg, int setShockTime, int setShockCount, Queue<GameObject> queue = null)
    {
        dmg = setdmg;
        voltTime = setShockTime;
        shockCount = setShockCount;
        StartCoroutine(startVolt());
    }
    private IEnumerator startVolt()
    {
        int count = 0;
        while (count < voltTime)
        {

            count++;
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);
        yield break;
    }
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            shockCount -= 1;
            if (shockCount > 0)
            {
                hitQueue.Enqueue(collision.gameObject); // キューにオブジェクトを追加
                collision.GetComponent<Health>().TakeDamage(dmg);
                VoltZone.GetComponent<Voltpropagation_Effect>().CreateVolt(hitQueue, shockCount);

            }
            if (shockCount < 0)
            {
                Destroy(this.gameObject);
            }

        }
    }
}
